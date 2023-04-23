using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace ELPS.Helpers
{
    public class PaymentRef
    {
        public static string RefrenceCode()
        {
           //generate 12 digit numbers
            var bytes = new byte[8];
            var rng = RandomNumberGenerator.Create();
            rng.GetBytes(bytes);
            ulong random = BitConverter.ToUInt64(bytes, 0) % 1000000000000;
            return  String.Format("{0:D12}", random);
        }

        public static string getHash(string hashItem, bool ConvertToLower = true)
        {
            string hash = "";

            var data = Encoding.UTF8.GetBytes(ConvertToLower ?  hashItem.ToLower() : hashItem);

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