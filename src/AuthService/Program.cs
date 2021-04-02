using LT.DigitalOffice.AuthService.Configuration;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using System.IO;

namespace LT.DigitalOffice.AuthService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            ConfigureLogger();

            CreateHostBuilder(args).Build().Run();
        }

        private static void ConfigureLogger()
        {
            var productionConfigurations = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.Production.json", optional: false)
                .Build();

            var logstashConfig = productionConfigurations
                .GetSection(LogstashConfig.LogstashSectionName)
                .Get<LogstashConfig>();

            Log.Logger = new LoggerConfiguration()
                .Enrich.WithProperty(
                    nameof(logstashConfig.KeyProperty),
                    logstashConfig.KeyProperty)
                .WriteTo.Http(logstashConfig.Url)
                .CreateLogger();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
#if RELEASE
                .UseSerilog()
#endif
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}