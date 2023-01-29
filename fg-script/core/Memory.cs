using System.ComponentModel;

namespace fg_script.core
{
    public enum ResultType
    {
        [Description("void")]
        VOID,
        [Description("str")]
        STRING,
        [Description("num")]
        NUMBER,
        [Description("bool")]
        BOOLEAN,
        [Description("tup")]
        TUPLE,
        [Description("null")]
        NULL,
        [Description("err")]
        ERROR
    }

    public class Memory
    {
        public List<Dictionary<string, Result>> MemStack { get; }

        public Memory()
        {
            MemStack = new List<Dictionary<string, Result>>();
            // init first mem
            MemStack.Add(new Dictionary<string, Result>());
        }

        public void MemPop()
        {
            MemStack.RemoveRange(MemStack.Count - 1, 1);
        }

        public void MemPush()
        {
            MemStack.Add(new Dictionary<string, Result>());
        }

        public void Store(string key, Result result)
        {
            MemStack.Last()[key] = result;
        }

        public void Replace(string var_name, Result new_value)
        {
            // current scope
            if (MemStack.Last().ContainsKey(var_name))
            {
                MemStack.Last()[var_name] = new_value;
                return;
            }
            // higher scope
            // reverse iterate
            for (int i = 0; i < MemStack.Count; i++)
            {
                var scope = MemStack[MemStack.Count - i - 1];
                if (scope.ContainsKey(var_name))
                {
                    scope[var_name] = new_value;
                    return;
                }
            }

            throw new FGScriptException("undefined value", var_name + " value is not defined anywhere", "");
        }

        public Result? GetValue(string var_name)
        {
            // current scope
            if (MemStack.Last().ContainsKey(var_name))
                return MemStack.Last()[var_name];
            // higher scope
            // reverse iterate
            for (int i = 0; i < MemStack.Count; i++)
            {
                var scope = MemStack[MemStack.Count - i - 1];
                if (scope.ContainsKey(var_name))
                    return scope[var_name];
            }
            return null;
        }

        public class Result
        {
            public object? Value { get; }
            public ResultType Type { get; }
            public Result(object? value, ResultType type)
            {
                Value = value;
                Type = type;
            }

            public Result(ResultType type) 
            {
                Type = type;
                Value = null;
            }

            public static Result Void()
            {
                return new(ResultType.VOID);
            }

            public static Result Null()
            {
                return new(ResultType.NULL);
            }
        }

        public void DebugStackMemory()
        {
            int depth = 0;
            foreach(var scope in MemStack)
            {
                depth++;
                foreach(var variable in scope)
                {
                    string name = variable.Key;
                    Result value = variable.Value;
                    Console.WriteLine("{0}::{1} => {2}", name, value.Type, value.Value);
                }
                Console.WriteLine("------- depth ----- {0} --- var_count {1}", depth, MemStack.Count);
            }
        }
    }
}
