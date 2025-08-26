
namespace GalArc.Models.Formats.Commons;

internal interface IConfigurableFormat;

internal interface IUnpackConfigurable : IConfigurableFormat
{
    ArcOptions UnpackOptions { get; }
}

internal interface IPackConfigurable : IConfigurableFormat
{
    ArcOptions PackOptions { get; }
}
