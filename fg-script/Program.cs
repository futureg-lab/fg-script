using fg_script.core;
using fg_script.utils;

try
{
    string filePath = "../../../examples/fn_decl.fg";
    string source = Utils.ReadTextFile(filePath);
    Console.WriteLine(source);
    Lexer lexer = new(source, "");
    List<Token> list = lexer.Tokenize();

    foreach (Token token in list)
    {
        Console.WriteLine(token);
    }

    Parser parser = new(filePath, ref list);
    List<Stmt> stmts = parser.Run();
    foreach (Stmt stmt in stmts)
    {
        if (stmt is Func)
        {
            foreach (var arg in ((Func)stmt).Args)
            {
                Console.WriteLine(String.Format("** {0} => {1}", arg.Name, arg.DataType));
            }
        }
        Console.WriteLine(stmt);
    }
}
catch (FGScriptException fgexception)
{
    Console.Error.WriteLine(fgexception.Message);
}
catch (Exception exception)
{
    throw exception;
}
