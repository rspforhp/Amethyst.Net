using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using SequentialParser;
using SequentialParser.AutoParser;
using SequentialParser.ManualParser;

namespace Tests;

public abstract class Program
{
    public struct OpenCurlyBracket
    {
        [StopStringOn('}')]
        public const string _ = "{";
    }
    public struct CloseCurlyBracket
    {
        [StopStringOn('{')]
        public const string _ = "}";
    }
    
    public struct OpenBracket
    {
        [StopStringOn(')')]
        public const string _ = "(";
    }
    public struct CloseBracket
    {
        [StopStringOn('(','{')]
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

        public ClassAccess Access;

        public enum ClassModifier
        {
            _,
            @static,
            @sealed,
            @abstract,
        }

        public ClassModifier[] Modifiers;

        [Position(nameof(Modifiers),PositionAttribute.Position.After)]
        public const string @class = "class";

        [StopStringOn('{','}')]
        public string Name;

        public OpenCurlyBracket _open;


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
        public MethodAccess Access;
        
        public enum MethodModifier
        {
            _,
            @static,
            @sealed,
            @abstract,
            @virtual,
        }

        public MethodModifier[] Modifiers;

        public string ReturnType;
        [StopStringOn('(',')')]
        public string Name;

        public OpenBracket _open;

        
        
        public CloseBracket _close;

        public OpenCurlyBracket _methodOpen;
        
        

        public CloseCurlyBracket _methodClose;
    }
    public static void Main(string[] args)
    {
        ParsableClasses.RegisterAll<Program>();
        var reader = new SimpleStringReader("""
                                            public abstract sealed class Test{
                                                public static void MethodTest(){
                                                
                                                }
                                            }
                                            """);
        var testInt = ParsableClasses.Parse<ClassParsing>(ref reader);
        var exc = ParsableClasses.LastException;
        Console.WriteLine(testInt);
    }
}