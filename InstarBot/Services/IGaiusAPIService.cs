using PaxAndromeda.Instar.Gaius;

namespace PaxAndromeda.Instar.Services;

public interface IGaiusAPIService : IDisposable
{
    Task<IEnumerable<Warning>?> GetWarnings(Snowflake userId);
    Task<IEnumerable<Caselog>?> GetCaselogs(Snowflake userId);
}