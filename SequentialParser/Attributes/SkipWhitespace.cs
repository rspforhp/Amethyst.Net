namespace SequentialParser.Attributes;

[System.AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
public sealed class SkipWhitespaceAttribute(bool before=true, bool after=true) : Attribute
{
    public readonly bool Before = before;
    public readonly bool After = after;
}