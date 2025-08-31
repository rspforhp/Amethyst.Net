using System.Reflection;

namespace SequentialParser.AutoParser;

public class EnumParser : ClassParser
{
    public readonly Type autoType;
    public EnumParser(Type t)
    {
        autoType = t;
    }
    public override object Parse(ref SimpleStringReader reader,FieldInfo optionalField)
    {
        //reader.SkipWhitespace();
        foreach (var eVal in autoType.GetEnumValues())
        {
            if (reader.Exists(eVal.ToString(), true))
            {
                //reader.SkipWhitespace();
                return eVal;
            }
        }

        throw new Exception($"No values match {autoType.Name} enum");
    }
}