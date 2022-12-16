namespace fg_script.core
{
    public enum ExprType
    {
        UNARY,
        BINARY,
        LITERAL,
        GENERIC
    }

    public class Expr
    {
        public ExprType Type { get; } = ExprType.GENERIC;
        public Expr(ExprType type)
        {
            Type = type;
        }
    }

    public class LiteralExpr : Expr
    {
        public string Value { get; }
        public LiteralExpr(string value)
            : base(ExprType.LITERAL)
        {
            Value = value;
        }
    }

    public class UnaryExpr : Expr
    {
        public Expr Operand { get; }
        public UnaryExpr(Expr operand)
            : base(ExprType.UNARY)
        {
            this.Operand = operand;
        }
    }

    public class BinaryExpr : Expr
    {
        public Expr? Left { get;  }
        public Expr? Right { get; }
        public BinaryExpr(Expr left, Expr right)
            : base(ExprType.BINARY)
        {
            this.Left = left;
            this.Right = right;
        }
    }
}
