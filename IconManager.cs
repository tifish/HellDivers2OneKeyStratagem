using Avalonia.Media.Imaging;
using Avalonia.Platform;

namespace HellDivers2OneKeyStratagem;

public static class IconManager
{
    private static readonly Dictionary<string, Bitmap> _icons = [];

    public static Bitmap? GetIcon(string name)
    {
        if (name == "")
            return null;

        if (_icons.TryGetValue(name, out var icon))
            return icon;

        var path = Path.Join(AppSettings.IconsDirectory, $"{name}.png");
        if (!File.Exists(path))
            return null;

        icon = new Bitmap(path);
        _icons[name] = icon;

        return icon;
    }

    public static Bitmap? GetNoneIcon()
    {
        return GetIcon("None");
    }

    public static void ConvertAllIcons()
    {
        if (!Directory.Exists(AppSettings.IconsDirectory))
            Directory.CreateDirectory(AppSettings.IconsDirectory);

        foreach (var stratagem in StratagemManager.Stratagems)
        {
            if (stratagem.IconName == "")
                continue;

            ConvertIcon(stratagem.Type, stratagem.IconName, stratagem.Id);
        }

        ConvertIcon(StratagemType.Y, "0x28da0bb825911c9a", "None");
    }

    private static void ConvertIcon(StratagemType stratagemType, string rawIconName, string iconName)
    {
        var rawIconPath = Path.Join(AppSettings.RawIconsDirectory, $"{rawIconName}.png");
        var iconPath = Path.Join(AppSettings.IconsDirectory, $"{iconName}.png");

        using var bitmap = new Bitmap(rawIconPath);
        using var writeableBitmap = new WriteableBitmap(bitmap.PixelSize, bitmap.Dpi, PixelFormat.Bgra8888, AlphaFormat.Premul);
        using var lockedBitmap = writeableBitmap.Lock();
        bitmap.CopyPixels(new Avalonia.PixelRect(0, 0, bitmap.PixelSize.Width, bitmap.PixelSize.Height),
                         lockedBitmap.Address, lockedBitmap.RowBytes * bitmap.PixelSize.Height, lockedBitmap.RowBytes);

        unsafe
        {
            var ptr = (byte*)lockedBitmap.Address;
            for (var y = 0; y < bitmap.PixelSize.Height; y++)
            {
                for (var x = 0; x < bitmap.PixelSize.Width; x++)
                {
                    var offset = y * lockedBitmap.RowBytes + x * 4; // BGRA = 4 bytes per pixel
                    var b = ptr[offset];     // Blue
                    var g = ptr[offset + 1]; // Green
                    var r = ptr[offset + 2]; // Red
                    var a = ptr[offset + 3]; // Alpha

                    // If pixel is black (or near black), make it transparent
                    if (r < 10 && g < 10 && b < 10)
                    {
                        ptr[offset + 3] = 0; // Set alpha to 0 (transparent)
                    }

                    if (r > 20)
                    {
                        switch (stratagemType)
                        {
                            case StratagemType.R:
                                ptr[offset + 2] = (byte)(218 * r / 255);
                                ptr[offset + 1] = (byte)(110 * r / 255);
                                ptr[offset] = (byte)(94 * r / 255);
                                break;
                            case StratagemType.B:
                                ptr[offset + 2] = (byte)(90 * r / 255);
                                ptr[offset + 1] = (byte)(188 * r / 255);
                                ptr[offset] = (byte)(212 * r / 255);
                                break;
                            case StratagemType.G:
                                ptr[offset + 2] = (byte)(116 * r / 255);
                                ptr[offset + 1] = (byte)(160 * r / 255);
                                ptr[offset] = (byte)(96 * r / 255);
                                break;
                            case StratagemType.Y:
                                ptr[offset + 2] = (byte)(208 * r / 255);
                                ptr[offset + 1] = (byte)(186 * r / 255);
                                ptr[offset] = (byte)(118 * r / 255);
                                break;
                        }
                    }

                    // Green => White
                    if (g > 20)
                    {
                        ptr[offset] = g;
                        ptr[offset + 1] = g;
                        ptr[offset + 2] = g;
                    }
                }
            }
        }

        writeableBitmap.Save(iconPath);
    }
}