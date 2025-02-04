# How to contribute

## Translate

1. Use ResX Manager extension or something to translate all the strings.

3. Go to GalArc/Common/Languages.cs , add your language name and culture like this:

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
   public override void Unpack(string filePath, string folderPath)
   {
       
   }
   public override void Pack(string folderPath, string filePath)
   {
       
   }
   ```

4. Add a new EngineInfo in GalArc/Common/EngineInfo.cs.