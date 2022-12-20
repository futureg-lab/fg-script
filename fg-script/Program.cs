using fg_script.core;
using fg_script.utils;

try
{
    string filePath = "../../../examples/basics.fg";
    string source = Utils.ReadTextFile(filePath);
    Console.WriteLine(source);
    Lexer lexer = new(source, "");
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
