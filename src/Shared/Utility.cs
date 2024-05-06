using System.Text;

namespace API;

public static class Utility
{
    public static string GenerateHexString(int length)
    {
        var rand = new Random();
        var bytes = new byte[length];
        rand.NextBytes(bytes);

        var sb = new StringBuilder();

        for (int i = 0; i < bytes.Length; i++)
        {
            sb.Append(bytes[i].ToString("X2"));
        }

        return sb.ToString();
    }

    public static byte[] StringToByteArray(string hex)
    {
        int NumberChars = hex.Length;
        byte[] bytes = new byte[NumberChars / 2];
        for (int i = 0; i < NumberChars; i += 2)
            bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
        return bytes;
    }

    public static string ByteArrayToString(byte[] value)
    {
        return BitConverter.ToString(value).Replace("-", "");
    }
}
