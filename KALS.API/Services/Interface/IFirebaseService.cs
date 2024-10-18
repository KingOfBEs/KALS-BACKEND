namespace KALS.API.Services.Interface;

public interface IFirebaseService
{
    Task<string> UploadFileToFirebaseAsync(IFormFile file);
    Task<List<string>> UploadFilesToFirebaseAsync(List<IFormFile> files);
}