using System.ComponentModel;
using System.Text.RegularExpressions;

namespace fg_script.core
{
    using BinaryOperator = Func<Memory.Result, Memory.Result, Memory.Result>;

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

    internal class ReturnHolder
    {
        public Memory.Result Evaluation { get; }
        public ReturnHolder(Memory.Result eval)
        {
            Evaluation = eval;
        }
    }

    internal class ErrorHolder
    {
        public Memory.Result Error { get; }
        public ErrorHolder(Memory.Result error)
        {
            Error = error;
        }
    }

    public class Interpreter : IVisitorSTmt<object?>, IVisitorExpr<Memory.Result>
    {
        public Memory Machine { get; } = new();
        // C# => fgscript
        Dictionary<Tuple<string, int>, FGScriptFunction> ImportedFunc = new();
        // fg-script => fg-script
        Dictionary<Tuple<string, int>, Func> UserDefFunc = new();

        public Interpreter()
        {
            LoadNativeFunctions();
        }

        private void LoadNativeFunctions()
        {
            // cast
            ImportFunction(new("to_str", 1, (Memory.Result[] args) =>
            {
                string a = __StringifyResult(args.First()) ;
                return new(a, ResultType.STRING);
            }));

            ImportFunction(new("to_num", 1, (Memory.Result[] args) =>
            {
                if (ShareSameType(ResultType.STRING, args))
                {
                    string input = (string)args.First().Value;
                    try
                    {
                        Double a = Double.Parse(input.Replace(".", ","));
                        return new(a, ResultType.NUMBER);
                    }
                    catch (Exception)
                    {
                        throw new FGRuntimeException("type error", Fmt("cannot convert str {0} to a num", input));
                    }
                }
                List<ResultType> expected = new() { ResultType.STRING };
                List<ResultType> cargs = args.ToList().ConvertAll<ResultType>(x => x.Type);
                throw FuncSignatureError("to_num", expected, cargs);
            }));

            ImportFunction(new("to_tup", 1, (Memory.Result[] args) =>
            {
                Dictionary<string, Memory.Result> tup = new();
                tup["0"] = args.First();
                return new(tup, ResultType.TUPLE);
            }));

            // math
            ImportFunction(new("min", 2, (Memory.Result[] args) =>
            {
                if (ShareSameType(ResultType.NUMBER, args))
                {
                    Double a = (Double)args.First().Value, b = (Double)args.Last().Value;
                    return new(a < b ? a : b, ResultType.NUMBER);
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

            ImportFunction(new("floor", 1, (Memory.Result[] args) =>
            {
                if (ShareSameType(ResultType.NUMBER, args))
                    return new(Math.Floor((Double)args.First().Value), ResultType.NUMBER);
                List<ResultType> expected = new() { ResultType.NUMBER };
                List<ResultType> cargs = args.ToList().ConvertAll<ResultType>(x => x.Type);
                throw FuncSignatureError("floor", expected, cargs);
            }));

            ImportFunction(new("ceil", 1, (Memory.Result[] args) =>
            {
                if (ShareSameType(ResultType.NUMBER, args))
                    return new(Math.Ceiling((Double)args.First().Value), ResultType.NUMBER);
                List<ResultType> expected = new() { ResultType.NUMBER };
                List<ResultType> cargs = args.ToList().ConvertAll<ResultType>(x => x.Type);
                throw FuncSignatureError("ceil", expected, cargs);
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

            ImportFunction(new("sin", 1, (Memory.Result[] args) =>
            {
                if (ShareSameType(ResultType.NUMBER, args))
                    return new(Math.Sin((Double)args.First().Value), ResultType.NUMBER);
                List<ResultType> expected = new() { ResultType.NUMBER };
                List<ResultType> cargs = args.ToList().ConvertAll<ResultType>(x => x.Type);
                throw FuncSignatureError("sin", expected, cargs);
            }));

            ImportFunction(new("cos", 1, (Memory.Result[] args) =>
            {
                if (ShareSameType(ResultType.NUMBER, args))
                    return new(Math.Cos((Double)args.First().Value), ResultType.NUMBER);
                List<ResultType> expected = new() { ResultType.NUMBER };
                List<ResultType> cargs = args.ToList().ConvertAll<ResultType>(x => x.Type);
                throw FuncSignatureError("cos", expected, cargs);
            }));

            ImportFunction(new("tan", 1, (Memory.Result[] args) =>
            {
                if (ShareSameType(ResultType.NUMBER, args))
                    return new(Math.Tan((Double)args.First().Value), ResultType.NUMBER);
                List<ResultType> expected = new() { ResultType.NUMBER };
                List<ResultType> cargs = args.ToList().ConvertAll<ResultType>(x => x.Type);
                throw FuncSignatureError("tan", expected, cargs);
            }));

            ImportFunction(new("abs", 1, (Memory.Result[] args) =>
            {
                if (ShareSameType(ResultType.NUMBER, args))
                    return new(Math.Abs((Double)args.First().Value), ResultType.NUMBER);
                List<ResultType> expected = new() { ResultType.NUMBER };
                List<ResultType> cargs = args.ToList().ConvertAll<ResultType>(x => x.Type);
                throw FuncSignatureError("abs", expected, cargs);
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

            // on lists and strings
            ImportFunction(new("len", 1, (Memory.Result[] args) =>
            {
                Double size = 0;
                var arg = args.First();
                
                if (arg.Type == ResultType.TUPLE)
                    size = ((Dictionary<string, Memory.Result>)arg.Value).Count;
                else if (arg.Type == ResultType.STRING)
                    size = ((string)arg.Value).Length;
                else
                {
                    List<ResultType> expected = new() { ResultType.STRING };
                    List<ResultType> cargs = args.ToList().ConvertAll<ResultType>(x => x.Type);
                    throw FuncSignatureError("len", expected, cargs);
                }
                return new(size, ResultType.NUMBER);
            }));

            // tuple operations
            ImportFunction(new("tpop", 1, (Memory.Result[] args) =>
            {
                var tuple = args.First();

                if (tuple.Type == ResultType.TUPLE)
                {
                    var value = (Dictionary<string, Memory.Result>)tuple.Value;
                    if (value.Count == 0)
                        return tuple;
                    string last_key = value.Last().Key;
                    value.Remove(last_key);
                    return tuple;
                }
                List<ResultType> expected = new() { ResultType.TUPLE };
                List<ResultType> cargs = args.ToList().ConvertAll<ResultType>(x => x.Type);
                throw FuncSignatureError("tpop", expected, cargs);
            }));

            ImportFunction(new("tpush", 2, (Memory.Result[] args) =>
            {
                var tuple = args.First();
                var new_item = args.Last();

                if (tuple.Type == ResultType.TUPLE)
                {
                    var value = (Dictionary<string, Memory.Result>)tuple.Value;
                    string new_key = value.Count.ToString();
                    value[new_key] = new_item;
                    return tuple;
                }
                List<ResultType> expected = new() { ResultType.TUPLE };
                List<ResultType> cargs = args.ToList().ConvertAll<ResultType>(x => x.Type);
                throw FuncSignatureError("tpush", expected, cargs);
            }));

            ImportFunction(new("tfirst", 1, (Memory.Result[] args) =>
            {
                var tuple = args.First();
                if (tuple.Type == ResultType.TUPLE)
                {
                    var value = (Dictionary<string, Memory.Result>)tuple.Value;
                    return value.First().Value;
                }
                List<ResultType> expected = new() { ResultType.TUPLE };
                List<ResultType> cargs = args.ToList().ConvertAll<ResultType>(x => x.Type);
                throw FuncSignatureError("tfirst", expected, cargs);
            }));

            ImportFunction(new("tlast", 1, (Memory.Result[] args) =>
            {
                var tuple = args.First();
                if (tuple.Type == ResultType.TUPLE)
                {
                    var value = (Dictionary<string, Memory.Result>)tuple.Value;
                    return value.Last().Value;
                }
                List<ResultType> expected = new() { ResultType.TUPLE };
                List<ResultType> cargs = args.ToList().ConvertAll<ResultType>(x => x.Type);
                throw FuncSignatureError("tlast", expected, cargs);
            }));

            ImportFunction(new("tempty", 1, (Memory.Result[] args) =>
            {
                var tuple = args.First();
                if (tuple.Type == ResultType.TUPLE)
                {
                    var value = (Dictionary<string, Memory.Result>)tuple.Value;
                    value.Clear();
                    return tuple;
                }
                List<ResultType> expected = new() { ResultType.TUPLE };
                List<ResultType> cargs = args.ToList().ConvertAll<ResultType>(x => x.Type);
                throw FuncSignatureError("tempty", expected, cargs);
            }));

            // utils
            ImportFunction(new("split", 2, (Memory.Result[] args) =>
            {
                var _sep = args.First();
                var _str = args.Last();
                if (_sep.Type == ResultType.STRING && _str.Type == ResultType.STRING)
                {
                    string sep = _sep.Value.ToString();
                    string str = _str.Value.ToString();
                    string[] chunks = str.Split(sep);
                    Dictionary<string, Memory.Result> res = new();
                    for (int i = 0; i < chunks.Length; i++)
                        res.Add(i.ToString(), new(chunks[i], ResultType.STRING));
                    return new(res, ResultType.TUPLE);
                }
                List<ResultType> expected = new() { ResultType.STRING, ResultType.STRING };
                List<ResultType> cargs = args.ToList().ConvertAll<ResultType>(x => x.Type);
                throw FuncSignatureError("split", expected, cargs);
            }));

            ImportFunction(new("join", 2, (Memory.Result[] args) =>
            {
                var str = args.First();
                var tup = args.Last();
                if (str.Type == ResultType.STRING && tup.Type == ResultType.TUPLE)
                {
                    var input = (Dictionary<string, Memory.Result>)tup.Value;
                    string output = "";
                    int i = 0, len = input.Count();
                    foreach (var item in input)
                    {
                        if (item.Value.Type == ResultType.TUPLE)
                            throw new FGRuntimeException(
                                "unable to join", 
                                Fmt("cannot join to a string, {0} is a tuple", __StringifyResult(item.Value)
                            ));
                        output += __StringifyResult(item.Value);
                        if (i + 1 < len) output += str.Value;
                        i++;
                    }
                    return new(output, ResultType.STRING);
                }
                List<ResultType> expected = new() { ResultType.STRING, ResultType.TUPLE };
                List<ResultType> cargs = args.ToList().ConvertAll<ResultType>(x => x.Type);
                throw FuncSignatureError("join", expected, cargs);
            }));
        }

        public static FGRuntimeException FuncSignatureError(string name, List<ResultType> expected, List<ResultType> got)
        {
            string str_expected = string.Join(", ", expected.ConvertAll<string>(__Describe));
            string str_got = string.Join(", ", got.ConvertAll<string>(__Describe));
            return new FGRuntimeException("type error", Fmt("{0} takes {1}, got {2} instead", name, str_expected, str_got));
        }

        public void ImportFunction(FGScriptFunction function)
        {
            ImportedFunc[Tuple.Create(function.Name, function.ArgCount)] = function;
        }

        public object? Run(Stmt stmt)
        {
            return stmt.Accept(this);
        }

        public Memory.Result Eval(Expr expr)
        {
            return expr.Accept(this);
        }

        private static string Fmt(string str, params object[] list)
        {
            for (int i = 0; i < list.Count(); i++)
                if (list[i] is ResultType type)
                    list[i] = __Describe(type);
            try
            {
                return string.Format(str, list);
            } 
            catch(FormatException _e)
            {
                throw new FGRuntimeException(_e.Message);
            } 
            catch(Exception e)
            {
                throw e;
            }
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
                throw new FGRuntimeException("type error", Fmt("{0} is not a boolean", repr));
            }
        }

        // "645.6444" => 645.64444
        private static Double EvalNumber(string repr)
        {
            try
            {
                return Double.Parse(repr.Replace(".", ","));
            }
            catch (Exception)
            {
                throw new FGRuntimeException("type error", Fmt("{0} is not a number", repr));
            }
        }

        // ""string"" => srting
        private static string EvalString(string str)
        {
            if (!str.StartsWith("\"") && str.EndsWith("\""))
                throw new FGRuntimeException("type error", Fmt("{0} is not a string", str));
            return str.Substring(1, str.Length - 2);
        }

        private static bool AreSameType(string raw_lexeme, ResultType reduced)
        {
            return raw_lexeme == "bool" && reduced == ResultType.BOOLEAN
                || raw_lexeme == "str" && reduced == ResultType.STRING
                || raw_lexeme == "num" && reduced == ResultType.NUMBER
                || raw_lexeme == "tup" && reduced == ResultType.TUPLE
                || raw_lexeme == "void" && reduced == ResultType.VOID; 
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
                throw new FGRuntimeException("type error", Fmt("type \"{0}\" was expected, got \"{1}\" instead", raw_lexeme, reduced));
        }

        // statements
        public object? VisitAssign(Assign stmt)
        {
            Memory.Result value = Eval(stmt.Variable.Value);

            // if flagged as auto => we can infer the type from the value
            // any type can hold a null literal
            if (!__TypeIsAutoInfered(stmt.Variable.DataType))
                TypeMismatchCheck(stmt.Variable.DataType.Lexeme, value.Type);

            // store it
            Machine.Store(stmt.Variable.Name.Lexeme, value);
            return null;
        }

        public object? VisitReAssign(ReAssign stmt)
        {
            // should exists first
            string var_name = stmt.Callee.Lexeme;
            Memory.Result? current = Machine.GetValue(var_name);
            if (current == null)
                throw new FGRuntimeException("re-assign", Fmt("\"{0}\" has not been defined yet", var_name));

            Memory.Result new_value = Eval(stmt.NewValue);
            // only a null can be re-assigned and promoted to a new type
            if (current.Type != new_value.Type && current.Type != ResultType.NULL)
                throw new FGRuntimeException("type error", Fmt("type \"{0}\" was expected, got \"{1}\" instead", current.Type, new_value.Type));

            Machine.Replace(var_name, new_value);
            
            return null;
        }
        
        private static string __Describe(ResultType type)
        {
            DescriptionAttribute[] attributes = (DescriptionAttribute[])type
                .GetType()
                .GetField(type.ToString())
                .GetCustomAttributes(typeof(DescriptionAttribute), false);
            return attributes.Length > 0 ? attributes[0].Description : string.Empty;
        }

        public object? VisitReAssignTupleIndex(ReAssignTuple stmt)
        {
            // should exists first
            string var_name = stmt.Callee.Lexeme;
            Memory.Result? current = Machine.GetValue(var_name);
            if (current == null)
                throw new FGRuntimeException("re-assign", Fmt("\"{0}\" has not been defined yet", var_name));

            if (current.Type != ResultType.TUPLE)
                throw new FGRuntimeException("array access", Fmt("\"{0}\" is not a tuple", var_name));

            Memory.Result new_value = Eval(stmt.NewValue);

            // mutate the stored value
            Dictionary<string, Memory.Result> to_mutate = (Dictionary<string, Memory.Result>) current.Value;

            List<string> done = new();
            foreach (Expr expr_index in stmt.Indexes)
            {
                Memory.Result index = Eval(expr_index);
                if (index.Type != ResultType.NUMBER && index.Type != ResultType.STRING)
                    throw new FGRuntimeException("type error", Fmt("index type num or str was expected, got \"{0}\" instead", new_value.Type));

                string sanitized_idx = __StringifyResult(index);
                if (!to_mutate.ContainsKey(sanitized_idx))
                    throw new FGRuntimeException("array access", Fmt("{0}[{1}] does not contain key {2}", var_name, string.Join(", ", done), sanitized_idx));
                done.Add(sanitized_idx);

                Memory.Result node = to_mutate[sanitized_idx];
                if (node.Value is Dictionary<string, Memory.Result>)
                {
                    to_mutate = (Dictionary<string, Memory.Result>)node.Value;
                    continue;
                }
                // re assign
                if (node.Type != new_value.Type && node.Type != ResultType.NULL)
                    throw new FGRuntimeException("type error", Fmt("type \"{0}\" was expected, got \"{1}\" instead", node.Type, new_value.Type));
                if (done.Count < stmt.Indexes.Count)
                    throw new FGRuntimeException("array access", Fmt("{0}[{1}] is not a tuple", var_name, string.Join(", ", done)));
                else
                    to_mutate[sanitized_idx] = new_value;
                break;
            }

            return null;
        }

        public object? VisitBlock(Block stmt)
        {
            object? interruption = null;
            Machine.MemPush(); // new scope
            
            foreach (var line in stmt.Statements)
            {
                if (line is Return || line is Break || line is Continue || line is Error)
                {
                    interruption = Run(line);
                    break;
                }
                else
                {
                    object? evaluation = Run(line);
                    if (evaluation is ReturnHolder || evaluation is ErrorHolder || evaluation is Break || evaluation is Continue)
                    {
                        interruption = evaluation;
                        break;
                    }
                }
            }

            Machine.MemPop(); // discard
            return interruption;
        }

        public object? VisitStmt(Stmt stmt)
        {
            Run(stmt);
            return null;
        }

        public object? VisitFunc(Func stmt)
        {
            var id = Tuple.Create(stmt.Name.Lexeme, stmt.Args.Count);
            UserDefFunc[id] = stmt;
            return null;
        }

        public object? VisitIf(If stmt)
        {
            static void IsItBoolean(Memory.Result res)
            {
                if (res.Type != ResultType.BOOLEAN)
                    throw new FGRuntimeException("type error", Fmt("type \"{0}\" was expected, got \"{1}\" instead", ResultType.BOOLEAN, res.Type));
            }

            // main body
            Memory.Result res = Eval(stmt.IfCondition);
            IsItBoolean(res);

            Boolean main_cond = (Boolean) res.Value;
            if (main_cond)
                return VisitBlock(stmt.IfBody);

            // has elseif
            if (!main_cond)
            {
                foreach (var elif in stmt.Branches)
                {
                    Memory.Result br_cond = Eval(elif.Condition);
                    IsItBoolean(br_cond);
                    if ((Boolean)br_cond.Value)
                        return VisitBlock(elif.Body);
                }
                // has else and main_cond is false
                if (stmt.ElseBody != null)
                    return VisitBlock(stmt.ElseBody);
            }
            return null;
        }

        public object? VisitBranch(Branch stmt)
        {
            throw new NotImplementedException();
        }

        public object? VisitFor(For stmt)
        {
            Token? key = stmt.KeyAlias;
            Token value = stmt.ValueAlias;

            Memory.Result iterable = Eval(stmt.ExprIterable);
            if (iterable.Type != ResultType.TUPLE)
                throw new FGRuntimeException("invalid loop expression", "value is not iterable");
            object? potential_ret = null;
            var tup = (Dictionary<string, Memory.Result>)iterable.Value; 
            foreach (var entry in tup)
            {
                Machine.MemPush();
                
                if (key != null)
                    Machine.Store(key.Lexeme, new(entry.Key, ResultType.STRING));
                Machine.Store(value.Lexeme, new(entry.Value.Value, entry.Value.Type));

                object? block_eval = VisitBlock(stmt.Body);
                if (block_eval != null)
                {
                    if (block_eval is Break)
                        break;
                    if (block_eval is Continue)
                        continue;
                    if (block_eval is ReturnHolder content)
                    {
                        potential_ret = content; // let a function handle this
                        break;
                    }
                    if (block_eval is ErrorHolder error)
                    {
                        potential_ret = error; // let the user handle this
                        break;
                    }
                }

                Machine.MemPop(); // discard key, value
            }

            return potential_ret;
        }

        public object? VisitWhile(While stmt)
        {
            Memory.Result cond = Eval(stmt.Condition);
            if (cond.Type != ResultType.BOOLEAN)
                throw new FGRuntimeException("type error", Fmt("type \"{0}\" was expected, got \"{1}\" instead", ResultType.BOOLEAN, cond.Type));
            
            Machine.MemPush(); // start new scope for the condition variable
            
            string temp_name = "__while__" + (new Random().Next());
            Machine.Store(temp_name, cond);

            Boolean value = (Boolean) cond.Value;
            object? potential_ret = null;
            while (value)
            {
                object? block_eval = VisitBlock(stmt.Body);
                if (block_eval != null)
                {
                    if (block_eval is Break)
                        break;
                    if (block_eval is Continue)
                        continue;
                    if (block_eval is ReturnHolder content)
                    {
                        potential_ret = content; // let a function handle this
                        break;
                    }
                    if (block_eval is ErrorHolder error)
                    {
                        potential_ret = error; // let the user handle this
                        break;
                    }
                }

                // re eval
                Machine.Replace(temp_name, Eval(stmt.Condition));
                Memory.Result? current = Machine.GetValue(temp_name);

                if (current == null) // should never happen
                    throw new FGRuntimeException("type error", Fmt("internal error, temp reference {0} was not found", temp_name));

                value = (Boolean) current.Value;
            }

            Machine.MemPop();
            return potential_ret;
        }

        public object? VisitReturn(Return stmt)
        {
            if (stmt.ReturnValue != null)
                return new ReturnHolder(Eval(stmt.ReturnValue));
            return new ReturnHolder(Memory.Result.Void());
        }

        public object? VisitError(Error stmt)
        {
            return new ErrorHolder(Eval(stmt.ThrownValue));
        }

        public object? VisitBreak(Break stmt)
        {
            return stmt;
        }

        public object? VisitContinue(Continue stmt)
        {
            return stmt;
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
            return Eval(stmt.Expr);
        }

        // expressions

        public Memory.Result VisitExpr(Expr expr)
        {
            return Eval(expr);
        }

        public Memory.Result VisitVarExpr(VarExpr expr)
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
            object? value;
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
                case TokenType.NULL:
                    value = null;
                    type = ResultType.NULL;
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

            string not_supp_msg = Fmt("{0} is not supported by {1}", eval_operand.Type, symbol.Lexeme);

            if (symbol.Type != TokenType.MINUS && symbol.Type != TokenType.NOT && symbol.Type != TokenType.REPR_OF)
                throw new FGRuntimeException(Fmt("\"{0}\" does not have a definition", symbol.Lexeme));

            if (symbol.Type == TokenType.REPR_OF)
                return new(__Describe(eval_operand.Type), ResultType.STRING);

            Memory.Result DoMinus(Memory.Result input)
            {
                if (input.Type != ResultType.NUMBER)
                    throw new FGRuntimeException("operand not supported", Fmt("{0} is not supported by {1}", input.Type, symbol.Lexeme));
                return new(-((Double)input.Value), input.Type);
            }

            Memory.Result DoNot (Memory.Result input)
            {
                if (input.Type != ResultType.BOOLEAN)
                    throw new FGRuntimeException("operand not supported", Fmt("{0} is not supported by {1}", input.Type, symbol.Lexeme));
                return new(!((Boolean)input.Value), input.Type);
            }

            Memory.Result result;

            switch(symbol.Type)
            {
                case TokenType.MINUS:
                    if (eval_operand.Type == ResultType.NUMBER)
                        return DoMinus(eval_operand);
                    else if (eval_operand.Type == ResultType.TUPLE)
                        return __LiftUnaryFnToTuple(eval_operand, DoMinus);
                    break;
                case TokenType.NOT:
                    if (eval_operand.Type == ResultType.BOOLEAN)
                        return DoNot(eval_operand);
                    else if (eval_operand.Type == ResultType.TUPLE)
                        return __LiftUnaryFnToTuple(eval_operand, DoNot);
                    break;
            }

            throw new FGRuntimeException(not_supp_msg);
        }
        public Memory.Result VisitBinaryExpr(BinaryExpr expr)
        {
            Token symbol = expr.OpSymbol;
            Memory.Result eval_left = Eval(expr.Left);
            Memory.Result eval_right = Eval(expr.Right);

            var BothSidesAre = (Memory.Result eval_left, Memory.Result eval_right, ResultType type) =>
            {
                return eval_left.Type == type && eval_right.Type == type;
            };

            var OneIsNull = (Memory.Result eval_left, Memory.Result eval_right) =>
            {
                return eval_left.Type == ResultType.NULL && eval_right.Type != ResultType.NULL
                    || eval_left.Type != ResultType.NULL && eval_right.Type == ResultType.NULL;
            };

            FGRuntimeException IncompMessage(Memory.Result eval_left, Memory.Result eval_right)
            {
                return new FGRuntimeException(
                    "non-supported operands",
                    Fmt("{0} does not support operands {1}, {2}", symbol.Lexeme, eval_left.Type, eval_right.Type)
                  );
            }
                
            Memory.Result DoMod(Memory.Result eval_left, Memory.Result eval_right)
            {
                if (BothSidesAre(eval_left, eval_right, ResultType.NUMBER))
                {
                    Double tmp = ((Double)eval_left.Value) % ((Double)eval_right.Value);
                    return new(tmp, ResultType.NUMBER);
                }
                else if (BothSidesAre(eval_left, eval_right, ResultType.TUPLE))
                {
                    return __LiftBinaryFnToTuple(eval_left, eval_right, DoMod);
                }
                else
                    throw IncompMessage(eval_left, eval_right);
            }

            Memory.Result DoPlus(Memory.Result eval_left, Memory.Result eval_right)
            {
                if (BothSidesAre(eval_left, eval_right, ResultType.NUMBER))
                {
                    Double tmp = ((Double)eval_left.Value) + ((Double)eval_right.Value);
                    return new(tmp, ResultType.NUMBER);
                }
                else if (eval_left.Type == ResultType.STRING || eval_right.Type == ResultType.STRING)
                {
                    string left = __StringifyResult(eval_left);
                    string right = __StringifyResult(eval_right);
                    return new(left + right, ResultType.STRING);
                }
                else if (BothSidesAre(eval_left, eval_right, ResultType.TUPLE))
                {
                    return __LiftBinaryFnToTuple(eval_left, eval_right, DoPlus);
                }
                else
                    throw IncompMessage(eval_left, eval_right);
            }

            Memory.Result DoMinus(Memory.Result eval_left, Memory.Result eval_right)
            {
                if (BothSidesAre(eval_left, eval_right, ResultType.NUMBER))
                {
                    Double tmp = ((Double)eval_left.Value) - ((Double)eval_right.Value);
                    return new(tmp, ResultType.NUMBER);
                }
                else if (BothSidesAre(eval_left, eval_right, ResultType.TUPLE))
                {
                    return __LiftBinaryFnToTuple(eval_left, eval_right, DoMinus);
                }
                else
                    throw IncompMessage(eval_left, eval_right);
            };

            Memory.Result DoDiv(Memory.Result eval_left, Memory.Result eval_right)
            {
                if (BothSidesAre(eval_left, eval_right, ResultType.NUMBER))
                {
                    Double tmp_right = (Double)eval_right.Value;
                    if (tmp_right == 0)
                        throw new FGRuntimeException("0 division error");
                    Double tmp = ((Double)eval_left.Value) / tmp_right;
                    return new Memory.Result(tmp, ResultType.NUMBER);
                }
                else if (BothSidesAre(eval_left, eval_right, ResultType.TUPLE))
                {
                    return __LiftBinaryFnToTuple(eval_left, eval_right, DoDiv);
                }
                else
                    throw IncompMessage(eval_left, eval_right);
            };

            Memory.Result DoMult(Memory.Result eval_left, Memory.Result eval_right)
            {
                if (BothSidesAre(eval_left, eval_right, ResultType.NUMBER))
                {
                    Double tmp = ((Double)eval_left.Value) * ((Double)eval_right.Value);
                    return new Memory.Result(tmp, ResultType.NUMBER);
                }
                else if (BothSidesAre(eval_left, eval_right, ResultType.TUPLE))
                {
                    return __LiftBinaryFnToTuple(eval_left, eval_right, DoMult);
                }
                else
                    throw IncompMessage(eval_left, eval_right);
            };

            Memory.Result DoEQ(Memory.Result eval_left, Memory.Result eval_right)
            {
                if (BothSidesAre(eval_left, eval_right, ResultType.NUMBER))
                {
                    Boolean tmp = ((Double)eval_left.Value) == ((Double)eval_right.Value);
                    return new Memory.Result(tmp, ResultType.BOOLEAN);
                }
                else if (BothSidesAre(eval_left, eval_right, ResultType.STRING))
                {
                    Boolean tmp = ((String)eval_left.Value).Equals((String)eval_right.Value);
                    return new Memory.Result(tmp, ResultType.BOOLEAN);
                }
                else if (BothSidesAre(eval_left, eval_right, ResultType.BOOLEAN))
                {
                    Boolean tmp = ((Boolean)eval_left.Value) == ((Boolean)eval_right.Value);
                    return new Memory.Result(tmp, ResultType.BOOLEAN);
                }
                else if (BothSidesAre(eval_left, eval_right, ResultType.NULL))
                {
                    return new Memory.Result(true, ResultType.BOOLEAN);
                }
                else if (OneIsNull(eval_left, eval_right))
                {
                    return new Memory.Result(false, ResultType.BOOLEAN);
                }
                else if (BothSidesAre(eval_left, eval_right, ResultType.TUPLE))
                {
                    return __LiftBinaryFnToTuple(eval_left, eval_right, DoEQ);
                }
                else
                    throw IncompMessage(eval_left, eval_right);
            };

            Memory.Result DoNotEQ(Memory.Result eval_left, Memory.Result eval_right)
            {
                Memory.Result temp = DoEQ(eval_left, eval_right);
                return new Memory.Result(!((Boolean)temp.Value), ResultType.BOOLEAN);
            };

            Memory.Result DoLT(Memory.Result eval_left, Memory.Result eval_right)
            {
                if (BothSidesAre(eval_left, eval_right, ResultType.NUMBER))
                {
                    Boolean tmp = ((Double)eval_left.Value) < ((Double)eval_right.Value);
                    return new(tmp, ResultType.BOOLEAN);
                }
                else if (BothSidesAre(eval_left, eval_right, ResultType.TUPLE))
                {
                    return __LiftBinaryFnToTuple(eval_left, eval_right, DoLT);
                }
                else
                    throw IncompMessage(eval_left, eval_right);
            }

            Memory.Result DoAND(Memory.Result eval_left, Memory.Result eval_right)
            {
                if (BothSidesAre(eval_left, eval_right, ResultType.BOOLEAN))
                {
                    Boolean tmp = ((Boolean)eval_left.Value) && ((Boolean)eval_right.Value);
                    return new(tmp, ResultType.BOOLEAN);
                }
                else if (BothSidesAre(eval_left, eval_right, ResultType.TUPLE))
                {
                    return __LiftBinaryFnToTuple(eval_left, eval_right, DoAND);
                }
                else
                    throw IncompMessage(eval_left, eval_right);
            }

            Memory.Result DoOR(Memory.Result eval_left, Memory.Result eval_right)
            {
                if (BothSidesAre(eval_left, eval_right, ResultType.BOOLEAN))
                {
                    Boolean tmp = ((Boolean)eval_left.Value) || ((Boolean)eval_right.Value);
                    return new(tmp, ResultType.BOOLEAN);
                }
                else if (BothSidesAre(eval_left, eval_right, ResultType.TUPLE))
                {
                    return __LiftBinaryFnToTuple(eval_left, eval_right, DoOR);
                }
                else
                    throw IncompMessage(eval_left, eval_right);
            }

            Memory.Result DoXOR(Memory.Result eval_left, Memory.Result eval_right)
            {
                if (BothSidesAre(eval_left, eval_right, ResultType.BOOLEAN))
                {
                    Boolean tmp = ((Boolean)eval_left.Value) ^ ((Boolean)eval_right.Value);
                    return new(tmp, ResultType.BOOLEAN);
                }
                else if (BothSidesAre(eval_left, eval_right, ResultType.TUPLE))
                {
                    return __LiftBinaryFnToTuple(eval_left, eval_right, DoXOR);
                }
                else
                    throw IncompMessage(eval_left, eval_right);
            }


            Memory.Result DoGT(Memory.Result eval_left, Memory.Result eval_right)
            {
                if (BothSidesAre(eval_left, eval_right, ResultType.NUMBER))
                {
                    Boolean tmp = ((Double)eval_left.Value) > ((Double)eval_right.Value);
                    return new(tmp, ResultType.BOOLEAN);
                }
                else if (BothSidesAre(eval_left, eval_right, ResultType.TUPLE))
                {
                    return __LiftBinaryFnToTuple(eval_left, eval_right, DoGT);
                }
                else
                    throw IncompMessage(eval_left, eval_right);
            }

            Memory.Result DoLE(Memory.Result eval_left, Memory.Result eval_right)
            {
                if (BothSidesAre(eval_left, eval_right, ResultType.NUMBER))
                {
                    Boolean tmp = ((Double)eval_left.Value) <= ((Double)eval_right.Value);
                    return new(tmp, ResultType.BOOLEAN);
                }
                else if (BothSidesAre(eval_left, eval_right, ResultType.TUPLE))
                {
                    return __LiftBinaryFnToTuple(eval_left, eval_right, DoLE);
                }
                else
                    throw IncompMessage(eval_left, eval_right);
            }

            Memory.Result DoGE(Memory.Result eval_left, Memory.Result eval_right)
            {
                if (BothSidesAre(eval_left, eval_right, ResultType.NUMBER))
                {
                    Boolean tmp = ((Double)eval_left.Value) >= ((Double)eval_right.Value);
                    return new(tmp, ResultType.BOOLEAN);
                }
                else if (BothSidesAre(eval_left, eval_right, ResultType.TUPLE))
                {
                    return __LiftBinaryFnToTuple(eval_left, eval_right, DoGE);
                }
                else
                    throw IncompMessage(eval_left, eval_right);
            }

            // this piece of code needs to be refactored smh
            switch (symbol.Type)
            {
                case TokenType.MOD:
                    return DoMod(eval_left, eval_right);
                case TokenType.PLUS:
                    return DoPlus(eval_left, eval_right);
                case TokenType.MINUS:
                    return DoMinus(eval_left, eval_right);
                case TokenType.DIV:
                    return DoDiv(eval_left, eval_right);
                case TokenType.MULT:
                    return DoMult(eval_left, eval_right);
                case TokenType.AND:
                    return DoAND(eval_left, eval_right);
                case TokenType.OR:
                    return DoOR(eval_left, eval_right);
                case TokenType.XOR:
                    return DoXOR(eval_left, eval_right);
                case TokenType.EQ:
                    return DoEQ(eval_left, eval_right);
                case TokenType.NEQ:
                    return DoNotEQ(eval_left, eval_right);
                case TokenType.LT:
                    return DoLT(eval_left, eval_right);
                case TokenType.GT:
                    return DoGT(eval_left, eval_right);
                case TokenType.LE:
                    return DoLE(eval_left, eval_right);
                case TokenType.GE:
                    return DoGE(eval_left, eval_right);
            }

            throw IncompMessage(eval_left, eval_right);
        }


        public Memory.Result VisitTupleExpr(TupleExpr expr)
        {
            Dictionary<string, Memory.Result> tup = new();

            // Expr can be a tup, num, str, bool
            foreach (KeyValuePair<string, Expr> entry in expr.Map)
                tup[entry.Key] = Eval(entry.Value);

            return new(tup, ResultType.TUPLE);
        }
        public Memory.Result VisitEnumExpr(EnumExpr expr)
        {
            Memory.Result __start = Eval(expr.Start);
            Memory.Result __end = Eval(expr.End);

            bool expected = (__start.Type == __end.Type && __start.Type == ResultType.NUMBER);
            if (!expected)
                throw new FGRuntimeException(Fmt("invalid enumeration, start, end should be of type \"num\""));

            Double start = (Double)__start.Value;
            Double end = (Double)__end.Value;

            Dictionary<string, Memory.Result> tup = new();
            int index = 0;
            for (Double i = start; i <= end; i += 1.0)
                tup[(index++).ToString()] = new(i, ResultType.NUMBER);

            return new(tup, ResultType.TUPLE);
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
                
                List<string> all_values = expr.Args.ConvertAll(x => __StringifyResult(Eval(x)));
                if (all_values.Count > 1)
                    total = Fmt("" + all_values.First(), all_values.Skip(1).ToArray());
                else
                    total += all_values.Count == 0 ? "" : all_values.First();

                string endline = callee.EndsWith("ln") ? "\n" : "";

                string all_text = Regex.Unescape(total + endline);

                if (callee.StartsWith("err"))
                    Console.Error.Write(all_text);
                else
                    Console.Out.Write(all_text);

                return Memory.Result.Void();
            }

            // scan (stdin)
            if (callee.Equals("scanln"))
            {
                if (expr.Args.Count != 1)
                    throw new FGRuntimeException("scanln failed", "1 arg expected");
                object __arg = expr.Args.First();
                // scanln(var_name => a VarCall) 
                if (__arg is VarCall)
                {
                    string? input = Console.ReadLine();
                    VarCall arg = (VarCall)__arg;

                    Memory.Result? current = Machine.GetValue(arg.Callee.Lexeme);
                    if (current == null)
                        throw new FGRuntimeException("scanln failed", Fmt("{0} has not been initialized yet", arg.Callee.Lexeme));
                    if (current.Type == ResultType.TUPLE)
                        throw new FGRuntimeException("scanln failed", Fmt("{0} is a tuple", arg.Callee.Lexeme));
                    if (input == null)
                        return Memory.Result.Void();

                    if (current.Type == ResultType.NUMBER)
                        current = new(EvalNumber(input), ResultType.NUMBER);

                    if (current.Type == ResultType.NULL)
                        current = new(input, ResultType.STRING);

                    Machine.Replace(arg.Callee.Lexeme, current);
                    return Memory.Result.Void();
                }
                else
                    throw new FGRuntimeException("scanln failed", "variable expected, got an expression instead");
            }

            var fid = Tuple.Create(callee, expr.Args.Count);
            // imported
            if (ImportedFunc.ContainsKey(fid))
            {
                var eval_args = expr
                    .Args
                    .ConvertAll<Memory.Result>(Eval)
                    .ToArray();
                return ImportedFunc[fid].RunAgainst(eval_args);
            }

            // fg-script defined
            if (UserDefFunc.ContainsKey(fid))
            {
                var eval_args = expr
                    .Args
                    .ConvertAll<Memory.Result>(Eval)
                    .ToArray();
                Func func = UserDefFunc[fid];

                // check if the args match
                // at this point we can assume that the sizes are the same
                Memory.Result output_value = Memory.Result.Void();
                Machine.MemPush();
                for (int i = 0; i < eval_args.Length; i++)
                {
                    var arg = func.Args[i];
                    ResultType curr_eval = eval_args[i].Type;
                    string fun_arg_lex = func.Args[i].DataType.Lexeme;

                    if (!__TypeIsAutoInfered(func.Args[i].DataType) 
                        && !AreSameType(fun_arg_lex, curr_eval))
                        throw new FGRuntimeException("type error", Fmt("type \"{0}\" was expected, got \"{1}\" instead", fun_arg_lex, curr_eval));

                    // store arg as local scope variable with the appropriate name
                    Machine.Store(func.Args[i].Name.Lexeme, eval_args[i]);
                }
                if (func.Body != null)
                {
                    object? block_eval = VisitBlock(func.Body);
                    if (block_eval is ReturnHolder content)
                    {
                        output_value = content.Evaluation;
                        if (!__TypeIsAutoInfered(func.ReturnType))
                            TypeMismatchCheck(func.ReturnType.Lexeme, output_value.Type);
                    }
                    else if (block_eval is ErrorHolder propag)
                        output_value = propag.Error; // let the user handle this
                }
                Machine.MemPop();

                return output_value;
            }

            List<string> args = expr
                .Args
                .ConvertAll(x => __Describe(Eval(x).Type));

            string temp = string.Join(", ", args);
            throw new FGRuntimeException("reference error", Fmt("{0} with args {1} (arg_count {2}) is undefined", callee, temp, args.Count));
        }

        public Memory.Result VisitVarCall(VarCall expr)
        {
            string var_name = expr.Callee.Lexeme;
            Memory.Result? result = Machine.GetValue(var_name);
            if (result == null)
                throw new FGRuntimeException("reference error", Fmt("{0} is undefined", var_name));
            return result;
        }

        public Memory.Result VisitTupleIndexAccessCall(TupleIndexAccessCall expr)
        {
            // should exists first
            string var_name = expr.Callee.Lexeme;
            Memory.Result? current = Machine.GetValue(var_name);
            if (current == null)
                throw new FGRuntimeException("re-assign", "\"" + var_name + "\" has not been defined yet");

            if (current.Type != ResultType.TUPLE)
                throw new FGRuntimeException("index access", "\"" + var_name + "\" is not a tuple");

            // mutate the stored value
            Dictionary<string, Memory.Result> to_mutate = (Dictionary<string, Memory.Result>)current.Value;
            Memory.Result? indexed_value = null;
            List<string> done = new();
            foreach (Expr expr_index in expr.Indexes)
            {
                Memory.Result index = Eval(expr_index);

                string sanitized_idx = __StringifyResult(index);
                if (!to_mutate.ContainsKey(sanitized_idx))
                    throw new FGRuntimeException("index access", Fmt("{0}[{1}] does not contain key {2}", var_name, string.Join(", ", done), sanitized_idx));
                done.Add(sanitized_idx);

                Memory.Result node = to_mutate[sanitized_idx];
                indexed_value = node;
                if (node.Value is Dictionary<string, Memory.Result> dictionary)
                {
                    to_mutate = dictionary;
                }
                else
                {
                    if (done.Count < expr.Indexes.Count)
                        throw new FGRuntimeException("array access", Fmt("{0}[{1}] is not a tuple", var_name, string.Join(", ", done)));
                    break;
                }
            }

            if (indexed_value == null) // should never happen
                throw new FGRuntimeException("internal error", "memory access violation");

            return indexed_value;
        }


        private static Memory.Result __LiftBinaryFnToTuple (
            Memory.Result eval_left,
            Memory.Result eval_right,
            Func<Memory.Result, Memory.Result, Memory.Result> binary)
        {
            var tleft = (Dictionary<string, Memory.Result>)eval_left.Value;
            var tright = (Dictionary<string, Memory.Result>)eval_right.Value;

            var res = new Dictionary<string, Memory.Result>();
            if (tleft.Count != tright.Count)
                throw new FGRuntimeException("not same cardinality", Fmt("left {0}, right {1} does not have the same dimension", tleft.Count, tright.Count));

            foreach (var value in tleft)
            {
                if (!tright.ContainsKey(value.Key))
                    throw new FGRuntimeException("structure error", Fmt("right side does not have index {1}", value.Key));
                Memory.Result vleft = tleft[value.Key];
                Memory.Result vright = tright[value.Key];
                if (vleft.Type == ResultType.TUPLE && vright.Type == ResultType.TUPLE)
                    res.Add(value.Key, __LiftBinaryFnToTuple(vleft, vright, binary));
                else
                    res.Add(value.Key, binary(vleft, vright));
            }
            return new(res, ResultType.TUPLE);
        }

        private static Memory.Result __LiftUnaryFnToTuple(
            Memory.Result input,
            Func<Memory.Result, Memory.Result> unary)
        {
            var tuple = (Dictionary<string, Memory.Result>)input.Value;
            var res = new Dictionary<string, Memory.Result>();
            foreach (var item in tuple)
            {
                if (item.Value.Type == ResultType.TUPLE)
                    res.Add(item.Key, __LiftUnaryFnToTuple(item.Value, unary));
                else
                    res.Add(item.Key, unary(item.Value));
            }
            return new(res, ResultType.TUPLE);
        }

        private static bool __TypeIsAutoInfered(Token token)
        {
            return token.Type == TokenType.TYPE && token.Lexeme.Equals("auto");
        }

        public static string __StringifyResult(Memory.Result eval)
        {
            object value = eval.Value;

            if (eval.Type == ResultType.NULL)
                return "null";

            if (eval.Type == ResultType.VOID)
                throw new FGRuntimeException("type error", "cannot stringify void type");

            if (value == null)
                throw new FGRuntimeException("internal error", "processed value is not valid");

            if (eval.Type == ResultType.BOOLEAN)
                return (Boolean)value ? "true" : "false";

            if (eval.Type == ResultType.TUPLE)
            {
                var tup = (Dictionary<string, Memory.Result>) eval.Value;
                List<string> items = new();
                foreach(var entry in tup)
                {
                    string val = __StringifyResult(entry.Value);
                    if (entry.Value.Type == ResultType.STRING)
                        val = Fmt("\"{0}\"", val);
                    items.Add(entry.Key + ": " + val);
                }
                return Fmt("[{0}]", string.Join(", ", items));
            }

            if (eval.Type == ResultType.NUMBER)
                return value.ToString().Replace(",", ".");

            if (eval.Type == ResultType.NULL)
                return "null";

            // str, num
            return value.ToString();
        }
    }
}
