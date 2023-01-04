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
            if (start > end)
                throw new Exception("start should be less than end");
            start = Math.Max(0, start);
            int initialLength = Math.Max(end, text.Length);
            text += "\n";
            for (int i = 0; i < initialLength; i++)
            {
                text += (i >= start && i < end ? "^" : "-");
            }
            return text + "\n";
        }

        public static string UnderlineTextLine(string text, CursorPosition pos, int offset = 0)
        {
            string[] splits = text.Split("\n");
            string line = splits[pos.Line - 1];
            return UnderlineText(line, pos.Col - 2 + offset, pos.Col - 1 + offset);
        }
    }
}