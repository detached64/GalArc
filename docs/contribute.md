# How to contribute

## Translation

1. Go to GalArc.GUI -> Properties -> Resources.resx , add a new language and translate the strings. 

2. Go to Log -> Properties -> Resources.resx , add a new language and translate the strings. 

3. Go to GalArc.GUI -> Controller -> UpdateContent.cs -> line 34 , add your language name and culture like this:

   ```C#
   internal static void InitLang()
   {
       OptionWindow.languages.Add("English", "en-US");
       OptionWindow.languages.Add("简体中文", "zh-CN");
       OptionWindow.languages.Add("日本語","ja-JP");		// add here
   }
   ```

## Add new format support

1. Add new folder and files in ArcFormats. 

2. Make sure the namespace is ArcFormats.(engine name) , and there's a public class named after the extension(upper). 

3. Add unpack or pack method there. Make sure the methods are like these:

   ```C#
   public static void Unpack(string filePath, string folderPath, Encoding encoding)
   {
       
   }
   public static void Pack(string folderPath, string filePath, string version, Encoding encoding)
   {
       
   }
   ```

   Even if you don't need encoding or version specified , don't remove them. 

4. Add a new EngineInfo in GalArc.GUI -> Resource -> EngineInfo.cs. You may refer to the constructors above to construct a new object. 