using fg_script.core;
using fg_script.utils;

void loadfile()
{
    string source = "";
    try
    {
        string filePath = "../../../examples/scanln.fg";
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
}


void interactive()
{
    Console.WriteLine(" Welcome to fg-script interactive v0.0.1 :: FutureG-lab");
    Interpreter engine = new();
    while (true)
    {
        string source = "";
        try
        {
            Console.Write("\n>>> ");
            string? current = Console.ReadLine();
            if (current == null || "".Equals(current))
                continue;

            source = current.Trim();

            if (!source.EndsWith("}") && !source.EndsWith(";"))
                source += ";";

            Lexer lexer = new(source, "");
            List<Token> list = lexer.Tokenize();

            Parser parser = new("<interactive>", ref list);
            List<Stmt> stmts = parser.Run();
            foreach (Stmt stmt in stmts)
            {
                object? eval_if_any = engine.Run(stmt);
                if (stmt is RootExpression && eval_if_any != null)
                    Console.Write(Interpreter.__StringifyResult((Memory.Result) eval_if_any));
            }
        }
        catch (SyntaxErrorException syntax_excp)
        {
            Console.Error.Write(Utils.UnderlineTextLine(source, syntax_excp.Cursor, 1));
            Console.Error.Write(syntax_excp.Message);
        }
        catch (FGScriptException fgexception)
        {
            Console.Error.Write(fgexception.Message);
        }
        catch (Exception e)
        {
            Console.Error.WriteLine(e);
        }
    }
}

interactive();