namespace fg_script.core
{
    public enum ResultType
    {
        VOID,
        STRING,
        NUMBER,
        BOOLEAN,
        TUPLE,
        NONE
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
            MemStack.Last()[key] = result;
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
                Console.WriteLine("------- depth ----- {0} {1}", depth, MemStack.Count);
            }
        }
    }
}
