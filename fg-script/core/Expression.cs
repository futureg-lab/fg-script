﻿namespace fg_script.core
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
        VAR_CALL,
        GENERIC
    }

    public class Expr : INode
    {
        public ExprType Type { get; } = ExprType.GENERIC;
        public Expr(ExprType type)
        {
            Type = type;
        }

        public virtual T Accept<T>(IVisitor<T> visitor)
        {
            return visitor.VisitExpr(this);
        }
    }

    public class VarExpr : Expr
    {
        public Token DataType { get; }
        public Token Name { get; }
        public Expr Value { get; }

        public VarExpr(Token datatype, Token name, Expr value)
            : base(ExprType.VAR)
        {
            DataType = datatype;
            Name = name;
            Value = value;
        }

        override public T Accept<T>(IVisitor<T> visitor)
        {
            return visitor.VisitVarExpr(this);
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
        public Token Value { get; }
        public LiteralExpr(Token value)
            : base(ExprType.LITERAL)
        {
            Value = value;
        }

        override public T Accept<T>(IVisitor<T> visitor)
        {
            return visitor.VisitLiteralExpr(this);
        }
    }

    public class UnaryExpr : Expr
    {
        public Token OpSymbol { get; }
        public Expr Operand { get; }
        public UnaryExpr(Token op, Expr operand)
            : base(ExprType.UNARY)
        {
            this.OpSymbol = op;
            this.Operand = operand;
        }

        override public T Accept<T>(IVisitor<T> visitor)
        {
            return visitor.VisitUnaryExpr(this);
        }
    }

    public class BinaryExpr : Expr
    {
        public Token OpSymbol { get; }
        public Expr Left { get; }
        public Expr Right { get; }
        public BinaryExpr(Token op, Expr left, Expr right)
            : base(ExprType.BINARY)
        {
            this.OpSymbol = op;
            this.Left = left;
            this.Right = right;
        }
        override public T Accept<T>(IVisitor<T> visitor)
        {
            return visitor.VisitBinaryExpr(this);
        }
    }

    public class FuncCall : Expr
    {
        public Token Callee { get;  }
        public List<Expr> Args { get; }
        public FuncCall(Token callee, List<Expr> args)
            : base(ExprType.FUNC_CALL)
        {
            Callee = callee;
            Args = args;
        }
        override public T Accept<T>(IVisitor<T> visitor)
        {
            return visitor.VisitFuncCall(this);
        }
    }

    public class VarCall : Expr
    {
        public Token Callee { get; }
        public VarCall(Token callee)
            : base(ExprType.VAR_CALL)
        {
            Callee = callee;
        }
        override public T Accept<T>(IVisitor<T> visitor)
        {
            return visitor.VisitVarCall(this);
        }
    }
}
