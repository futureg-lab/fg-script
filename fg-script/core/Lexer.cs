namespace fg_script.core
{
    public class Lexer
    {
        protected char? CurrentChar { get; set; } = null;
        protected string Source { get; set; } = "";
        protected string Filepath { get; set; } = "";
        protected CursorPosition Cursor = new(1, 0, -1);

        private Dictionary<string, TokenType> ReservedWords = new();
        private Dictionary<string, TokenType> ReservedSymbols = new();
        private Dictionary<string, TokenType> EncloseSymbols = new();

        public Lexer(string source, string filepath = "<console>")
        {
            Source = source;
            Filepath = filepath;
            InitReservedWords();
            InitReservedSymbols();
            InitEnclosedSymbols();
        }

        private void InitReservedWords()
        {
            ReservedWords.Add("fn", TokenType.FUN_DECL);
            ReservedWords.Add("extern", TokenType.EXTERN);
            ReservedWords.Add("expose", TokenType.EXPOSE);
            ReservedWords.Add("define", TokenType.DEFINE);
            ReservedWords.Add("false", TokenType.BOOL);
            ReservedWords.Add("true", TokenType.BOOL);
            ReservedWords.Add("null", TokenType.NULL);

            // branching, loops
            ReservedWords.Add("for", TokenType.FOR);
            ReservedWords.Add("while", TokenType.WHILE);
            ReservedWords.Add("if", TokenType.IF);
            ReservedWords.Add("elif", TokenType.ELSE_IF);
            ReservedWords.Add("else", TokenType.ELSE);

            // types
            ReservedWords.Add("bool", TokenType.TYPE);
            ReservedWords.Add("num", TokenType.TYPE);
            ReservedWords.Add("str", TokenType.TYPE);
            ReservedWords.Add("tup", TokenType.TYPE);

            // other
            ReservedWords.Add("is", TokenType.IS);
            ReservedWords.Add("and", TokenType.AND);
            ReservedWords.Add("or", TokenType.OR);
            ReservedWords.Add("not", TokenType.NOT);
            ReservedWords.Add("ret", TokenType.RETURN);
            ReservedWords.Add("err", TokenType.ERROR);
        }

        private void InitReservedSymbols()
        {
            // 1 char
            ReservedSymbols.Add("=", TokenType.ASSIGN);

            ReservedSymbols.Add("+", TokenType.PLUS);
            ReservedSymbols.Add("-", TokenType.MINUS);
            ReservedSymbols.Add("*", TokenType.MULT);
            ReservedSymbols.Add("/", TokenType.DIV);

            ReservedSymbols.Add("%", TokenType.MOD);
            ReservedSymbols.Add(">", TokenType.GT);
            ReservedSymbols.Add("<", TokenType.LT);
            ReservedSymbols.Add("!", TokenType.NOT);

            ReservedSymbols.Add(",", TokenType.COMMA);
            ReservedSymbols.Add(".", TokenType.DOT);
            ReservedSymbols.Add(";", TokenType.SEMICOLUMN);


            // 2 chars
            ReservedSymbols.Add("==", TokenType.EQ);
            ReservedSymbols.Add("!=", TokenType.NEQ);
            ReservedSymbols.Add(">=", TokenType.GE);
            ReservedSymbols.Add("<=", TokenType.LE);
            ReservedSymbols.Add("->", TokenType.RET_OP);
            ReservedSymbols.Add("..", TokenType.DBL_DOT);
        }

        private void InitEnclosedSymbols()
        {
            EncloseSymbols.Add("(", TokenType.LEFT_PARENTH);
            EncloseSymbols.Add(")", TokenType.RIGHT_PARENTH);
            EncloseSymbols.Add("{", TokenType.LEFT_BRACE);
            EncloseSymbols.Add("}", TokenType.RIGHT_BRACE);
            EncloseSymbols.Add("[", TokenType.LEFT_BRACKET);
            EncloseSymbols.Add("]", TokenType.RIGHT_BRACKET);
        }

        public List<Token> Tokenize()
        {
            List<Token> tokens = new();
            NextChar(); // -1 + 1 = 0
            while (!HasEnded())
            {                
                if (IsSpaceOrTab(CurrentChar))
                {
                    NextChar();
                    continue;
                }
                
                if (IsNewLine(CurrentChar))
                {
                    string str = "\\n";
                    // windows
                    if (CurrentChar == '\r' && PeekNextChar() == '\n')
                    {
                        str = "\\r" + str;
                        NextChar(); // skip next \n
                    }
                    tokens.Add(new(str, TokenType.NEW_LINE, Cursor.Copy()));
                    
                    // update cursor position
                    Cursor.Col = 0;
                    Cursor.Line++;

                    NextChar();
                    continue;
                }

                if (CurrentChar == '/' && (PeekNextChar() == '/' || PeekNextChar() == '*'))
                {
                    string str = PeekNextChar() == '*' ? MakeMultiLineComment() : MakeComment();
                    tokens.Add(new(str, TokenType.COMMENT, Cursor.Copy()));
                    // nextchar is handled in MakeComment | MakeMultiLineComment
                    continue;
                }

                string lexeme = "" + CurrentChar;
                if (lexeme.StartsWith("\""))
                {
                    string str = MakeString();
                    tokens.Add(new(str, TokenType.STRING, Cursor.Copy()));
                    // nextchar is handled in MakeString
                    continue;
                }

                // has to be alphanum + _
                if (IsNum(CurrentChar))
                {
                    string str = MakeNumber();
                    tokens.Add(new(str, TokenType.NUMBER, Cursor.Copy()));
                    // nextchar is handled in MakeNumber
                    continue;
                }

                if (IsAlpha(CurrentChar))
                {
                    string str = MakeStandardExpression();
                    TokenType type = TokenType.KEYWORD_OR_NAME;
                    if (ReservedWords.ContainsKey(str))
                    {
                        type = ReservedWords[str];
                    }
                    tokens.Add(new(str, type, Cursor.Copy()));
                    // nextchar is handled in MakeStandardExpression
                    continue;
                }

                // +, -, *, /, =, ..., >=, !, <, ..
                if (IsStandardSymbol(CurrentChar))
                {
                    string str = MakeStandardSymbols();
                    TokenType type = TokenType.KEYWORD_OR_NAME;
                    if (ReservedSymbols.ContainsKey(str))
                    {
                        type = ReservedSymbols[str];
                    }
                    tokens.Add(new(str, type, Cursor.Copy()));
                    // nextchar is handled in MakeStandardSymbols
                    continue;
                }

                // {, }, [, ], ( and ) 
                if (IsEncloseSymbol(CurrentChar) && EncloseSymbols.ContainsKey(lexeme))
                {
                    TokenType type = EncloseSymbols[lexeme];
                    tokens.Add(new(lexeme, type, Cursor.Copy()));
                    NextChar();
                    continue;
                }

                tokens.Add(new(lexeme, TokenType.UNKNOWN, Cursor.Copy()));
                NextChar();
            }
            tokens.Add(new("EOF", TokenType.EOF, Cursor.Copy()));
            return tokens;
        }

        protected string MakeComment()
        {
            string str = "//";
            NextChar();
            NextChar();
            while (!HasEnded() && !IsNewLine(CurrentChar))
            {
                str += CurrentChar;
                NextChar();
            }
            return str;
        }

        // Does not allow multiline comment nesting
        protected string MakeMultiLineComment()
        {
            string str = "/*";
            NextChar();
            NextChar();
            while ( !(CurrentChar == '*' && PeekNextChar() == '/') )
            {
                str += CurrentChar;
                NextChar();
                if (HasEnded())
                    throw new SyntaxErrorException("interminated comment", Cursor.Copy(), Filepath);
            }
            str += "*/";
            NextChar();
            NextChar();
            return str;
        }

        // standard string : "hello world!"
        protected string MakeString()
        {
            string str = "";
            NextChar(); // ignore first '"'
            while (CurrentChar != '"')
            {
                str += CurrentChar;
                NextChar();
                if (HasEnded())
                    throw new SyntaxErrorException("interminated string", Cursor.Copy(), Filepath);
            }
            NextChar(); // ignore '"'
            return string.Format("\"{0}\"", str);
        }

        // [0-9]+
        protected string MakeNumber()
        {
            string str = "";
            bool dotEncountered = false;
            while (IsNum(CurrentChar) || CurrentChar == '.')
            {
                if (CurrentChar == '.')
                {
                    if (dotEncountered)
                        throw new SyntaxErrorException("invalid number", Cursor.Copy(), Filepath);
                    dotEncountered = true;
                }
                str += CurrentChar;
                NextChar();
            }
            return str;
        }

        protected string MakeStandardSymbols()
        {
            string str = "";
            while (IsStandardSymbol(CurrentChar))
            {
                str += CurrentChar;
                NextChar();
            }
            return str;
        }

        // [a-zA-Z0-9_]+
        protected string MakeStandardExpression()
        {
            string str = "";
            while (IsStandardExpression(CurrentChar))
            {
                str += CurrentChar;
                NextChar();
            }
            return str;
        }

        // new line
        public static bool IsNewLine(char? character)
        {
            return character == '\n' || character == '\r';
        }

        public static bool IsSpaceOrTab(char? character)
        {
            return character == ' ' || character == '\t';
        }

        // [0-9]
        public static bool IsNum(char? character)
        {
            return character >= '0' && character <= '9';
        }

        // [a-zA-Z]
        public static bool IsAlpha(char? character)
        {
            return character >= 'a' && character <= 'z'
                || character >= 'A' && character <= 'Z';
        }

        public static bool IsAlphaNum(char? character)
        {
            return IsAlpha(character) || IsNum(character);
        }

        // 
        public static bool IsStandardExpression(char? character)
        {
            return IsAlpha(character) || character == '_' || IsNum(character);
        }

        public static bool IsStandardSymbol(char? character)
        {
            if (character == null)
                return false;

            string symbols = "=+-*/%><!,.;";
            return symbols.Contains((char) character);
        }

        public static bool IsEncloseSymbol(char? character)
        {
            if (character == null)
                return false;

            string list = "(){}[]";
            return list.Contains((char) character);
        }

        public void NextChar()
        {
            Cursor.Pos++;
            if (Cursor.Pos >= Source.Length)
                CurrentChar = null;
            else
                CurrentChar = Source[Cursor.Pos];
            Cursor.Col++;
        }

        public char? PeekNextChar()
        {
            return Cursor.Pos + 1 >= Source.Length ? null : Source[Cursor.Pos + 1];
        }

        public bool HasEnded()
        {
            return CurrentChar == null;
        }
    }
}
