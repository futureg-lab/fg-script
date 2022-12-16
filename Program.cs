using fg_script.core;
using fg_script.utils;

/*
List<Token> list = new List<Token>();
Parser parser = new Parser(ref list);

Console.WriteLine(Utils.UnderlineText("Hello, World!", 3, 8));
*/

try
{
    string source = "let x = \"hello world;\n 1234.65    145!";
    Lexer lexer = new Lexer(source, "<none>");
    List<Token> list = lexer.Tokenize();

    foreach (Token token in list)
    {
        Console.WriteLine(token);
    }
} catch(FGScriptException fgexception)
{
    Console.Error.WriteLine(fgexception.Message);
} catch(Exception exception)
{
    throw exception;
}
