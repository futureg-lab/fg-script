namespace fg_script.core
{
    public enum TokenType
    {
        EOF = -1,
        KEYWORD_OR_NAME, // alphanum | _
        UNKNOWN,
        // operators
        EQUAL,
        PLUS,
        MINUS,
        DIV,
        MULT,
        MOD,

        // types
        STRING,
        NUMBER,
        BOOL
    }

    public class CursorPosition
    {
        public int Line { get; set; }
        public int Col { get; set; }
        public int Pos { get; set; }
        public CursorPosition(int line, int col, int pos)
        {
            Line = line;
            Col = col;
            Pos = pos;
        }
        public CursorPosition Copy()
        {
            return new(Line, Col, Pos);
        }

        public override string ToString()
        {
            return String.Format("line {0}, col {1} (pos {2})", Line, Col, Pos);
        }
    }

    public class Token
    {
        public CursorPosition Cursor { get; set; } = new(0, 0, -1);
        // type id
        public TokenType Type { get; set; } = TokenType.UNKNOWN;
        // string from source
        public string? Lexeme { get; set; } = null;

        public Token(string lexeme, TokenType type, CursorPosition cursor)
        {
            Lexeme = lexeme;
            Type = type;
            Cursor = cursor;
        }

        public override string ToString()
        {
            return String.Format("[type = {0}, lexeme = \"{1}\", position {2}]", Type, Lexeme, Cursor);
        }
    }
}
