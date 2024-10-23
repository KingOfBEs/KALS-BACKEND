namespace KALS.API.Services.Interface;

public interface IFirebaseService
{
    Task<string> UploadFileToFirebaseAsync(string base64Image);
    Task<List<string>> UploadFilesToFirebaseAsync(List<string> base64Images);
}