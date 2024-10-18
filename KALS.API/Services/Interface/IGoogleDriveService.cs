using KALS.API.Models.GoogleDrive;

namespace KALS.API.Services.Interface;

public interface IGoogleDriveService
{
    Task<GoogleDriveResponse> UploadToGoogleDrive(IFormFile file);
}