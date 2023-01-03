namespace fg_script.core
{
    public class Parser
    {
        private readonly List<Token> Tokens;
        private readonly string Filepath;
        private int Position { get; set; } = 0;

        public Parser(string filepath, ref List<Token> tokens)
        {
            Filepath = filepath;
            Tokens = tokens.FindAll(token => {
                return token.Type != TokenType.COMMENT
                    && token.Type != TokenType.NEW_LINE;
            });
        }

        public List<Stmt> Run()
        {
            List<Stmt> sequence = new();
            while (!HasEnded())
            {
                Stmt? stmt = ProcessStatement();
                if (stmt != null)
                    sequence.Add(stmt);
            }
            return sequence;
        }

        protected Stmt? ProcessStatement()
        {
            if (Match(TokenType.FUN_DECL)) return StateFuncDeclaration();
            if (Match(TokenType.TYPE)) return StateAssignDeclaration();
            if (Match(TokenType.DEFINE)) return StateDefineType();
            if (Match(TokenType.EXPOSE)) return StateExposeDeclaration();
            if (Match(TokenType.EXTERN)) return StateExternDeclaration();
            if (Match(TokenType.IF)) return StateIf();
            if (Match(TokenType.LEFT_BRACE)) return StateBlock();
            if (Match(TokenType.WHILE)) return StateWhile();
            if (Match(TokenType.FOR)) return StateFor();

            // if (Match(TokenType.RETURN)) return StateReturn();
            // if (Match(TokenType.BREAK)) return StateBreak();
            // if (Match(TokenType.CONTINUE)) return StateContinue();

            if (HasEnded())  
                return null;
            else
                throw Error("unexpected symbol");
        }

        protected Func StateFuncDeclaration()
        {
            Consume(TokenType.FUN_DECL, "fn was expected");
            Token name = Consume(TokenType.KEYWORD_OR_NAME, "invalid function name");
            Consume(TokenType.LEFT_PARENTH, "( was expected");

            List<ArgExpr> args = new();
            while(true)
            {
                Token arg_type = Consume(TokenType.TYPE, "type was expected");
                Token arg_name = Consume(TokenType.KEYWORD_OR_NAME, "invalid arg name");
                ArgExpr arg = new(arg_type.Lexeme, arg_name.Lexeme);
                args.Add(arg);

                // ignore
                if (Match(TokenType.COMMA) || Match(TokenType.RIGHT_PARENTH))
                {
                    if (Match(TokenType.COMMA)) Consume(TokenType.COMMA);
                    if (Match(TokenType.RIGHT_PARENTH))
                    {
                        Consume(TokenType.RIGHT_PARENTH);
                        break;
                    }
                } else
                {
                   throw Error(") was expected");
                }
            }
            Block body = StateBlock();
            return new Func(name.Lexeme, args, body);
        }

        protected Assign StateAssignDeclaration()
        {
            throw new Exception("todo");
        }
        protected Define StateDefineType()
        {
            throw new Exception("todo");
        }
        protected Expose StateExposeDeclaration()
        {
            throw new Exception("todo");
        }
        protected Expose StateExternDeclaration()
        {
            throw new Exception("todo");
        }

        protected If StateIf ()
        {
            throw new Exception("todo");
        }

        protected For StateFor()
        {
            throw new Exception("todo");
        }

        protected While StateWhile()
        {
            throw new Exception("todo");
        }

        protected Block StateBlock()
        {
            Consume(TokenType.LEFT_BRACE, "{ was expected");
            Block block = new();
            while (!Match(TokenType.RIGHT_BRACE))
            {
                if (HasEnded())
                    throw Error("} was expected");
                Stmt? stmt = ProcessStatement();
                if (stmt != null)
                    block.Add(stmt);
            }
            Consume(TokenType.RIGHT_BRACE, "} was expected");
            return block;
        }

        protected Token Consume(TokenType type, string err_message = "")
        {
            Token current = CurrentToken();
            if (current.Type == type)
            {
                NextToken();
                return current;
            }
            throw new SyntaxErrorException(err_message, current.Cursor, Filepath);
        }

        protected bool Match(TokenType type)
        {
            return CurrentToken().Type == type;
        }

        protected bool HasEnded()
        {
            return CurrentToken().Type == TokenType.EOF;
        }

        protected Token TokenAt(int pos) 
        {
            if (pos < 0)
                throw new FGScriptException("out of bound", "Cursor has invalid value", "");
            if (pos >= Tokens.Count)
                pos = Tokens.Count - 1; // always EOF
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

        public SyntaxErrorException Error(string message)
        {
            return new SyntaxErrorException(message, CurrentToken().Cursor, Filepath);
        }

        protected Token CurrentToken()
        {
            return TokenAt(Position);
        }
    }
}
