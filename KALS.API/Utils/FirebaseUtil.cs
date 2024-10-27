using System.Net.Http.Headers;
using System.Text.Json;

namespace KALS.API.Utils;

public class FirebaseUtil
{
    public static async Task<string> UploadFileToFirebase(IFormFile file, IConfiguration configuration)
    {
        var uploadedUrl = null as string;
        var firebaseStorageBaseUrl = configuration["Firebase:FirebaseStorageBaseUrl"];
        try
        {
            using (var client = new HttpClient())
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

                        var response = await client.PostAsync(firebaseStorageUrl, content);
                        if (response.IsSuccessStatusCode)
                        {
                            var responseBody = await response.Content.ReadAsStringAsync();
                            var downloadUrl = ParseDownloadUrl(responseBody, fileName, configuration);
                            uploadedUrl = downloadUrl;
                        }
                        else
                        {
                            var errorMessage = $"Error uploading file {fileName} to Firebase Storage. Status Code: {response.StatusCode}\nContent: {await response.Content.ReadAsStringAsync()}";
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            throw new Exception("Error uploading files to Firebase Storage", ex);
        }

        return uploadedUrl;
    }
    public static async Task<List<string>> UploadFilesToFirebase(List<IFormFile> files, IConfiguration configuration)
    {
        var uploadedUrls = new List<string>();
        var firebaseStorageBaseUrl = configuration["Firebase:FirebaseStorageBaseUrl"];
        try
        {
            using (var client = new HttpClient())
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

                            var response = await client.PostAsync(firebaseStorageUrl, content);
                            if (response.IsSuccessStatusCode)
                            {
                                var responseBody = await response.Content.ReadAsStringAsync();
                                var downloadUrl = ParseDownloadUrl(responseBody, fileName, configuration);
                                uploadedUrls.Add(downloadUrl);
                            }
                            else
                            {
                                var errorMessage = $"Error uploading file {fileName} to Firebase Storage. Status Code: {response.StatusCode}\nContent: {await response.Content.ReadAsStringAsync()}";
                            }
                        }
                    }
                    
                }
            }
        }
        catch (Exception ex)
        {
            throw new Exception("Error uploading files to Firebase Storage", ex);
        }

        return uploadedUrls;
    }

    private static string ParseDownloadUrl(string responseBody, string fileName, IConfiguration configuration)
    {
        var firebaseStorageBaseUrl = configuration["Firebase:FirebaseStorageBaseUrl"];
        var json = JsonDocument.Parse(responseBody);
        var nameElement = json.RootElement.GetProperty("name");
        var downloadUrl = $"{firebaseStorageBaseUrl}/{Uri.EscapeDataString(nameElement.GetString())}?alt=media";
        return downloadUrl;
    }
}