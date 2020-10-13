using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;

[assembly: InternalsVisibleToAttribute("LT.DigitalOffice.AuthenticationService.Business.UnitTests")]
namespace LT.DigitalOffice.AuthenticationService.Business
{
    internal static class UserPassword
    {
        private static string internalSalt = "LT.DigitalOffice.SALT3";

        internal static string GetPasswordHash(string userLogin, string salt, string userPassword)
        {
            return Encoding.UTF8.GetString(new SHA512Managed().ComputeHash(
                    Encoding.UTF8.GetBytes($"{salt}{userLogin}{userPassword}{internalSalt}")));
        }
    }
}
