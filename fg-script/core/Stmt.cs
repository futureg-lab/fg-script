﻿using System;
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
        public List<ArgExpr> Args { get; }
        public Block Body { get; }

        public Func(Token name, List<ArgExpr> args, Block body)
        {
            Name = name;
            Args = args;
            Body = body;
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
        override public T Accept<T>(IVisitor<T> vistor)
        {
            return vistor.VisitReturn(this);
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
        override public T Accept<T>(IVisitor<T> vistor)
        {
            return vistor.VisitExpose(this);
        }
    }

    public class Extern : Stmt
    {
        override public T Accept<T>(IVisitor<T> vistor)
        {
            return vistor.VisitExtern(this);
        }
    }
}
