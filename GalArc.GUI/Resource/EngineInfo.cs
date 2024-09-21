using System;
using System.Collections.Generic;
using System.Linq;

namespace GalArc.Resource
{
    internal class EngineInfo
    {
        internal string EngineName { get; set; }
        internal string UnpackFormat { get; set; }
        internal string PackFormat { get; set; }
        internal string PackVersion { get; set; }
        internal bool isUnpackEncodingEnabled { get; set; }
        internal bool isPackEncodingEnabled { get; set; }
        internal bool isDecryptScriptEnabled { get; set; }

        internal EngineInfo(string engineName, string unpackFormat, string packFormat, string packVersion, bool isUnpackEncodingEnabled, bool isPackEncodingEnabled, bool isDecryptScriptEnabled)
        {
            EngineName = engineName;
            UnpackFormat = unpackFormat;
            PackFormat = packFormat;
            PackVersion = packVersion;
            this.isUnpackEncodingEnabled = isUnpackEncodingEnabled;
            this.isPackEncodingEnabled = isPackEncodingEnabled;
            this.isDecryptScriptEnabled = isDecryptScriptEnabled;
        }
        internal EngineInfo(string engineName, string unpackFormat, string packFormat)
        : this(engineName, unpackFormat, packFormat, string.Empty, false, false, false)
        {
        }
        internal EngineInfo(string engineName, string unpackFormat, string packFormat, string packVersion)
            : this(engineName, unpackFormat, packFormat, packVersion, false, false, false)
        {
        }
        internal EngineInfo(string engineName, string unpackFormat, string packFormat, bool isUnpackEncodingEnabled, bool isPackEncodingEnabled)
            : this(engineName, unpackFormat, packFormat, string.Empty, isUnpackEncodingEnabled, isPackEncodingEnabled, false)
        {
        }

    }
    internal class EngineInfos
    {
        public static List<EngineInfo> engineInfos = new List<EngineInfo>
        {
            new EngineInfo("AdvHD","ARC/PNA","ARC/PNA","1/2",false,false,true),
            new EngineInfo("Ai5Win","ARC/VSD",string.Empty),
            new EngineInfo("Ai6Win","ARC",string.Empty),
            new EngineInfo("Artemis","PFS","PFS","8/6/2",true,true,false),
            new EngineInfo("BiShop","BSA", "BSA","1/2"),
            new EngineInfo("Cmvs","CPZ","CPZ","1"),
            new EngineInfo("EntisGLS","NOA","NOA",true,true),
            new EngineInfo("InnocentGrey","IGA/DAT","IGA/DAT"),
            new EngineInfo("KID","DAT","DAT"),
            new EngineInfo("Kirikiri","XP3","XP3","1/2"),
            new EngineInfo("Majiro","ARC","ARC","1/2"),
            new EngineInfo("NeXAS","PAC", string.Empty,true,false),
            new EngineInfo("NextonLikeC","LST",string.Empty),
            new EngineInfo("NitroPlus","PAK","PAK"),
            new EngineInfo("NScripter","NS2","NS2"),
            new EngineInfo("Palette","PAK","PAK"),
            new EngineInfo("RPGMaker","RGSSAD/RGSS2A/RGSS3A","RGSSAD/RGSS2A/RGSS3A","1"),
            new EngineInfo("Softpal","PAC","PAC","1/2",false,false,true),
            new EngineInfo("SystemNNN","GPK/VPK","GPK/VPK","1/2"),
            new EngineInfo("Triangle","CG/CGF/SUD","CG/CGF/SUD","1"),
            new EngineInfo("Yuris","YPF", string.Empty, string.Empty,false,false,true),
        };
    }
}
