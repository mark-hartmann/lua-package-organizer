using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Pastel;

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

            Console.Write($"[{progressBar}] {percentage}% ".Pastel(Color.Coral) + message);
        }

        public void Dispose()
        {
            Console.CursorVisible = _cursorVisible;
        }
    }

    internal static class Terminal
    {
        public enum MessageType
        {
            Error,
            Debug,
            Notice,
            Success,
            Warning
        }

        public static void WriteError(string message, params string[] additionalMessages) =>
            WriteLine(MessageType.Error, message, Color.Firebrick, true, additionalMessages);

        public static void WriteDebug(string message, params string[] additionalMessages) =>
            WriteLine(MessageType.Debug, message, Color.Gray, true, additionalMessages);

        public static void WriteNotice(string message, params string[] additionalMessages) =>
            WriteLine(MessageType.Notice, message, Color.LightGray, true, additionalMessages);
        
        public static void WriteWarning(string message, params string[] additionalMessages) =>
            WriteLine(MessageType.Warning, message, Color.Olive, true, additionalMessages);

        public static void WriteSuccess(string message, params string[] additionalMessages) =>
            WriteLine(MessageType.Success, message, Color.LightGreen, true, additionalMessages);

        public static void WriteLine(MessageType? type, string message, Color textColor, bool showTime = true,
            IEnumerable<string> additionalMessages = null)
        {
            Write(type, message + Environment.NewLine, textColor, showTime);

            if (additionalMessages == null)
                return;

            foreach (var additionalMessage in additionalMessages)
            {
                Write(null, additionalMessage + Environment.NewLine, Color.LightGray, false);
            }
        }

        public static void Write(MessageType? type, string message, Color textColor, bool showTime = true)
        {
            // Resolves the longest name so it can be used to set the totalWidth
            var totalWidth = Enum.GetNames(typeof(MessageType)).Max(s => s.Length);
            var messageTypeName = type != null ? Enum.GetName(typeof(MessageType), type) : "";

            // totalWidth + 2 because of the braces around the message type
            var messageType = $"{messageTypeName?.ToLower()}".PadLeft(totalWidth + (showTime ? 0 : 14), ' ');

            if (showTime)
                Console.Write($"{DateTime.Now:HH:mm:ss.ffff} ");

            Console.Write($"{messageType} ".Pastel(Color.Olive));
            Console.Write($"{message}".Pastel(textColor));
        }
    }
}