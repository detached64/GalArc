# How to contribute

## Translate

1. Use ResX Manager extension or something to translate all the strings.

2. Go to GalArc/Infrastructure/Cultures/CultureManager.cs and add your culture there.

## Add new format support

1. Add new folder and files in GalArc/Models/Formats.

2. Make sure the namespace is GalArc.Models.Formats.(engine name) , and there's a public class named after the extension(upper). For example, for .xp3, the class name should be XP3.

3. Fill the required properties and methods in the class.

## Find and add custom scheme in database

Follow the tutorials [here](./database/).
