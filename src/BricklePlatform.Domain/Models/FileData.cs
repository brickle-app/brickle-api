using System.IO;

namespace BricklePlatform.Domain.Models;

public class FileData
{
    public string FileName { get; }
    public Stream Content { get; }
    public string ContentType { get; }
    public long Length { get; }

    public FileData(string fileName, Stream content, string contentType, long length)
    {
        FileName = fileName;
        Content = content;
        ContentType = contentType;
        Length = length;
    }
}