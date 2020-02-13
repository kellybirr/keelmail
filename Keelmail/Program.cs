using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Keelmail
{
    public sealed class Program
    {
        public static DateTime StartedUtc { get; private set; }

        private static IHost WebHost;
        private static IConfiguration Configuration;

        public static void Main(string[] args)
        {
            StartedUtc = DateTime.UtcNow;

            WebHost = Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    // set up config like ASP.Net core would.
                    string envName = hostingContext.HostingEnvironment.EnvironmentName;
                    config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                          .AddJsonFile($"appsettings.{envName}.json", optional: true, reloadOnChange: true)
                          .AddConfigMapJsonFiles()
                          .AddEnvironmentVariables()
                          .AddCommandLine(args);

                    Configuration = config.Build();
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                }).Build();

            WebHost.Run();
        }
    }
}
