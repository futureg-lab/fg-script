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
            Assert.AreEqual("EOF", token.Lexeme);
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
            Assert.AreEqual(TokenType.NUMBER, token.Type);
            Assert.AreEqual(num, token.Lexeme);
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
            string source = "num define  bool  tup   if else elif for   true  false    and or is fn" +
                 "  extern expose ret somethingThatMeansNothing  err in";


            Lexer lexer = new(source, "<test>");
            List<Token> list = lexer.Tokenize();

            List<TokenType> expected = new()
            {
                TokenType.TYPE, TokenType.DEFINE, TokenType.TYPE, TokenType.TYPE,
                TokenType.IF, TokenType.ELSE, TokenType.ELSE_IF, TokenType.FOR, 
                TokenType.BOOL, TokenType.BOOL, 
                TokenType.AND, TokenType.OR, TokenType.IS, TokenType.FUN_DECL,
                TokenType.EXTERN, TokenType.EXPOSE, TokenType.RETURN, TokenType.KEYWORD_OR_NAME,
                TokenType.ERROR, TokenType.IN,
                TokenType.EOF
            };

            Assert.AreEqual(expected.Count, list.Count);

            for (int i = 0; i < expected.Count; i++)
            {
                Assert.AreEqual(expected[i], list[i].Type);
            }
        }

        [TestMethod]
        public void TestReservedSymbolsAndOperators()
        {
            string source = "+ \n\n  - :  *  /  =   % ==  !=  >=   <= < >  ! \n  ->  () {}[] , . .. ;";

            Lexer lexer = new(source, "<test>");
            List<Token> list = lexer.Tokenize();

            List<TokenType> expected = new()
            {
                TokenType.PLUS, TokenType.NEW_LINE, TokenType.NEW_LINE,
                TokenType.MINUS, TokenType.COLON, TokenType.MULT, TokenType.DIV, TokenType.ASSIGN,
                TokenType.MOD, TokenType.EQ, TokenType.NEQ, TokenType.GE, TokenType.LE,
                TokenType.LT, TokenType.GT, TokenType.NOT, TokenType.NEW_LINE, TokenType.RET_OP, 
                TokenType.LEFT_PARENTH, TokenType.RIGHT_PARENTH, TokenType.LEFT_BRACE, TokenType.RIGHT_BRACE,
                TokenType.LEFT_BRACKET, TokenType.RIGHT_BRACKET, TokenType.COMMA, TokenType.DOT, TokenType.DBL_DOT,
                TokenType.SEMICOLON,
                TokenType.EOF
            };

            Assert.AreEqual(expected.Count, list.Count);

            for (int i = 0; i < expected.Count; i++)
            {
                Assert.AreEqual(expected[i], list[i].Type);
            }
        }

        [TestMethod]
        public void TestCommentOfAllTypes()
        {
            string source =
                "/** Example function\n *v1.0 */" +
                "fn test (num x, num y) -> num {\n" +
                "\tif (x + y) % 2 is not 0 {\n" +
                "\t\tret 1 // we are good\n" +
                "\t} else {\n" +
                "\t\terr \"unable to solve\"\n" +
                "\t}\n" +
                "}";

            Lexer lexer = new(source, "<test>");
            List<Token> list = lexer.Tokenize();

            List<string> comments_expected = new()
            {
                "/** Example function\n *v1.0 */",
                "// we are good"
            };

            List<Token> comments_res = list.FindAll((Token token) =>
            {
                return token.Type == TokenType.COMMENT;
            });

            Assert.AreEqual(comments_expected.Count, comments_res.Count);

            for (int i = 0; i < comments_expected.Count; i++)
            {
                Assert.AreEqual(comments_expected[i], comments_res[i].Lexeme);
            }
        }

        [TestMethod]
        public void TestInvalidNestedMultilineComment()
        {
            // last */ should be ignored
            string source = "/* one /* two /* three */ */";
            Lexer lexer = new(source, "<test>");
            List<Token> list = lexer.Tokenize();
            List<string> expected = new()
            {
                "/* one /* two /* three */",
                "*/" // TokenType == KEYWORD_OR_NAME
            };

            Assert.AreEqual(3, list.Count);
            Assert.AreEqual(expected[0], list[0].Lexeme);
            Assert.AreEqual(expected[1], list[1].Lexeme);
        }

        [TestMethod]
        public void TestInvalidCommentShouldTriggerAnException()
        {
            string source = "/** Example non terminated comment \n\n ";

            Lexer lexer = new(source, "<test>");

            Assert.ThrowsException<SyntaxErrorException>(() =>
            {
                lexer.Tokenize();
            });
        }
    }
}