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
            if (Match(TokenType.EXPOSE)) return StateExposeDeclaration();
            if (Match(TokenType.EXTERN)) return StateExternDeclaration();
            if (Match(TokenType.IF)) return StateIf();
            if (Match(TokenType.LEFT_BRACE)) return StateBlock();
            if (Match(TokenType.WHILE)) return StateWhile();
            if (Match(TokenType.FOR)) return StateFor();

            if (Match(TokenType.RETURN)) return StateReturn();
            if (Match(TokenType.ERROR)) return StateError();
            if (Match(TokenType.BREAK) || Match(TokenType.CONTINUE))
                return StateBreakOrContinue();

            if (Match(TokenType.KEYWORD_OR_NAME))
            {
                 if (MatchNext(TokenType.ASSIGN)) return StateReAssign();
                if (MatchNext(TokenType.LEFT_BRACKET)) return StateReAssignTuple();
            }


            if (HasEnded())
                return null;
            return StateRootExpression();
        }

        protected Stmt StateBreakOrContinue()
        {
            Stmt stmt;
            if (Match(TokenType.BREAK))
            {
                Consume(TokenType.BREAK);
                stmt = new Break();
            }
            else if (Match(TokenType.CONTINUE))
            {
                Consume(TokenType.CONTINUE);
                stmt = new Continue();
            }
            else
                throw Error("unexpected token");
            Consume(TokenType.SEMICOLON, @""";"" was expected");
            return stmt;
        }

        protected Func StateBodyLessFunction()
        {
            Consume(TokenType.FUN_DECL, @"""fn"" was expected");
            Token name = Consume(TokenType.KEYWORD_OR_NAME, "invalid function name");

            Consume(TokenType.LEFT_PARENTH, @"""("" was expected");
            List <ArgExpr> args = new();
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
            Consume(TokenType.RIGHT_PARENTH, @""")"" was expected");

            Consume(TokenType.RET_OP, @"""->"" was expeceted");
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
            if (Match(TokenType.ASSIGN) || Match(TokenType.SEMICOLON))
            {
                if (Match(TokenType.ASSIGN))
                {
                    Consume(TokenType.ASSIGN);
                    if (Match(TokenType.NULL))
                        value = new LiteralExpr(Consume(TokenType.NULL));
                    else
                        value = ConsumeGenExpr();
                    Consume(TokenType.SEMICOLON, @""";"" was expected");
                }
            }
            else
            {
                throw Error(@"""="" or "";"" was expected");
            }

            if (value == null)
                throw Error(v_name.Lexeme + " is unitialized");

            VarExpr expr = new(type, v_name, value);
            return new(expr);
        }

        protected ReAssign StateReAssign()
        {
            Token v_name = Consume(TokenType.KEYWORD_OR_NAME, "variable name was expected");
            Consume(TokenType.ASSIGN, "\"=\" was expected");
            Expr expr = ConsumeGenExpr();
            Consume(TokenType.SEMICOLON, "\";\" was expected");
            return new(v_name, expr);
        }
        
        protected ReAssignTuple StateReAssignTuple()
        {
            Token v_name = Consume(TokenType.KEYWORD_OR_NAME, "variable name was expected");
            Consume(TokenType.LEFT_BRACKET, @"""["" was expected");

            List<Expr> indexes = new();

            while (!Match(TokenType.RIGHT_BRACKET))
            {
                indexes.Add(ConsumeGenExpr());
                if (!Match(TokenType.RIGHT_BRACKET))
                    Consume(TokenType.COMMA);
            }
            Consume(TokenType.RIGHT_BRACKET, @"""]"" was expected");

            Consume(TokenType.ASSIGN, "\"=\" was expected");
            Expr expr = ConsumeGenExpr();
            Consume(TokenType.SEMICOLON, "\";\" was expected");

            return new(v_name, indexes, expr);
        }

        protected Expose StateExposeDeclaration()
        {
            Consume(TokenType.EXPOSE, @"""expose"" was expected");
            Func func = StateFuncDeclaration();
            return new(func);
        }

        protected Extern StateExternDeclaration()
        {
            Consume(TokenType.EXTERN, @"""extern"" was expected");
            Func func = StateBodyLessFunction();
            Consume(TokenType.SEMICOLON, @""";"" was expected");
            return new(func);
        }

        protected If StateIf ()
        {
            Consume(TokenType.IF, @"""if"" was expected");
            Expr cond = ConsumeGenExpr();
            Block body = StateBlock();

            If ifstmt = new(cond, body);

            while (Match(TokenType.ELSE_IF))
            {
                Consume(TokenType.ELSE_IF);
                Expr elseif_cond = ConsumeGenExpr();
                Block elseif_body = StateBlock();
                Branch elseif_branch = new(elseif_cond, elseif_body);
                ifstmt.Branches.Add(elseif_branch);
            }

            if (Match(TokenType.ELSE))
            {
                Consume(TokenType.ELSE, @"""else"" was expected");
                ifstmt.ElseBody = StateBlock();
            }

            return ifstmt;
        }

        // for_loop ::= for name | (name, name) in expr 
        protected For StateFor()
        {
            Consume(TokenType.FOR, @"""for"" was expected");


            Token ? KeyAlias, KeyValue;

            if (Match(TokenType.LEFT_PARENTH))
            {
                Consume(TokenType.LEFT_PARENTH, @"""("" was expected");
                KeyAlias = Consume(TokenType.KEYWORD_OR_NAME, "key alias expected");
                Consume(TokenType.COMMA, @""","" expected");
                KeyValue = Consume(TokenType.KEYWORD_OR_NAME, "key value expected");
                Consume(TokenType.RIGHT_PARENTH, @""")"" was expected");
            }
            else
            {
                KeyAlias = null;
                KeyValue = Consume(TokenType.KEYWORD_OR_NAME, "key value expected");
            }

            Consume(TokenType.IN, @"""in"" was expected");

            Expr expr_iter = ConsumeGenExpr();
            Block body = StateBlock();

            return new(body, KeyAlias, KeyValue, expr_iter);
        }

        protected While StateWhile()
        {
            Consume(TokenType.WHILE, @"""while"" was expected");
            Expr cond = ConsumeGenExpr();
            Block body = StateBlock();
            return new(body, cond);
        }

        protected Block StateBlock()
        {
            Consume(TokenType.LEFT_BRACE, @"""{"" was expected");
            Block block = new();
            while (!Match(TokenType.RIGHT_BRACE))
            {
                if (HasEnded())
                    throw Error(@"""}"" was expected");
                Stmt ? stmt = ProcessStatement();
                if (stmt != null)
                    block.Add(stmt);
            }
            Consume(TokenType.RIGHT_BRACE, @"""}"" was expected");
            return block;
        }

        protected RootExpression StateRootExpression()
        {
            Expr expr = ConsumeGenExpr();
            Consume(TokenType.SEMICOLON, @""";"" was expected");
            return new(expr);
        }

        protected Return StateReturn()
        {
            Consume(TokenType.RETURN, @"""ret"" was expected");
            Expr returned = ConsumeGenExpr();
            Consume(TokenType.SEMICOLON, @""";"" was expected");
            return new(returned);
        }

        protected Error StateError()
        {
            Consume(TokenType.ERROR, @"""err"" was expected");
            Expr thrown = ConsumeGenExpr();
            Consume(TokenType.SEMICOLON, @""";"" was expected");
            return new(thrown);
        }


        // Idea :
        // Expressions
        // Ex: 1+x*(1-2) and 3 <= 3, "some strings", ...etc

        // 1. A general expression can be thought like this :
        // gen_expr  ::= or_expr (..) gen_expr | or_expr
        // or_expr   ::= and_expr (or) or_expr | and_expr
        // and_expr  ::= comp_expr (and) and_expr | expr
        // comp_expr ::= expr (== | >= | == | >= | != | < | >) comp_expr | expr
        // expr      ::= term (+| -) expr | term
        // term      ::= factor (* | /) term | factor
        // factor    ::= (gen_expr) | unary
        // unary     ::= <literal> | <func_call> | <var_name> | <monoadic_operation> | <tuple>

        // gen_expr  ::= or_expr (..) gen_expr | or_expr
        protected Expr ConsumeGenExpr()
        {
            Expr expr = ConsumeOrExpr();
            if (Match(TokenType.DBL_DOT))
            {
                Consume(TokenType.DBL_DOT);
                Expr right = ConsumeGenExpr();
                return new EnumExpr(expr, right);
            }
            return expr;
        }

        // or_expr   ::= and_expr (or) or_expr | and_expr
        protected Expr ConsumeOrExpr()
        {
            Expr expr = ConsumeAndExpr();
            if (Match(TokenType.OR))
            {
                Token op_token = Consume(TokenType.OR);
                Expr right = ConsumeOrExpr();
                return new BinaryExpr(op_token, expr, right);
            }
            return expr;
        }

        // and_expr  ::= comp_expr (and) and_expr | expr
        protected Expr ConsumeAndExpr()
        {
            Expr expr = ConsumeComparisonExpr();
            if (Match(TokenType.AND))
            {
                Token op_token = Consume(TokenType.AND);
                Expr right = ConsumeAndExpr();
                return new BinaryExpr(op_token, expr, right);
            }
            return expr;
        }

        // comp_expr ::= expr (== | >= | == | >= | != | < | >) comp_expr | expr
        protected Expr ConsumeComparisonExpr()
        {
            Expr expr = ConsumeExpr();
            List<TokenType> bin = new()
            {
                TokenType.EQ,   // ==
                TokenType.NEQ,  // !=
                TokenType.LT,   // <
                TokenType.GT,   // >
                TokenType.LE,   // <=
                TokenType.GE    // >=
            };
            foreach (var op in bin)
            {
                if (Match(op))
                {
                    Token op_token = Consume(op);
                    Expr right = ConsumeComparisonExpr();
                    expr = new BinaryExpr(op_token, expr, right);
                    break;
                }
            }
            return expr;
        }

        // expr      ::= term (+| -) expr | term
        protected Expr ConsumeExpr()
        {
            Expr expr = ConsumeTerm();
            List<TokenType> bin = new()
            {
                TokenType.PLUS,
                TokenType.MINUS
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


        // term      ::= factor (* | /) term | factor
        protected Expr ConsumeTerm()
        {
            Expr expr = ConsumeFactor();

            List<TokenType> bin = new()
            {
                TokenType.MULT,
                TokenType.DIV,
                TokenType.MOD
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

        // factor   ::= (gen_expr) | unary
        protected Expr ConsumeFactor()
        {
            if (Match(TokenType.LEFT_PARENTH))
            {
                // try making an expression
                Consume(TokenType.LEFT_PARENTH);
                Expr expr = ConsumeGenExpr();
                Consume(TokenType.RIGHT_PARENTH, @""")"" was expected");
                return expr;
            }
            return ConsumeUnary();
        }

        // unary    ::= <literal> | <func_call> | <var_name> | <monoadic_operation> | <tuple>
        protected Expr ConsumeUnary()
        {
            if (Match(TokenType.MINUS) || Match(TokenType.NOT) || Match(TokenType.REPR_OF))
            {
                Token op;
                if (Match(TokenType.MINUS)) op = Consume(TokenType.MINUS);
                else if (Match(TokenType.NOT)) op = Consume(TokenType.NOT);
                else op = Consume(TokenType.REPR_OF);

                Expr expr = ConsumeUnary();
                return new UnaryExpr(op, expr);
            }

            if (Match(TokenType.LEFT_BRACKET))
                return ConsumeTuple();

            // Literals
            if (Match(TokenType.STRING))
                return new LiteralExpr(Consume(TokenType.STRING));
            if (Match(TokenType.NUMBER))
                return new LiteralExpr(Consume(TokenType.NUMBER));
            if (Match(TokenType.BOOL))
                return new LiteralExpr(Consume(TokenType.BOOL));
            if (Match(TokenType.NULL))
                return new LiteralExpr(Consume(TokenType.NULL));

            // var call
            if (Match(TokenType.KEYWORD_OR_NAME)) 
            {
                if (MatchNext(TokenType.LEFT_PARENTH))
                    return ConsumeFuncCall();
                else if (MatchNext(TokenType.LEFT_BRACKET))
                    return ConsumeArrayAccessCall();
                else
                    return new VarCall(Consume(TokenType.KEYWORD_OR_NAME));
            }

            throw Error("invalid expression");
        }

        protected TupleExpr ConsumeTuple()
        {
            var FetchKey = () =>
            {
                string key = "";
                if (Match(TokenType.STRING))
                    key = Consume(TokenType.STRING, "string expected").Lexeme;
                else if (Match(TokenType.KEYWORD_OR_NAME))
                    key = Consume(TokenType.KEYWORD_OR_NAME, "name expected").Lexeme;
                else if (Match(TokenType.NUMBER))
                    key = Consume(TokenType.NUMBER, "number expected").Lexeme;
                return key;
            };

            var IsKeyable = () => Match(TokenType.STRING) 
                || Match(TokenType.KEYWORD_OR_NAME) 
                || Match(TokenType.NUMBER);

            Consume(TokenType.LEFT_BRACKET, @"""["" was expected");


            TupleExpr tuple = new();
            bool auto_keys = true;

            // empty tuple
            if (Match(TokenType.RIGHT_BRACKET)) 
            { 
                Consume(TokenType.RIGHT_BRACKET);
                return tuple;
            }

            // tup ::= [ (string | word | number) : tup (, string | word | number : tup)* ]

            // first item decides the key type
            if (MatchNext(TokenType.COLON) && IsKeyable()) 
            {
                // disable autokeys
                auto_keys = false;
                string key = FetchKey();
                Consume(TokenType.COLON);
                if (Match(TokenType.LEFT_BRACKET))
                    tuple.Set(key, ConsumeTuple());
                else
                    tuple.Set(key, ConsumeGenExpr());
            }
            else
            {
                // auto_keys is true
                if (Match(TokenType.LEFT_BRACKET))
                    tuple.Append(ConsumeTuple());
                else
                    tuple.Append(ConsumeGenExpr());
            }


            while (!Match(TokenType.RIGHT_BRACKET))
            {
                Consume(TokenType.COMMA, @""","" was expected");
                if (!auto_keys)
                {
                    string key = FetchKey();
                    Consume(TokenType.COLON, @""":"" was expected");
                    if (Match(TokenType.LEFT_BRACKET))
                        tuple.Set(key, ConsumeTuple());
                    else
                        tuple.Set(key, ConsumeGenExpr());
                }
                else
                {
                    if (Match(TokenType.LEFT_BRACKET))
                        tuple.Append(ConsumeTuple());
                    else
                        tuple.Append(ConsumeGenExpr());
                }

                if (Match(TokenType.RIGHT_BRACKET))
                    break;
            }

            Consume(TokenType.RIGHT_BRACKET, @"""]"" was expected");

            return tuple;
        }

        protected FuncCall ConsumeFuncCall()
        {
            Token callee = Consume(TokenType.KEYWORD_OR_NAME, "callee name expected");
            Consume(TokenType.LEFT_PARENTH, @"""("" was expected");

            List <Expr> args = new();
            while(!Match(TokenType.RIGHT_PARENTH))
            {
                Expr expr = ConsumeGenExpr();
                args.Add(expr);

                if (Match(TokenType.COMMA))
                    Consume(TokenType.COMMA);
                else
                    break;
            }

            Consume(TokenType.RIGHT_PARENTH, @""")"" was expected");

            FuncCall res = new(callee, args);
            return res;
        }

        protected TupleIndexAccessCall ConsumeArrayAccessCall()
        {
            Token callee = Consume(TokenType.KEYWORD_OR_NAME, "tuple name expected");
            Consume(TokenType.LEFT_BRACKET, @"""["" was expected");
            
            List<Expr> indexes = new();
            
            while (!Match(TokenType.RIGHT_BRACKET))
            {
                indexes.Add(ConsumeGenExpr());
                if (!Match(TokenType.RIGHT_BRACKET))
                    Consume(TokenType.COMMA);
            }
            Consume(TokenType.RIGHT_BRACKET, @"""]"" was expected");
            return new(callee, indexes);
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

        protected Token ConsumeAny(string err_message, params TokenType[] types)
        {
            Token current = CurrentToken();

            foreach (TokenType type in types)
            {
                if (current.Type == type)
                {
                    NextToken();
                    return current;
                }
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
