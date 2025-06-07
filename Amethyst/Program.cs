using System.Diagnostics;
using Parser;
using Parser.Attributes;
using Parser.Rules;

namespace Amethyst;

public static class Program
{
    public static void Main(string[] args)
    {
        Console.WriteLine("Hello, World!");
        var parserInstance = new ParserInstance();
        parserInstance.RegisterFileParser<AmethystShardParser>();
        parserInstance.ParseFile(@"C:\Janet\Janet\CS\amethyst-sharp\Amethyst\hello_world.shard");
    }
}
//File extension names to be used:
//amethyst, shard, geode
//owo ?
// amethyst - project file
// shard - code file
// geode - solution file

public class AmethystShardParser : AbstractFileParser
{
    public override string Extension => "shard";
    public override void Parse()
    {
        var rule = new FromTypeParser<Member>(this);
        rule.Parse();
        Console.WriteLine(rule.ParsedValue);
        Debugger.Break();
    }
}

[GenExRule]
public struct Member
{
    public enum Type
    {
        @_,func,field,etc
    }

    [EndsWithWhiteSpace]
    public AbstractSubParser.WithPosition<Type> MemberType;
    [EndsWithWhiteSpace]
    public AbstractSubParser.WithPosition<string> ReturnType;
    [EndsWithWhiteSpace]
    public AbstractSubParser.WithPosition<string> MethodName;

    [GenExRule]
    public struct ParamElement
    {
        [EndsWithWhiteSpace]
        public string ParamType;
        [EndsWithWhiteSpace]
        public string Name;
    }
    
    [GenExRule]
    public struct ParamsList
    {
        public AbstractSubParser.WithPosition<AbstractSubParser.WithPosition<ParamElement>[]> Elements;
    }

 
    [RoundScope]
    [EndsWithWhiteSpace]
    public AbstractSubParser.WithPosition<ParamsList> Params;

    [GenExRule]
    public struct InCode
    {
        
    }

    [CurveScope]
    [EndsWithWhiteSpace]
    public AbstractSubParser.WithPosition<InCode> Code;
}