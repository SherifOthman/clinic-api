using ClinicManagement.Application.Common.Interfaces;

namespace ClinicManagement.Infrastructure.Services;

public class FileSystemService : IFileSystem
{
    public bool Exists(string path) => File.Exists(path);

    public bool DirectoryExists(string path) => Directory.Exists(path);

    public void CreateDirectory(string path) => Directory.CreateDirectory(path);

    public void DeleteFile(string path) => File.Delete(path);

    public async Task<string> ReadAllTextAsync(string path, CancellationToken cancellationToken = default)
        => await File.ReadAllTextAsync(path, cancellationToken);

    public async Task WriteAllTextAsync(string path, string content, CancellationToken cancellationToken = default)
        => await File.WriteAllTextAsync(path, content, cancellationToken);

    public Stream CreateFileStream(string path, FileMode mode, FileAccess access = FileAccess.ReadWrite)
        => new FileStream(path, mode, access);

    public string Combine(params string[] paths) => Path.Combine(paths);

    public string GetBaseDirectory() => AppDomain.CurrentDomain.BaseDirectory;

    public string GetExtension(string path) => Path.GetExtension(path);
}