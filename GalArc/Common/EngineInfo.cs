using System;
using System.Collections.Generic;

namespace GalArc.Common
{
    public class EngineInfo
    {
        public string EngineName { get; set; }
        public string UnpackFormat { get; set; }
        public string PackFormat { get; set; }

        internal EngineInfo(string engineName, string unpackFormat, string packFormat)
        {
            EngineName = engineName;
            UnpackFormat = unpackFormat;
            PackFormat = packFormat;
        }
    }

    public class EngineInfos
    {
        public static readonly List<EngineInfo> engineInfos = new List<EngineInfo>
        {
            new EngineInfo("AdvHD","ARC/PNA","ARC/PNA"),
            new EngineInfo("Ai5Win","ARC/DAT/VSD",string.Empty),
            new EngineInfo("Ai6Win","ARC","ARC"),
            new EngineInfo("Artemis","PFS","PFS"),
            new EngineInfo("BiShop","BSA", "BSA"),
            new EngineInfo("Cmvs","CPZ","CPZ"),
            new EngineInfo("EntisGLS","NOA","NOA"),
            new EngineInfo("Eushully","ALF",string.Empty),
            new EngineInfo("InnocentGrey","IGA/DAT","IGA/DAT"),
            new EngineInfo("KID","DAT","DAT"),
            new EngineInfo("Kirikiri","XP3","XP3"),
            new EngineInfo("Majiro","ARC","ARC"),
            new EngineInfo("NekoSDK","PAK/DAT","PAK/DAT"),
            new EngineInfo("NeXAS","PAC",string.Empty),
            new EngineInfo("NextonLikeC","LST",string.Empty),
            new EngineInfo("NitroPlus","PAK","PAK"),
            new EngineInfo("NScripter","NS2/NSA","NS2/NSA"),
            new EngineInfo("Palette","PAK","PAK"),
            new EngineInfo("PJADV","DAT/PAK","DAT/PAK"),
            new EngineInfo("RPGMaker","RGSSAD/RGSS2A/RGSS3A","RGSSAD/RGSS2A/RGSS3A"),
            new EngineInfo("Seraph","DAT",string.Empty),
            new EngineInfo("Siglus","Scene.PCK/Gameexe.DAT",string.Empty),
            new EngineInfo("Softpal","PAC","PAC"),
            new EngineInfo("SystemNNN","GPK/VPK","GPK/VPK"),
            new EngineInfo("Triangle","CG/CGF/SUD","CG/CGF/SUD"),
            new EngineInfo("Yuris","YPF",string.Empty),
        };
    }
}
