namespace fg_script.core
{
    public class Parser
    {
        private readonly List<Token> Tokens;
        private int Position { get; set; } = 0;
        private Program Program { get; set; } = new(null);


        public Parser(string filepath, ref List<Token> tokens)
        {
            Program = new(filepath);
            Tokens = tokens;
        }


        public Program Run()
        {
            while (CurrentToken().Type != TokenType.EOF)
                ConsumeNext();
            return Program;
        }

        public void ConsumeNext()
        {
            switch (CurrentToken().Type)
            {
                case TokenType.FUN_DECL:
                    break;
                case TokenType.EXTERN:
                    break;
                case TokenType.EXPOSE:
                    break;
                case TokenType.IF:
                    break;
                case TokenType.ELSE_IF:
                    break;
                case TokenType.ELSE:
                    break;
                case TokenType.WHILE:
                    break;
                case TokenType.FOR:
                    break;
                case TokenType.TYPE:
                    break;
                case TokenType.KEYWORD_OR_NAME:
                    break;
                case TokenType.ASSIGN:
                    break;
                case TokenType.STRING:
                    break;
                case TokenType.NUMBER:
                    break;
                case TokenType.LEFT_BRACE:
                    break;
                case TokenType.LEFT_PARENTH:
                    break;
                case TokenType.LEFT_BRACKET:
                    break;
                default:
                    break;
            }
            NextToken();
        }

        protected Token TokenAt(int pos) 
        {
            if (pos < 0 || pos >= Tokens.Count)
                throw new ParseException("Cursor has invalid value");
            return Tokens[pos];
        }

        protected Token? PeekNextToken()
        {
            if (Position == Tokens.Count)
            {
                return null;
            }
            return TokenAt(Position + 1);
        }

        protected Token NextToken() 
        {
            return TokenAt(Position++);
        }

        protected Token CurrentToken()
        {
            return TokenAt(Position);
        }
    }
}
