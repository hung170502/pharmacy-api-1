using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pharmacy_API.Supports
{
    public static class StringExtensions
    {
        public static string ToBase64(this string input)
        {
            var bytes = Encoding.UTF8.GetBytes(input);
            return Convert.ToBase64String(bytes);
        }
    }
}
