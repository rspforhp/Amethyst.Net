using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using SequentialParser;
using SequentialParser.Attributes;
using SequentialParser.AutoParser;
using SequentialParser.ManualParser;

namespace Tests;

public abstract class Program
{
    public struct OpenCurlyBracket
    {
        [SkipWhitespace]
        public const string _ = "{";
    }
    public struct CloseCurlyBracket
    {
        [SkipWhitespace]
        
        public const string _ = "}";
    }
    
    public struct OpenBracket
    {
        [SkipWhitespace]
        public const string _ = "(";
    }
    public struct CloseBracket
    {
        [SkipWhitespace]
        public const string _ = ")";
    }
   
    public class ClassParsing
    {
        public enum ClassAccess
        {
            _,
            @public,
            @private,
            @protected,
            @internal,
        }

        [SkipWhitespace]
        public ClassAccess Access;

        public enum ClassModifier
        {
            _,
            @static,
            @sealed,
            @abstract,
        }

        [SkipWhitespace]
        public ClassModifier[] Modifiers;

        [Position(nameof(Modifiers),PositionAttribute.Position.After)]
        [SkipWhitespace]
        public const string @class = "class";

        [StopStringOn('{','}')]
        [SkipWhitespace]
        public string Name;

        public OpenCurlyBracket _open;


        [SkipWhitespace]
        public MethodParsing[] Methods;

        public CloseCurlyBracket _close;
    }

    public class MethodParsing
    {
        public enum MethodAccess
        {
            _,
            @public,
            @private,
            @protected,
            @internal,
        }

        [OptionalValue]
        [SkipWhitespace]
        public MethodAccess Access;
        
        public enum MethodModifier
        {
            _,
            @static,
            @sealed,
            @abstract,
            @virtual,
        }

        [SkipWhitespace]
        public MethodModifier[] Modifiers;

        [SkipWhitespace]
        public string ReturnType;
        [StopStringOn('(',')')]
        [SkipWhitespace]
        public string Name;

        public OpenBracket _open;

        
        
        public CloseBracket _close;

        public OpenCurlyBracket _methodOpen;
        
        

        public CloseCurlyBracket _methodClose;
    }


    public struct Digit
    {
        [CharRange('0','9')]
        public  char digit;
    }
    
    
    
    public static void Main(string[] args)
    {
        ParsableClasses.RegisterAll<Program>();
        var reader = new SimpleStringReader("""
                                            0123ab
                                            """);
        var testInt = ParsableClasses.Parse<Digit>(ref reader);
        var exc = ParsableClasses.LastException;
        Console.WriteLine(testInt);
    }
}