using SequentialParser;

namespace Tests;

public static class Program
{
   
    public static void Main(string[] args)
    {
            var reader = new SimpleStringReader(@"""one\r\ntwo\r\nthree""");
        var parsed = AmethystShit.StringLiteral.Parse(ref reader);
        Console.WriteLine(parsed);
    }
}