using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace LT.DigitalOffice.AuthenticationService.Business
{
    static class UserPassword
    {
        internal static string internalSalt = "LT.DigitalOffice.SALT3";

        public static string GetPasswordHash(string userLogin, string salt, string userPassword)
        {
            return Encoding.UTF8.GetString(new SHA512Managed().ComputeHash(
                    Encoding.UTF8.GetBytes($"{salt}{userLogin}{userPassword}{internalSalt}")));
        }
    }
}
