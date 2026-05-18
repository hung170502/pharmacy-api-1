using System.Reflection;
using System.Text;
using System.Web;

namespace Pharmacy_API.Supports
{
    public static class UrlUtil
    {
        #region BuildQueryUrl
        public static string BuildQueryUrl(string url, object obj)
        {
            if (obj == null)
            {
                return url;
            }

            StringBuilder stringBuilder = new StringBuilder();

            PropertyInfo[] properties = obj.GetType().GetProperties();

            foreach (PropertyInfo info in properties)
            {
                var value = info.GetValue(obj, null);
                if (value != null)
                {
                    if (stringBuilder.Length > 0)
                    {
                        stringBuilder.Append('&');
                    }
                    stringBuilder.Append(info.Name + "=" + HttpUtility.UrlEncode(value.ToString()));
                }
            }

            if (stringBuilder.Length > 0)
            {
                return url + "?" + stringBuilder.ToString();
            }

            return string.Empty;
        }
        #endregion
    }
}