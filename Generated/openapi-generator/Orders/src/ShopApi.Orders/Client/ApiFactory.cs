using System;
using Microsoft.Extensions.DependencyInjection;
using ShopApi.Orders.Api;

namespace ShopApi.Orders.Client
{
    public interface IApiFactory
    {
        IResult Create<IResult>() where IResult : IApi;
    }

    public class ApiFactory : IApiFactory
    {
        public IServiceProvider Services { get; }

        public ApiFactory(IServiceProvider services)
        {
            Services = services;
        }

        public IResult Create<IResult>() where IResult : IApi
        {
            return Services.GetRequiredService<IResult>();
        }
    }
}