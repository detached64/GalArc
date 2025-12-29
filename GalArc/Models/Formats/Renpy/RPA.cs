using GalArc.Infrastructure.Progress;
using GalArc.Models.Formats.Commons;
using GalArc.Models.Utils;
using Razorvine.Pickle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace GalArc.Models.Formats.Renpy;

internal partial class RPA : ArcFormat
{
    public override string Name => "RPA";
    public override string Description => "Ren'Py RPA Archive";

    private const string MagicPattern = @"^RPA-(\d)\.(\d) $";

    private class RenpyEntry : Entry
    {
        public byte[] Prefix { get; set; }
    }

    public override void Unpack(string input, string output)
    {
        using FileStream fs = File.OpenRead(input);
        using BinaryReader br = new(fs);
        string magic = Encoding.ASCII.GetString(br.ReadBytes(8));
        Match match = MagicRegex().Match(magic);
        if (!match.Success)
        {
            throw new InvalidArchiveException();
        }
        int major = int.Parse(match.Groups[1].Value);
        int minor = int.Parse(match.Groups[2].Value);
        if (!long.TryParse(Encoding.ASCII.GetString(br.ReadBytes(16)), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out long indexOffset))
        {
            throw new InvalidArchiveException("Invalid index offset.");
        }
        fs.Position++;
        if (!uint.TryParse(Encoding.ASCII.GetString(br.ReadBytes(8)), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out uint key))
        {
            throw new InvalidArchiveException("Invalid key.");
        }
        long indexSize = fs.Length - indexOffset;
        fs.Position = indexOffset;
        byte[] index = ZlibHelper.Decompress(br.ReadBytes((int)indexSize));
        using Unpickler unpickler = new();
        IDictionary result = unpickler.loads(index) as IDictionary;
        List<RenpyEntry> entries = new(result.Count);
        foreach (DictionaryEntry item in result)
        {
            RenpyEntry entry = new();
            entry.Name = (string)item.Key;
            IList values = (IList)item.Value;
            IList infos = (IList)values[0];
            entry.Offset = Convert.ToUInt32(infos[0]);
            entry.Size = Convert.ToUInt32(infos[1]);
            if (major >= 3)
            {
                entry.Offset ^= key;
                entry.Size ^= key;
            }
            if (infos.Count > 2 && infos[2] != null)
            {
                switch (infos[2])
                {
                    case string prefixStr:
                        entry.Prefix = new byte[prefixStr.Length];
                        int i = 0;
                        foreach (char c in prefixStr)
                        {
                            entry.Prefix[i++] = (byte)c;
                        }
                        break;
                    case byte[] prefixBytes:
                        entry.Prefix = prefixBytes;
                        break;
                }
                entry.Size -= (uint)entry.Prefix.Length;
            }
            entries.Add(entry);
        }
        ProgressManager.SetMax(entries.Count);
        foreach (RenpyEntry entry in entries)
        {
            fs.Position = entry.Offset;
            byte[] data = br.ReadBytes((int)entry.Size);
            if (entry.Prefix?.Length > 0)
            {
                byte[] combined = new byte[entry.Prefix.Length + data.Length];
                Buffer.BlockCopy(entry.Prefix, 0, combined, 0, entry.Prefix.Length);
                Buffer.BlockCopy(data, 0, combined, entry.Prefix.Length, data.Length);
                data = combined;
            }
            string path = Path.Combine(output, entry.Name);
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllBytes(path, data);
            ProgressManager.Progress();
        }
    }

    [GeneratedRegex(MagicPattern)]
    private static partial Regex MagicRegex();
}
