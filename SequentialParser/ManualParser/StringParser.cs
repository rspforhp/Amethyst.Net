using System.Reflection;
using System.Text.RegularExpressions;

namespace SequentialParser.ManualParser;

public class StringParser : ManualParser<String>
{
    public override object Parse(ref SimpleStringReader reader,FieldInfo optionalField)
    {
        char[] stopAt = [];
        if (optionalField != null)
        {
            var atr = optionalField.GetCustomAttribute<StopStringOnAttribute>();
            if (atr != null)
            {
                stopAt = atr.StopAt;
            }
        }
        reader.SkipWhitespace();
        string readAsMuchAsICan = reader.ReadUntill(s =>s.Length>0&& (char.IsWhiteSpace(s[^1]) || stopAt.Contains(s[^1])));
        if (readAsMuchAsICan.Length == 0) return null;
        reader.SkipWhitespace();
        return readAsMuchAsICan;
    }
}