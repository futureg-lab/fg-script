namespace fg_script.core
{
    public class Parser
    {
        private readonly List<Token> Tokens;

        int Position { get; set; } = 0;

        public Parser(ref List<Token> tokens)
        {
            Tokens = tokens;
        }


        protected Token? TokenAt(int pos) 
        {
            if (pos < 0)
                throw new ParseException("Cursor has negative value");
            pos = Math.Max(0, Math.Min(pos, Tokens.Count - 1));
            return Tokens[pos];
        }

        protected Token? PeekNextToken() 
        {
            return TokenAt(Position + 1);
        }

        protected Token? NextToken() 
        {
            return TokenAt(Position++);
        }

        protected Token? CurrentToken()
        {
            return TokenAt(Position);
        }
    }
}
