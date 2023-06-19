using PaxAndromeda.Instar;
using PaxAndromeda.Instar.Gaius;
using PaxAndromeda.Instar.Services;

namespace InstarBot.Tests.Services;

public class MockGaiusAPIService : IGaiusAPIService
{
    private readonly Dictionary<Snowflake, List<Warning>> _warnings;
    private readonly Dictionary<Snowflake, List<Caselog>> _caselogs;
    private readonly bool _inhibit;
    
    public MockGaiusAPIService(Dictionary<Snowflake,List<Warning>> warnings, Dictionary<Snowflake,List<Caselog>> caselogs, bool inhibit = false)
    {
        _warnings = warnings;
        _caselogs = caselogs;
        _inhibit = inhibit;
    }

    public void Dispose()
    {
        // do nothing
    }

    public Task<IEnumerable<Warning>?> GetWarnings(Snowflake userId)
    {
        if (_inhibit)
            return Task.FromResult<IEnumerable<Warning>?>(null);
        
        return !_warnings.ContainsKey(userId)
            ? Task.FromResult<IEnumerable<Warning>?>(Array.Empty<Warning>())
            : Task.FromResult<IEnumerable<Warning>?>(_warnings[userId]);
    }

    public Task<IEnumerable<Caselog>?> GetCaselogs(Snowflake userId)
    {
        if (_inhibit)
            return Task.FromResult<IEnumerable<Caselog>?>(null);
        
        return !_caselogs.ContainsKey(userId)
            ? Task.FromResult<IEnumerable<Caselog>?>(Array.Empty<Caselog>())
            : Task.FromResult<IEnumerable<Caselog>?>(_caselogs[userId]);
    }
}