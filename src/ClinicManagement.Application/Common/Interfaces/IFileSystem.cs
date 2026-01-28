namespace ClinicManagement.Application.Common.Interfaces;

public interface IFileSystem
{
    bool Exists(string path);
    bool DirectoryExists(string path);
    void CreateDirectory(string path);
    void DeleteFile(string path);
    Task<string> ReadAllTextAsync(string path, CancellationToken cancellationToken = default);
    Task WriteAllTextAsync(string path, string content, CancellationToken cancellationToken = default);
    Stream CreateFileStream(string path, FileMode mode, FileAccess access = FileAccess.ReadWrite);
    string Combine(params string[] paths);
    string GetBaseDirectory();
    string GetExtension(string path);
}