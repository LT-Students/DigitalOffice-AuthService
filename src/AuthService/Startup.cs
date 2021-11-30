using System;
using System.Collections.Generic;
using HealthChecks.UI.Client;
using LT.DigitalOffice.AuthService.Broker.Consumers;
using LT.DigitalOffice.AuthService.Models.Dto.Configurations;
using LT.DigitalOffice.AuthService.Token;
using LT.DigitalOffice.AuthService.Token.Interfaces;
using LT.DigitalOffice.Kernel.BrokerSupport.Configurations;
using LT.DigitalOffice.Kernel.BrokerSupport.Extensions;
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
using Serilog;

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

    private (string username, string password) GetRabbitMqCredentials()
    {
      static string GetString(string envVar, string formAppsettings, string generated, string fieldName)
      {
        string str = Environment.GetEnvironmentVariable(envVar);
        if (string.IsNullOrEmpty(str))
        {
          str = formAppsettings ?? generated;

          Log.Information(
            formAppsettings == null
              ? $"Default RabbitMq {fieldName} was used."
              : $"RabbitMq {fieldName} from appsetings.json was used.");
        }
        else
        {
          Log.Information($"RabbitMq {fieldName} from environment was used.");
        }

        return str;
      }

      return (GetString("RabbitMqUsername", _rabbitMqConfig.Username, $"{_serviceInfoConfig.Name}_{_serviceInfoConfig.Id}", "Username"),
        GetString("RabbitMqPassword", _rabbitMqConfig.Password, _serviceInfoConfig.Id, "Password"));
    }

    private void ConfigureRabbitMq(IServiceCollection services)
    {
      (string username, string password) = GetRabbitMqCredentials();

      services.AddMassTransit(x =>
      {
        x.UsingRabbitMq((context, cfg) =>
          {
            cfg.Host(_rabbitMqConfig.Host, "/", host =>
            {
              host.Username(username);
              host.Password(password);
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

      Version = "1.3.1";
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
              .AllowAnyOrigin()
              //.WithOrigins(
              //    "https://*.ltdo.xyz",
              //    "http://*.ltdo.xyz",
              //    "http://ltdo.xyz",
              //    "http://ltdo.xyz:9818",
              //    "http://localhost:4200",
              //    "http://localhost:4500")
              .AllowAnyHeader()
              .AllowAnyMethod();
            //.WithMethods("POST");
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
