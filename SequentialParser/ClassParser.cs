using System.Reflection;

namespace SequentialParser;

public abstract class ClassParser
{
    public abstract object Parse(ref SimpleStringReader reader,FieldInfo optionalField);
}