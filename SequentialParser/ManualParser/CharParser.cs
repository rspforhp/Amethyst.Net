using System.Reflection;
using System.Text.RegularExpressions;
using SequentialParser.Attributes;

namespace SequentialParser.ManualParser;

public class CharParser : ManualParser<char>
{
    public override object Parse(ref SimpleStringReader reader,FieldInfo optionalField)
    {
        string readAsMuchAsICan = reader.Read(1);
        if (readAsMuchAsICan.Length == 0) return null;
        if (optionalField != null)
        {
            var crs = optionalField.GetCustomAttributes<CharRangeAttribute>().ToList();
            if (crs.Any(cr => cr.InRange(readAsMuchAsICan[0])))
            {
                return readAsMuchAsICan[0];
            }
            if(crs.Count>0) throw new Exception($"Char out of bounds {readAsMuchAsICan[0]}");
        }
        return readAsMuchAsICan[0];
    }
}