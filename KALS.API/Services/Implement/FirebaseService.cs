using System.Net.Http.Headers;
using System.Text.Json;
using AutoMapper;
using KALS.API.Services.Interface;

namespace KALS.API.Services.Implement;

public class FirebaseService: BaseService<FirebaseService>, IFirebaseService
{
    private HttpClient _httpClient;
    public FirebaseService(ILogger<FirebaseService> logger, IMapper mapper, IHttpContextAccessor httpContextAccessor, 
        IConfiguration configuration, HttpClient httpClient) : base(logger, mapper, httpContextAccessor, configuration)
    {
        _httpClient = httpClient;
    }

    public async Task<string> UploadFileToFirebaseAsync(IFormFile file)
    {
        var uploadedUrl = null as string;
        var firebaseStorageBaseUrl = _configuration["Firebase:FirebaseStorageBaseUrl"];
        try
        {
            if (file.Length > 0)
            {
                string fileName = Path.GetFileName(file.FileName);
                string firebaseStorageUrl = $"{firebaseStorageBaseUrl}?uploadType=media&name=images/{Guid.NewGuid()}_{fileName}";
                        
                using (var stream = new MemoryStream())
                {
                    await file.CopyToAsync(stream);
                    stream.Position = 0;
                    var content = new ByteArrayContent(stream.ToArray());
                    content.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);

                    var response = await _httpClient.PostAsync(firebaseStorageUrl, content);
                    if (response.IsSuccessStatusCode)
                    {
                        var responseBody = await response.Content.ReadAsStringAsync();
                        var downloadUrl = ParseDownloadUrl(responseBody, fileName);
                        uploadedUrl = downloadUrl;
                    }
                    else
                    {
                        var errorMessage = $"Error uploading file {fileName} to Firebase Storage. Status Code: {response.StatusCode}\nContent: {await response.Content.ReadAsStringAsync()}";
                        _logger.LogError(errorMessage);
                    }
                }
            }
        }

        catch (Exception ex)
        {
            _logger.LogError($"Error uploading files to Firebase Storage: {ex.Message}");
            throw new Exception("Error uploading files to Firebase Storage", ex);
        }

        return uploadedUrl;
    }

    public async Task<List<string>> UploadFilesToFirebaseAsync(List<IFormFile> files)
    {
        var uploadedUrls = new List<string>();
        var firebaseStorageBaseUrl = _configuration["Firebase:FirebaseStorageBaseUrl"];
        try
        {
            foreach (var file in files)
            {
                if (file.Length > 0)
                {
                    string fileName = Path.GetFileName(file.FileName);
                    string firebaseStorageUrl = $"{firebaseStorageBaseUrl}?uploadType=media&name=images/{Guid.NewGuid()}_{fileName}";
                        
                    using (var stream = new MemoryStream())
                    {
                        await file.CopyToAsync(stream);
                        stream.Position = 0;
                        var content = new ByteArrayContent(stream.ToArray());
                        content.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);

                        var response = await _httpClient.PostAsync(firebaseStorageUrl, content);
                        if (response.IsSuccessStatusCode)
                        {
                            var responseBody = await response.Content.ReadAsStringAsync();
                            var downloadUrl = ParseDownloadUrl(responseBody, fileName);
                            uploadedUrls.Add(downloadUrl);
                        }
                        else
                        {
                            var errorMessage = $"Error uploading file {fileName} to Firebase Storage. Status Code: {response.StatusCode}\nContent: {await response.Content.ReadAsStringAsync()}";
                            _logger.LogError(errorMessage);
                        }
                    }
                }
                    
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error uploading files to Firebase Storage: {ex.Message}");
            throw new Exception("Error uploading files to Firebase Storage", ex);
        }

        return uploadedUrls;
    }
    
    private string ParseDownloadUrl(string responseBody, string fileName)
    {
        var firebaseStorageBaseUrl = _configuration["Firebase:FirebaseStorageBaseUrl"];
        var json = JsonDocument.Parse(responseBody);
        var nameElement = json.RootElement.GetProperty("name");
        var downloadUrl = $"{firebaseStorageBaseUrl}/{Uri.EscapeDataString(nameElement.GetString())}?alt=media";
        return downloadUrl;
    }
}