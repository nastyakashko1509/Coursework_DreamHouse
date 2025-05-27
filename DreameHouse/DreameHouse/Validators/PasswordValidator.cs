using System.Text.RegularExpressions;

namespace DreameHouse.Validators
{
    public class PasswordValidator
    {
        public bool IsValidPassword(string password)
        {
            if (string.IsNullOrEmpty(password))
                return false;

            string pattern = @"^(?=.*[A-Za-z])(?=.*\d).{5,}$";
            return Regex.IsMatch(password, pattern);
        }
    }
}
