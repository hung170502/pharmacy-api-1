using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Pharmacy_API.Supports
{
    public static class VietnamesePhoneHelper
    {
        private static readonly Dictionary<string, string> Carriers = new()
        {
            { "086", "Viettel" }, { "096", "Viettel" }, { "097", "Viettel" },
            { "098", "Viettel" }, { "032", "Viettel" }, { "033", "Viettel" },
            { "034", "Viettel" }, { "035", "Viettel" }, { "036", "Viettel" },
            { "037", "Viettel" }, { "038", "Viettel" }, { "039", "Viettel" },
            { "089", "MobiFone" }, { "090", "MobiFone" }, { "093", "MobiFone" },
            { "070", "MobiFone" }, { "076", "MobiFone" }, { "077", "MobiFone" },
            { "078", "MobiFone" }, { "079", "MobiFone" },
            { "091", "Vinaphone" }, { "094", "Vinaphone" },
            { "081", "Vinaphone" }, { "082", "Vinaphone" }, { "083", "Vinaphone" },
            { "084", "Vinaphone" }, { "085", "Vinaphone" }, { "088", "Vinaphone" },
            { "092", "Vietnamobile" }, { "052", "Vietnamobile" },
            { "056", "Vietnamobile" }, { "058", "Vietnamobile" },
            { "099", "Gmobile" }, { "059", "Gmobile" },
            { "087", "Itelecom" }
        };

        public static string Normalize(string rawPhone)
        {
            var phone = Regex.Replace(rawPhone, @"[\s\.\-\(\)\+]", "");

            if (phone.StartsWith("84") && phone.Length == 11)
                return "0" + phone[2..];
            if (phone.StartsWith("+84") && phone.Length == 12)
                return "0" + phone[3..];
            if (phone.StartsWith("0084") && phone.Length == 13)
                return "0" + phone[4..];

            return phone;
        }

        public static bool IsValidFormat(string phone)
        {
            return Regex.IsMatch(phone, @"^0[3|5|7|8|9]\d{8}$");
        }

        public static string GetCarrier(string phone)
        {
            var prefix3 = phone[..3];
            var prefix4 = phone.Length >= 4 ? phone[..4] : "";
            return Carriers.GetValueOrDefault(prefix4) ??
                   Carriers.GetValueOrDefault(prefix3) ??
                   "Không xác định";
        }
    }
}