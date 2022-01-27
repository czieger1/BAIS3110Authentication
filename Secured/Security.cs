using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace BAIS3110Authentication.Secured
{
    public class Security
    {
        public byte[] GenerateSalt(int length)
        {
            var salt = new byte[length];

            using (var random = RandomNumberGenerator.Create())
            {
                random.GetBytes(salt);
            }

            return salt;
        }

        public string CalculateHash(string input, byte[] salted)
        {
            //var salt = GenerateSalt(16);
            var salt = salted;

            var bytes = KeyDerivation.Pbkdf2(input, salt, KeyDerivationPrf.HMACSHA512, 10000, 16);

            return $"{ Convert.ToBase64String(salt) }:{ Convert.ToBase64String(bytes) }";
        }

        public bool CheckMatch(string hash, string input)
        {
            try
            {
                var str = hash.Replace(" ", "");
                var parts = str.Split(':');

                var salt = Convert.FromBase64String(parts[0]);

                var bytes = KeyDerivation.Pbkdf2(input, salt, KeyDerivationPrf.HMACSHA512, 10000, 16);

                return parts[1].Equals(Convert.ToBase64String(bytes));
            }
            catch
            {
                return false;
            }
        }
    }
}
