namespace console.app.Interfaces
{
    public interface IFileReader
    {
        string[] ReadFile<T>(string path);
    }
}
