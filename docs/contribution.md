# How to contribute

## Translate

1. Go to GalArc.GUI -> Properties -> Resources.resx , add a new language and translate the strings. 

2. Go to Log -> Properties -> Resources.resx , add a new language and translate the strings. 

3. Go to GalArc.GUI -> Resource -> Languages.cs -> line 9 , add your language name and culture like this:

   ```C#
   internal static Dictionary<string, string> languages = new Dictionary<string, string>
   {
       { "简体中文" , "zh-CN" },
       { "English" , "en-US" },
       { "日本語" , "ja-JP" }			// add here
   };
   ```

## Add new format support

1. Add new folder and files in ArcFormats. 

2. Make sure the namespace is ArcFormats.(engine name) , and there's a public class named after the extension(upper). 

3. Add unpack or pack method there. Make sure the methods are like these:

   ```C#
   public static void Unpack(string filePath, string folderPath)
   {
       
   }
   public static void Pack(string folderPath, string filePath)
   {
       
   }
   ```

4. Add a new EngineInfo in GalArc.GUI -> Resource -> EngineInfo.cs. You may refer to the constructors above to construct a new object. 