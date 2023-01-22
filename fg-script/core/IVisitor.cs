using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fg_script.core
{
    public interface IVisitorSTmt<T>
    {
        // Statements
        public T VisitStmt(Stmt stmt);
        public T VisitBlock(Block stmt);
        public T VisitFunc(Func stmt);
        public T VisitIf(If stmt);
        public T VisitBranch(Branch stmt);
        public T VisitFor(For stmt);
        public T VisitWhile(While stmt);
        public T VisitAssign(Assign stmt);
        public T VisitReturn(Return stmt);

        public T VisitError(Error stmt);
        public T VisitBreak(Break stmt);
        public T VisitContinue(Continue stmt);
        public T VisitExpose(Expose stmt);
        public T VisitExtern(Extern stmt);
        public T VisitRootExpression(RootExpression stmt);
        public T VisitReAssign(ReAssign stmt);
        public T VisitReAssignTupleIndex(ReAssignTuple stmt);
    }

    public interface IVisitorExpr<T>
    {
        // Expressions
        public T VisitExpr(Expr expr);
        public T VisitVarExpr(VarExpr expr);
        public T VisitEnumExpr(EnumExpr expr);
        public T VisitArgExpr(ArgExpr expr);
        public T VisitLiteralExpr(LiteralExpr expr);
        public T VisitUnaryExpr(UnaryExpr expr);
        public T VisitTupleExpr(TupleExpr expr);
        public T VisitBinaryExpr(BinaryExpr expr);
        public T VisitFuncCall(FuncCall expr);
        public T VisitVarCall(VarCall expr);
        public T VisitTupleIndexAccessCall(TupleIndexAccessCall expr);
    }

    public interface INodeStmt
    {
        public T Accept<T>(IVisitorSTmt<T> vistor);
    }

    public interface INodeExpr
    {
        public T Accept<T>(IVisitorExpr<T> vistor);
    }
}
