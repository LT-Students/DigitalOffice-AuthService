using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using System;

namespace LT.DigitalOffice.AuthService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .Build();

            string seqServerUrl = Environment.GetEnvironmentVariable("SeqServerUrl");
            if (string.IsNullOrEmpty(seqServerUrl))
            {
                seqServerUrl = configuration.GetSection("Serilog:WriteTo:Seq")["Args:serverUrl"];
            }

            string seqApiKey = Environment.GetEnvironmentVariable("seqApiKey");
            if (string.IsNullOrEmpty(seqApiKey))
            {
                seqApiKey = configuration.GetSection("Serilog:WriteTo:Seq")["Args:apiKey"];
            }

            Log.Logger = new LoggerConfiguration().ReadFrom
                .Configuration(configuration)
                .Enrich.WithProperty("Service", "AuthService")
                .WriteTo.Seq(
                    serverUrl: seqServerUrl,
                    apiKey: seqApiKey)
                .CreateLogger();

            try
            {
                CreateHostBuilder(args).Build().Run();
            }
            catch (Exception exc)
            {
                Log.Fatal(exc, "Can not properly start AuthService.");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseSerilog()
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}