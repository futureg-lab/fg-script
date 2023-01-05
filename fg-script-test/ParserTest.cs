using fg_script.core;

namespace fg_script_test
{
    [TestClass]
    public class ParserTest
    {
        [TestMethod]
        public void BasicExpressionOrder()
        {
            string source = @"
                num x = 1 + 2 * 3;
                num y = 1 * 2 + 3;
                bool q = 2 * (1 + 2) * 3 <= 3 and 4 != 8;

                // should be interpreted as
                // (2 + 3) <= 4
                bool a = 2 + 3 <= 4;

                // should be interpreted as
                // ((2*3)+3) != 4
                bool b = 2 * 3 + 3 != 4;
            ";

            Lexer lexer = new(source);
            List<Token> tokens = lexer.Tokenize();

            Parser parser = new("<test>", ref tokens);
            List<Stmt> stmts = parser.Run();

            PrintVisitor printer = new PrintVisitor();
            List<string> tests = new()
            {
                "(num:x => (+ 1 (* 2 3)))",
                "(num:y => (+ (* 1 2) 3))",
                "(bool:q => (<= (* 2 (* (+ 1 2) 3)) (!= (and 3 4) 8)))",
                "(bool:a => (<= (+ 2 3) 4))",
                "(bool:b => (!= (+ (* 2 3) 3) 4))"
            };

            Assert.AreEqual(tests.Count, stmts.Count);

            for (int i = 0; i < tests.Count; i++)
                Assert.AreEqual(printer.Print(stmts[i]), tests[i]);
        }
    }
}
