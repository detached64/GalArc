namespace GalArc.Templates
{
    public class ArcOptions
    {
    }

    public class VersionOptions : ArcOptions
    {
        public string Version { get; set; }
    }

    public class ScriptUnpackOptions : ArcOptions
    {
        public bool DecryptScripts { get; set; } = true;
    }

    public class ScriptPackOptions : ArcOptions
    {
        public bool EncryptScripts { get; set; } = true;
    }

    public class VersionScriptPackOptions : VersionOptions
    {
        public bool EncryptScripts { get; set; } = true;
    }
}
