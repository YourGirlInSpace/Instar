using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using PaxAndromeda.Instar.Gaius;

namespace PaxAndromeda.Instar.Services;

public sealed class GaiusAPIService : IGaiusAPIService
{
    private const string BaseURL = "https://api.gaiusbot.me";
    private const string WarningsBaseURL = BaseURL + "/warnings";
    private const string CaselogsBaseURL = BaseURL + "/caselogs";
    
    private readonly HttpClient _client;
    private readonly string _apiKey;
    public GaiusAPIService(IConfiguration config)
    {
        _apiKey = config.GetValue<string>("GaiusAPIKey")!;
        _client = new HttpClient();

#if !DEBUG
        VerifyKey();
#endif
    }
    
#if !DEBUG
    private void VerifyKey()
    {
        var targetGuild = _config.GetValue<ulong>("TargetGuild");
        var keyData = Encoding.UTF8.GetString(Convert.FromBase64String(_apiKey));
        if (!ulong.TryParse(keyData[..keyData.IndexOf(':')], out var keyGuild))
            throw new ConfigurationException("Gaius API key is not in the correct format.");
        if (keyGuild != targetGuild)
            throw new ConfigurationException("Configured Gaius API key is not for this guild.")
            {
                Data =
                {
                    { "TargetGuild", targetGuild },
                    { "KeyGuild", keyGuild }
                }
            };
    }
#endif
    
    public async Task<IEnumerable<Warning>> GetAllWarnings()
    {
        var response = await Get($"{WarningsBaseURL}/all");

        var result = JsonConvert.DeserializeObject<Warning[]>(response);
        return result ?? Array.Empty<Warning>();
    }
    
    public async Task<IEnumerable<Caselog>> GetAllCaselogs()
    {
        var response = await Get($"{CaselogsBaseURL}/all");
        return ParseCaselogs(response);
    }

    public async Task<IEnumerable<Warning>> GetWarningsAfter(DateTime dt)
    {
        var response = await Get($"{WarningsBaseURL}/after/{dt:O}");

        var result = JsonConvert.DeserializeObject<Warning[]>(response);
        return result ?? Array.Empty<Warning>();
    }

    public async Task<IEnumerable<Caselog>> GetCaselogsAfter(DateTime dt)
    {
        var response = await Get($"{CaselogsBaseURL}/after/{dt:O}");
        return ParseCaselogs(response);
    }

    public async Task<IEnumerable<Warning>?> GetWarnings(Snowflake userId)
    {
        var result = await Get($"{WarningsBaseURL}/{userId.ID}");
        return JsonConvert.DeserializeObject<Warning[]>(result);
    }

    public async Task<IEnumerable<Caselog>?> GetCaselogs(Snowflake userId)
    {
        var result = await Get($"{CaselogsBaseURL}/{userId.ID}");
        return ParseCaselogs(result);
    }

    private static IEnumerable<Caselog> ParseCaselogs(string response)
    {
        // Remove the "totalCases" portion if it exists
        if (response.Contains("totalCases", StringComparison.OrdinalIgnoreCase))
            response = response[..(response.LastIndexOf("\"totalCases\"", StringComparison.OrdinalIgnoreCase)-1)] + "}";

        if (response.Length <= 2)
            yield break;

        var result = JsonConvert.DeserializeObject<Dictionary<int, Caselog>>(response)?.Values.ToArray();
        if (result is null)
            yield break;

        foreach (var caselog in result)
            yield return caselog;
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