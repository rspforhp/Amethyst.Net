namespace RuleLexer.LexRules;

public struct EnumRule<T>  : ILexRule<EnumRule<T>> where T : struct, Enum
{
    public static string[] Names = Enum.GetNames<T>();
    public static T[] Values = Enum.GetValues<T>();
    public override string ToString()
    {
        return $"Enum<{typeof(T).Name}>";
    }

    public UnmanagedString AfterRead { get; set; }
    public bool Read(ref StringLexer reader, out string read)
    {
        foreach (var @enum in Values)
        {
            if (!reader.Exists(@enum.ToString(), true)) continue;
            read = @enum.ToString();
            //Value = @enum;
            return true;
        }

        read = "";
        return false;
    }
}