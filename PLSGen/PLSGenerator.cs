using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using PLGSGen;

namespace PLSGen;

[Generator()]
public class PLSGenerator : IIncrementalGenerator
{
    //Syntaxis is basically from antlr lol
    //but much and i mean, much more simplified imo
    public static List<RuleElement> GetRuleList(ref SimpleStringReader simpleReader)
    {
        var ruleList = new List<RuleElement>();
        while (true)
        {
            SimpleRule? parsedRule = null;
            bool negate = false;
            simpleReader.SkipWhitespace();
            if (simpleReader.Exists("~", true)) negate = true;
            simpleReader.SkipWhitespace();
            if (simpleReader.Exists("\"", true))
            {
                
                var word = simpleReader.ReadUntill(str =>
                {
                    var s = UnEscape(str);
                    var endsWith = s.EndsWith("\"");
                    int amount = 0;
                    for (int i = s.Length - 2; i >= 0; i--)
                    {
                        char c = s[i];
                        if (c == '\\') amount++;
                        else break;
                    }
                    return endsWith&&(amount%2==0);
                });
                simpleReader.Position++;
                if (word.Length > short.MaxValue)
                {
                    throw new NotSupportedException("String is too big, perhaps an error!");
                }

                word = UnEscape(word);
                parsedRule = new SimpleRule(word);
            }
            else
            {
                char curC = simpleReader.Peek(1)[0];
                if (char.IsLetter(curC))
                {
                    var readRuleName = simpleReader.ReadUntill(str => str.Any(c => !char.IsLetter(c)));
                    parsedRule = new SimpleRule(readRuleName, true);
                }
                else if (curC == '(')
                {
                    simpleReader.Position++;
                    var newScope = GetRuleList(ref simpleReader);
                    parsedRule = new SimpleRule(newScope);
                }
                else if (curC == '\'')
                {
                    simpleReader.Position++;
                    char from = '\0';
                    from = simpleReader.Read(1)[0];
                    if (simpleReader.Exists("'", true))
                    {
                        simpleReader.SkipWhitespace();
                        if (simpleReader.Exists("..", true))
                        {
                            simpleReader.SkipWhitespace();
                            char to = '\0';
                            if (simpleReader.Exists("'", true))
                            {
                                to = simpleReader.Read(1)[0];
                                if (simpleReader.Exists("'", true))
                                {
                                    parsedRule = new SimpleRule(from, to);
                                }
                            }
                        }
                    }
                }
                else if (curC == '[')
                {
                    simpleReader.Position++;
                    string chars = simpleReader.ReadUntill(a => a.EndsWith("]") && !a.EndsWith("\\]"));
                    if (simpleReader.Exists("]", true))
                    {
                        chars=UnEscape(chars);
                        var ar = chars.ToCharArray();
                        parsedRule = new SimpleRule(ar);
                    }
                }

                if (parsedRule == null) Debugger.Break();
            }

            RuleElement.RuleRelationType type = RuleElement.RuleRelationType._;
            bool breakAfter = false;

            simpleReader.SkipWhitespace();
            if (simpleReader.Exists("*", true))
            {
                parsedRule = new SimpleRule(parsedRule.Value).MakeOptional();
            }

            simpleReader.SkipWhitespace();
            if (simpleReader.Exists("+", true))
            {
                parsedRule = new SimpleRule(parsedRule.Value);
            }

            simpleReader.SkipWhitespace();
            if (simpleReader.Exists("?", true))
            {
                parsedRule = parsedRule.Value.MakeOptional();
            }

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
            else if (simpleReader.Exists(")", true))
            {
                breakAfter = true;
            }
            else throw new NotSupportedException("WHAT?");

            if (parsedRule.HasValue)
            {
                if (negate)
                    parsedRule = parsedRule.Value.NegateMe();
                ruleList.Add(new RuleElement(parsedRule.Value, type));
            }
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

        return ruleList;
    }
    private static string ToLiteral(string input) {
        StringBuilder literal = new StringBuilder(input.Length + 2);
        foreach (var c in input) {
            switch (c) {
                case '\"': literal.Append("\\\""); break;
                case '\\': literal.Append(@"\\"); break;
                case '\0': literal.Append(@"\0"); break;
                case '\a': literal.Append(@"\a"); break;
                case '\b': literal.Append(@"\b"); break;
                case '\f': literal.Append(@"\f"); break;
                case '\n': literal.Append(@"\n"); break;
                case '\r': literal.Append(@"\r"); break;
                case '\t': literal.Append(@"\t"); break;
                case '\v': literal.Append(@"\v"); break;
                default:
                    // ASCII printable character
                    if (c >= 0x20 && c <= 0x7e) {
                        literal.Append(c);
                        // As UTF16 escaped character
                    } else {
                        literal.Append(@"\u");
                        literal.Append(((int)c).ToString("x4"));
                    }
                    break;
            }
        }
        return literal.ToString();
    }
    public static string UnEscape(string chars)
    {
        Regex regex = new Regex(@"\\U([0-9A-F]{4})", RegexOptions.IgnoreCase);
        chars = regex.Replace (chars, match => ((char)int.Parse (match.Groups[1].Value,
            NumberStyles.HexNumber)).ToString ());
        chars = Regex.Unescape(chars);
        return chars;
    }
    public static string Escape(string chars)
    {
        return ToLiteral(chars);
    }
    public static string HandleFile(string content)
    {
        if (content.Length == 0) return "";
        StringBuilder b = new();
        b.AppendLine($"using PLSGen;");
        b.AppendLine("using System.Diagnostics;");
        b.AppendLine("namespace LexRules{");
        SimpleStringReader simpleReader = new(content);
        while (true)
        {
            simpleReader.SkipWhitespace();
            var ruleName =
                simpleReader.ReadUntill(s => s.EndsWith("=") || s.Length == 0 || char.IsWhiteSpace(s, s.Length - 1));
            if (ruleName.Length == 0) break;
            simpleReader.SkipWhitespace();
            if (simpleReader.Read(1) != "=") return $"Syntax error at: {simpleReader.Position} expected '='";

            b.AppendLine("[DebuggerDisplay(\"{DebugView()}\")]");
            b.AppendLine($"public partial class {ruleName} : LexRule {{");
            var ruleList = GetRuleList(ref simpleReader);

            b.AppendLine($"public override string Name=>\"{ruleName}\";");
            b.AppendLine($"public static readonly SimpleRule StaticRule={(new SimpleRule(ruleList).ToString())};");
            b.AppendLine($"public {ruleName}():base() {{");
            b.Append($"Elements=");
            b.Append($"{ruleName}.StaticRule");
            b.AppendLine($";");

            b.AppendLine("}");

            //Debugger.Break();
            b.AppendLine("}");
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
            string builder = HandleFile(content);
            spc.AddSource($"PLSG.{nameAndContent.name}.g", builder);
        });
    }
}