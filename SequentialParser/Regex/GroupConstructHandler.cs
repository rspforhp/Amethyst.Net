using System.Reflection.Metadata;
using System.Text;

namespace SequentialParser.Regex;

public abstract class GroupConstructHandler
{
    public abstract bool Handle(ref AdvancedStringReader reader,StringBuilder b,uint startPos,uint endPos);
}