using Colorful;
using TextProcessor;

namespace LuaPackageOrganizer.Commands.Output
{
    public class BasicOutput : IOutput
    {
        private readonly MessageProcessor _messageProcessor = new MessageProcessor();

        public void Write(string message)
        {
            Console.Write(_messageProcessor.Process(message));
        }

        public void WriteLine(string message)
        {
            Console.WriteLine(_messageProcessor.Process(message));
        }
    }
}