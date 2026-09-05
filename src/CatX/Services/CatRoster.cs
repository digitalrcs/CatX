using CatX.Models;

namespace CatX.Services;

/// <summary>Backwards-compatible selection of one to eight independently styled cats.</summary>
internal static class CatRoster
{
    public const int MaximumCats = 8;
    public static readonly string[] Styles = ["Marmalade", "Midnight", "Snowball", "Tuxedo", "Calico",
        "Realistic Tabby", "Realistic Orange", "Realistic White", "Realistic Grey", "Realistic Tuxedo", "Realistic Black", "Realistic Bicolor"];

    public static List<AppSettings> Resolve(AppSettings settings)
    {
        if (settings.CatStyle == "No cat") return [];
        var result = new List<AppSettings>();
        for (var i = 0; i < Math.Clamp(settings.CatCount, 1, MaximumCats); i++)
        {
            var style = i == 0 ? settings.CatStyle : settings.AdditionalCatStyles?.ElementAtOrDefault(i-1);
            if (style is null || !Styles.Contains(style)) style = Styles[i % Styles.Length];
            result.Add(new AppSettings { CatStyle = style, RoamEverySeconds = settings.RoamEverySeconds,
                ChaseCursor = settings.ChaseCursor, PlayfulMouse = settings.PlayfulMouse,
                UnlockChord = settings.UnlockChord, AutoLockAfterSeconds = settings.AutoLockAfterSeconds });
        }
        return result;
    }
}
