
#nullable enable

using System;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using Polly.Timeout;
using Polly.Extensions.Http;
using Polly;

namespace ShopApi.Users.Extensions
{
    public static class IHttpClientBuilderExtensions
    {
        public static IHttpClientBuilder AddRetryPolicy(this IHttpClientBuilder client, int retries)
        {
            client.AddPolicyHandler(RetryPolicy(retries));

            return client;
        }

        public static IHttpClientBuilder AddTimeoutPolicy(this IHttpClientBuilder client, TimeSpan timeout)
        {
            client.AddPolicyHandler(TimeoutPolicy(timeout));

            return client;
        }

        public static IHttpClientBuilder AddCircuitBreakerPolicy(this IHttpClientBuilder client, int handledEventsAllowedBeforeBreaking, TimeSpan durationOfBreak)
        {
            client.AddTransientHttpErrorPolicy(builder => CircuitBreakerPolicy(builder, handledEventsAllowedBeforeBreaking, durationOfBreak));

            return client;
        }

        private static Polly.Retry.AsyncRetryPolicy<HttpResponseMessage> RetryPolicy(int retries)
            => HttpPolicyExtensions
                .HandleTransientHttpError()
                .Or<TimeoutRejectedException>()
                .RetryAsync(retries);

        private static AsyncTimeoutPolicy<HttpResponseMessage> TimeoutPolicy(TimeSpan timeout)
            => Policy.TimeoutAsync<HttpResponseMessage>(timeout);

        private static Polly.CircuitBreaker.AsyncCircuitBreakerPolicy<HttpResponseMessage> CircuitBreakerPolicy(
            PolicyBuilder<HttpResponseMessage> builder, int handledEventsAllowedBeforeBreaking, TimeSpan durationOfBreak)
                => builder.CircuitBreakerAsync(handledEventsAllowedBeforeBreaking, durationOfBreak);
    }
}