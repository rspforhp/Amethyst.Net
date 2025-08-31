using System.Reflection;
using SequentialParser.Attributes;

namespace SequentialParser.AutoParser;

public class AutoParser : ClassParser
{
    public readonly Type autoType;
    public AutoParser(Type t)
    {
        autoType = t;
    }
    public override object Parse(ref SimpleStringReader reader,FieldInfo optionalField)
    {
        object o = Activator.CreateInstance(autoType);
        var fields = autoType.GetFields().ToList();
        foreach (var fieldInfo in fields.ToArray())
        {
            var posAtr = fieldInfo.GetCustomAttribute<PositionAttribute>();
            if (posAtr == null) continue;
            fields.Remove(fieldInfo);
            var target = fields.Find(a => a.Name == posAtr.Field);
            var targetIndex = fields.IndexOf(target);
            int needIndex = posAtr.Pos switch
            {
                PositionAttribute.Position.Before => targetIndex,
                PositionAttribute.Position.After => targetIndex + 1,
                _ => 0
            };
            fields.Insert(needIndex,fieldInfo);
        }
        foreach (var fieldInfo in fields)
        {
            if (fieldInfo.Attributes.HasFlag(FieldAttributes.Static) &&
                !fieldInfo.Attributes.HasFlag(FieldAttributes.Literal)) continue;
            var ft = fieldInfo.FieldType;
            var parsed=ParsableClasses.Parse(ref reader, ft,fieldInfo);
            if (fieldInfo.Attributes.HasFlag(FieldAttributes.Literal))
            {
                var value = fieldInfo.GetValue(o);
                if (value==null||!value.Equals(parsed))
                {
                    throw new Exception($"DID NOT MATCH {value} with {parsed} in \"{fieldInfo.Name}\"");
                }
            }
            else
            {
                if (parsed == null&& fieldInfo.GetCustomAttribute<OptionalValueAttribute>()==null ) throw new Exception($"Found null when field not optional \"{fieldInfo.Name}\"");
                fieldInfo.SetValue(o,parsed);
            }
        }

        return o;
    }
}