using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Autofac.Core;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Autofac.Extensions.Hosting
{
    public static class AutofacHostBuilderExtensions
    {
        public static IHostBuilder UseAutofac(this IHostBuilder builder, Action<ContainerBuilder> configure = null)
        {
            builder.UseServiceProviderFactory(new AutofacServiceProviderFactory(configure));
            builder.ConfigureContainer<ContainerBuilder>((_, cb) => cb.RegisterBuildCallback(cr =>
            {
                var lifetime =  cr.Resolve<IApplicationLifetime>();
                var logger = cr.Resolve<ILogger<Container>>();
                lifetime.ApplicationStopped.Register(() =>
                {
                        
                    cr.Dispose();
                    logger.LogTrace("Autofac container disposed");
                });

            }));
            return builder;
        }

        public static IHostBuilder ConfigureAutofac(this IHostBuilder builder,
            Action<HostBuilderContext, ContainerBuilder> configure)
            =>  builder.ConfigureContainer(configure);
        
        public static IHostBuilder ConfigureAutofac(this IHostBuilder builder,
            Action<ContainerBuilder> configure)
            =>  builder.ConfigureAutofac((_, cb) => configure(cb));

        public static IHostBuilder AddAutofacModule<T>(this IHostBuilder builder) where T : IModule
            => builder.ConfigureAutofac(
                
                (ctx, cb) =>
                {
                    var constructors = typeof(T).GetConstructors();
                    var knownTypes = new Dictionary<Type, object>
                    {
                        [typeof(IConfiguration)] = ctx.Configuration, 
                        [typeof(IHostingEnvironment)] = ctx.HostingEnvironment, 
                        [typeof(HostBuilderContext)] = ctx
                    };
                    var cnt = -1;
                    ConstructorInfo constructor = null;
                    ParameterInfo[] constrParams = null;
                    foreach (var item in constructors)
                    {
                        var parameters = item.GetParameters();
                        var known = parameters.All(info => knownTypes.ContainsKey(info.ParameterType));
                        if(!known) continue;
                        if (parameters.Length <= cnt) continue;

                        cnt = parameters.Length;
                        constructor = item;
                        constrParams = parameters;
                    }
                    if(constructor == null)
                        throw new DependencyResolutionException(
                            $"Cannot find compatible constructor for module {typeof(T)}, can have parametrized constructor with {nameof(IConfiguration)}, {nameof(IHostingEnvironment)} or/and {nameof(HostBuilderContext)} parameters");

                    var args = constrParams.Select(p => knownTypes[p.ParameterType]).ToArray();

                    var module = (IModule) constructor.Invoke(args);
                    cb.RegisterModule(module);
                });

        public static IHostBuilder AddAutofacModule(this IHostBuilder builder, IModule module)
            => builder.ConfigureAutofac(cb => cb.RegisterModule(module));
        
        public static IHostBuilder AddAutofacModule(this IHostBuilder builder, Func<HostBuilderContext, IModule> factory)
            => builder.ConfigureAutofac((ctx, cb) => cb.RegisterModule(factory(ctx)));

    }
    
    
}