using System.Security.Cryptography;
using System.Text;
using SwimmingClub.Application.Common.Interfaces;

namespace SwimmingClub.Application.Common.Services;

public class ColorService : IColorService
{
    private static readonly string[] Palette =
    [
        "#4CAF50", "#2196F3", "#FF9800", "#9C27B0", "#F44336",
        "#00BCD4", "#FF5722", "#607D8B", "#3F51B5", "#009688",
        "#E91E63", "#795548", "#673AB7", "#CDDC39", "#03A9F4",
        "#8BC34A", "#FFC107", "#FF4081", "#536DFE", "#69F0AE",
        "#7C4DFF", "#448AFF", "#FF6E40", "#B0BEC5", "#26C6DA"
    ];

    private static readonly HashSet<string> RecentlyAssigned = new();

    public string GenerateBookingColor(Guid? customerId = null, Guid? bookingTypeId = null)
    {
        var seed = customerId?.ToString() ?? bookingTypeId?.ToString() ?? Guid.NewGuid().ToString();
        return GetDeterministicColor(seed);
    }

    public string GenerateCustomerColor(Guid customerId)
    {
        return GetDeterministicColor(customerId.ToString());
    }

    public string GetUniqueColor(IEnumerable<string> usedColors)
    {
        var usedSet = usedColors.Where(c => !string.IsNullOrEmpty(c)).ToHashSet();
        foreach (var color in Palette)
        {
            if (!usedSet.Contains(color))
                return color;
        }
        return Palette[0];
    }

    private static string GetDeterministicColor(string seed)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(seed));
        var index = Math.Abs(BitConverter.ToInt32(hash, 0)) % Palette.Length;
        return Palette[index];
    }
}
