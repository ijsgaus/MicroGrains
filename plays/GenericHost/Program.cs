using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Autofac.Core.Registration;
using Autofac.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Orleans.Hosting;

namespace GenericHost
{

    
    
    class Program
    {
        static async Task Main(string[] args)
        {
            using (var host = new HostBuilder()
                .ConfigureLogging(cfg => cfg.AddConsole(opt => opt.IncludeScopes = true))
                .UseAutofac()
                .AddSilo((sp, hb) =>
                {
                    hb //.ConfigureLogging(cfg => cfg.AddProvider(new AProvider(sp.GetRequiredService<ILoggerFactory>(), "SILO")))
                        .UseLocalhostClustering();
                })
                .UseConsoleLifetime()
                .Build())
            {
                await host.StartAsync();
                await host.WaitForShutdownAsync();
                await host.StopAsync();
            }
        }
    }
}