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
            if (Match(TokenType.KEYWORD_OR_NAME)) return StateFuncCallRoot();

            if (Match(TokenType.RETURN)) return StateReturn();
            if (Match(TokenType.ERROR)) return StateError();

            // if (Match(TokenType.BREAK)) return StateBreak();
            // if (Match(TokenType.CONTINUE)) return StateContinue();

            if (HasEnded())
                return null;
            throw Error("unexpected token");
        }


        protected Func StateBodyLessFunction()
        {
            Consume(TokenType.FUN_DECL, "fn was expected");
            Token name = Consume(TokenType.KEYWORD_OR_NAME, "invalid function name");

            Consume(TokenType.LEFT_PARENTH, "( was expected");
            List<ArgExpr> args = new();
            while (!Match(TokenType.RIGHT_PARENTH))
            {
                Token arg_type = Consume(TokenType.TYPE, "valid return type was expected");
                Token arg_name = Consume(TokenType.KEYWORD_OR_NAME, "invalid arg name");
                ArgExpr arg = new(arg_type, arg_name);
                args.Add(arg);

                if (Match(TokenType.COMMA))
                    Consume(TokenType.COMMA);
                else
                    break;
            }
            Consume(TokenType.RIGHT_PARENTH, ") was expected");

            Consume(TokenType.RET_OP, "-> was expeceted");
            Token ret_type = Consume(TokenType.TYPE, "valid return type was expected");

            return new Func(name, args, ret_type, null);
        }

        protected Func StateFuncDeclaration()
        {
            Func func = StateBodyLessFunction();
            func.Body = StateBlock();
            return func;
        }

        protected Assign StateAssignDeclaration()
        {
            Token type = Consume(TokenType.TYPE, "type was expected");
            Token v_name = Consume(TokenType.KEYWORD_OR_NAME, "variable name was expected");
            Expr? value = null;
            if (Match(TokenType.ASSIGN) || Match(TokenType.SEMICOLUMN))
            {
                if (Match(TokenType.ASSIGN))
                {
                    Consume(TokenType.ASSIGN);
                    if (Match(TokenType.NULL))
                        value = new LiteralExpr(Consume(TokenType.NULL));
                    else
                        value = ConsumeExpr();
                    Consume(TokenType.SEMICOLUMN, "; was expected");
                }
            }
            else
            {
                throw Error("= or ; was expected");
            }

            if (value == null)
                throw Error(v_name.Lexeme + " is unitialized");

            VarExpr expr = new(type, v_name, value);
            return new(expr);
        }

        // statements
        protected Define StateDefineType()
        {
            throw new Exception("todo");
        }

        protected Expose StateExposeDeclaration()
        {
            Consume(TokenType.EXPOSE, "expose was expected");
            Func func = StateFuncDeclaration();
            return new(func);
        }

        protected Extern StateExternDeclaration()
        {
            Consume(TokenType.EXTERN, "extern was expected");
            Func func = StateBodyLessFunction();
            Consume(TokenType.SEMICOLUMN, "; was expected");
            return new(func);
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

        protected FuncCallDirect StateFuncCallRoot()
        {
            FuncCall fcall = ConsumeFuncCall();
            Consume(TokenType.SEMICOLUMN, "; was expected");
            return new FuncCallDirect(fcall);
        }

        protected Return StateReturn()
        {
            Consume(TokenType.RETURN, "ret was expected");
            Expr returned = ConsumeExpr();
            Consume(TokenType.SEMICOLUMN, "; was expected");
            return new(returned);
        }

        protected Error StateError()
        {
            Consume(TokenType.ERROR, "err was expected");
            Expr thrown = ConsumeExpr();
            Consume(TokenType.SEMICOLUMN, "; was expected");
            return new(thrown);
        }

        // Expressions
        // Ex: 1+x*(1-2), "some strings", ...etc
        // expr     ::= term (+| - | or) expr | term
        // term     ::= factor (* | / | and) term | factor
        // factor   ::= (expr) | unary
        // unary    ::= literal | func_call | var_name

        // expr     ::= term (+| - | or) expr | term
        protected Expr ConsumeExpr()
        {
            Expr expr = ConsumeTerm();

            List<TokenType> bin = new()
            {
                TokenType.PLUS,
                TokenType.MINUS,
                TokenType.OR
            };

            foreach (var op in bin)
            {
                if (Match(op))
                {
                    Token op_token = Consume(op);
                    Expr right = ConsumeExpr();
                    expr = new BinaryExpr(op_token, expr, right);
                    break;
                }
            }

            return expr;
        }

        // term     ::= factor (* | / | and) term | factor
        protected Expr ConsumeTerm()
        {
            Expr expr = ConsumeFactor();

            List<TokenType> bin = new()
            {
                TokenType.MULT,
                TokenType.DIV,
                TokenType.AND
            };

            foreach (var op in bin)
            {
                if (Match(op))
                {
                    Token op_token = Consume(op);
                    Expr right = ConsumeTerm();
                    expr = new BinaryExpr(op_token, expr, right);
                    break;
                }
            }

            return expr;
        }

        // factor   ::= (expr) | unary
        protected Expr ConsumeFactor()
        {
            if (Match(TokenType.LEFT_PARENTH))
            {
                Consume(TokenType.LEFT_PARENTH);
                Expr expr = ConsumeExpr();
                Consume(TokenType.RIGHT_PARENTH, ") was expected");
                return expr;
            }
            return ConsumeUnary();
        }

        // unary    ::= literal | func_call | var_name | monoadic_op
        protected Expr ConsumeUnary()
        {
            if (Match(TokenType.MINUS) || Match(TokenType.NOT))
            {
                Token op = Match(TokenType.MINUS) ? 
                    Consume(TokenType.MINUS) : Consume(TokenType.NOT);
                Expr expr = ConsumeUnary();
                return new UnaryExpr(op, expr);
            }

            // Literals
            if (Match(TokenType.STRING))
                return new LiteralExpr(Consume(TokenType.STRING));
            if (Match(TokenType.NUMBER))
                return new LiteralExpr(Consume(TokenType.NUMBER));
            if (Match(TokenType.BOOL))
                return new LiteralExpr(Consume(TokenType.BOOL));

            // var call
            if (Match(TokenType.KEYWORD_OR_NAME)) 
            {
                if (MatchNext(TokenType.LEFT_PARENTH))
                    return ConsumeFuncCall();
                else
                    return new VarCall(Consume(TokenType.KEYWORD_OR_NAME));
            }

            throw Error("invalid expression");
        }

        protected FuncCall ConsumeFuncCall()
        {
            Token callee = Consume(TokenType.KEYWORD_OR_NAME, "callee name expected");
            Consume(TokenType.LEFT_PARENTH, "( was expected");

            List<Expr> args = new();
            while(!Match(TokenType.RIGHT_PARENTH))
            {
                Expr expr = ConsumeExpr();
                args.Add(expr);

                if (Match(TokenType.COMMA))
                    Consume(TokenType.COMMA);
                else
                    break;
            }

            Consume(TokenType.RIGHT_PARENTH, ") was expected");

            FuncCall res = new(callee, args);
            return res;
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
        protected bool MatchNext(TokenType type)
        {
            return PeekNextToken().Type == type;
        }

        protected bool HasEnded()
        {
            return CurrentToken().Type == TokenType.EOF;
        }

        protected Token TokenAt(int pos) 
        {
            if (pos < 0) pos = 0;
            if (pos >= Tokens.Count) pos = Tokens.Count - 1;
            return Tokens[pos];
        }

        protected Token PeekNextToken()
        {
            return TokenAt(Position + 1);
        }

        protected Token PeekPrevToken()
        {
            return TokenAt(Position - 1);
        }

        protected Token NextToken() 
        {
            return TokenAt(Position++);
        }

        protected Token PrevToken()
        {
            return TokenAt(Position--);
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
