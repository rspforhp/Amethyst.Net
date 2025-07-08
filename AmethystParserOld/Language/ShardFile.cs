using System.Diagnostics;
using AmethystParser.Lexing;

namespace AmethystParser.Language;

public class ShardFile : IOwnARule,ICanBeRead
{
    
    
    
    

    public void Read(StringLexer l)
    {
        
        
        
        //Debugger.Break();
    }

    public static LexRule Rule { get; set; }=null;
}