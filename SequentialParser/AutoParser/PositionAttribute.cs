namespace SequentialParser.AutoParser;
/// <summary>
/// Anotate fields with this for auto parser order, u must put this on constant fields as they do not preserve their order
/// </summary>
[System.AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
public sealed class PositionAttribute : Attribute
{
    public readonly string Field;
    public readonly Position Pos;

    public enum Position
    {
        Before,
        After,
    }
    public PositionAttribute(string field,Position p)
    {
        Field = field;
        Pos = p;
    }
}