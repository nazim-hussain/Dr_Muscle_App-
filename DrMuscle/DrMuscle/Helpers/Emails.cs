using System;
using System.Text.RegularExpressions;

namespace DrMuscle.Helpers
{
    public static class Emails
    {
		private static Regex EmailRegex = new Regex(@"^([\w\.\+\-]+)@([\w\-]+)((\.(\w){2,3})+)$");
		public static bool ValidateEmail(string email)
		{
            return Regex.IsMatch(email, @"^[a-z0-9!#$%&'*+/=?.^_`{|}~-]+(?:\\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@([-a-z0-9]+\.)+[a-z]{2,5}$");
            //var regex = new Regex(@"[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?");
            //return regex.IsMatch(email);
        }

        public static bool DeepValidationEmail(String email)
        {
            return Regex.IsMatch(email, @"^[a-z0-9](?!.*?[^\na-z0-9]{2}).*?[a-z0-9]+@([-a-z0-9]+\.)+[a-z]{2,5}$");
        }
    }
}
