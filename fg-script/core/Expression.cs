namespace fg_script.core
{
    public enum ExprType
    {
        PROGRAM,
        UNARY,
        BINARY,
        LITERAL,
        VAR,
        ARG,
        FUNC_CALL,
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

    public class VarExpr : Expr
    {
        public string DataType { get; }
        public string Name { get; }
        public Expr Value { get; }

        public VarExpr(string datatype, string name, Expr value)
            : base(ExprType.VAR)
        {
            DataType = datatype;
            Name = name;
            Value = value;
        }
    }

    public class ArgExpr : Expr
    {
        public string DataType { get; }
        public string Name { get; }

        public ArgExpr(string datatype, string name)
            : base(ExprType.ARG)
        {
            DataType = datatype;
            Name = name;
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
        public string OpSymbol { get; }
        public Expr Operand { get; }
        public UnaryExpr(string op, Expr operand)
            : base(ExprType.UNARY)
        {
            this.OpSymbol = op;
            this.Operand = operand;
        }
    }

    public class BinaryExpr : Expr
    {
        public string OpSymbol { get; }
        public Expr? Left { get; }
        public Expr? Right { get; }
        public BinaryExpr(string op, Expr left, Expr right)
            : base(ExprType.BINARY)
        {
            this.OpSymbol = op;
            this.Left = left;
            this.Right = right;
        }
    }

    public class FuncCall : Expr
    {
        public string Callee { get;  }
        public List<Expr> Args { get; }
        public FuncCall(string callee, List<Expr> args)
            : base(ExprType.FUNC_CALL)
        {
            Callee = callee;
            Args = args;
        }
    }
}
