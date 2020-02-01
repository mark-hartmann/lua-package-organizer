using System;
using Console = Colorful.Console;

namespace LuaPackageOrganizer.Commands.Output
{
    public class BasicOutput : IOutput
    {
        private readonly TextProcessor _processor = new TextProcessor();

        public void Write(string message)
        {
            Console.Write(_processor.Process(message));
        }

        public void WriteLine(string message)
        {
            Console.Write(message + Environment.NewLine);
        }
    }
}