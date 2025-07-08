using System.Globalization;
using AmethystLexer.Literal;
using RuleLexer;
using RuleLexer.LexRules;

CultureInfo.CurrentCulture=CultureInfo.InvariantCulture;//TODO: change stuff everywhere to invariant

StringLexer r=new("""
                  public static class AProgram
                  {
                      public static entry System.Void main()
                      {
                          for (int i = 0; i < 10; i++)
                          {
                              System.Console.WriteLine(Fib(i));
                          }
                      }
                      public static int Fib(int n) 
                      {
                          if (n < 2)
                          {
                              return n;
                          }
                          return Fib(n - 1) + Fib(n - 2);
                      }
                  }
                  """);
//f.Read(r);
StringLexer test = new StringLexer("\'a\'");
CharLiteral l = new();
var f =test.Read(ref l,out var read);
Console.WriteLine(read);