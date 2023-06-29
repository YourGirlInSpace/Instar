using PaxAndromeda.Instar.Metrics;

namespace PaxAndromeda.Instar.Services;

public interface IMetricService
{
    Task<bool> Emit(Metric metric, double value);
}