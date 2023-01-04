using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fg_script.core
{
    public class PrintVisitor : IVisitor<string>
    {
        private int Depth { get; set; } = 0;

        public string Print(Stmt stmt)
        {
            return stmt.Accept(this);
        }

        public string Print(Expr expr)
        {
            return expr.Accept(this);
        }

        public string VisitAssign(Assign stmt)
        {
            return VisitVarExpr(stmt.Variable);
        }

        public string VisitBinaryExpr(BinaryExpr expr)
        {
            string op = expr.OpSymbol.Lexeme;
            string left = Print(expr.Left);
            string right = Print(expr.Right);
            return string.Format("({0} {1} {2})", op, left, right);
        }

        public string VisitUnaryExpr(UnaryExpr expr)
        {
            string op = expr.OpSymbol.Lexeme;
            string left = Print(expr.Operand);
            return string.Format("({0} {1})", op, left);
        }

        public string VisitVarCall(VarCall expr)
        {
            return string.Format("{0}", expr.Callee.Lexeme);
        }

        public string VisitVarExpr(VarExpr expr)
        {
            string name = expr.Name.Lexeme;
            string datatype = expr.DataType.Lexeme;
            Expr value = expr.Value;
            return string.Format("({0}:{1} => {2})", datatype, name, Print(value));
        }

        public string VisitArgExpr(ArgExpr expr)
        {
            string name = expr.Name.Lexeme;
            string datatype = expr.DataType.Lexeme;
            return string.Format("{0}:{1}", datatype, name);
        }

        public string VisitFuncCall(FuncCall expr)
        {
            string callee = expr.Callee.Lexeme;
            List<string> args = new();
            foreach(var arg in expr.Args)
            {
                args.Add(Print(arg));
            }
            string all_args = string.Join(", ", args);
            return string.Format("{0}({1})", callee, all_args);
        }

        public string VisitLiteralExpr(LiteralExpr expr)
        {
            return expr.Value.Lexeme;
        }

        public string VisitBlock(Block stmt)
        {
            Depth++;

            string indent = "";
            for (int i = 0; i < Depth; i++, indent += "  ");

            string all = indent + "block:\n";
            List<string> lines = stmt
                .Statements
                .ConvertAll<string>(item => indent + indent + Print(item));

            all += string.Join("\n", lines);

            Depth--;
            return all;
        }

        public string VisitFuncCallDirect(FuncCallDirect stmt)
        {
            return string.Format("(#root_call {0})", Print(stmt.Fcall));
        }

        public string VisitBreak(Break stmt)
        {
            throw new NotImplementedException();
        }

        public string VisitContinue(Continue stmt)
        {
            throw new NotImplementedException();
        }

        public string VisitDefine(Define stmt)
        {
            throw new NotImplementedException();
        }

        public string VisitExpose(Expose stmt)
        {
            throw new NotImplementedException();
        }

        public string VisitExpr(Expr expr)
        {
            throw new NotImplementedException();
        }

        public string VisitExtern(Extern stmt)
        {
            throw new NotImplementedException();
        }

        public string VisitFor(For stmt)
        {
            throw new NotImplementedException();
        }

        public string VisitFunc(Func stmt)
        {
            string name = stmt.Name.Lexeme;
            string ret_type = stmt.ReturnType.Lexeme;
            List<string> args = new();
            foreach (var arg in stmt.Args)
            {
                args.Add(Print(arg));
            }
            string all_args = string.Join(", ", args);
            return string.Format("(#declare {0} ({1}) -> {2})\n{3}", name, all_args, ret_type, Print(stmt.Body));
        }


        public string VisitIf(If stmt)
        {
            throw new NotImplementedException();
        }

        public string VisitReturn(Return stmt)
        {
            Expr returned = stmt.ReturnValue;
            return string.Format("(#return {0})", Print(returned));
        }

        public string VisitError(Error stmt)
        {
            Expr thrown = stmt.ThrownValue;
            return string.Format("(#error {0})", Print(thrown));
        }

        public string VisitStmt(Stmt stmt)
        {
            throw new NotImplementedException();
        }

        public string VisitWhile(While stmt)
        {
            throw new NotImplementedException();
        }
    }
}
