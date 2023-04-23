using ELPS.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ELPS.Domain.Helper
{
    public static class ExtensionMethod
    {
        public static string Capitalize(this String value)
        {
            
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }
            var word = value.ToLower();
            return char.ToUpper(word[0]) + word.Substring(1);
        }
    }
    public class DistinctComparer<T> : IEqualityComparer<T> where T:EntityBase
    {
        public bool Equals(T x, T y)
        {
            return x.Id == y.Id;

        }

        public int GetHashCode(T obj)
        {
            return obj.Id.GetHashCode();

        }

    }

    
}
