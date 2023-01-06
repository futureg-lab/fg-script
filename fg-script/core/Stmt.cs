using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fg_script.core
{
    public class Stmt : INode
    {
        public virtual T Accept<T>(IVisitor<T> vistor)
        {
            return vistor.VisitStmt(this);
        }
    }

    public class Block : Stmt
    {
        public List<Stmt> Statements { get; } = new();

        override public T Accept<T>(IVisitor<T> vistor)
        {
            return vistor.VisitBlock(this);
        }

        public void Add(Stmt stmt)
        {
            Statements.Add(stmt);
        }
    }

    public class Func : Stmt
    {
        public Token Name { get; }
        public Token ReturnType { get; }
        public List<ArgExpr> Args { get; }
        public Block? Body { get; set; }

        public Func(Token name, List<ArgExpr> args, Token ret_type, Block? body)
        {
            Name = name;
            Args = args;
            Body = body;
            ReturnType = ret_type;
        }

        override public T Accept<T>(IVisitor<T> vistor)
        {
            return vistor.VisitFunc(this);
        }
    }
    
    public class FuncCallDirect : Stmt
    {
        public FuncCall Fcall { get; }
        public FuncCallDirect(FuncCall fcall)
        {
            Fcall = fcall;
        }

        override public T Accept<T>(IVisitor<T> vistor)
        {
            return vistor.VisitFuncCallDirect(this);
        }
    }

    public class If : Stmt
    {
        public Expr IfCondition { get; }
        public Block IfBody { get; }
        public Block? ElseBody { get; set; }

        public List<Branch> Branches { get; }

        public If(Expr condition, Block body)
        {
            IfCondition = condition;
            IfBody = body;
            Branches = new();
        }

        override public T Accept<T>(IVisitor<T> vistor)
        {
            return vistor.VisitIf(this);
        }
    }

    public class Branch : Stmt
    {
        public Expr Condition { get; }
        public Block Body { get; }
        public Branch (Expr condition, Block body)
        {
            Condition = condition;
            Body = body;
        }

        override public T Accept<T>(IVisitor<T> vistor)
        {
            return vistor.VisitBranch(this);
        }
    }

    public class For : Stmt
    {
        override public T Accept<T>(IVisitor<T> vistor)
        {
            return vistor.VisitFor(this);
        }
    }

    public class While : Stmt
    {
        override public T Accept<T>(IVisitor<T> vistor)
        {
            return vistor.VisitWhile(this);
        }
    }

    public class Assign : Stmt
    {
        public VarExpr Variable { get;  }

        public Assign(VarExpr variable)
        {
            Variable = variable;
        }

        override public T Accept<T>(IVisitor<T> vistor)
        {
            return vistor.VisitAssign(this);
        }
    }

    public class Return : Stmt
    {
        public Expr ReturnValue { get; }

        public Return (Expr returnValue)
        {
            ReturnValue = returnValue;
        }

        override public T Accept<T>(IVisitor<T> vistor)
        {
            return vistor.VisitReturn(this);
        }
    }

    public class Error : Stmt
    {
        public Expr ThrownValue { get; }

        public Error(Expr thrown)
        {
            ThrownValue = thrown;
        }

        override public T Accept<T>(IVisitor<T> vistor)
        {
            return vistor.VisitError(this);
        }
    }

    public class Break : Stmt
    {
        override public T Accept<T>(IVisitor<T> vistor)
        {
            return vistor.VisitBreak(this);
        }
    }

    public class Continue : Stmt
    {
        override public T Accept<T>(IVisitor<T> vistor)
        {
            return vistor.VisitContinue(this);
        }
    }
    public class Define : Stmt
    {
        override public T Accept<T>(IVisitor<T> vistor)
        {
            return vistor.VisitDefine(this);
        }
    }

    public class Expose : Stmt
    {
        public Func ExposedFunc { get; }
        public Expose(Func exposedFunc)
        {
            ExposedFunc = exposedFunc;
        }

        override public T Accept<T>(IVisitor<T> vistor)
        {
            return vistor.VisitExpose(this);
        }
    }

    public class Extern : Stmt
    {
        public Func ExternalFunc { get; }
        public Extern(Func exposedFunc)
        {
            ExternalFunc = exposedFunc;
        }
        override public T Accept<T>(IVisitor<T> vistor)
        {
            return vistor.VisitExtern(this);
        }
    }
}
