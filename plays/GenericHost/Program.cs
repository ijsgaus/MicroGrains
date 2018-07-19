using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Autofac.Core.Registration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Orleans.Hosting;

namespace GenericHost
{

    public class AProvider : ILoggerProvider
    {
        private readonly ILoggerFactory _factory;
        private readonly string _ext;

        public AProvider(ILoggerFactory factory, string ext)
        {
            _factory = factory;
            _ext = ext;
        }

        public void Dispose()
        {
            
        }

        public ILogger CreateLogger(string categoryName)
        {
            return _factory.CreateLogger(_ext + "::" + (categoryName ?? ""));
        }
    }
    
    class Program
    {
        static async Task Main(string[] args)
        {
            using (var host = new HostBuilder()
                .ConfigureLogging(cfg => cfg.AddConsole(opt => opt.IncludeScopes = true))
                .AddAutofac()
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