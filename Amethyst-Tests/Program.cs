using System.Globalization;
using System.Runtime.InteropServices;
using LexRules;
using LexRules.CSharpLiterals;
using PLGSGen;
using PLGSGen.Rework;
using PLSGen;

CultureInfo.CurrentCulture=CultureInfo.InvariantCulture;//TODO: change stuff everywhere to invariant

SimpleStringReader reader = new SimpleStringReader("34134a");
var testRule = new ListRule(new RangeRule('0','9'));
var lex = testRule.Lex(ref reader,out var s);
Console.WriteLine(testRule);
/*
Console.WriteLine("test1");
var t = new StringLiteral()  ;
SimpleStringReader n = new("\"test of a string  literal :3\"");
var suc=t.Read(ref n);
Console.WriteLine(t.ReadValue);
*/