namespace fg_script.core
{
    public class Parser
    {
        // private:
        List<Token> Tokens { get; set;  }

        int Position { get; set; } = 0;

        // public:
        public Parser(ref List<Token> tokens)
        {
            Tokens = tokens;
        }


        // protected:
        public Token? TokenAt(UInt32 pos) 
        {
            return null;
        }

        public Token? PeekNextToken() { return null;  }
        public Token? NextToken() { return null; }
        public Token? CurrentToken() { return null;  }
    }
}
