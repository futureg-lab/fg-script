using System.Text.RegularExpressions;

namespace fg_script.core
{
    public enum TokenType
    {
        EOF = -1,
        KEYWORD_OR_NAME, // alphanum | _
        UNKNOWN,

        // operators
        ASSIGN,             // =
        PLUS,               // +
        MINUS,              // -
        DIV,                // /
        MULT,               // *
        MOD,                // %
        LT,                 // <
        GT,                 // >
        NOT,                // ! or not
        COMMA,              // ,
        SEMICOLUMN,         // ;
        DOT,                // .

        COMMENT,            // //
        
        EQ,                 // ==
        NEQ,                // !=
        GE,                 // >=
        LE,                 // <=
        RET_OP,             // ->
        DBL_DOT,            // x..y == (x, x+1, ..., y)
        NEW_LINE,           // \n

        // enclosures
        LEFT_PARENTH,       // (
        RIGHT_PARENTH,      // )
        LEFT_BRACE,         // {
        RIGHT_BRACE,        // }
        LEFT_BRACKET,       // [
        RIGHT_BRACKET,      // ]


        // LITERALS
        STRING,             // "any string"
        NUMBER,             // 1235.6
        BOOL,               // false | true
        TUPLE,              // ( a, b, c, d, ...)
        NULL,               // null keyword

        // keywords
        EXTERN,             // extern
        EXPOSE,             // expose
        DEFINE,             // define (for custom tuples)
        FUN_DECL,           // fn
        TYPE,               // bool, tup, num
        IN,                 // (i, val) in (1, 2, 3, 4..)
        IS,                 // x is y -> bool
        OR,                 // x or y -> bool
        AND,                // x and y -> bool

        IF,                 // if
        ELSE,               // else
        ELSE_IF,            // elif
        FOR,                // for
        WHILE,              // while
        ERROR,              // err
        RETURN,             // ret
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
        public string Lexeme { get; set; }

        public Token(string lexeme, TokenType type, CursorPosition cursor)
        {
            Lexeme = lexeme;
            Type = type;
            Cursor = cursor;
        }

        public override string ToString()
        {
            return String.Format("[type = {0}, lexeme = {1}, position {2}]", Type, Lexeme, Cursor);
        }
    }
}
