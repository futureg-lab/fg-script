using fg_script.core;
using System.Reflection;

namespace fg_script_test
{
    [TestClass]
    public class ProgramTest
    {
        public string Compress(string str)
        {
            return str
                .Replace("\n", "")
                .Replace("\t", "")
                .Replace("\r", "")
                .Replace(" ", "");
        }

        public void TestSetFrom(string source, List<string> tests, bool compress = false)
        {

            Lexer lexer = new(source);
            List<Token> tokens = lexer.Tokenize();
            Parser parser = new("<test>", ref tokens);
            List<Stmt> stmts = parser.Run();
            PrintVisitor printer = new PrintVisitor();

            Assert.AreEqual(tests.Count, stmts.Count);
            for (int i = 0; i < tests.Count; i++)
            {
                string expected = tests[i];
                string got = printer.Print(stmts[i]);
                if (!compress)
                    Assert.AreEqual(expected, got);
                else
                    Assert.AreEqual(Compress(expected), Compress(got));
            }
        }

        [TestMethod]
        public void BasicExpressionOrder()
        {
            string source = @"
                num x = 1 + 2 * 3;
                num y = 1 * 2 + 3;
                num z = (1 + 2) * 3;
                auto un = repr_of 2 + - 3;
            ";
            List<string> tests = new()
            {
                "(num:x => (+ 1 (* 2 3)))",
                "(num:y => (+ (* 1 2) 3))",
                "(num:z => (* (+ 1 2) 3))",
                "(auto:un => (+ (repr_of 2) (- 3)))"
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
            
                bool q = someArray[2 * (1 + 2)] * 3 <= 3 and 4 != 8;
            ";
            List<string> tests = new()
            {
                "(bool:a => (<= (+ 2 3) 4))",
                "(bool:b => (!= (+ (* 2 3) 3) 4))",
                "(bool:q => (and (<= (* someArray[(* 2 (+ 1 2))] 3) 3) (!= 4 8)))",
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
                2 * 4 + x * (someCall() + z[1 + 1]);
                
                str s = ""some value"";
                s = ""new value"";
            ";
            List<string> tests = new()
            {
                "(#root someFuncCall())",
                "(#root anotherFuncCall(5, (+ (* 1 2) 4), false))",
                "(num:x => null)",
                "(bool:y => (or true false))",
                "(#return (+ 1 1))",
                "(#error \"some error\")",
                "(#root (+ (* 2 4) (* x (+ someCall() z[(+ 1 1)]))))",
                "(str:s => \"some value\")",
                "(#reassign s => \"new value\")",
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

        [TestMethod]
        public void TupleExprShouldParse()
        {
            string source = @"
                tup a = [1, 2, 3];
                tup b = [1, 2, [4, 5], 6, [7, 8], [], 9];
                tup c = [a : 1, b : 4, ""x"" : [3, 4], z : [8 : 3, 4 : 9]];
                tup d = [45 : ""some value"", hello: world, x: some_func(), z: (1 + 3) * 4];
                tup e = [1, 2, 3, [2, [3, [x : 4]]]];
            ";
            List<string> tests = new()
            {
                "(tup:a => [0:1, 1:2, 2:3])",
                "(tup:b => [0:1, 1:2, 2:[0:4, 1:5], 3:6, 4:[0:7, 1:8], 5:[], 6:9])",
                "(tup:c => [a:1, b:4, \"x\":[0:3, 1:4], z:[8:3, 4:9]])",
                "(tup:d => [45:\"some value\", hello:world, x:some_func(), z:(* (+ 1 3) 4)])",
                "(tup:e => [0:1, 1:2, 2:3, 3:[0:2, 1:[0:3, 1:[x:4]]]])"
            };
            TestSetFrom(source, tests);
        }

        [TestMethod]
        public void ForAndWhileLoopTest()
        {
            string source = @"
                while x and y {
	                num z = random();
	                print(z + ""yay!"");
                }
                
                for x in [1, 2, 3] {
	                print(""hello world!"");
                }

                for (k, v) in [1, 2, 3] * 2 + 3 {
	                print(""hello world!"");
                }

                for x in 1 .. 10 {
	                print(x);
	                if x >= 3 and x < 5 {
		                continue;
	                } elif x < 9 {
		                break;
	                }
                }
            ";
            List<string> tests = new()
            {
                @"
                (#while (and x y)
                    (num:z => random())
                    (#root print((+ z ""yay!""))))",
                @"
                (#for (, x) #in [0:1, 1:2, 2:3]
                    (#root print(""hello world!"")))",
                @"
                (#for (k, v) #in (+ (* [0:1, 1:2, 2:3] 2) 3)
                    (#root print(""hello world!"")))",
                @"
                (#for (, x) #in (#enum_from 1 #to 10)
                    (#root print(x))
                    (#if (and (>= x 3) (< x 5)) =>
                        (#continue)
                    (#branch (< x 9) =>
                        (#break))))"
            };
            TestSetFrom(source, tests, true);
        }
        [TestMethod]
        public void SampleProgram()
        {
            string source = @"
                fn to_json(auto value) -> str {
                    // println(""repr is "" + repr_of value);
                    if (repr_of value) is ""tup"" {
                        str tmp = ""{"";
                        num size = len(value);
                        num i = 0;
                        for (k, v) in value {
                            str row = ""\"""" + k + ""\""""+ "" : "" + to_json(v);
                            tmp = tmp + row;
                            if i + 1 < size {
                                tmp = tmp + "","";
                            }
                            i = i + 1;
                        }
                        tmp = tmp + ""}"";
                        ret tmp;
                    }
                    if repr_of value is ""str"" {
                        ret ""\"""" + value + ""\"""";
                    }
                    ret to_str(value);
                }

                auto out = to_json([
                    a: 1,
                    b: [c : 3],
                    c: [1, 2],
                    d: [
                        h: ""some str"",
                        e: ""another""
                    ]
                ]);
            ";

            Lexer lexer = new(source, "<test>");
            List<Token> list = lexer.Tokenize();

            Parser parser = new("<test>", ref list);
            List<Stmt> stmts = parser.Run();
            Interpreter engine = new();

            foreach (Stmt stmt in stmts)
            {
                engine.Run(stmt);
            }

            string expected = "{\"a\" : 1,\"b\" : {\"c\" : 3},\"c\" : {\"0\" : 1,\"1\" : 2},\"d\" : {\"h\" : \"some str\",\"e\" : \"another\"}}";
            Assert.AreEqual(expected, engine.Machine.GetValue("out")?.Value);
        }

    }
}
