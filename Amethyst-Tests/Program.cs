using System.Globalization;
using PLGSGen;
using PLSGen;

CultureInfo.CurrentCulture=CultureInfo.InvariantCulture;//TODO: change stuff everywhere to invariant
Console.WriteLine("test1");
//var handleFile = PLSGenerator.HandleFile(File.ReadAllText(@"C:\Janet\Janet\CS\amethyst-sharp\Amethyst-Tests\example.plsg"));
var t = new BooleanRule();
SimpleStringReader n = new("true");
t.Read(ref n);
