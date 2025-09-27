using System.Net.Http;

namespace ShopApi.Inventory.Api
{
    public interface IApi
    {
        HttpClient HttpClient { get; }
    }
}