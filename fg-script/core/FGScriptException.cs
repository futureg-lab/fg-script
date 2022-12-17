namespace fg_script.core
{
    public class FGScriptException : Exception
    {
        public FGScriptException(string what, string reason, string full_description)
        :   base(String.Format("FG-Script {0} error : {1}\n {2}", what, reason, full_description))
        {
        }
    }

    public class RuntimeException : FGScriptException
    {
        public RuntimeException(string reason, string full_description = "") 
            : base("runtime", reason, full_description)
        {
        }
    }

    public class SyntaxErrorException : FGScriptException
    {
        public SyntaxErrorException(string reason, CursorPosition cursor, string filename, string full_description = "")
            : base(
                  "parsing",
                  String.Format("{0} :: {1}, filename {2}",
                    reason,
                    cursor,
                    filename
                  ),
                    full_description
                )
        {
        }
    }
}
