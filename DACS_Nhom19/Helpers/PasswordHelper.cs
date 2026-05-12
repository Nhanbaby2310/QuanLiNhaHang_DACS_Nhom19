using System.Security.Cryptography;

namespace DACS_Nhom19.Helpers
{
    /// <summary>
    /// Helper để hash và verify mật khẩu bằng PBKDF2 (HMACSHA256).
    /// Định dạng lưu: "PBKDF2|iterations|saltBase64|hashBase64".
    /// Vẫn có thể verify được mật khẩu plain-text cũ của data seed (để không làm vỡ DB hiện tại),
    /// và tự động rehash ở lần đăng nhập kế tiếp.
    /// </summary>
    public static class PasswordHelper
    {
        private const int SaltSize = 16;          // 128-bit
        private const int KeySize = 32;           // 256-bit
        private const int Iterations = 100_000;   // đủ mạnh cho đồ án
        private const string Prefix = "PBKDF2";
        private static readonly HashAlgorithmName Algo = HashAlgorithmName.SHA256;

        /// <summary>Sinh chuỗi hash mới cho mật khẩu.</summary>
        public static string Hash(string password)
        {
            if (string.IsNullOrEmpty(password))
                throw new ArgumentException("Mật khẩu không được rỗng.", nameof(password));

            var salt = RandomNumberGenerator.GetBytes(SaltSize);
            var key = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, Algo, KeySize);

            return string.Join('|',
                Prefix,
                Iterations.ToString(),
                Convert.ToBase64String(salt),
                Convert.ToBase64String(key));
        }

        /// <summary>
        /// Kiểm tra mật khẩu. Trả về true nếu khớp.
        /// Nếu <paramref name="storedValue"/> đang là plain text (dữ liệu cũ) thì so trực tiếp.
        /// </summary>
        public static bool Verify(string password, string storedValue)
        {
            if (string.IsNullOrEmpty(storedValue)) return false;

            // Dạng hash mới
            if (storedValue.StartsWith(Prefix + "|", StringComparison.Ordinal))
            {
                var parts = storedValue.Split('|');
                if (parts.Length != 4) return false;

                if (!int.TryParse(parts[1], out int iters)) return false;

                byte[] salt, key;
                try
                {
                    salt = Convert.FromBase64String(parts[2]);
                    key = Convert.FromBase64String(parts[3]);
                }
                catch
                {
                    return false;
                }

                var computed = Rfc2898DeriveBytes.Pbkdf2(password, salt, iters, Algo, key.Length);
                return CryptographicOperations.FixedTimeEquals(computed, key);
            }

            // Fallback: dữ liệu cũ lưu plain text
            return string.Equals(password, storedValue, StringComparison.Ordinal);
        }

        /// <summary>Đã là hash mới chưa?</summary>
        public static bool IsHashed(string storedValue)
        {
            return !string.IsNullOrEmpty(storedValue)
                   && storedValue.StartsWith(Prefix + "|", StringComparison.Ordinal);
        }
    }
}
