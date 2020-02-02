using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Pastel;
using TextProcessor;

namespace LuaPackageOrganizer.Commands.Output
{
    public class BasicOutput : IOutput
    {
        private readonly MessageProcessor _messageProcessor = new MessageProcessor();
        private static string Time => DateTime.Now.ToString("HH:mm:ss.ffff");

        public BasicOutput()
        {
            var messageTypes = Enum.GetNames(typeof(Terminal.MessageType));
            var maxLength = messageTypes.Max(mt => mt.Length);

            var colorMappings = new Dictionary<string, Color>
            {
                {"warning", Color.Olive},
                {"error", Color.Firebrick},
                {"success", Color.LightGreen},
                {"notice", Color.LightGray},
                {"debug", Color.DarkGray},
            };

            _messageProcessor.AddCustomNode("time", () => Time + ' ');

            foreach (var messageType in messageTypes)
            {
                _messageProcessor.AddCustomNode(messageType.ToLower(),
                    () => messageType.ToLower().PadLeft(maxLength, ' ').Pastel(Color.Olive));

                _messageProcessor.AddCustomNode(messageType.ToLower(),
                    s => s.Pastel(colorMappings[messageType.ToLower()]));
            }
        }

        public void Write(string message)
        {
            Console.Write(_messageProcessor.Process(message));
        }

        public void WriteLine(string message)
        {
            Console.WriteLine(_messageProcessor.Process(message));
        }

        public void WriteError(params string[] messages)
        {
            foreach (var message in messages)
                WriteLine($"<time /> <error /> <error> {message}</error>");
        }

        public void WriteNotice(params string[] messages)
        {
            foreach (var message in messages)
                WriteLine($"<time /> <notice /> <notice> {message}</notice>");
        }

        public void WriteSuccess(params string[] messages)
        {
            foreach (var message in messages)
                WriteLine($"<time /> <success /> <success> {message}</success>");
        }

        public void WriteWarning(params string[] messages)
        {
            foreach (var message in messages)
                WriteLine($"<time /> <warning /> <warning> {message}</warning>");
        }
    }
}