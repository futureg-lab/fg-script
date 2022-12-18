using fg_script.core;
using fg_script.utils;

try
{
    string source = 
        "/** Example function v1.0 */" +
        "fn test (num x, num y) -> num {\n" +
        "\tif (x + y) % 2 is not 0 {\n" +
        "\t\tret 1 // we are good\n" +
        "\t} else {\n" +
        "\t\terr \"unable to solve\"\n"+
        "\t}\n" +
        "}";
    Console.WriteLine(source);
    Lexer lexer = new(source, "<none>");
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
