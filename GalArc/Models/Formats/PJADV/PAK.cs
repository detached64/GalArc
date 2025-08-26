
namespace GalArc.Models.Formats.PJADV;

internal class PAK : DAT
{
    public override string Name => "PAK";
    public override string Description => "PJADV PAK Archive";
    public override bool CanWrite => true;
}
