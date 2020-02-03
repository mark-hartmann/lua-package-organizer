namespace LuaPackageOrganizer.Commands.Output
{
    public interface IOutput
    {
        public void Write(string message);

        public void WriteLine(string message);

        public void WriteError(params string[] messages);

        public void WriteNotice(params string[] messages);

        public void WriteSuccess(params string[] messages);

        public void WriteWarning(params string[] messages);
    }
}