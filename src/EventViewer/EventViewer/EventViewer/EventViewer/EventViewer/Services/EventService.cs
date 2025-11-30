using EventViewer.ExternalContracts;
using Microsoft.Extensions.Configuration;

namespace EventViewer.Services;

public interface IEventService
{
    Task<List<DeserializedEvent>> GetEventsAsync(string aggregateId = "");
}

public class EventService : IEventService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    public EventService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
    }

    public async Task<List<DeserializedEvent>> GetEventsAsync(string aggregateId = "")
    {
        try
        {
            var url = _configuration["ExternalApi:EventsUrl"];
            if (!string.IsNullOrEmpty(aggregateId))
            {
                url = $"{url}{System.Net.WebUtility.UrlEncode(aggregateId)}";
            }
            if (string.IsNullOrEmpty(url))
            {
                return new List<DeserializedEvent>();
            }

            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var events = System.Text.Json.JsonSerializer.Deserialize<List<DeserializedEvent>>(content,
                new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return events ?? new List<DeserializedEvent>();
        }
        catch (Exception ex)
        {
            // Log the error if needed
            return new List<DeserializedEvent>();
        }
    }
}
