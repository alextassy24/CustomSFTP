using CustomSftpTool.Data;

namespace CustomSftpTool.Commands
{
    public class Message
    {
        public static void Display(string message, MessageType messageType)
        {
            var color = messageType switch
            {
                MessageType.Info => ConsoleColor.White,
                MessageType.Warning => ConsoleColor.Yellow,
                MessageType.Error => ConsoleColor.Red,
                MessageType.Success => ConsoleColor.Green,
                MessageType.Debug => ConsoleColor.Cyan,
                _ => ConsoleColor.White,
            };

            Console.ForegroundColor = color;

            Console.WriteLine(message);

            Console.ResetColor();
        }
    }
}
