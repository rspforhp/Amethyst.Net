using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using PLGSGen;
using PLGSGen.Rework;

namespace PLSGen;
//TODO: its unfinished, and idk if i will finish it lol, the source gen part atleast
[Generator()]
public class PLSGenerator : IIncrementalGenerator
{
    //Syntaxis is basically from antlr lol
    //but much and i mean, much more simplified imo
    public static List<(string name, ILexRule rule)> GetRuleList(ref SimpleStringReader simpleReader)
    {
        List<(string name, ILexRule rule)> l = new();
        bool failed = false;
        while (!failed)
        {
            var rule=GetSingleRule(ref simpleReader, out failed);
            if(!failed)
                l.Add(rule);
        }
        return l;
    }

    public static string ReadQuotedString(ref SimpleStringReader reader)
    {
        return reader.ReadQuotedString();
    }
    public static ILexRule GetRuleElement(ref SimpleStringReader reader, out bool failed)
    {
        if (reader.Exists("\"",false))
        {
            failed = false;
            return new StringRule(reader.ReadQuotedString());
        }
        
        
        var ruleC = reader.ReadUntill(a => a.EndsWith("("));
        switch (ruleC)
        {
            case "switch":
            {
                reader.Position++;
                List<ILexRule> rules = new(6);
                ILexRule r = null;
                bool singleFailed = false;
                while (!singleFailed)
                {
                    reader.SkipWhitespace();
                    r = GetRuleElement(ref reader, out singleFailed);
                    if (!singleFailed)
                    {
                        rules.Add(r);
                    }
                    reader.SkipWhitespace();
                    if (!reader.Exists(",", true))
                    {
                        reader.SkipWhitespace();
                        if (!reader.Exists(")", true))
                        {
                            throw new Exception("Unexpected end");
                        }
                        else break;
                    }
                }
                failed = false;
                return new SwitchRule(rules);
            }
        }


        failed = true;
        return null;
    }
    public static (string name, ILexRule rule) GetSingleRule(ref SimpleStringReader reader,out bool failed)
    {
        string n = "";
        ILexRule r = null;
        reader.SkipWhitespace();
        n = reader.ReadUntill(c => c.EndsWith("="));
        if (n.Length == 0 || n.Length > 256)
        {
            failed = true;
            return default;
        }
        n = n.TrimEnd();
        reader.SkipWhitespace();
        reader.Position++;
        reader.SkipWhitespace();
        r = GetRuleElement(ref reader, out failed);
        reader.SkipWhitespace();
        if (!reader.Exists(";", true)) failed = true;
        return (n, r);
    }
    public static string HandleFile(string content,string name)
    {
        if (content.Length == 0) return "";
        StringBuilder b = new();
        SimpleStringReader simpleReader = new(content);
        var ruleList = GetRuleList(ref simpleReader);
      
        b.AppendLine($"using PLSGen;");
        b.AppendLine($"using PLGSGen;");
        b.AppendLine($"using PLGSGen.Rework;");
        b.AppendLine("using System.Diagnostics;");
        b.AppendLine($"namespace LexRules.{name}{{");
       
        foreach (var rule in ruleList)
        {
            b.AppendLine("[DebuggerDisplay(\"{DebuggerDisplay()}\")]");
            b.AppendLine($"public partial struct {rule.name} : ILexRule {{");
            b.Append($$"""
                     public Func<object> CloningProp { get; set; }
                      public string DebuggerDisplay()
                      {
                          return UnderlyingRule.DebuggerDisplay();
                      }
                     
                      public bool Optional { get; set; }
                      public string Label { get; set; }
                      public string LexedText { get; set; }
                      public uint LexedPosition { get; set; }
                      public bool HasLexed { get; set; }
                      public bool _Lex(ref SimpleStringReader reader, out string readText)
                      {
                          return UnderlyingRule.Lex(ref reader,out readText);
                      }
                      
                      public ILexRule UnderlyingRule;
                      public {{rule.name}}()
                      {
                            UnderlyingRule={{rule.rule.GenerateRuleTree()}};    
                            CloningProp=()=>new {{rule.name}}();
                      }
                      
                   
                     }
                     """);
        }
        b.AppendLine("}");
        return b.ToString();
    }

    public void Initialize(IncrementalGeneratorInitializationContext initContext)
    {
        IncrementalValuesProvider<AdditionalText> textFiles =
            initContext.AdditionalTextsProvider.Where(static file => file.Path.EndsWith(".plsg"));
        IncrementalValuesProvider<(string name, string content)> namesAndContents =
            textFiles.Select((text, cancellationToken) => (name: Path.GetFileNameWithoutExtension(text.Path),
                content: text.GetText(cancellationToken)!.ToString()));

        // generate a class that contains their values as const strings
        initContext.RegisterSourceOutput(namesAndContents, (spc, nameAndContent) =>
        {
            if (nameAndContent.content.Length == 0) return;
            var content = nameAndContent.content;
            string builder = HandleFile(content,nameAndContent.name);
            spc.AddSource($"PLSG.{nameAndContent.name}.g", builder);
        });
    }
}