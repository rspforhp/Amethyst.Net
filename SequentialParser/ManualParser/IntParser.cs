using System.Reflection;
using System.Text.RegularExpressions;

namespace SequentialParser.ManualParser;

public class IntParser : ManualParser<int>
{
    public override object Parse(ref SimpleStringReader reader,FieldInfo optionalField)
    {
        reader.SkipWhitespace();
        string readAsMuchAsICan = reader.ReadUntill(s =>s.Length>0&& char.IsWhiteSpace(s[^1]));
        if (readAsMuchAsICan.Length == 0) return null;
        reader.SkipWhitespace();
        return int.Parse(readAsMuchAsICan);
    }
}