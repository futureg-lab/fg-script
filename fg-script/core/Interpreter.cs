namespace fg_script.core
{
    // C# to fg-script api
    public class FGScriptFunction
    {
        public string Name { get; }
        public int ArgCount { get; }
        Func<Memory.Result[], Memory.Result> Definition { get; }

        public FGScriptFunction(string name, int argc, Func<Memory.Result[], Memory.Result> definition)
        {
            Name = name;
            ArgCount = argc;
            Definition = definition;
        }

        public Memory.Result RunAgainst(Memory.Result[] input)
        {
            if (ArgCount != input.Count())
                throw new FGScriptException("arg count error", ArgCount + " args expected", "");

            var result = Definition(input);
            return result;
        }
    }

    public class Interpreter : IVisitorSTmt<object?>, IVisitorExpr<Memory.Result>
    {
        public Memory Machine { get; } = new();

        Dictionary<Tuple<string, int>, FGScriptFunction> ImportedFunc = new();

        public Interpreter()
        {
            LoadNativeFunctions();
        }

        private void LoadNativeFunctions()
        {
            ImportFunction(new("min", 2, (Memory.Result[] args) =>
            {
                if (ShareSameType(ResultType.NUMBER, args))
                {
                    Double a = (Double)args.First().Value, b = (Double)args.Last().Value;
                    return new(a > b ? a : b, ResultType.NUMBER);
                }
                List<ResultType> expected = new() { ResultType.NUMBER, ResultType.NUMBER };
                List<ResultType> cargs = args.ToList().ConvertAll<ResultType>(x => x.Type);
                throw FuncSignatureError("min", expected, cargs);
            }));

            ImportFunction(new("max", 2, (Memory.Result[] args) =>
            {
                if (ShareSameType(ResultType.NUMBER, args))
                {
                    Double a = (Double)args.First().Value, b = (Double)args.Last().Value;
                    return new(a > b ? a : b, ResultType.NUMBER);
                }
                List<ResultType> expected = new() { ResultType.NUMBER, ResultType.NUMBER };
                List<ResultType> cargs = args.ToList().ConvertAll<ResultType>(x => x.Type);
                throw FuncSignatureError("max", expected, cargs);
            }));

            ImportFunction(new("pow", 2, (Memory.Result[] args) =>
            {
                if (ShareSameType(ResultType.NUMBER, args))
                {
                    Double a = (Double)args.First().Value, b = (Double)args.Last().Value;
                    return new(Math.Pow(a, b), ResultType.NUMBER);
                }
                List<ResultType> expected = new() { ResultType.NUMBER };
                List<ResultType> cargs = args.ToList().ConvertAll<ResultType>(x => x.Type);
                throw FuncSignatureError("pow", expected, cargs);
            }));

            ImportFunction(new("log", 1, (Memory.Result[] args) =>
            {
                if (ShareSameType(ResultType.NUMBER, args))
                    return new(Math.Log((Double)args.First().Value), ResultType.NUMBER);
                List<ResultType> expected = new() { ResultType.NUMBER };
                List<ResultType> cargs = args.ToList().ConvertAll<ResultType>(x => x.Type);
                throw FuncSignatureError("log", expected, cargs);
            }));

            ImportFunction(new("sqrt", 1, (Memory.Result[] args) =>
            {
                if (ShareSameType(ResultType.NUMBER, args))
                    return new(Math.Sqrt((Double)args.First().Value), ResultType.NUMBER);
                List<ResultType> expected = new() { ResultType.NUMBER };
                List<ResultType> cargs = args.ToList().ConvertAll<ResultType>(x => x.Type);
                throw FuncSignatureError("sqrt", expected, cargs);
            }));

            ImportFunction(new("rand", 0, (Memory.Result[] args) =>
            {
                Random random = new();
                return new(random.NextDouble(), ResultType.NUMBER);
            }));
        }

        public FGRuntimeException FuncSignatureError(string name, List<ResultType> expected, List<ResultType> got)
        {
            return new FGRuntimeException(Fmt("{0} takes {1}, got {2} instead", name, string.Join(", ", expected), string.Join(", ", got)));
        }

        public void ImportFunction(FGScriptFunction function)
        {
            ImportedFunc[Tuple.Create(function.Name, function.ArgCount)] = function;
        }

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

        private static Boolean EvalBoolean(string repr)
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
        private static Double EvalNumber(string repr)
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
        private static string EvalString(string str)
        {
            if (!str.StartsWith("\"") && str.EndsWith("\""))
                throw new FGRuntimeException(str + " is not a string");
            return str.Substring(1, str.Length - 2);
        }

        private static bool AreSameType(string raw_lexeme, ResultType reduced)
        {
            return raw_lexeme == "bool" && reduced == ResultType.BOOLEAN
                || raw_lexeme == "str" && reduced == ResultType.STRING
                || raw_lexeme == "num" && reduced == ResultType.NUMBER
                || raw_lexeme == "tup" && reduced == ResultType.TUPLE; 
        }

        public static bool ShareSameType(ResultType type, Memory.Result[] tests)
        {
            foreach(var test in tests)
                if (test.Type != type)
                    return false;
            return true;
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
            Memory.Result cond = Eval(stmt.Condition);
            if (cond.Type != ResultType.BOOLEAN)
                throw new FGRuntimeException(Fmt("type \"{0}\" was expected, got \"{1}\" instead", ResultType.BOOLEAN, cond.Type));
            
            Machine.MemPush(); // start new scope for the condition variable
            
            string temp_name = "__while__var__";
            Machine.Store(temp_name, cond);


            Boolean value = (Boolean) cond.Value;

            while (value)
            {
                Run(stmt.Body);

                Memory.Result? current = Machine.GetValue(temp_name);
                if (current != null)
                    // eval again
                    value = (Boolean) current.Value;
                else
                    throw new FGRuntimeException(Fmt("internal error, temp reference {} was not found", temp_name));
            }

            Machine.MemPop();
            return null;
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

            // debug
            if (callee.Equals("__mem_debug__"))
            {
                Machine.DebugStackMemory();
                return Memory.Result.Void();
            }

            // print (stderr/stdout)
            HashSet<string> prints = new() { "print", "println", "err_print", "err_println" };
            if (prints.Contains(callee))
            {
                string total = "";
                
                List<object> all_values = expr.Args.ConvertAll(x => Eval(x).Value);
                if (all_values.Count > 1)
                    total = Fmt("" + all_values.First(), all_values.Skip(1).ToArray());
                else
                    total += all_values.Count == 0 ? "" : all_values.First();

                string endline = callee.EndsWith("ln") ? "\n" : "";
                if (callee.StartsWith("err"))
                    Console.Error.Write(total + endline);
                else
                    Console.Out.Write(total + endline);
            }

            // imported

            var fid = Tuple.Create(callee, expr.Args.Count);
            if (ImportedFunc.ContainsKey(fid))
            {
                var eval_args = expr
                    .Args
                    .ConvertAll<Memory.Result>(Eval)
                    .ToArray();
                return ImportedFunc[fid].RunAgainst(eval_args);

                // return new("hello", ResultType.STRING)
            }


            // fg-script defined

            return Memory.Result.Void();
        }

        public Memory.Result VisitVarCall(VarCall expr)
        {
            string var_name = expr.Callee.Lexeme;
            Memory.Result? result = Machine.GetValue(var_name);
            if (result == null)
                throw new FGRuntimeException(Fmt("reference error {0} is undefined", var_name));
            return result;
        }

        public Memory.Result VisitArrayAccessCall(ArrayAccessCall expr)
        {
            throw new NotImplementedException();
        }
    }
}
