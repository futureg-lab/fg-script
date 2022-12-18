using fg_script.core;

namespace fg_script_test
{
    [TestClass]
    public class LexerTest
    {
        [TestMethod]
        public void TestEmptyStringShouldContainEOF()
        {
            Lexer lexer = new("", "<test>");
            var list = lexer.Tokenize();

            // last item is EOF
            Assert.AreEqual(1, list.Count);

            Token token = list.First();
            Assert.AreEqual(token.Lexeme, "EOF");
        }

        [TestMethod]
        public void TestMakeNumber()
        {
            string num = "122345.65787";
            Lexer lexer = new(num, "<test>");
            var list = lexer.Tokenize();

            // last item is EOF
            Assert.AreEqual(2, list.Count);
            
            Token token = list.First();
            Assert.AreEqual(token.Type, TokenType.NUMBER);
            Assert.AreEqual(token.Lexeme, num);
        }

        [TestMethod]
        public void TestMakeString()
        {
            string content = "one two three and a number 126487.68978";
            string str = String.Format("\"{0}\"", content);
            Lexer lexer = new(str, "<test>");
            var list = lexer.Tokenize();

            // last item is EOF
            Assert.AreEqual(2, list.Count);

            Token token = list.First();

            Assert.AreEqual(token.Type, TokenType.STRING);
            Assert.AreEqual(token.Lexeme, str);
        }

        [TestMethod]
        public void TestMakeNumberStringAndAlphaNum()
        {
            string source = "let myVar_123 \"123.6\" 123.6";
            Lexer lexer = new(source, "<test>");
            List<Token> list = lexer.Tokenize();
            
            List<string> expected = new() 
            {
                "let", "myVar_123",
                "\"123.6\"", "123.6", "EOF"
            };

            Assert.AreEqual(expected.Count, list.Count);

            for (int i = 0; i < expected.Count; i++)
            {
                Assert.AreEqual(expected[i], list[i].Lexeme);
            }
        }

        [TestMethod]
        public void TestSyntaxErrorInterminatedString()
        {
            string content = "Hello, world!";
            string str = String.Format("\"{0}", content);
            Lexer lexer = new(str, "<test>");

            Assert.ThrowsException<SyntaxErrorException>(() =>
            {
                lexer.Tokenize();
            });
        }

        [TestMethod]
        public void TestReservedKeywords()
        {
            string source = "num   bool  tup   if else elif loop   true  false    and or is fn" +
                 "  extern expose ret somethingThatMeansNothing  err";


            Lexer lexer = new(source, "<test>");
            List<Token> list = lexer.Tokenize();

            List<TokenType> expected = new()
            {
                TokenType.TYPE, TokenType.TYPE, TokenType.TYPE,
                TokenType.IF, TokenType.ELSE, TokenType.ELSE_IF, TokenType.LOOP, 
                TokenType.BOOL, TokenType.BOOL, 
                TokenType.AND, TokenType.OR, TokenType.IS, TokenType.FUN_DECL,
                TokenType.EXTERN, TokenType.EXPOSE, TokenType.RETURN, TokenType.KEYWORD_OR_NAME,
                TokenType.ERROR, 
                TokenType.EOF
            };

            Assert.AreEqual(expected.Count, list.Count);

            for (int i = 0; i < expected.Count; i++)
            {
                Assert.AreEqual(expected[i], list[i].Type);
            }
        }
    }
}