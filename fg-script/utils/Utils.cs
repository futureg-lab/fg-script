using fg_script.core;

namespace fg_script.utils 
{
    public class Utils
    {
        public static string ReadTextFile(string filePath)
        {
            try
            {
                return File.ReadAllText(filePath);
            }
            catch (IOException exception)
            {
                throw new FileException(exception.Message, exception.StackTrace ?? "");
            }
        }

        public static string UnderlineText(string text, int start, int end)
        {
            int initialLength = text.Length;
            text += "\n";
            for (int i = 0; i < initialLength; i++)
                text += (i >= start && i < end ? "^" : "_");
            return text + "\n";
        }
    }
}