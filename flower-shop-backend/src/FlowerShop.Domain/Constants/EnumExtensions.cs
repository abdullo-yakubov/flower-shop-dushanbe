using System.Text;

namespace FlowerShop.Domain.Constants;

public static class EnumExtensions
{
    /// <summary>PhotoReady → "photo_ready", CourierEnRoute → "courier_en_route". Matches the frontend API contract.</summary>
    public static string ToApiString<TEnum>(this TEnum value) where TEnum : struct, Enum
    {
        var name = value.ToString();
        var sb = new StringBuilder(name.Length + 4);
        for (var i = 0; i < name.Length; i++)
        {
            var c = name[i];
            if (char.IsUpper(c) && i > 0) sb.Append('_');
            sb.Append(char.ToLowerInvariant(c));
        }
        return sb.ToString();
    }
}
