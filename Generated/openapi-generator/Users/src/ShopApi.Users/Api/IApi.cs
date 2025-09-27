using System.Net.Http;

namespace ShopApi.Users.Api
{
    public interface IApi
    {
        HttpClient HttpClient { get; }
    }
}