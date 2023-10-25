using RwaWebModels.ConcertContext;
using RwaWebModels.Interfaces;
using System.Net.Http.Json;
using System.Net;
using System.Text.Json;
using Microsoft.Extensions.Options;
using System.Web;

namespace RwaWeb.Data
{
    public class ConcertApiAdapter : IConcertContextService
    {
        private readonly HttpClient _httpClient;
        private readonly IOptions<RwaApiOptions> _options;

        public ConcertApiAdapter(HttpClient httpClient, IOptions<RwaApiOptions> options)
        {
            _httpClient = httpClient;
            _options = options;
        }

        public async Task<CreateResult> CreateConcertAsync(Concert newConcert)
        {
            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, "api/Concert");
            httpRequestMessage.Content = JsonContent.Create(newConcert);

            var httpResponseMessage = await _httpClient.SendAsync(httpRequestMessage);
            var responseMessage = await httpResponseMessage.Content.ReadAsStringAsync();

            if (httpResponseMessage.StatusCode == HttpStatusCode.BadRequest)
            {
                var failedCreateOperation = JsonSerializer.Deserialize<CreateResult>(responseMessage, RwaApiConfiguration.GetSerializerOptions());
                return failedCreateOperation ?? throw new InvalidOperationException("Failed to parse response");
            }
            else if (httpResponseMessage.StatusCode != HttpStatusCode.Created)
            {
                throw new InvalidOperationException(nameof(CreateConcertAsync), new Exception(responseMessage));
            }

            var returnedConcert = JsonSerializer.Deserialize<Concert>(responseMessage, RwaApiConfiguration.GetSerializerOptions());

            if (returnedConcert == null)
            {
                throw new InvalidOperationException("Concert was not created successfully");
            }

            return new CreateResult
            {
                Success = true,
                NewId = returnedConcert.Id
            };
        }

        public async Task<DeleteResult> DeleteConcertAsync(int id)
        {
            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Delete, $"api/Concert/{id}");
            var httpResponseMessage = await _httpClient.SendAsync(httpRequestMessage);

            if (httpResponseMessage.StatusCode == HttpStatusCode.BadRequest)
            {
                var responseMessage = await httpResponseMessage.Content.ReadAsStringAsync();
                var failedCreateOperation = JsonSerializer.Deserialize<DeleteResult>(responseMessage, RwaApiConfiguration.GetSerializerOptions());
                return failedCreateOperation ?? throw new InvalidOperationException("Failed to parse response");
            }
            else if (httpResponseMessage.StatusCode != HttpStatusCode.OK)
            {
                throw new InvalidOperationException(nameof(DeleteConcertAsync), new Exception(await httpResponseMessage.Content.ReadAsStringAsync()));
            }

            return new DeleteResult
            {
                Success = true
            };
        }

        public async Task<Concert?> GetConcertByIdAsync(int id)
        {
            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, $"api/Concert/{id}");
            var httpResponseMessage = await _httpClient.SendAsync(httpRequestMessage);
            var responseMessage = await httpResponseMessage.Content.ReadAsStringAsync();

            if (httpResponseMessage.StatusCode != HttpStatusCode.OK)
            {
                throw new InvalidOperationException(nameof(GetConcertByIdAsync), new Exception(responseMessage));
            }

            return JsonSerializer.Deserialize<Concert>(responseMessage, RwaApiConfiguration.GetSerializerOptions());
        }

        public async Task<ICollection<Concert>> GetConcertsByIdAsync(ICollection<int> ids)
        {
            var listOfIds = JsonSerializer.Serialize(ids);
            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, $"api/Concert/GetConcertsByIds?listOfIds={listOfIds}");
            var httpResponseMessage = await _httpClient.SendAsync(httpRequestMessage);
            var responseMessage = await httpResponseMessage.Content.ReadAsStringAsync();

            if (httpResponseMessage.StatusCode != HttpStatusCode.OK)
            {
                throw new InvalidOperationException(nameof(GetConcertsByIdAsync), new Exception(responseMessage));
            }

            return JsonSerializer.Deserialize<List<Concert>>(responseMessage, RwaApiConfiguration.GetSerializerOptions()) ?? new List<Concert>();
        }

        public async Task<ICollection<Concert>> GetUpcomingConcertsAsync(int count)
        {
            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, "api/Concert/GetUpcomingConcerts");
            var httpResponseMessage = await _httpClient.SendAsync(httpRequestMessage);
            var responseMessage = await httpResponseMessage.Content.ReadAsStringAsync();

            if (httpResponseMessage.StatusCode != HttpStatusCode.OK)
            {
                throw new InvalidOperationException(nameof(GetUpcomingConcertsAsync), new Exception(responseMessage));
            }

            return JsonSerializer.Deserialize<List<Concert>>(responseMessage, RwaApiConfiguration.GetSerializerOptions()) ?? new List<Concert>();
        }

        public async Task<UpdateResult> UpdateConcertAsync(Concert model)
        {
            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Put, "api/Concert");
            httpRequestMessage.Content = JsonContent.Create(model);
            var httpResponseMessage = await _httpClient.SendAsync(httpRequestMessage);

            if (httpResponseMessage.StatusCode == HttpStatusCode.BadRequest)
            {
                var responseMessage = await httpResponseMessage.Content.ReadAsStringAsync();
                var failedCreateOperation = JsonSerializer.Deserialize<CreateResult>(responseMessage, RwaApiConfiguration.GetSerializerOptions());
                return failedCreateOperation ?? throw new InvalidOperationException("Failed to parse response");
            }
            else if (httpResponseMessage.StatusCode != HttpStatusCode.Accepted)
            {
                throw new InvalidOperationException(nameof(UpdateConcertAsync), new Exception(await httpResponseMessage.Content.ReadAsStringAsync()));
            }

            return new UpdateResult
            {
                Success = true,
            };
        }
    }
}
