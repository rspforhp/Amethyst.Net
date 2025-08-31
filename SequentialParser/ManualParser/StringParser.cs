using System.Reflection;
using System.Text.RegularExpressions;
using SequentialParser.Attributes;

namespace SequentialParser.ManualParser;

public class StringParser : ManualParser<String>
{
    public override object Parse(ref SimpleStringReader reader,FieldInfo optionalField)
    {
        //reader.SkipWhitespace();
        char[] stopAt = [];
        if (optionalField != null)
        {
            var atr = optionalField.GetCustomAttribute<StopStringOnAttribute>();
            if (atr != null)
            {
                stopAt = atr.StopAt;
            }
            
            if (optionalField.Attributes.HasFlag(FieldAttributes.Literal))
            {
                var value =(string) optionalField.GetValue(null);
                if (!reader.Exists(value,true))
                {
                    throw new Exception($"DID NOT MATCH {value} in \"{optionalField.Name}\"");
                }
                return value;
            }
            
        }
        string readAsMuchAsICan = reader.ReadUntill(s =>s.Length>0&& (char.IsWhiteSpace(s[^1]) || stopAt.Contains(s[^1])));
        if (readAsMuchAsICan.Length == 0) return null;
        //reader.SkipWhitespace();
        return readAsMuchAsICan;
    }
}