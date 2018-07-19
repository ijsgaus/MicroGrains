using System;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Orleans.Hosting;
using HostBuilderContext = Microsoft.Extensions.Hosting.HostBuilderContext;

namespace GenericHost
{
    public static class AutofacSiloExtensions
    {
        public static IHostBuilder AddSilo(this IHostBuilder hostBuilder, Action<IServiceProvider, ISiloHostBuilder> configureSilo)
        {
            hostBuilder.ConfigureServices(sc =>
            {
                sc.AddSingleton<IHostedService>(sp =>
                    new SiloHostService(configureSilo, sp));


            });
            return hostBuilder;
        }

        private class SiloHostService : IHostedService
        {
            private readonly Action<IServiceProvider, ISiloHostBuilder> _configure;
            private readonly IServiceProvider _provider;
            private ILifetimeScope _scope;
            private ISiloHost _siloHost;
            


            public SiloHostService(Action<IServiceProvider, ISiloHostBuilder> configure, IServiceProvider provider)
            {
                _configure = configure;
                _provider = provider;
            }

            public async Task StartAsync(CancellationToken cancellationToken)
            {
                var siloHostBuilder = new SiloHostBuilder();
                _configure(_provider, siloHostBuilder);
                var parentScope = _provider.GetRequiredService<ILifetimeScope>();
                siloHostBuilder.UseServiceProviderFactory(cs =>
                {
                    _scope = parentScope.BeginLifetimeScope(cb => { cb.Populate(cs); });
                    return new AutofacServiceProvider(_scope);
                });
                _siloHost = siloHostBuilder.Build();
                await _siloHost.StartAsync(cancellationToken);
            }

            public async Task StopAsync(CancellationToken cancellationToken)
            {
                if (_siloHost != null)
                {
                    
                    await _siloHost.StopAsync(cancellationToken);
                    await _siloHost.Stopped;
                    _siloHost.Dispose();
                    _scope.Dispose();
                }
            }

        }
    }
}