using System.Security.Cryptography;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;

public static class PasswordHelper
{
	public static (string Hash, string Salt) HashPassword(string password)
	{
		// 產生 128-bit 隨機 Salt
		byte[] saltBytes = new byte[16];
		using (var rng = RandomNumberGenerator.Create())
		{
			rng.GetBytes(saltBytes);
		}
		string salt = Convert.ToBase64String(saltBytes);

		// 使用 PBKDF2 雜湊
		string hash = Convert.ToBase64String(KeyDerivation.Pbkdf2(
			password: password,
			salt: saltBytes,
			prf: KeyDerivationPrf.HMACSHA256,
			iterationCount: 10000,
			numBytesRequested: 32
		));

		return (hash, salt);
	}
}
