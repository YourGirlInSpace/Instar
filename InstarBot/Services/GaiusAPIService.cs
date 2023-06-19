using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using PaxAndromeda.Instar.Gaius;

namespace PaxAndromeda.Instar.Services;

public class GaiusAPIService : IGaiusAPIService
{
    private const string BaseURL = "https://api.gaiusbot.me";
    
    private readonly HttpClient _client;
    private readonly string _apiKey;
    public GaiusAPIService(IConfiguration config)
    {
        _apiKey = config.GetValue<string>("GaiusAPIKey")!;
        _client = new HttpClient();
    }

    public async Task<IEnumerable<Warning>?> GetWarnings(Snowflake userId)
    {
        var result = await Get($"{BaseURL}/warnings/{userId.ID}");
        return JsonConvert.DeserializeObject<Warning[]>(result);
    }

    public async Task<IEnumerable<Caselog>?> GetCaselogs(Snowflake userId)
    {
        var result = await Get($"{BaseURL}/caselogs/{userId.ID}");
        
        // Remove the "totalCases" portion if it exists
        if (result.Contains("totalCases", StringComparison.OrdinalIgnoreCase))
            result = result[..(result.LastIndexOf("\"totalCases\"", StringComparison.OrdinalIgnoreCase)-1)] + "}";

        return result.Length <= 2 
            ? Array.Empty<Caselog>() 
            : JsonConvert.DeserializeObject<Dictionary<int, Caselog>>(result)?.Values.ToArray();
    }

    private async Task<string> Get(string url)
    {
        var hrm = CreateRequest(url);
        var response = await _client.SendAsync(hrm);
        
        return await response.Content.ReadAsStringAsync();
    }

    private HttpRequestMessage CreateRequest(string url)
    {
        var hrm = new HttpRequestMessage();
        hrm.RequestUri = new Uri(url);
        hrm.Headers.Add("Accept", "application/json");
        hrm.Headers.Add("api-key", _apiKey);

        return hrm;
    }

    public void Dispose()
    {
        _client.Dispose();
    }
}