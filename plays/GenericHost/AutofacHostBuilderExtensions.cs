using System;
using Autofac;
using Autofac.Core;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace GenericHost
{
    public static class AutofacHostBuilderExtensions
    {
        public static IHostBuilder AddAutofac(this IHostBuilder builder, Action<ContainerBuilder> configure = null)
        {
            void Configure(ContainerBuilder bld)
            {
                bld.RegisterBuildCallback(cr =>
                {
                    var lifetime =  cr.Resolve<IApplicationLifetime>();
                    var logger = cr.Resolve<ILogger<Container>>();
                    lifetime.ApplicationStopped.Register(() =>
                    {
                        
                        cr.Dispose();
                        logger.LogInformation("Container disposed");
                    });

                });
                configure?.Invoke(bld);
            }
            
            builder.UseServiceProviderFactory(new AutofacServiceProviderFactory());
            builder.ConfigureContainer<ContainerBuilder>(Configure);
            return builder;
        }
    }
    
    
}