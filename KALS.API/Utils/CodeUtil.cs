namespace KALS.API.Utils;

public class CodeUtil
{
    public static string GenerateWarrantyCode(Guid productId)
    {
        return $"{productId:N}-{DateTime.Now.Ticks}-{Guid.NewGuid().ToString().Substring(0, 8)}";
    }
}