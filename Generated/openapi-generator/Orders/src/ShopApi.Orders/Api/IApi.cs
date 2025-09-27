using System.Net.Http;

namespace ShopApi.Orders.Api
{
    public interface IApi
    {
        HttpClient HttpClient { get; }
    }
}