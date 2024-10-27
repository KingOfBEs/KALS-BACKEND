using System.Text;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using KALS.API.Models.GoogleDrive;
using Newtonsoft.Json;
using File = Google.Apis.Drive.v3.Data.File;

namespace KALS.API.Utils;

public class GoogleDriveUtil
{
    public static async Task<GoogleDriveResponse> UploadToGoogleDrive(IFormFile file, IConfiguration configuration, ILogger logger)
    {
        var allowedExtensions = new[] { ".pdf", ".docx", ".doc", ".docx", ".xls", ".xlsx", ".ppt", ".pptx" };
        var extension = Path.GetExtension(file.FileName).ToLower();

        if (!allowedExtensions.Contains(extension))
            throw new InvalidOperationException(
                "Chỉ các định dạng tệp .pdf, .doc, .docx, .xls, .xlsx, .ppt, và .pptx được phép tải lên.");
        GoogleCredential credential;


        var credentialsSection = configuration.GetSection("GoogleDrive:CredentialsPath");
        var credentialsJson = credentialsSection.GetChildren().ToDictionary(x => x.Key, x => x.Value);
        var credentialsJsonString = JsonConvert.SerializeObject(credentialsJson);

        try
        {
            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(credentialsJsonString)))
            {
                credential = GoogleCredential.FromStream(stream).CreateScoped(DriveService.ScopeConstants.DriveFile);
            }

            var service = new DriveService(new BaseClientService.Initializer
            {
                HttpClientInitializer = credential,
                ApplicationName = "Google Drive Upload Console App"
            });

            var listRequest = service.Files.List();
            listRequest.Q =
                $"name='{file.FileName}' and '{configuration["GoogleDrive:FolderId"]}' in parents and trashed=false";
            listRequest.Fields = "files(id, name)";

            var fileList = await listRequest.ExecuteAsync();

            if (fileList.Files.Count > 0)
            {
                var existingFileId = fileList.Files[0].Id;
                var existingFileUrl = $"https://drive.google.com/file/d/{existingFileId}/view?usp=sharing";
                var response = new GoogleDriveResponse
                {
                    Url = existingFileUrl,
                    Existed = true
                };
                return response;
            }
            var fileMetaData = new File()
            {
                Name = file.FileName,
                Parents = new List<string> { configuration["GoogleDrive:FolderId"] }
            };
            
            FilesResource.CreateMediaUpload request;
            using (var stream = file.OpenReadStream())
            {
                request = service.Files.Create(fileMetaData, stream, file.ContentType);
                request.Fields = "id";
                await request.UploadAsync();
            }

            var fileUploaded = request.ResponseBody;
            var urlUploaded = $"https://drive.google.com/file/d/{fileUploaded.Id}/view?usp=sharing";
            var responseUploaded = new GoogleDriveResponse()
            {
                Url = urlUploaded,
                Existed = false
            };
            return responseUploaded;
        }
        catch (Exception ex)
        {
            logger.LogError($"Error uploading file to Google Drive: {ex.Message}");
            throw new InvalidOperationException("Error uploading file to Google Drive.");
        }
    }
}