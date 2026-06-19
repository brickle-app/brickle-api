namespace BricklePlatform.Api.Application.Dtos;

public class FileResponseDto
{
    public string FileUrl { get; set; }

    public FileResponseDto(string fileUrl)
    {
        FileUrl = fileUrl ?? throw new ArgumentNullException(nameof(fileUrl));
    }

    public FileResponseDto()
    {
        FileUrl = string.Empty;
    }
}