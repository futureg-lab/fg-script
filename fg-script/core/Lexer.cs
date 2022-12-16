﻿namespace fg_script.core
{
    public class Lexer
    {
        protected char? CurrentChar { get; set; } = null;
        protected string Source { get; set; } = "";
        protected string Filepath { get; set; } = "";
        protected CursorPosition Cursor = new(0, 0, -1);

        public List<Token> Tokenize()
        {
            List<Token> tokens = new();
            NextChar(); // -1 + 1 = 0
            while (!HasEnded())
            {
                string lexeme = "" + CurrentChar;
                if (lexeme == " ")
                {
                    NextChar();
                    continue;
                }

                if (lexeme.StartsWith("\""))
                {
                    string str = MakeString();
                    Token token = new(str, TokenType.STRING, Cursor.Copy());
                    tokens.Add(token);
                    continue;
                }

                // has to be alphanum + _
                if (IsAlpha(CurrentChar))
                {
                    string str = MakeStandardExpression();
                    Token token = new(str, TokenType.KEYWORD_OR_NAME, Cursor.Copy());
                    tokens.Add(token);
                    continue;
                }

                if (IsNum(CurrentChar))
                {
                    string str = MakeNumber();
                    Token token = new(str, TokenType.NUMBER, Cursor.Copy());
                    tokens.Add(token);
                    continue;
                }

                {
                    Token token = new(lexeme, TokenType.UNKNOWN, Cursor.Copy());
                    tokens.Add(token);
                    NextChar();
                }
            }
            tokens.Add(new("EOF", TokenType.EOF, Cursor.Copy()));
            return tokens;
        }

        //
        // FGScript specific token eaters
        //


        //
        // Basic token eaters
        //

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
            return str;
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

        public Lexer(string source, string filepath = "<console>")
        {
            Source = source;
            Filepath = filepath;
        }

        // [0-9]
        public static bool IsNum(char? character)
        {
            if (character == null)
                return false;
            return character >= '0' && character <= '9';
        }

        // [a-zA-Z]
        public static bool IsAlpha(char? character)
        {
            if (character == null)
                return false;
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

        public void NextChar()
        {
            Cursor.Pos++;
            if (Cursor.Pos >= Source.Length)
                CurrentChar = null;
            else
                CurrentChar = Source[Cursor.Pos];

            if (CurrentChar == '\n')
            {
                Cursor.Line++;
                Cursor.Col = 0;
                NextChar();
            }
            else
            {
                Cursor.Col++;
            }
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