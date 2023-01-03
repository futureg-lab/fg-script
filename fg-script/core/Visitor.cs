using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fg_script.core
{
    public interface IVisitor<T>
    {
        public T VisitStmt(Stmt stmt);
        public T VisitBlock(Block stmt);
        public T VisitFunc(Func stmt);
        public T VisitIf(If stmt);
        public T VisitFor(For stmt);
        public T VisitWhile(While stmt);
        public T VisitAssign(Assign stmt);
        public T VisitReturn(Return stmt);
        public T VisitBreak(Break stmt);
        public T VisitContinue(Continue stmt);
        public T VisitDefine(Define stmt);
        public T VisitExpose(Expose stmt);
        public T VisitExtern(Extern stmt);
    }

    public interface INode
    {
        public T Accept<T>(IVisitor<T> vistor);
    }
}
