
#nullable enable

using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ShopApi.Users.Client;

namespace ShopApi.Users.Extensions
{
    public static class IHostBuilderExtensions
    {
        public static IHostBuilder ConfigureApi(this IHostBuilder builder)
        {
            builder.ConfigureServices((context, services) => 
            {
                HostConfiguration config = new HostConfiguration(services);

                IServiceCollectionExtensions.AddApi(services, config);
            });

            return builder;
        }

        public static IHostBuilder ConfigureApi(this IHostBuilder builder, Action<HostBuilderContext, IServiceCollection, HostConfiguration> options)
        {
            builder.ConfigureServices((context, services) => 
            {
                HostConfiguration config = new HostConfiguration(services);

                options(context, services, config);

                IServiceCollectionExtensions.AddApi(services, config);
            });

            return builder;
        }
    }
}