using System.Globalization;
using System.Runtime.InteropServices;
using LexRules;
using PLGSGen;
using PLSGen;

CultureInfo.CurrentCulture=CultureInfo.InvariantCulture;//TODO: change stuff everywhere to invariant
Console.WriteLine("test1");
var handleFile = PLSGenerator.HandleFile(File.ReadAllText(@"C:\Janet\Janet\CS\amethyst-sharp\Amethyst-Tests\example.plsg"));

var t = new StringLiteral()  ;
SimpleStringReader n = new("\"test of a string  literal :3\"");
var suc=t.Read(ref n);
Console.WriteLine(t.ReadValue);
