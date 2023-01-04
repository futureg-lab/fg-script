using fg_script.utils;

namespace fg_script_test
{
    [TestClass]
    public class UtilsTest
    {
        [TestMethod]
        public void UnderlinedTextShouldAppearCorrectly()
        {
            string initString = "Hello, World!";
            string expectedString = "Hello, World!" +
                                  "\n---^^^^------\n";
            string resultString = Utils
                .UnderlineText(initString, 3, 7);

            Assert.AreEqual(expectedString, resultString);
        }
    }
}