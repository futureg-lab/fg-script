namespace fg_script.utils 
{
    public class Utils
    {
        public static string? ReadTextFile(string filePath)
        {
            return "";
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