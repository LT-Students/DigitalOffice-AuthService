using GreenPipes;
using LT.DigitalOffice.AuthenticationService.Broker.Consumers;
using LT.DigitalOffice.AuthenticationService.Business;
using LT.DigitalOffice.AuthenticationService.Business.Interfaces;
using LT.DigitalOffice.AuthenticationService.Token;
using LT.DigitalOffice.AuthenticationService.Token.Interfaces;
using LT.DigitalOffice.AuthenticationService.Validation;
using LT.DigitalOffice.AuthenticationService.Validation.Interfaces;
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
            services.AddSingleton<IJwtSigningDecodingKey>(signingKey);

            services.AddTransient<ITokenEngine, TokenEngine>();

            services.AddTransient<IJwtValidator, JwtValidator>();

            services.Configure<TokenSettings>(Configuration.GetSection("TokenSettings"));

            services
                .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.RequireHttpsMetadata = true;
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidIssuer = Configuration.GetSection("TokenSettings:TokenIssuer").Value,
                        ValidateAudience = true,
                        ValidAudience = Configuration.GetSection("TokenSettings:TokenAudience").Value,
                        ValidateLifetime = true,
                        IssuerSigningKey = signingDecodingKey.GetKey(),
                        ValidateIssuerSigningKey = true
                    };
                });
        }

        private void ConfigureRabbitMq(IServiceCollection services)
        {
            string appSettingSection = "RabbitMQ";
            string serviceName = Configuration.GetSection(appSettingSection)["Username"];
            string servicePassword = Configuration.GetSection(appSettingSection)["Password"];

            var uri = $"rabbitmq://localhost/UserService_{serviceName}";

            services.AddMassTransit(x =>
            {
                x.AddConsumer<JwtConsumer>();

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

                        ep.ConfigureConsumer<JwtConsumer>(context);
                    });
                });

                x.AddRequestClient<IUserCredentialsRequest>(new Uri(uri));
            });
        }

        private void ConfigureCommands(IServiceCollection services)
        {
            services.AddTransient<ILoginCommand, LoginCommand>();
        }

        private void ConfigureValidators(IServiceCollection services)
        {
            services.AddTransient<ILoginValidator, LoginValidator>();
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseHealthChecks("/api/healthcheck");

            app.UseExceptionHandler(tempApp => tempApp.Run(CustomExceptionHandler.HandleCustomException));

            app.UseHttpsRedirection();

            app.UseRouting();

            string corsUrl = Configuration.GetSection("Settings")["CorsUrl"];

            app.UseCors(
                builder =>
                    builder
                        .WithOrigins(corsUrl)
                        .AllowAnyHeader()
                        .AllowAnyMethod());

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}