using System.Diagnostics;

namespace Parser;

[AttributeUsage(AttributeTargets.Struct)]
public sealed class FileStructureAttribute(string fileExtension) : Attribute
{
    public readonly string FileExt = fileExtension;
}

/// <summary>
/// 
/// </summary>
/// <param name="expr">
/// Example: @switch{1:upper 2:normal}
/// Separated by spaces, names of structure elements, or if starting with @ decorational names they DO NOTHING
/// Have scopes which always default a multiple containing segment
/// put a '*' inside to inherit it from usage, ie. [*] for just a list without [] needed in code
/// for inherited scopes, end the scope when control say's to
/// if expr == "$0$", it's not implemented, and completely ignore it, without checking for if the syntaxis is valid
/// if theres NUM: before an element its seen as a control statement
/// </param>
[AttributeUsage(AttributeTargets.Struct)]
public sealed class StructStructureAttribute(string expr) : Attribute
{
    public readonly string ExprText = expr;
    public SimpleExpression Expr => ExprText;
}

public struct SimpleExpression
{
    public readonly string Text;

    public static implicit operator SimpleExpression(string s) => new SimpleExpression(s);
    public static implicit operator string(SimpleExpression s) => s.Text;



    private struct ExpressionElement
    {
        public readonly string Text;
        
        public bool IsNameDecor;
        public string Name;
        public (char?, char?) Scope=(null,null);
        public bool ScopeInherited;


        public static readonly (char, char)[] ScopeChars = [
            ('(',')'),('<','>'),('[',']'),('{','}')
        ];

        public static (char?, char?) GetUsedScope(string fullStr, out bool inherit,out int scopeStart)
        {
            scopeStart = fullStr.Length;
            foreach (var scope in ScopeChars)
            {
                var index1 = fullStr.LastIndexOf(scope.Item1);
                var index2 = fullStr.LastIndexOf(scope.Item2);
                if (index1 == -1 || index2 == -1) continue;
                if (index1 > index2) throw new Exception("WTF?");
                inherit = fullStr[index1 + 1] == '*';
                scopeStart = index1;
                return scope;
            }
            inherit = false;
            return (null,null);
        }
        public ExpressionElement(string t)
        {
            Text = t;
            var @index = Text.IndexOf('@');
            if (index != -1) IsNameDecor = true;
            Scope= GetUsedScope(Text, out ScopeInherited,out var lastIndex);
            Name = Text[(index + 1)..lastIndex];
        }

        private static string Either(bool cond, string l, string r)
        {
            return cond ? l : r;
        }

        public override string ToString()
        {
            return $"{Text} || {Either(IsNameDecor,"@","")}{Name}{Scope.Item1}{Either(ScopeInherited,"*","")}{Scope.Item2}";
        }
    }
    
    public SimpleExpression(string t)
    {
        Text = t;
        var split = Text.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).Select(a => new ExpressionElement(a)).ToList();
        Debugger.Break();
    }
    
}