namespace CustomSftpTool
{
    public static class ConsoleColors
    {
        public static string Red(string value)
        {
            return $"\u001b[31m{value}\u001b[0m";
        }

        public static string Green(string value)
        {
            return $"\u001b[32m{value}\u001b[0m";
        }

        public static string Yellow(string value)
        {
            return $"\u001b[33m{value}\u001b[0m";
        }

        public static string Cyan(string value)
        {
            return $"\u001b[36m{value}\u001b[0m";
        }
    }
}
