using System.Net;

namespace KALS.API.Utils;

public class SmsUtil
{
    public static String sendSMS(String[] phones, String content, IConfiguration configuration)
    {
        String url = configuration["SMS:base_url"] + "/sms/send";
        if (phones.Length <= 0)
            return "";
        
        int type = 5;
        String sender = configuration["SMS:device_id"];
        NetworkCredential myCreds = new NetworkCredential(configuration["SMS:access_token"], configuration["SMS:password"]);
        WebClient client = new WebClient();
        client.Credentials = myCreds;
        client.Headers[HttpRequestHeader.ContentType] = "application/json";

        string builder = "{\"to\":[";

        for (int i = 0; i < phones.Length; i++)
        {
            builder += "\"" + phones[i] + "\"";
            if (i < phones.Length - 1)
            {
                builder += ",";
            }
        }
        builder += "], \"content\": \"" + Uri.EscapeDataString(content) + "\", \"type\":" + type + ", \"sender\": \"" + sender + "\"}";

        String json = builder.ToString();
        return client.UploadString(url, json);
    }
}