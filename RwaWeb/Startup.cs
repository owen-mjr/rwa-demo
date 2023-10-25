using RwaWeb.Data;
using RwaWebModels.Interfaces;

using Polly;
using Polly.Contrib.WaitAndRetry;
using Polly.Extensions.Http;

using Microsoft.Net.Http.Headers;


namespace RwaWeb
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<RwaApiOptions>(Configuration.GetSection("App:RwaApi"));
            services.AddOptions();
            AddConcertContextService(services);
        }

        private void AddConcertContextService(IServiceCollection services)
        {
            var baseUri = Configuration.GetValue<string>("App:RwaApi:BaseUri");
            services.AddHttpClient<IConcertContextService, ConcertApiAdapter>(httpClient =>
            {
                httpClient.BaseAddress = new Uri(baseUri);
                httpClient.DefaultRequestHeaders.Add(HeaderNames.Accept, "application/json");
            })
            .AddPolicyHandler(GetRetryPolicy())
            .AddPolicyHandler(GetCircuitBreakerPolicy());
        }

        private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
        {
            var delay = Backoff.DecorrelatedJitterBackoffV2(TimeSpan.FromMilliseconds(500), retryCount: 3);

            return HttpPolicyExtensions
              .HandleTransientHttpError()
              .WaitAndRetryAsync(delay);
        }

        private static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy()
        {
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.NotFound)
                .CircuitBreakerAsync(5, TimeSpan.FromSeconds(30));
        }
    }
}
