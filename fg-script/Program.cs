using fg_script.core;
using fg_script.utils;


string source = "";

try
{
    /*
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
    }*/
    string filePath = "../../../examples/tuple.fg";
    source = Utils.ReadTextFile(filePath);

    Lexer lexer = new(source, "");
    List<Token> list = lexer.Tokenize();

    Parser parser = new(filePath, ref list);
    List<Stmt> stmts = parser.Run();

    PrintVisitor printer = new PrintVisitor();
    Interpreter engine = new Interpreter();
    foreach (Stmt stmt in stmts)
    {
        // Console.WriteLine(printer.Print(stmt));
        engine.Run(stmt);
    }
    // engine.Machine.DebugStackMemory();
}
catch (SyntaxErrorException syntax_excp)
{
    Console.Error.WriteLine(Utils.UnderlineTextLine(source, syntax_excp.Cursor, 1));
    Console.Error.Write(syntax_excp.Message);
}
catch (FGScriptException fgexception)
{
    Console.Error.WriteLine(fgexception.Message);
}
catch (Exception e)
{
    Console.Error.WriteLine(e);
}