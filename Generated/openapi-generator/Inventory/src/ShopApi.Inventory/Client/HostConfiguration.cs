
#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using ShopApi.Inventory.Api;
using ShopApi.Inventory.Model;

namespace ShopApi.Inventory.Client
{
    public class HostConfiguration
    {
        private readonly IServiceCollection _services;
        private readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions();

        internal bool HttpClientsAdded { get; private set; }

        public HostConfiguration(IServiceCollection services)
        {
            _services = services;
            _jsonOptions.Converters.Add(new JsonStringEnumConverter());
            _jsonOptions.Converters.Add(new DateTimeJsonConverter());
            _jsonOptions.Converters.Add(new DateTimeNullableJsonConverter());
            _jsonOptions.Converters.Add(new DateOnlyJsonConverter());
            _jsonOptions.Converters.Add(new DateOnlyNullableJsonConverter());
            _jsonOptions.Converters.Add(new ItemJsonConverter());
            _jsonOptions.Converters.Add(new OrderJsonConverter());
            _jsonOptions.Converters.Add(new OrderCreateJsonConverter());
            _jsonOptions.Converters.Add(new StockUpdateJsonConverter());
            _jsonOptions.Converters.Add(new UserJsonConverter());
            _jsonOptions.Converters.Add(new UserCreateJsonConverter());
            JsonSerializerOptionsProvider jsonSerializerOptionsProvider = new(_jsonOptions);
            _services.AddSingleton(jsonSerializerOptionsProvider);
            _services.AddSingleton<IApiFactory, ApiFactory>();
            _services.AddSingleton<InventoryApiEvents>();
            _services.AddSingleton<OrdersApiEvents>();
            _services.AddSingleton<UsersApiEvents>();
        }

        public HostConfiguration AddApiHttpClients
        (
            Action<HttpClient>? client = null, Action<IHttpClientBuilder>? builder = null)
        {
            if (client == null)
                client = c => c.BaseAddress = new Uri(ClientUtils.BASE_ADDRESS);

            List<IHttpClientBuilder> builders = new List<IHttpClientBuilder>();

            builders.Add(_services.AddHttpClient<IInventoryApi, InventoryApi>(client));
            builders.Add(_services.AddHttpClient<IOrdersApi, OrdersApi>(client));
            builders.Add(_services.AddHttpClient<IUsersApi, UsersApi>(client));
            
            if (builder != null)
                foreach (IHttpClientBuilder instance in builders)
                    builder(instance);

            HttpClientsAdded = true;

            return this;
        }

        public HostConfiguration ConfigureJsonOptions(Action<JsonSerializerOptions> options)
        {
            options(_jsonOptions);

            return this;
        }

        public HostConfiguration AddTokens<TTokenBase>(TTokenBase token) where TTokenBase : TokenBase
        {
            return AddTokens(new TTokenBase[]{ token });
        }

        public HostConfiguration AddTokens<TTokenBase>(IEnumerable<TTokenBase> tokens) where TTokenBase : TokenBase
        {
            TokenContainer<TTokenBase> container = new TokenContainer<TTokenBase>(tokens);
            _services.AddSingleton(services => container);

            return this;
        }

        public HostConfiguration UseProvider<TTokenProvider, TTokenBase>() 
            where TTokenProvider : TokenProvider<TTokenBase>
            where TTokenBase : TokenBase
        {
            _services.AddSingleton<TTokenProvider>();
            _services.AddSingleton<TokenProvider<TTokenBase>>(services => services.GetRequiredService<TTokenProvider>());

            return this;
        }
    }
}