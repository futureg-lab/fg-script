
namespace fg_script.core
{
    public class PrintVisitor : IVisitorSTmt<string>, IVisitorExpr<string>
    {
        private int Depth { get; set; } = 0;

        private string Indent(string text)
        {
            string indent = "";
            for (int i = 0; i < Depth; i++, indent += "    ") ;
            return indent + text;
        }

        public string Print(Stmt? stmt)
        {
            if (stmt == null)
                return "";
            else
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

        public string VisitEnumExpr(EnumExpr expr)
        {
            return string.Format("(#enum_from {0} #to {1})", Print(expr.Start), Print(expr.End));
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
                args.Add(Print(arg));
            string all_args = string.Join(", ", args);
            return string.Format("{0}({1})", callee, all_args);
        }

        public string VisitLiteralExpr(LiteralExpr expr)
        {
            return expr.Value.Lexeme;
        }
        public string VisitTupleExpr(TupleExpr expr)
        {
            List<string> list = new();
            foreach(var item in expr.Map)
                list.Add(item.Key + ":" + Print(item.Value));
            string list_str = string.Join(", ", list);
            return String.Format("[{0}]", list_str);
        }

        public string VisitArrayAccessCall(ArrayAccessCall expr)
        {
            List<string> all = expr.Indexes.ConvertAll(x => Print(x));
            return string.Format("{0}[{1}]", expr.Callee.Lexeme, string.Join(", ", all));
        }

        public string VisitBlock(Block stmt)
        {
            Depth++;

            string all = "";
            List<string> lines = stmt
                .Statements
                .ConvertAll<string>(item => Indent(Print(item)));
            all += string.Join("\n", lines);

            Depth--;
            return all;
        }

        public string VisitRootExpression(RootExpression stmt)
        {
            return string.Format("(#root {0})", Print(stmt.Expr));
        }

        public string VisitBreak(Break stmt)
        {
            return "(#break)";
        }

        public string VisitContinue(Continue stmt)
        {
            return "(#continue)";
        }

        public string VisitExpose(Expose stmt)
        {
            return string.Format("(#expose {0})", Print(stmt.ExposedFunc));
        }

        public string VisitExpr(Expr expr)
        {
            throw new NotImplementedException();
        }

        public string VisitExtern(Extern stmt)
        {
            return string.Format("(#extern {0})", Print(stmt.ExternalFunc));
        }

        public string VisitFunc(Func stmt)
        {
            string name = stmt.Name.Lexeme;
            string ret_type = stmt.ReturnType.Lexeme;
            List<string> args = new();
            foreach (var arg in stmt.Args)
                args.Add(Print(arg));
            string all_args = string.Join(", ", args);
            return string.Format("(#declare {0} ({1}) -> {2}\n{3})", name, all_args, ret_type, Print(stmt.Body));
        }

        public string VisitIf(If stmt)
        {
            string if_str = "\n" + Print(stmt.IfBody);
            string else_str = "";
            if (stmt.ElseBody != null)
            {
                string tmp = string.Format("(#else \n{0})", Print(stmt.ElseBody));
                else_str = "\n" + Indent(tmp);
            }

            List<string> branch_strs = stmt
                .Branches
                .ConvertAll(branch => "\n" + Indent(Print(branch)));

            string branch_str = string.Join("", branch_strs);
            string cond_str = Print(stmt.IfCondition);
            return string.Format("(#if {0} => {1}{2}{3})", cond_str, if_str, branch_str, else_str);
        }

        public string VisitBranch(Branch stmt)
        {
            string cond_str = Print(stmt.Condition);
            string body_str = Print(stmt.Body);
            return string.Format("(#branch {0} => \n{1})", cond_str, body_str);
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
            return Print(stmt);
        }
        public string VisitFor(For stmt)
        {
            string key = "";
            if (stmt.KeyAlias != null)
                key = stmt.KeyAlias.Lexeme;
            string value = stmt.ValueAlias.Lexeme;
            string expr = Print(stmt.ExprIterable);
            string body = Print(stmt.Body);
            return string.Format("(#for ({0}, {1}) #in {2}\n{3})", key, value, expr, body);
        }

        public string VisitWhile(While stmt)
        {
            return string.Format("(#while {0}\n{1})", Print(stmt.Condition), Print(stmt.Body)); 
        }

        public string VisitReAssign(ReAssign stmt)
        {
            return String.Format("(#reassign {0} => {1})", stmt.Callee.Lexeme, Print(stmt.NewValue));
        }

        public string VisitReAssignArray(ReAssignArray stmt)
        {
            List<string> all = stmt.Indexes.ConvertAll(x => Print(x));
            return String.Format("(#reassign_arr {0}[{1}] => {2})", stmt.Callee.Lexeme, string.Join(", ", all), Print(stmt.NewValue));
        }
    }
}
