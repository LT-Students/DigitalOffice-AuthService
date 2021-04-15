using HealthChecks.UI.Client;
using LT.DigitalOffice.AuthService.Broker.Consumers;
using LT.DigitalOffice.AuthService.Models.Dto.Configurations;
using LT.DigitalOffice.AuthService.Token;
using LT.DigitalOffice.AuthService.Token.Interfaces;
using LT.DigitalOffice.Kernel.Configurations;
using LT.DigitalOffice.Kernel.Extensions;
using LT.DigitalOffice.Kernel.Middlewares.ApiInformation;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;

namespace LT.DigitalOffice.AuthService
{
    public class Startup : BaseApiInfo
    {
        public const string CorsPolicyName = "LtDoCorsPolicy";

        private readonly BaseServiceInfoConfig _serviceInfoConfig;
        private readonly RabbitMqConfig _rabbitMqConfig;

        public IConfiguration Configuration { get; }

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
            services.AddMassTransit(x =>
            {
                x.UsingRabbitMq((context, cfg) =>
                {
                    cfg.Host(_rabbitMqConfig.Host, "/", host =>
                    {
                        host.Username($"{_serviceInfoConfig.Name}_{_serviceInfoConfig.Id}");
                        host.Password(_serviceInfoConfig.Id);
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

                x.AddRequestClients(_rabbitMqConfig);
            });
        }

        #endregion

        #region public methods

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;

            _serviceInfoConfig = Configuration
                .GetSection(BaseServiceInfoConfig.SectionName)
                .Get<BaseServiceInfoConfig>();

            _rabbitMqConfig = Configuration
                .GetSection(BaseRabbitMqConfig.SectionName)
                .Get<RabbitMqConfig>();

            Version = "1.2.5";
            Description = "AuthService is an API intended to work with user authentication, create token for user.";
            StartTime = DateTime.UtcNow;
            ApiName = $"LT Digital Office - {_serviceInfoConfig.Name}";
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddPolicy(
                    CorsPolicyName,
                    builder =>
                    {
                        builder
                            .WithOrigins(
                                "https://*.ltdo.xyz",
                                "http://*.ltdo.xyz",
                                "http://ltdo.xyz",
                                "http://ltdo.xyz:9818",
                                "http://localhost:4200",
                                "http://localhost:4500")
                            .AllowAnyHeader()
                            .WithMethods("POST");
                    });
            });

            services.AddHttpContextAccessor();

            services.Configure<BaseRabbitMqConfig>(Configuration.GetSection(BaseRabbitMqConfig.SectionName));
            services.Configure<BaseServiceInfoConfig>(Configuration.GetSection(BaseServiceInfoConfig.SectionName));
            services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders =
                    ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
            });

            services.AddBusinessObjects();

            services
                .AddHealthChecks()
                .AddRabbitMqCheck();

            services.AddControllers();
            services.AddMassTransitHostedService();

            ConfigureRabbitMq(services);
            ConfigureJwt(services);
        }

        public void Configure(IApplicationBuilder app, ILoggerFactory loggerFactory)
        {
            app.UseForwardedHeaders();

            app.UseExceptionsHandler(loggerFactory);

            app.UseApiInformation();

            app.UseRouting();

            app.UseCors(CorsPolicyName);

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers().RequireCors(CorsPolicyName);

                endpoints.MapHealthChecks($"/{_serviceInfoConfig.Id}/hc", new HealthCheckOptions
                {
                    ResultStatusCodes = new Dictionary<HealthStatus, int>
                    {
                        { HealthStatus.Unhealthy, 200 },
                        { HealthStatus.Healthy, 200 },
                        { HealthStatus.Degraded, 200 },
                    },
                    Predicate = check => check.Name != "masstransit-bus",
                    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
                });
            });
        }

        #endregion
    }
}