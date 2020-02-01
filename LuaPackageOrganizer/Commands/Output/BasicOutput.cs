using System;
using Console = Colorful.Console;

namespace LuaPackageOrganizer.Commands.Output
{
    public class BasicOutput : IOutput
    {
        public void Write(string message)
        {
            Console.Write(message);
        }

        public void WriteLine(string message)
        {
            Console.Write(message + Environment.NewLine);
        }
    }
}