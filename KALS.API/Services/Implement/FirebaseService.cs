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

    public async Task<string> UploadFileToFirebaseAsync(string base64Image)
    {
        var uploadedUrl = null as string;
        var firebaseStorageBaseUrl = _configuration["Firebase:FirebaseStorageBaseUrl"];
        try
        {
            if (string.IsNullOrEmpty(base64Image))
            {
                base64Image = base64Image.Trim();
                // string fileName = Path.GetFileName(file.FileName);
                string firebaseStorageUrl = $"{firebaseStorageBaseUrl}?uploadType=media&name=images/{Guid.NewGuid()}";
                        
                byte[] imageBytes = Convert.FromBase64String(base64Image);
                using (var stream = new MemoryStream(imageBytes))
                {
                    var content = new ByteArrayContent(stream.ToArray());
                    content.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");

                    var response = await _httpClient.PostAsync(firebaseStorageUrl, content);
                    if (response.IsSuccessStatusCode)
                    {
                        var responseBody = await response.Content.ReadAsStringAsync();
                        var downloadUrl = ParseDownloadUrl(responseBody);
                        uploadedUrl = downloadUrl;
                    }
                    else
                    {
                        var errorMessage = $"Error uploading image to Firebase Storage. Status Code: {response.StatusCode}\nContent: {await response.Content.ReadAsStringAsync()}";
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

    public async Task<List<string>> UploadFilesToFirebaseAsync(List<string> base64ImageList)
    {
        var uploadedUrls = new List<string>();
        var firebaseStorageBaseUrl = _configuration["Firebase:FirebaseStorageBaseUrl"];
        try
        {
            foreach (var base64Image in base64ImageList)
            {
                if (string.IsNullOrEmpty(base64Image))
                {
                    var base64ImageTrim = base64Image.Trim();
                    // string fileName = Path.GetFileName(file.FileName);
                    string firebaseStorageUrl = $"{firebaseStorageBaseUrl}?uploadType=media&name=images/{Guid.NewGuid()}";
                    
                    byte[] imageBytes = Convert.FromBase64String(base64ImageTrim);
                    
                    using (var stream = new MemoryStream(imageBytes))
                    {
                        
                        var content = new ByteArrayContent(stream.ToArray());
                        content.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
                    
                        var response = await _httpClient.PostAsync(firebaseStorageUrl, content);
                        if (response.IsSuccessStatusCode)
                        {
                            var responseBody = await response.Content.ReadAsStringAsync();
                            var downloadUrl = ParseDownloadUrl(responseBody);
                            uploadedUrls.Add(downloadUrl);
                        }
                        else
                        {
                            var errorMessage = $"Error uploading image to Firebase Storage. Status Code: {response.StatusCode}\nContent: {await response.Content.ReadAsStringAsync()}";
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
    
    private string ParseDownloadUrl(string responseBody)
    {
        var firebaseStorageBaseUrl = _configuration["Firebase:FirebaseStorageBaseUrl"];
        var json = JsonDocument.Parse(responseBody);
        var nameElement = json.RootElement.GetProperty("name");
        var downloadUrl = $"{firebaseStorageBaseUrl}/{Uri.EscapeDataString(nameElement.GetString())}?alt=media";
        return downloadUrl;
    }
}