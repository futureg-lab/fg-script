namespace fg_script.core
{
    public class Expr : INodeExpr
    {
        public virtual T Accept<T>(IVisitorExpr<T> visitor)
        {
            return visitor.VisitExpr(this);
        }
    }

    public class EnumExpr : Expr
    {
        public Expr Start { get;  }
        public Expr End { get;  }
        public EnumExpr(Expr start, Expr end)
        {
            Start = start;
            End = end;
        }
        override public T Accept<T>(IVisitorExpr<T> visitor)
        {
            return visitor.VisitEnumExpr(this);
        }
    }

    public class VarExpr : Expr
    {
        public Token DataType { get; }
        public Token Name { get; }
        public Expr Value { get; }

        public VarExpr(Token datatype, Token name, Expr value)
        {
            DataType = datatype;
            Name = name;
            Value = value;
        }

        override public T Accept<T>(IVisitorExpr<T> visitor)
        {
            return visitor.VisitVarExpr(this);
        }
    }

    public class ArgExpr : Expr
    {
        public Token DataType { get; }
        public Token Name { get; }

        public ArgExpr(Token datatype, Token name)
        {
            DataType = datatype;
            Name = name;
        }
        override public T Accept<T>(IVisitorExpr<T> visitor)
        {
            return visitor.VisitArgExpr(this);
        }
    }

    public class LiteralExpr : Expr
    {
        public Token Value { get; }
        public LiteralExpr(Token value)
        {
            Value = value;
        }

        override public T Accept<T>(IVisitorExpr<T> visitor)
        {
            return visitor.VisitLiteralExpr(this);
        }
    }

    public class TupleExpr : Expr
    {
        private int Count { get; set; } = 0;
        public Dictionary<string, Expr> Map { get; }
        public TupleExpr() 
        {
            Map = new Dictionary<string, Expr>();
        }

        public void Set(string key, Expr value)
        {
            Map.Add(key, value);
            Count++;
        }

        public void Append(Expr value)
        {
            Map.Add(Count.ToString(), value);
            Count++;
        }
        override public T Accept<T>(IVisitorExpr<T> visitor)
        {
            return visitor.VisitTupleExpr(this);
        }
    }


    public class UnaryExpr : Expr
    {
        public Token OpSymbol { get; }
        public Expr Operand { get; }
        public UnaryExpr(Token op, Expr operand)
        {
            this.OpSymbol = op;
            this.Operand = operand;
        }

        override public T Accept<T>(IVisitorExpr<T> visitor)
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
        {
            this.OpSymbol = op;
            this.Left = left;
            this.Right = right;
        }
        override public T Accept<T>(IVisitorExpr<T> visitor)
        {
            return visitor.VisitBinaryExpr(this);
        }
    }

    public class FuncCall : Expr
    {
        public Token Callee { get;  }
        public List<Expr> Args { get; }
        public FuncCall(Token callee, List<Expr> args)
        {
            Callee = callee;
            Args = args;
        }
        override public T Accept<T>(IVisitorExpr<T> visitor)
        {
            return visitor.VisitFuncCall(this);
        }
    }

    public class VarCall : Expr
    {
        public Token Callee { get; }
        public VarCall(Token callee)
        {
            Callee = callee;
        }
        override public T Accept<T>(IVisitorExpr<T> visitor)
        {
            return visitor.VisitVarCall(this);
        }
    }

    public class ArrayAccessCall : Expr
    {
        public Token Callee { get; }
        public Expr Index { get; }
        public ArrayAccessCall(Token callee, Expr index)
        {
            Callee = callee;
            Index = index;
        }
        override public T Accept<T>(IVisitorExpr<T> visitor)
        {
            return visitor.VisitArrayAccessCall(this);
        }
    }
}
