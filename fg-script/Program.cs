using fg_script.core;
using fg_script.utils;

try
{
    string source = "num   bool  tup   if else elif loop   true  false    and or is fn" +
                 "  extern expose ret somethingThatMeansNothing  err";
    Console.WriteLine(source);
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
