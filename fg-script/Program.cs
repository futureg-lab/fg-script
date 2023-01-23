using fg_script.core;
using fg_script.utils;

// TODO:
// fg-script [print|run] [filename] [-h|--help]
// print => prints the AST
// run => runs the script
// no args => interactive mode


void loadfile()
{
    string source = "";
    try
    {
        string filePath = "../../../examples/tuple_to_json.fg";
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
    List<string> lines = new();
    int depth = 0;
    while (true)
    {
        string source = "";
        try
        {
            if (depth <= 0)
                Console.Write("\n>>> ");
            string? current = Console.ReadLine();
            if (current == null || "".Equals(current))
                continue;
            current = current.Trim();
            
            // start : check current line
            HashSet<TokenType> open = new()
            {
               TokenType.LEFT_BRACE,
               TokenType.LEFT_BRACKET,
               TokenType.LEFT_PARENTH
            };
            HashSet<TokenType> closed = new()
            {
               TokenType.RIGHT_BRACE,
               TokenType.RIGHT_BRACKET,
               TokenType.RIGHT_PARENTH
            };

            Lexer lexer_line = new(current, "");
            List<Token> line_tokens = lexer_line.Tokenize();
            foreach (Token token in line_tokens)
            {
                if (open.Contains(token.Type)) depth++;
                if (closed.Contains(token.Type)) depth--;
            }
            // end : check current line
            lines.Add(current);

            if (depth > 0)
                continue;

            source = string.Join("\n", lines);

            if (!source.EndsWith("}") && !source.EndsWith(";") && lines.Count == 1)
                source += ";";
            lines.Clear();

            // if depth < 0, let the parser handle the error
            Lexer lexer = new(source, "");
            List<Token> list = lexer.Tokenize();

            Parser parser = new("<interactive>", ref list);
            List<Stmt> stmts = parser.Run();
            foreach (Stmt stmt in stmts)
            {
                object? eval_if_any = engine.Run(stmt);
                if (stmt is RootExpression && eval_if_any != null)
                {
                    var res = (Memory.Result)eval_if_any;
                    if (res.Type == ResultType.VOID)
                        continue;
                    Console.Write(Interpreter.__StringifyResult(res));
                }
            }
        }
        catch (SyntaxErrorException syntax_excp)
        {
            depth = 0;
            lines.Clear();
            Console.Error.Write(Utils.UnderlineTextLine(source, syntax_excp.Cursor, 1));
            Console.Error.Write(syntax_excp.Message);
        }
        catch (FGScriptException fgexception)
        {
            depth = 0;
            lines.Clear();
            Console.Error.Write(fgexception.Message);
        }
        catch (Exception e)
        {
            depth = 0;
            lines.Clear();
            Console.Error.WriteLine(e);
        }
    }
}

interactive();
// loadfile();