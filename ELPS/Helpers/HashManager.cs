using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace ELPS.Helpers
{
    public class HashManager
    {
        public static bool compair(string email,string appId, string hash)
        {
            if (PaymentRef.getHash(email.Trim() + appId.Trim()).ToLower() == hash.ToLower())
            {
                return true;
            }
            else
                return false;
        }

        public static string GetHash(string hashItem)
        {
            string hash = "";
            var data = Encoding.UTF8.GetBytes(hashItem);
            byte[] x;
            using (SHA512 shaM = new SHA512Managed())
            {
                x = shaM.ComputeHash(data);
            }
            StringBuilder stringBuilder = new StringBuilder();

            foreach (byte b in x)
                stringBuilder.AppendFormat("{0:X2}", b);

            hash = stringBuilder.ToString();

            return hash;
        }
    }
}