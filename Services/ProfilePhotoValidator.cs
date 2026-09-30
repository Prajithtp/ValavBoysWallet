namespace ValavBoysWallet.Services;

public static class ProfilePhotoValidator
{
    public const int MaxBytes = 512 * 1024;

    public static string? GetContentType(byte[] bytes)
    {
        if (bytes.Length == 0 || bytes.Length > MaxBytes) return null;
        if (bytes.Length >= 24 && bytes.AsSpan(0, 8).SequenceEqual(
            new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 })) return "image/png";
        if (bytes.Length >= 4 && bytes[0] == 255 && bytes[1] == 216 &&
            bytes[2] == 255 && bytes[^2] == 255 && bytes[^1] == 217) return "image/jpeg";
        return null;
    }
}
