﻿//------------------------------------------------------------------------------
// <auto-generated>
//     此代码由工具生成。
//     运行时版本:4.0.30319.42000
//
//     对此文件的更改可能会导致不正确的行为，并且如果
//     重新生成代码，这些更改将会丢失。
// </auto-generated>
//------------------------------------------------------------------------------

namespace ArcFormats.Properties {
    using System;
    
    
    /// <summary>
    ///   一个强类型的资源类，用于查找本地化的字符串等。
    /// </summary>
    // 此类是由 StronglyTypedResourceBuilder
    // 类通过类似于 ResGen 或 Visual Studio 的工具自动生成的。
    // 若要添加或移除成员，请编辑 .ResX 文件，然后重新运行 ResGen
    // (以 /str 作为命令选项)，或重新生成 VS 项目。
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "17.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class Resources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Resources() {
        }
        
        /// <summary>
        ///   返回此类使用的缓存的 ResourceManager 实例。
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("ArcFormats.Properties.Resources", typeof(Resources).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   重写当前线程的 CurrentUICulture 属性，对
        ///   使用此强类型资源类的所有资源查找执行重写。
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   查找类似 Error: Specified class not found. 的本地化字符串。
        /// </summary>
        internal static string logErrorClassNotFound {
            get {
                return ResourceManager.GetString("logErrorClassNotFound", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 Path contains invalid characters. Switching the encoding in the settings might work. 的本地化字符串。
        /// </summary>
        internal static string logErrorContainsInvalid {
            get {
                return ResourceManager.GetString("logErrorContainsInvalid", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 Decompression failed. 的本地化字符串。
        /// </summary>
        internal static string logErrorDecompressFailed {
            get {
                return ResourceManager.GetString("logErrorDecompressFailed", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 Decryption failed. 的本地化字符串。
        /// </summary>
        internal static string logErrorDecScrFailed {
            get {
                return ResourceManager.GetString("logErrorDecScrFailed", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 Encryption failed. 的本地化字符串。
        /// </summary>
        internal static string logErrorEncScrFailed {
            get {
                return ResourceManager.GetString("logErrorEncScrFailed", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 {0} v{1} archive temporarily not supported. 的本地化字符串。
        /// </summary>
        internal static string logErrorNotSupportedVersion {
            get {
                return ResourceManager.GetString("logErrorNotSupportedVersion", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 Error: Specified pack method not found. 的本地化字符串。
        /// </summary>
        internal static string logErrorPackMethodNotFound {
            get {
                return ResourceManager.GetString("logErrorPackMethodNotFound", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 Error: Specified unpack method not found. 的本地化字符串。
        /// </summary>
        internal static string logErrorUnpackMethodNotFound {
            get {
                return ResourceManager.GetString("logErrorUnpackMethodNotFound", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 Try to decompress: {0}…… 的本地化字符串。
        /// </summary>
        internal static string logTryDecompress {
            get {
                return ResourceManager.GetString("logTryDecompress", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 Try to decompress {0} with {1}…… 的本地化字符串。
        /// </summary>
        internal static string logTryDecompressWithMethod {
            get {
                return ResourceManager.GetString("logTryDecompressWithMethod", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 Try to decrypt: {0}…… 的本地化字符串。
        /// </summary>
        internal static string logTryDecScr {
            get {
                return ResourceManager.GetString("logTryDecScr", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 Try to encrypt: {0}…… 的本地化字符串。
        /// </summary>
        internal static string logTryEncScr {
            get {
                return ResourceManager.GetString("logTryEncScr", resourceCulture);
            }
        }
    }
}
