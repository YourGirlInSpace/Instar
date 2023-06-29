using System.Diagnostics.CodeAnalysis;
using PaxAndromeda.Instar.Gaius;

namespace PaxAndromeda.Instar.Services;

[SuppressMessage("ReSharper", "UnusedMember.Global")]
public interface IGaiusAPIService : IDisposable
{
    Task<IEnumerable<Warning>> GetAllWarnings();
    Task<IEnumerable<Caselog>> GetAllCaselogs();
    Task<IEnumerable<Warning>> GetWarningsAfter(DateTime dt);
    Task<IEnumerable<Caselog>> GetCaselogsAfter(DateTime dt);
    Task<IEnumerable<Warning>?> GetWarnings(Snowflake userId);
    Task<IEnumerable<Caselog>?> GetCaselogs(Snowflake userId);
}