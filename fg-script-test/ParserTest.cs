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

        public string Compress(string str)
        {
            return str
                .Replace("\n", "")
                .Replace("\t", "")
                .Replace("\r", "")
                .Replace(" ", "");
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

        [TestMethod]
        public void BasicStatements()
        {
            string source = @"
                someFuncCall();
                anotherFuncCall(5, 1 * 2 + 4, false);
                num x = null;
                bool y = true or false;
                ret 1 + 1;
                err ""some error"";
            ";
            List<string> tests = new()
            {
                "(#root_call someFuncCall())",
                "(#root_call anotherFuncCall(5, (+ (* 1 2) 4), false))",
                "(num:x => null)",
                "(bool:y => (or true false))",
                "(#return (+ 1 1))",
                "(#error \"some error\")"
            };
            TestSetFrom(source, tests);
        }


        [TestMethod]
        public void ExposeAndFuncDeclarationWithBlock()
        {
            string source = @"
                expose fn sayHelloMaj(str name) -> str {
                    str res = upperCase(""Hello "" + name);
                    ret res + ""!"";
                }
            ";

            string expected = @"
                (#expose (#declare sayHelloMaj (str:name) -> str
                    (str:res => upperCase((+ ""Hello "" name)))
                    (#return (+ res ""!""))))
            ";

            Lexer lexer = new(source);
            List<Token> tokens = lexer.Tokenize();
            Parser parser = new("<test>", ref tokens);
            List<Stmt> stmts = parser.Run();

            // expose
            Assert.AreEqual(1, stmts.Count);
            
            Expose stmt = (Expose)stmts.First();
            Func func = stmt.ExposedFunc;
            Block? body = func.Body;

            Assert.IsNotNull(body);
            Assert.AreEqual(2, body.Statements.Count);

            PrintVisitor printer = new PrintVisitor();
            string stringified = printer.Print(stmt);
            Assert.AreEqual(Compress(expected), Compress(stringified));
        }

        [TestMethod]
        public void ExternFuncDeclaration()
        {
            string source = "extern fn fetchData(str url, num param) -> str;";

            string expected = "(#extern (#declare fetchData(str:url, num:param) -> str)\n)";

            Lexer lexer = new(source);
            List<Token> tokens = lexer.Tokenize();
            Parser parser = new("<test>", ref tokens);
            List<Stmt> stmts = parser.Run();

            // extern
            Assert.AreEqual(1, stmts.Count);
            Extern stmt = (Extern)stmts.First();
            Func func = stmt.ExternalFunc;

            // the func block should be null
            Assert.IsNull(func.Body);


            PrintVisitor printer = new PrintVisitor();
            string stringified = printer.Print(stmt);
            Assert.AreEqual(Compress(expected), Compress(stringified));
        }

        [TestMethod]
        public void ExternFuncWithBodyParseShouldFail()
        {
            string source = @"
                // extern always expects the statement to end
                // with a ; after the ret type
                extern fn someFunc(str x, num y, tup z) -> str {
                    ret ""some stuff"";
                }
            ";

            Lexer lexer = new(source);
            List<Token> tokens = lexer.Tokenize();
            Assert.ThrowsException<SyntaxErrorException>(() =>
            {
                Parser parser = new("<test>", ref tokens);
                parser.Run();
            });
        }
    }
}
