using System;
using System.Drawing;
using System.Globalization;
using Pastel;

namespace LuaPackageOrganizer
{
    public class ProgressBar : IDisposable
    {
        private readonly int _cposl;
        private readonly int _cpost;
        private readonly bool _cursorVisible;
        private readonly int _maxWidth;

        public ProgressBar(int maxWidth = 10)
        {
            _cpost = Console.CursorTop;
            _cposl = Console.CursorLeft;
            _cursorVisible = Console.CursorVisible;
            _maxWidth = maxWidth;

            Console.CursorVisible = false;
        }

        public void Refresh(float progress, string message)
        {
            Console.SetCursorPosition(_cposl, _cpost);

            var width = (int) (progress / 100 * _maxWidth);
            var fill = _maxWidth - width;

            var percentage = progress.ToString(CultureInfo.InvariantCulture).PadLeft(3, ' ');
            var output =
                $"[{string.Empty.PadLeft(width, '=')}{string.Empty.PadLeft(fill, '-')}] {percentage}% ";

            Console.Write(output.Pastel(Color.Coral) + message);
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
    }
}