using System.IO;
using System.Text.Json;
using System.Windows.Media.Imaging;

namespace CatX.Services;

/// <summary>Loads only the selected coat. Frozen bitmaps can be decoded off the UI thread.</summary>
internal sealed class RealisticCatFrames
{
    private sealed record ClipInfo(int Frames, double Fps, bool Loop);
    private sealed record Manifest(Dictionary<string, ClipInfo> Clips);
    private readonly Dictionary<string, (BitmapSource[] Frames, double Fps, bool Loop)> _clips = new();

    public RealisticCatFrames(string coat)
    {
        using var metadata = Open("manifest.json");
        var manifest = JsonSerializer.Deserialize<Manifest>(metadata, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
            ?? throw new InvalidDataException("The realistic cat animation manifest is missing.");
        foreach (var (name, clip) in manifest.Clips)
        {
            var frames = new BitmapSource[clip.Frames];
            for (var i = 0; i < frames.Length; i++)
            {
                using var stream = Open($"{coat}/{name}/{i:000}.png");
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.StreamSource = stream;
                bitmap.EndInit();
                bitmap.Freeze();
                frames[i] = bitmap;
            }
            _clips[name] = (frames, clip.Fps, clip.Loop);
        }
    }

    public BitmapSource Sample(string name, double seconds, bool reverse = false)
    {
        var clip = _clips[name];
        var position = Math.Max(0, seconds * clip.Fps);
        // The source Lie action includes getting back up in its second half. Stop at
        // the resting midpoint so the cat does not stand again just before sleeping.
        var lastFrame = name == "lie" ? clip.Frames.Length / 2 : clip.Frames.Length - 1;
        if (reverse) position = Math.Max(0, lastFrame - seconds * lastFrame / 2);
        if (clip.Loop) position %= clip.Frames.Length;
        else position = Math.Min(position, lastFrame);
        var first = (int)position;
        return clip.Frames[first];
    }

    private static Stream Open(string path) => System.Windows.Application.GetResourceStream(
        new Uri($"pack://application:,,,/CatX;component/Assets/Realistic/{path}", UriKind.Absolute))?.Stream
        ?? throw new FileNotFoundException("Realistic cat frames are missing. Rebuild CatX after rendering the assets.", path);
}
