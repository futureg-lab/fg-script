using fg_script.core;
using fg_script.utils;

try
{
    string source = "let myVar_123 \"123.6\" 123.6";
    Lexer lexer = new Lexer(source, "<none>");
    List<Token> list = lexer.Tokenize();

    foreach (Token token in list)
    {
        Console.WriteLine(token);
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
