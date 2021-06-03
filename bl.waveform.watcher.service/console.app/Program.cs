using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using System.Threading.Tasks;

namespace console.app
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var services = ConfigurationServices();

            var servicesProvider = services.BuildServiceProvider();

            await servicesProvider.GetService<App>().RunAsync(args);
        }

        public static IServiceCollection ConfigurationServices() {
            IServiceCollection services = new ServiceCollection();
            var serviceProvider = services.BuildServiceProvider();
            var config = LoadConfiguration();
            services.AddSingleton(config);
            services.AddSingleton<App>();
            services.AddSingleton<DatConvertor>();
            services.AddSingleton<MpegConvertor>();
            return services;
        }

        public static IConfiguration LoadConfiguration() {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Path.GetDirectoryName(typeof(Program).Assembly.Location))
                .AddJsonFile("appsettings.json",true,true)
                .AddEnvironmentVariables();
            return builder.Build();
        }
    }
}
