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
        List<Stmt> Statements { get; } = new();

        override public T Accept<T>(IVisitor<T> vistor)
        {
            return vistor.VisitBlock(this);
        }

        public void Add(Stmt stmt)
        {
            Statements.Add(stmt);
        }

        public override string ToString()
        {
            return string.Format("Block [stmts_count = {0}]", Statements.Count);
        }
    }

    public class Func : Stmt
    {
        public string Name { get; }
        public List<ArgExpr> Args { get; }
        public Block Body { get; }

        public Func(string name, List<ArgExpr> args, Block body)
        {
            Name = name;
            Args = args;
            Body = body;
        }

        override public T Accept<T>(IVisitor<T> vistor)
        {
            return vistor.VisitFunc(this);
        }

        public override string ToString()
        {
            return string.Format("Func [name = {0}, argc = {1}]", Name, Args.Count);
        }
    }

    public class If : Stmt
    {
        override public T Accept<T>(IVisitor<T> vistor)
        {
            return vistor.VisitIf(this);
        }

        public override string ToString()
        {
            return "If";
        }
    }

    public class For : Stmt
    {
        override public T Accept<T>(IVisitor<T> vistor)
        {
            return vistor.VisitFor(this);
        }
        public override string ToString()
        {
            return "For";
        }
    }

    public class While : Stmt
    {
        override public T Accept<T>(IVisitor<T> vistor)
        {
            return vistor.VisitWhile(this);
        }
        public override string ToString()
        {
            return "While";
        }
    }

    public class Assign : Stmt
    {
        override public T Accept<T>(IVisitor<T> vistor)
        {
            return vistor.VisitAssign(this);
        }
        public override string ToString()
        {
            return "Assign";
        }
    }

    public class Return : Stmt
    {
        override public T Accept<T>(IVisitor<T> vistor)
        {
            return vistor.VisitReturn(this);
        }
        public override string ToString()
        {
            return "Return [<todo_literal_type>]";
        }
    }

    public class Break : Stmt
    {
        override public T Accept<T>(IVisitor<T> vistor)
        {
            return vistor.VisitBreak(this);
        }
        public override string ToString()
        {
            return "Break";
        }
    }

    public class Continue : Stmt
    {
        override public T Accept<T>(IVisitor<T> vistor)
        {
            return vistor.VisitContinue(this);
        }
        public override string ToString()
        {
            return "If";
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
        override public T Accept<T>(IVisitor<T> vistor)
        {
            return vistor.VisitExpose(this);
        }
        public override string ToString()
        {
            return "Expose [<todo_func_name>]";
        }
    }

    public class Extern : Stmt
    {
        override public T Accept<T>(IVisitor<T> vistor)
        {
            return vistor.VisitExtern(this);
        }
        public override string ToString()
        {
            return "Extern [<todo_func_name>]";
        }
    }
}
