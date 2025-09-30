using Auth_IdentityService.Application.Services;
using System.Text.RegularExpressions;

namespace Auth_IdentityService.Infrastructure.Services
{
	public class PasswordValidator : IPasswordValidator
	{
		public bool IsValid(string password, out List<string> errors)
		{
			errors = new List<string>();

			if (string.IsNullOrEmpty(password))
			{
				errors.Add("Password is required.");
				return false;
			}

			// Minimum 8 karakter
			if (password.Length < 8)
			{
				errors.Add("Password must be at least 8 characters long.");
			}

			// Büyük harf kontrolü
			if (!password.Any(char.IsUpper))
			{
				errors.Add("Password must contain at least one uppercase letter.");
			}

			// Küçük harf kontrolü
			if (!password.Any(char.IsLower))
			{
				errors.Add("Password must contain at least one lowercase letter.");
			}

			// Rakam kontrolü
			if (!password.Any(char.IsDigit))
			{
				errors.Add("Password must contain at least one digit.");
			}

			// Özel karakter kontrolü
			if (!Regex.IsMatch(password, @"[!@#$%^&*()_+\-=\[\]{};':""\\|,.<>\/?]+"))
			{
				errors.Add("Password must contain at least one special character.");
			}

			// Sık kullanılan şifreler kontrolü
			var commonPasswords = new[] { "password", "12345678", "qwerty123", "admin123" };
			if (commonPasswords.Contains(password.ToLower()))
			{
				errors.Add("Password is too common.");
			}

			return errors.Count == 0;
		}
	}
}