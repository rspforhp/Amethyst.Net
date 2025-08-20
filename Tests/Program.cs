using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using SequentialParser;
using SequentialParser.Regex;
using SequentialParser.Regex.CharacterClasses;
using UnicodeCategory = SequentialParser.Regex.CharacterClasses.UnicodeCategory;

namespace Tests;

public static class Program
{
    public class TestHandler : GroupConstructHandler
    {
        public override bool Handle(ref AdvancedStringReader reader,StringBuilder b, uint startPos, uint endPos)
        {
            return true;
        }
    }
    public static void Main(string[] args)
    {
      
        /*
        var g = ClassCharacter.Get(@"((\d{1,3})$testNumber$t@nameOfLink@)");
        var reader = new AdvancedStringReader("123t:3")
            .AddHandler("testNumber",new TestHandler())
            .AddGroup("nameOfLink", ClassCharacter.Get<GroupConstruct>(@"(:\d)"));
        var read=reader.Read(g);
        Console.WriteLine(AmethystShit.CopyHandlersAndNamesHere);
        Console.WriteLine(read);
        */

        var balancedBrackets = ClassCharacter.Get<GroupConstruct>(@"(\{@br@?\})");
        AdvancedStringReader reader = new AdvancedStringReader("{{{}}}").AddGroup("br",balancedBrackets);
        var readText= reader.Read(balancedBrackets);
        Console.WriteLine(readText);
        // Console.WriteLine(AmethystShit.CopyHandlersAndNamesHere);


    }
}