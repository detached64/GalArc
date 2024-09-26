using System;
using System.Collections.Generic;

namespace GalArc.Resource
{
    internal class EngineInfo
    {
        internal string EngineName { get; set; }
        internal string UnpackFormat { get; set; }
        internal string PackFormat { get; set; }

        internal EngineInfo(string engineName, string unpackFormat, string packFormat)
        {
            EngineName = engineName;
            UnpackFormat = unpackFormat;
            PackFormat = packFormat;
        }
    }

    internal class EngineInfos
    {
        public static List<EngineInfo> engineInfos = new List<EngineInfo>
        {
            new EngineInfo("AdvHD","ARC/PNA","ARC/PNA"),
            new EngineInfo("Ai5Win","ARC/DAT/VSD",string.Empty),
            new EngineInfo("Ai6Win","ARC",string.Empty),
            new EngineInfo("Artemis","PFS","PFS"),
            new EngineInfo("BiShop","BSA", "BSA"),
            new EngineInfo("Cmvs","CPZ","CPZ"),
            new EngineInfo("EntisGLS","NOA","NOA"),
            new EngineInfo("InnocentGrey","IGA/DAT","IGA/DAT"),
            new EngineInfo("KID","DAT","DAT"),
            new EngineInfo("Kirikiri","XP3","XP3"),
            new EngineInfo("Majiro","ARC","ARC"),
            new EngineInfo("NeXAS","PAC", string.Empty),
            new EngineInfo("NextonLikeC","LST",string.Empty),
            new EngineInfo("NitroPlus","PAK","PAK"),
            new EngineInfo("NScripter","NS2","NS2"),
            new EngineInfo("Palette","PAK","PAK"),
            new EngineInfo("PJADV","DAT/PAK","DAT/PAK"),
            new EngineInfo("RPGMaker","RGSSAD/RGSS2A/RGSS3A","RGSSAD/RGSS2A/RGSS3A"),
            new EngineInfo("Softpal","PAC","PAC"),
            new EngineInfo("SystemNNN","GPK/VPK","GPK/VPK"),
            new EngineInfo("Triangle","CG/CGF/SUD","CG/CGF/SUD"),
            new EngineInfo("Yuris","YPF", string.Empty),
        };
    }
}
