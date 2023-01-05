using fg_script.core;

namespace fg_script_test
{
    [TestClass]
    public class ParserTest
    {
        public void TestSetFrom(string source, List<string> tests)
        {

            Lexer lexer = new(source);
            List<Token> tokens = lexer.Tokenize();

            Parser parser = new("<test>", ref tokens);
            List<Stmt> stmts = parser.Run();

            PrintVisitor printer = new PrintVisitor();

            Assert.AreEqual(tests.Count, stmts.Count);

            for (int i = 0; i < tests.Count; i++)
                Assert.AreEqual(printer.Print(stmts[i]), tests[i]);
        }

        [TestMethod]
        public void BasicExpressionOrder()
        {
            string source = @"
                num x = 1 + 2 * 3;
                num y = 1 * 2 + 3;
                num z = (1 + 2) * 3;
            ";
            List<string> tests = new()
            {
                "(num:x => (+ 1 (* 2 3)))",
                "(num:y => (+ (* 1 2) 3))",
                "(num:z => (* (+ 1 2) 3))"
            };
            TestSetFrom(source, tests);
        }

        [TestMethod]
        public void BasicBooleanExpressionOrder()
        {
            string source = @"
                bool e = x or y and z;
                bool f = x and y or z;
            ";
            List<string> tests = new()
            {
                "(bool:e => (or x (and y z)))",
                "(bool:f => (or (and x y) z))",
            };
            TestSetFrom(source, tests);
        }

        [TestMethod]
        public void GeneralExpressionOrder()
        {
            string source = @"
                // (2 + 3) <= 4
                bool a = 2 + 3 <= 4;
                // ((2*3)+3) != 4
                bool b = 2 * 3 + 3 != 4;
            
                bool q = 2 * (1 + 2) * 3 <= 3 and 4 != 8;
            ";
            List<string> tests = new()
            {
                "(bool:a => (<= (+ 2 3) 4))",
                "(bool:b => (!= (+ (* 2 3) 3) 4))",
                "(bool:q => (<= (* 2 (* (+ 1 2) 3)) (and 3 (!= 4 8))))",
            };
            TestSetFrom(source, tests);
        }
    }
}
