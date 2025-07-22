using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using LexRules.CSharpLiterals;
using PLGSGen;
using PLGSGen.Rework;

public static partial class Program
{
    public static void Main(string[] args)
    {
        CultureInfo.CurrentCulture=CultureInfo.InvariantCulture;//TODO: change stuff everywhere to invariant
        //var handleFile = PLSGenerator.HandleFile(File.ReadAllText(@"C:\Janet\Janet\CS\amethyst-sharp\Amethyst-Tests\New_CSharpLiterals.plsg"),"test");
        BooleanRule r = new();
        var re = new SimpleStringReader("false");
        r.Lex(ref re, out string text);
    }
}

