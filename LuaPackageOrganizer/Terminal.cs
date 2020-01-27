﻿using System;
using System.Drawing;
using System.Linq;
using Colorful;
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
        public enum MessageType
        {
            Error,
            Debug,
            Notice,
            Success,
        }

        public static void WriteError(string message) => WriteLine(MessageType.Error, message, Color.Firebrick);

        public static void WriteDebug(string message) => WriteLine(MessageType.Debug, message, Color.Gray);

        public static void WriteNotice(string message) => WriteLine(MessageType.Notice, message, Color.LightGray);

        public static void WriteSuccess(string message) => WriteLine(MessageType.Success, message, Color.LightGreen);

        public static void WriteLine(MessageType? type, string message, Color textColor)
        {
            WriteLine(type, message, new StyleSheet(textColor));
        }

        public static void WriteLine(MessageType? type, string message, StyleSheet styleSheet)
        {
            Write(type, message + Environment.NewLine, styleSheet);
        }

        public static void Write(MessageType? type, string message, Color textColor)
        {
            Write(type, message, new StyleSheet(textColor));
        }

        public static void Write(MessageType? type, string message, StyleSheet stylesheet)
        {
            // Resolves the longest name so it can be used to set the totalWidth
            var totalWidth = Enum.GetNames(typeof(MessageType)).Max(s => s.Length);
            var messageTypeName = type != null ? Enum.GetName(typeof(MessageType), type) : "";
            
            // totalWidth + 2 because of the braces around the message type
            var messageType = $"[{messageTypeName}]".PadLeft(totalWidth + 2, ' ');

            Console.Write($"{DateTime.Now} ", Color.LightGray);
            Console.Write($"{messageType} ", Color.Olive);

            Console.WriteStyled($"{message}", stylesheet);
        }
    }
}