namespace fg_script.core
{
    public enum ResultType
    {
        VOID,
        STRING,
        NUMBER,
        BOOLEAN,
        TUPLE,
        NULL
    }

    public class Memory
    {
        public Stack<Dictionary<string, Result>> MemStack { get; }
        public Stack<FuncCall> CallStack { get; }

        public Memory()
        {
            MemStack = new Stack<Dictionary<string, Result>>();
            CallStack = new Stack<FuncCall>();

            // init first mem
            MemStack.Push(new Dictionary<string, Result>());
        }

        public void MemPop()
        {
            MemStack.Pop();
        }

        public void MemPush()
        {
            MemStack.Push(new Dictionary<string, Result>());
        }

        public void Store(string key, Result result)
        {
            MemStack.Peek()[key] = result;
        }

        public Result? GetValue(string var_name)
        {
            // current scope
            if (MemStack.Peek().ContainsKey(var_name))
                return MemStack.Peek()[var_name];
            // higher scope
            foreach (var scope in MemStack)
                if (scope.ContainsKey(var_name))
                    return scope[var_name];
            return null;
        }

        public class Result
        {
            public object Value { get; }
            public ResultType Type { get; }
            public Result(object value, ResultType type)
            {
                Value = value;
                Type = type;
            }

            public Result(ResultType type) 
            {
                Type = type;
                Value = "none";
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
