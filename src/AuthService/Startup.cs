using LT.DigitalOffice.AuthService.Broker.Consumers;
using LT.DigitalOffice.AuthService.Business.Commands;
using LT.DigitalOffice.AuthService.Business.Commands.Interfaces;
using LT.DigitalOffice.AuthService.Configuration;
using LT.DigitalOffice.AuthService.Token;
using LT.DigitalOffice.AuthService.Token.Interfaces;
using LT.DigitalOffice.AuthService.Validation;
using LT.DigitalOffice.AuthService.Validation.Interfaces;
using LT.DigitalOffice.Broker.Requests;
using LT.DigitalOffice.Kernel.Broker;
using LT.DigitalOffice.Kernel.Extensions;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;

namespace LT.DigitalOffice.AuthService
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        private RabbitMqConfig _rabbitMqConfig;
        
        #region private methods

        private void ConfigureJwt(IServiceCollection services)
        {
            var signingKey = new SigningSymmetricKey();
            var signingDecodingKey = (IJwtSigningDecodingKey)signingKey;

            services.AddSingleton<IJwtSigningEncodingKey>(signingKey);
            services.AddSingleton<IJwtSigningDecodingKey>(signingKey);

            services.AddTransient<ITokenEngine, TokenEngine>();

            services.AddTransient<ITokenValidator, TokenValidator>();

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
            _rabbitMqConfig = Configuration
                .GetSection(BaseRabbitMqOptions.RabbitMqSectionName)
                .Get<RabbitMqConfig>();

            services.AddMassTransit(x =>
            {
                x.UsingRabbitMq((context, cfg) =>
                {
                    cfg.Host(_rabbitMqConfig.Host, "/", host =>
                    {
                        host.Username($"{_rabbitMqConfig.Username}_{_rabbitMqConfig.Password}");
                        host.Password(_rabbitMqConfig.Password);
                    });

                    cfg.ReceiveEndpoint(_rabbitMqConfig.ValidateTokenEndpoint, ep =>
                    {
                        ep.ConfigureConsumer<CheckTokenConsumer>(context);
                    });

                    cfg.ReceiveEndpoint(_rabbitMqConfig.GetTokenEndpoint, ep =>
                    {
                        ep.ConfigureConsumer<GetTokenConsumer>(context);
                    });
                });

                x.AddConsumer<CheckTokenConsumer>();
                x.AddConsumer<GetTokenConsumer>();

                x.AddRequestClient<IUserCredentialsRequest>(
                  new Uri($"{_rabbitMqConfig.BaseUrl}/{_rabbitMqConfig.GetUserCredentialsEndpoint}"));
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

        #endregion

        #region public methods

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddHealthChecks();
            services.AddControllers();
            services.AddMassTransitHostedService();

            ConfigureRabbitMq(services);
            ConfigureJwt(services);
            ConfigureCommands(services);
            ConfigureValidators(services);
        }

        public void Configure(IApplicationBuilder app, ILoggerFactory loggerFactory)
        {
            app.AddExceptionsHandler(loggerFactory);

#if RELEASE
            app.UseHttpsRedirection();
#endif

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
                
                endpoints.MapHealthChecks($"/{_rabbitMqConfig.Password}/hc", new HealthCheckOptions
                {
                    Predicate = _ => true,
                    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
                });
            });
        }

        #endregion
    }
}