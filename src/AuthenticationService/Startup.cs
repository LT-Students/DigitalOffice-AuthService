using FluentValidation;
using GreenPipes;
using LT.DigitalOffice.AuthenticationService.Broker.Consumers;
using LT.DigitalOffice.AuthenticationService.Business;
using LT.DigitalOffice.AuthenticationService.Business.Interfaces;
using LT.DigitalOffice.AuthenticationService.Token;
using LT.DigitalOffice.AuthenticationService.Token.Interfaces;
using LT.DigitalOffice.AuthentificationService.Broker.Requests;
using LT.DigitalOffice.AuthentificationService.Models.Dto;
using LT.DigitalOffice.AuthentificationService.Validation;
using LT.DigitalOffice.Broker.Requests;
using LT.DigitalOffice.Kernel;
using LT.DigitalOffice.Kernel.Broker;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System;

namespace LT.DigitalOffice.AuthenticationService
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<RabbitMQOptions>(Configuration);

            ConfigureJwt(services);

            services.AddHealthChecks();

            services.AddControllers();

            ConfigureRabbitMq(services);

            services.AddMassTransitHostedService();

            ConfigureCommands(services);
            ConfigureValidators(services);
        }

        private void ConfigureJwt(IServiceCollection services)
        {
            var signingKey = new SigningSymmetricKey();
            var signingDecodingKey = (IJwtSigningDecodingKey)signingKey;

            services.AddSingleton<IJwtSigningEncodingKey>(signingKey);
            services.AddTransient<INewToken, NewToken>();

            var validationParametersnew = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = Configuration.GetSection("TokenSettings:TokenIssuer").Value,
                ValidateAudience = true,
                ValidAudience = Configuration.GetSection("TokenSettings:TokenAudience").Value,
                ValidateLifetime = true,
                IssuerSigningKey = signingDecodingKey.GetKey(),
                ValidateIssuerSigningKey = true
            };

            services.AddTransient<IJwtValidator, JwtValidator>(service => new JwtValidator(validationParametersnew));

            services.Configure<TokenOptions>(Configuration.GetSection("TokenSettings"));

            services
                .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.RequireHttpsMetadata = true;
                    options.TokenValidationParameters = validationParametersnew;
                });
        }

        private void ConfigureRabbitMq(IServiceCollection services)
        {
            string appSettingSection = "RabbitMQ";
            string serviceName = Configuration.GetSection(appSettingSection)["Username"];
            string servicePassword = Configuration.GetSection(appSettingSection)["Password"];

            var userServiceUrl = $"rabbitmq://localhost/UserService_{serviceName}";
            var messageServiceUrl = $"rabbitmq://localhost/MessageService_{serviceName}"

            services.AddMassTransit(x =>
            {
                x.AddConsumer<UserJwtConsumer>();

                x.UsingRabbitMq((context, cfg) =>
                {
                    cfg.Host("localhost", "/", host =>
                    {
                        host.Username($"{serviceName}_{servicePassword}");
                        host.Password(servicePassword);
                    });

                    cfg.ReceiveEndpoint($"{serviceName}_ValidationJwt", ep =>
                    {
                        ep.PrefetchCount = 16;
                        ep.UseMessageRetry(r => r.Interval(2, 100));

                        ep.ConfigureConsumer<UserJwtConsumer>(context);
                    });
                });

                x.AddRequestClient<IUserCredentialsRequest>(new Uri(userServiceUrl));
                x.AddRequestClient<IUserEmailRequest>(new Uri(userServiceUrl));
                x.AddRequestClient<IUserDescriptionRequest>(new Uri(messageServiceUrl));
            });
        }

        private void ConfigureCommands(IServiceCollection services)
        {
            services.AddTransient<IUserLoginCommand, UserLoginCommand>();
        }

        private void ConfigureValidators(IServiceCollection services)
        {
            services.AddTransient<IValidator<UserLoginInfoRequest>, UserLoginValidator>();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseExceptionHandler(tempApp => tempApp.Run(CustomExceptionHandler.HandleCustomException));

            app.UseHttpsRedirection();

            app.UseRouting();

            string corsUrl = Configuration.GetSection("Settings")["CorsUrl"];

            app.UseCors(builder =>
                builder
                    .WithOrigins(corsUrl)
                    .AllowAnyHeader()
                    .AllowAnyMethod());

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHealthChecks("api/healthcheck");
            });
        }
    }
}