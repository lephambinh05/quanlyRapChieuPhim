using System.Security.Cryptography;
using System.Text;
using QRCoder;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace CinemaManagement.Services
{
    public interface ITwoFactorService
    {
        Task<string> GenerateSecretAsync();
        Task<string> GenerateQrCodeAsync(string email, string secret);
        Task<bool> ValidateCodeAsync(string secret, string code);
        // Backup codes methods đã được xóa
        Task<string> GetCurrentCodeAsync(string secret);
    }

    public class TwoFactorService : ITwoFactorService
    {
        private readonly IConfiguration _configuration;

        public TwoFactorService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<string> GenerateSecretAsync()
        {
            return await Task.Run(() =>
            {
                using var rng = new RNGCryptoServiceProvider();
                var bytes = new byte[20];
                rng.GetBytes(bytes);
                return Base32Encoding.ToBase32String(bytes);
            });
        }

        public async Task<string> GenerateQrCodeAsync(string email, string secret)
        {
            return await Task.Run(() =>
            {
                var issuer = "Cinema Management";
                var otpauthUrl = $"otpauth://totp/{Uri.EscapeDataString(issuer)}:{Uri.EscapeDataString(email)}?secret={secret}&issuer={Uri.EscapeDataString(issuer)}&algorithm=SHA1&digits=6&period=30";

                using var qrGenerator = new QRCoder.QRCodeGenerator();
                using var qrCodeData = qrGenerator.CreateQrCode(otpauthUrl, QRCoder.QRCodeGenerator.ECCLevel.Q);
                using var qrCode = new QRCoder.QRCode(qrCodeData);
                using var qrCodeImage = qrCode.GetGraphic(20);

                using var ms = new MemoryStream();
                qrCodeImage.Save(ms, ImageFormat.Png);
                var imageBytes = ms.ToArray();
                return Convert.ToBase64String(imageBytes);
            });
        }

        public async Task<bool> ValidateCodeAsync(string secret, string code)
        {
            if (string.IsNullOrEmpty(secret) || string.IsNullOrEmpty(code) || code.Length != 6)
                return false;

            var currentCode = await GetCurrentCodeAsync(secret);
            return code == currentCode;
        }

        // Backup codes methods đã được xóa

        public async Task<string> GetCurrentCodeAsync(string secret)
        {
            return await Task.Run(() =>
            {
                var timeStep = 30; // 30 seconds
                var digits = 6;
                var algorithm = "SHA1";

                var counter = (long)(DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds / timeStep;

                var secretBytes = Base32Encoding.ToBytes(secret);
                var counterBytes = BitConverter.GetBytes(counter);
                if (BitConverter.IsLittleEndian)
                    Array.Reverse(counterBytes);

                using var hmac = new HMACSHA1(secretBytes);
                var hash = hmac.ComputeHash(counterBytes);

                var offset = hash[hash.Length - 1] & 0xf;
                var binary = ((hash[offset] & 0x7f) << 24) |
                            ((hash[offset + 1] & 0xff) << 16) |
                            ((hash[offset + 2] & 0xff) << 8) |
                            (hash[offset + 3] & 0xff);

                var otp = binary % (int)Math.Pow(10, digits);
                return otp.ToString("D6");
            });
        }
    }

    // Helper class for Base32 encoding
    public static class Base32Encoding
    {
        private static readonly string Base32Chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";

        public static string ToBase32String(byte[] input)
        {
            if (input == null || input.Length == 0)
                return string.Empty;

            var bits = input.Length * 8;
            var chars = (bits + 4) / 5;
            var result = new char[chars];

            var buffer = 0;
            var bufferSize = 0;
            var charIndex = 0;

            foreach (var b in input)
            {
                buffer = (buffer << 8) | b;
                bufferSize += 8;

                while (bufferSize >= 5)
                {
                    result[charIndex++] = Base32Chars[(buffer >> (bufferSize - 5)) & 31];
                    bufferSize -= 5;
                }
            }

            if (bufferSize > 0)
            {
                result[charIndex++] = Base32Chars[(buffer << (5 - bufferSize)) & 31];
            }

            return new string(result, 0, charIndex);
        }

        public static byte[] ToBytes(string input)
        {
            input = input.TrimEnd('=');
            var byteCount = input.Length * 5 / 8;
            var returnArray = new byte[byteCount];

            byte curByte = 0, bitsRemaining = 8;
            var mask = 0;
            var arrayIndex = 0;

            foreach (var c in input)
            {
                var cValue = Base32Chars.IndexOf(char.ToUpper(c));
                if (cValue < 0) continue;

                if (bitsRemaining > 5)
                {
                    mask = cValue << (bitsRemaining - 5);
                    curByte = (byte)(curByte | mask);
                    bitsRemaining -= 5;
                }
                else
                {
                    mask = cValue >> (5 - bitsRemaining);
                    curByte = (byte)(curByte | mask);
                    returnArray[arrayIndex++] = curByte;
                    curByte = (byte)(cValue << (3 + bitsRemaining));
                    bitsRemaining += 3;
                }
            }

            if (arrayIndex != returnArray.Length)
                returnArray[arrayIndex] = curByte;

            return returnArray;
        }
    }
} 