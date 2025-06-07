using System.Text;
using Parser.Rules;

namespace Parser;

public abstract class AbstractFileParser : AbstractParser
{
    public virtual string Extension {  get; }

 
}  