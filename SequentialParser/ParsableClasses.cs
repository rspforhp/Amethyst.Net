using System.Reflection;
using SequentialParser.Attributes;
using SequentialParser.AutoParser;
using SequentialParser.ManualParser;

namespace SequentialParser;

public static class ParsableClasses
{
    
    
    public static  readonly Dictionary<Type,ClassParser> Classes = new Dictionary<Type,ClassParser>();


    static ParsableClasses()
    {
        Register<int,IntParser>();
        Register<string,StringParser>();
        Register<char,CharParser>();
    }

    
    
    
    public static T Parse<T>(ref SimpleStringReader reader)
    {
        return Parse<T>(ref reader, null);
    }

    public static T Parse<T>(ref SimpleStringReader reader,FieldInfo optionalField)
    {
        object o = Parse(ref reader, typeof(T),optionalField);
        if (o == null) return default(T);
        return (T)o;
    }

    public static Exception LastException;


    public static ArrayParser GetArrayParser(Type arType)
    {
        if (!arType.IsArray) return null;
        if (Classes.TryGetValue(arType,out ClassParser p))
            return (ArrayParser)p;
        ArrayParser parser = new ArrayParser(arType);
        Classes.Add(arType,parser);
        return parser;
    }
    public static ClassParser GetCompatibleParser(Type t)
    {
        if (t.IsArray) return GetArrayParser(t);
        if (Classes.TryGetValue(t, out var parser)) return parser;
        //Fallback-optional
        foreach (var pair in Classes) if (pair.Key.IsAssignableTo(t)) return pair.Value;
        return null;
    }


    public static event Action<Type, FieldInfo, object,Exception> OnParse = delegate(Type type,FieldInfo optionalField,object parsed,Exception error)
    {
        Console.WriteLine($"{type} {optionalField} {parsed} {error?.Message}");
    }; 
    
    public static object Parse(ref SimpleStringReader reader,Type t,FieldInfo optionalField)
    {
        var oldPos = reader.Position;
        try
        {
            var parser = GetCompatibleParser(t);

            SkipWhitespaceAttribute skipWhitespace = optionalField==null?null:optionalField.GetCustomAttribute<SkipWhitespaceAttribute>();
            if (skipWhitespace is { Before: true }) reader.SkipWhitespace();
            var o = parser.Parse(ref reader,optionalField);
            OnParse(t, optionalField, o,null);
            if (skipWhitespace is { After: true }) reader.SkipWhitespace();
            if (o == null || !o.GetType().IsAssignableTo(t))
            {
                reader.Position = oldPos;
                return null;
            }
            return o;
        }
        catch (Exception e)
        {
            OnParse(t, optionalField,null,e);
            LastException = e;
            Console.WriteLine(e);
            reader.Position = oldPos;
            return null;
        }
    }
    

    
    public static void Register<P>(Type t) where P :ClassParser, new() => Classes.Add(t,new P());
    public static void Register<T,P>() where P : ManualParser<T>, new() => Register<P>(typeof(T));

    public static void Register(Type t,ClassParser p) => Classes.Add(t,p);
    public static void Register<T>(ClassParser p) => Register(typeof(T),p);
    
    public static void Register<T>() => Register(typeof(T));
    public static void Register(Type t)
    {
        if (t.IsEnum)
        {
            Classes.Add(t, new AutoParser.EnumParser(t));
        }
        else 
            Classes.Add(t, new AutoParser.AutoParser(t));
    }

    public static void RegisterAll(Type t)
    {
        Register(t);
        foreach (var nestedType in t.GetNestedTypes()) RegisterAll(nestedType);
    }
    public static void RegisterAll<T>() => RegisterAll(typeof(T));
}