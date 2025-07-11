using System.Diagnostics;
using System.Text;
using Microsoft.CodeAnalysis;
using PLGSGen;

namespace PLSGen;

[Generator()]
public class PLSGenerator : IIncrementalGenerator
{
    //Syntaxis is basically from antlr lol
    //but much and i mean, much more simplified imo
    public static string HandleFile(string content)
    {
        if (content.Length == 0) return "";
        StringBuilder b = new();
        b.AppendLine($"using PLSGen;");
        SimpleStringReader simpleReader = new(content);
        while (true)
        {
            simpleReader.SkipWhitespace();
            var ruleName = simpleReader.ReadUntill(s=>s.EndsWith("=")||char.IsWhiteSpace(s, s.Length-1));
            simpleReader.SkipWhitespace();
            if (simpleReader.Read(1) != "=") return $"Syntax error at: {simpleReader.Position} expected '='";
            b.AppendLine("namespace LexRules{");

            b.AppendLine($"public partial class {ruleName} : LexRule {{");
            var ruleList = new RuleList();

            while (true)
            {
                SimpleRule? parsedRule = null;
                simpleReader.SkipWhitespace();
                if (simpleReader.Exists("\"",true))
                {
                    var word = simpleReader.ReadUntill(str => str.EndsWith("\""));
                    simpleReader.Position++;
                    if (word.Length > short.MaxValue)
                    {
                        throw new NotSupportedException("String is too big, perhaps an error!");
                    }
                    parsedRule = new SimpleRule(word);
                }
                else
                {
                    Debugger.Break();
                }

                RuleElement.RuleRelationType type = RuleElement.RuleRelationType._;
                bool breakAfter = false;
                simpleReader.SkipWhitespace();
                if (simpleReader.Exists("|", true))
                {
                    type = RuleElement.RuleRelationType.Or;
                }
                else if (simpleReader.Exists("&", true))
                {
                    type = RuleElement.RuleRelationType.And;
                }
                else if (simpleReader.Exists(";", true))
                {
                    breakAfter = true;
                }
                else throw new NotSupportedException("WHAT?");

                if (parsedRule.HasValue)
                    ruleList.Add(new RuleElement(parsedRule.Value, type));
                else
                {
                    Debugger.Break();
                    break;
                }
                if (breakAfter)
                {
                   // Debugger.Break();
                    break;
                }
            }

            b.AppendLine($"public override string Name=>\"{ruleName}\";");
            b.AppendLine($"public {ruleName}():base() {{");


            b.Append($"Elements=");

            b.AppendLine(ruleList.ToString());
            

            b.Append($";");
            
            b.AppendLine("}");

            //Debugger.Break();
            b.AppendLine("}");
            b.AppendLine("}");

            break;
        }
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
           string builder = HandleFile(content);
            spc.AddSource($"PLSG.{nameAndContent.name}.g", builder);
        });
    }
}