using System.ComponentModel;
using System.Configuration;
using System.Globalization;

namespace PaxAndromeda.Instar.Converters;

/// <summary>
/// Provides convert methods for snowflakes in configuration
/// </summary>
public class SnowflakeConverter : ConfigurationConverterBase
{
    public override bool CanConvertTo(
        ITypeDescriptorContext ctx, Type type)
    {
        return type == typeof(string);
    }

    public override bool CanConvertFrom(
        ITypeDescriptorContext ctx, Type type)
    {
        return type == typeof(string);
    }

    public override object ConvertFrom(
        ITypeDescriptorContext? ctx, CultureInfo? ci, object data)
    {

        var id = ulong.Parse((string)data,
            CultureInfo.InvariantCulture);

        return new Snowflake(id);
    }
}