using System.Text.RegularExpressions;

namespace KALS.API.Utils;

public class ConvertUtil
{
    private Stream ConvertBase64ToStream(string base64)
    {
        base64 = base64.Trim();
        if ((base64.Length % 4 != 0) || !Regex.IsMatch(base64, @"^[a-zA-Z0-9\+/]*={0,3}$", RegexOptions.None)) throw new ArgumentException("Invalid image");
        byte[] bytes = Convert.FromBase64String(base64);
        return new MemoryStream(bytes);
    }
}