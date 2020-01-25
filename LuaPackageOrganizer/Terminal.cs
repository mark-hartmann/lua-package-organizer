using System;
using System.Drawing;
using Console = Colorful.Console;

namespace LuaPackageOrganizer
{
    public class ProgressBar : IDisposable
    {
        private readonly int _cposl;
        private readonly int _cpost;
        private readonly bool _cursorVisible;

        public ProgressBar()
        {
            _cpost = Console.CursorTop;
            _cposl = Console.CursorLeft;
            _cursorVisible = Console.CursorVisible;

            Console.CursorVisible = false;
        }

        public void Refresh(int progress, string message)
        {
            Console.SetCursorPosition(_cposl, _cpost);

            var percentage = progress.ToString().PadLeft(3, ' ');
            var progressBar = new string('=', progress / 10).PadRight(10, '-');

            Console.Write($"[{progressBar}] {percentage}% {message}");
        }

        public void Dispose()
        {
            Console.CursorVisible = _cursorVisible;
        }
    }

    internal static class Terminal
    {
        public static void WriteError(string message) => WriteLine("ERROR", message, Color.Firebrick);
        public static void WriteDebug(string message) => WriteLine("DEBUG", message, Color.Olive);
        public static void WriteNotice(string message) => WriteLine("INFO", message, Color.Gray);
        public static void WriteSuccess(string message) => WriteLine("SUCCESS", message, Color.Green);

        private static void WriteLine(string type, string message, Color messageColor)
        {
            // Makes the messages appearing nicely
            var messageType = $"[{type}]".PadLeft(9, ' ');
            
            Console.Write($"{DateTime.Now} ", Color.Gray);   
            Console.Write($"{messageType} ", Color.Olive);   
            Console.WriteLine($"{message}", messageColor);   
        }
    }
}