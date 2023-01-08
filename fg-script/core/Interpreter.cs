namespace fg_script.core
{
    public class Interpreter : IVisitorSTmt<object?>, IVisitorExpr<Memory.Result>
    {
        public Memory Machine { get; } = new();

        public void Run(Stmt stmt)
        {
            stmt.Accept(this);
        }

        public Memory.Result Eval(Expr expr)
        {
            return expr.Accept(this);
        }

        private static string Fmt(string str, params object[] list)
        {
            return string.Format(str, list);
        }

        public static Boolean EvalBoolean(string repr)
        {
            try
            {
                // true, false, 0 or any number
                // return Boolean.Parse(repr);
                if (repr.Equals("true") || repr.Equals("false"))
                    return Boolean.Parse(repr);
                else
                    return EvalNumber(repr) == 0;
            }
            catch (Exception)
            {
                throw new FGRuntimeException(repr + "is not a boolean");
            }
        }

        // "645.6444" => 645.64444
        public static Double EvalNumber(string repr)
        {
            try
            {
                return Double.Parse(repr);
            }
            catch (Exception)
            {
                throw new FGRuntimeException(repr + " is not a number");
            }
        }

        // ""string"" => srting
        public static string EvalString(string str)
        {
            if (!str.StartsWith("\"") && str.EndsWith("\""))
                throw new FGRuntimeException(str + " is not a string");
            return str.Substring(1, str.Length - 2);
        }

        public static bool AreSameType(string raw_lexeme, ResultType reduced)
        {
            return raw_lexeme == "bool" && reduced == ResultType.BOOLEAN
                || raw_lexeme == "str" && reduced == ResultType.STRING
                || raw_lexeme == "num" && reduced == ResultType.NUMBER
                || raw_lexeme == "tup" && reduced == ResultType.TUPLE; 
        }

        public static void TypeMismatchCheck(string raw_lexeme, ResultType reduced)
        {
            if (!AreSameType(raw_lexeme, reduced))
            {
                string got;
                switch(reduced)
                {
                    case ResultType.BOOLEAN:
                        got = "bool";
                        break;
                    case ResultType.STRING:
                        got = "str";
                        break;
                    case ResultType.NUMBER:
                        got = "num";
                        break;
                    case ResultType.TUPLE:
                        got = "tup";
                        break;
                    default:
                        throw new FGRuntimeException(reduced + " is not a valid type");
                }
                throw new FGRuntimeException(Fmt("type \"{0}\" was expected, got \"{1}\" instead", raw_lexeme, got));
            }
        }

        // statements
        public object? VisitAssign(Assign stmt)
        {
            Memory.Result value = Eval(stmt.Variable.Value);

            TypeMismatchCheck(stmt.Variable.DataType.Lexeme, value.Type);

            // store it
            Machine.Store(stmt.Variable.Name.Lexeme, value);
            return null;
        }

        public object? VisitBlock(Block stmt)
        {
            Machine.MemPush(); // new scope
            
            foreach (var line in stmt.Statements)
                Run(line);

            Machine.MemPop(); // discard
            return null;
        }

        public object? VisitStmt(Stmt stmt)
        {
            Run(stmt);
            return null;
        }

        public object? VisitFunc(Func stmt)
        {
            throw new NotImplementedException();
        }

        public object? VisitIf(If stmt)
        {
            throw new NotImplementedException();
        }

        public object? VisitBranch(Branch stmt)
        {
            throw new NotImplementedException();
        }

        public object? VisitFor(For stmt)
        {
            throw new NotImplementedException();
        }

        public object? VisitWhile(While stmt)
        {
            throw new NotImplementedException();
        }

        public object? VisitReturn(Return stmt)
        {
            throw new NotImplementedException();
        }

        public object? VisitError(Error stmt)
        {
            throw new NotImplementedException();
        }

        public object? VisitBreak(Break stmt)
        {
            throw new NotImplementedException();
        }

        public object? VisitContinue(Continue stmt)
        {
            throw new NotImplementedException();
        }

        public object? VisitExpose(Expose stmt)
        {
            throw new NotImplementedException();
        }

        public object? VisitExtern(Extern stmt)
        {
            throw new NotImplementedException();
        }

        public object? VisitRootExpression(RootExpression stmt)
        {
            Eval(stmt.Expr);
            return null;
        }

        // expressions

        public Memory.Result VisitExpr(Expr expr)
        {
            throw new NotImplementedException();
        }

        public Memory.Result VisitVarExpr(VarExpr expr)
        {
            throw new NotImplementedException();
        }

        public Memory.Result VisitEnumExpr(EnumExpr expr)
        {
            throw new NotImplementedException();
        }

        public Memory.Result VisitArgExpr(ArgExpr expr)
        {
            throw new NotImplementedException();
        }

        // numbers, booleans, strings
        public Memory.Result VisitLiteralExpr(LiteralExpr expr)
        {
            object value;
            ResultType type;
            switch (expr.Value.Type)
            {
                case TokenType.NUMBER:
                    value =  EvalNumber(expr.Value.Lexeme);
                    type = ResultType.NUMBER;
                    break;
                case TokenType.STRING:
                    value = EvalString(expr.Value.Lexeme);
                    type = ResultType.STRING;
                    break;
                case TokenType.BOOL:
                    value = EvalBoolean(expr.Value.Lexeme);
                    type = ResultType.BOOLEAN;
                    break;
                default:
                    throw new FGRuntimeException("invalid literal value");
            }

            return new(value, type);
        }

        public Memory.Result VisitUnaryExpr(UnaryExpr expr)
        {
            // like a function call
            Token symbol = expr.OpSymbol;
            // reduced to a point
            Memory.Result eval_operand = Eval(expr.Operand);
            Memory.Result result;

            string not_supp_msg = Fmt("{0} is not supported by {1}", eval_operand.Type, symbol.Lexeme);

            if (symbol.Type != TokenType.MINUS && symbol.Type != TokenType.NOT)
                throw new FGRuntimeException(Fmt("\"{0}\" does not have a definition", symbol.Lexeme));

            if (eval_operand.Type == ResultType.BOOLEAN)
            {
                if (symbol.Type == TokenType.NOT)
                    result = new(!((Boolean)eval_operand.Value), eval_operand.Type);
                else
                    throw new FGRuntimeException(not_supp_msg);
            }
            else if (eval_operand.Type == ResultType.NUMBER)
            {
                if (symbol.Type == TokenType.MINUS)
                    result = new(-((Double)eval_operand.Value), eval_operand.Type);
                else
                    throw new FGRuntimeException(not_supp_msg);
            }
            else
                throw new FGRuntimeException(not_supp_msg);

            return result;
        }
        public Memory.Result VisitBinaryExpr(BinaryExpr expr)
        {
            Token symbol = expr.OpSymbol;
            Memory.Result eval_left = Eval(expr.Left);
            Memory.Result eval_right = Eval(expr.Right);

            var BothSidesAre = (ResultType type) =>
            {
                return eval_left.Type == type && eval_right.Type == type;
            };


            string incomp_message = Fmt("{0} does not support operands {1}, {2}", symbol.Lexeme, eval_left.Type, eval_right.Type);

            // this piece of code needs to be refactored smh
            switch (symbol.Type)
            {
                case TokenType.PLUS:
                    if (eval_left.Type == ResultType.STRING || eval_right.Type == ResultType.STRING)
                    {
                        // concatenate
                        string tmp = eval_left.Value.ToString() + eval_right.Value.ToString();
                        return new(tmp, ResultType.STRING);
                    }
                    else if(BothSidesAre(ResultType.NUMBER))
                    {
                        Double tmp = ((Double)eval_left.Value) + ((Double)eval_right.Value);
                        return new(tmp, ResultType.NUMBER);
                    }
                    else if (BothSidesAre(ResultType.TUPLE))
                    {
                        // todo
                        throw new NotImplementedException();
                    }
                    else
                        throw new FGRuntimeException(incomp_message);
                case TokenType.MINUS:
                    if (BothSidesAre(ResultType.NUMBER))
                    {
                        Double tmp = ((Double)eval_left.Value) - ((Double)eval_right.Value);
                        return new(tmp, ResultType.NUMBER);
                    }
                    else if (BothSidesAre(ResultType.TUPLE))
                    {
                        // todo
                        throw new NotImplementedException();
                    }
                    else
                        throw new FGRuntimeException(incomp_message);
                case TokenType.DIV:
                    if (BothSidesAre(ResultType.NUMBER))
                    {
                        Double tmp_right = (Double)eval_right.Value;
                        if (tmp_right == 0)
                            throw new FGRuntimeException("0 division error");
                        Double tmp = ((Double)eval_left.Value) / tmp_right;
                        return new(tmp, ResultType.NUMBER);
                    }
                    else if (BothSidesAre(ResultType.TUPLE))
                    {
                        // todo
                        throw new NotImplementedException();
                    }
                    else
                        throw new FGRuntimeException(incomp_message);
                case TokenType.MULT:
                    if (BothSidesAre(ResultType.NUMBER))
                    {
                        Double tmp = ((Double)eval_left.Value) * ((Double)eval_right.Value);
                        return new(tmp, ResultType.NUMBER);
                    }
                    else if (BothSidesAre(ResultType.TUPLE))
                    {
                        // todo
                        throw new NotImplementedException();
                    }
                    else
                        throw new FGRuntimeException(incomp_message);
                // TODO
                // == <= >= < > and or and ..
            }

            throw new FGRuntimeException(incomp_message);
        }


        public Memory.Result VisitTupleExpr(TupleExpr expr)
        {
            throw new NotImplementedException();
        }


        public Memory.Result VisitFuncCall(FuncCall expr)
        {
            string callee = expr.Callee.Lexeme;
            if (callee.Equals("__mem_debug__"))
            {
                Machine.DebugStackMemory();
                return Memory.Result.Void();
            }
            if (callee.Equals("print") || callee.Equals("println"))
            {
                string total = "";
                foreach(var arg in expr.Args)
                {
                    Memory.Result res = Eval(arg);
                    total += res.Value;
                }
                if (callee.EndsWith("ln"))
                    Console.WriteLine(total);
                else
                    Console.Write(total);
            }
            return Memory.Result.Void();
        }

        public Memory.Result VisitVarCall(VarCall expr)
        {
            throw new NotImplementedException();
        }

        public Memory.Result VisitArrayAccessCall(ArrayAccessCall expr)
        {
            throw new NotImplementedException();
        }
    }
}
