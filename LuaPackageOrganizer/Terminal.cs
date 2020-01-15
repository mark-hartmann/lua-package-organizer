using System;
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

    class Terminal
    {
    }
}