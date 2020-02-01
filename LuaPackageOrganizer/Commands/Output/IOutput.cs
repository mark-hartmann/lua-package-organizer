namespace LuaPackageOrganizer.Commands.Output
{
    public interface IOutput
    {
        public void Write(string message);
        
        public void WriteLine(string message);
    }
}