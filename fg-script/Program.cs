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
    string filePath = "../../../examples/assign.fg";
    source = Utils.ReadTextFile(filePath);

    Console.WriteLine(source);
    Lexer lexer = new(source, "");
    List<Token> list = lexer.Tokenize();

    foreach (Token token in list)
    {
        Console.WriteLine(token);
    }

    Parser parser = new(filePath, ref list);
    List<Stmt> stmts = parser.Run();


    PrintVisitor printer = new PrintVisitor();
    foreach (Stmt stmt in stmts)
    {
        Console.WriteLine(printer.Print(stmt));

        if (stmt is Func)
        {
            foreach (var arg in ((Func)stmt).Args)
            {
                Console.WriteLine(String.Format("** {0} => {1}", arg.Name, arg.DataType));
            }
        }
        // Console.WriteLine(stmt);
    }
}
catch (SyntaxErrorException syntax_excp)
{
    int min = syntax_excp.Cursor.Pos;
    Console.Error.WriteLine(syntax_excp);
    Console.Error.WriteLine("\n" + Utils.UnderlineText(source, min - 1, min));
}
catch (FGScriptException fgexception)
{
    Console.Error.WriteLine(fgexception.Message);
}