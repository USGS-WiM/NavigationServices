using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json;

namespace NavigationAgent.ServiceAgents
{
    public class OverRideUrlEncodedContent : ByteArrayContent
    {
        public OverRideUrlEncodedContent(IEnumerable<KeyValuePair<string, string>> nameValueCollection)
            : base(OverRideUrlEncodedContent.GetContentByteArray(nameValueCollection))
        {
            base.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");
        }
        private static byte[] GetContentByteArray(IEnumerable<KeyValuePair<string, string>> nameValueCollection)
        {
            if (nameValueCollection == null)
            {
                throw new ArgumentNullException("nameValueCollection");
            }
            StringBuilder stringBuilder = new StringBuilder();
            foreach (KeyValuePair<string, string> current in nameValueCollection)
            {
                if (stringBuilder.Length > 0)
                {
                    stringBuilder.Append('&');
                }

                stringBuilder.Append(current.Key);
                stringBuilder.Append('=');
                stringBuilder.Append(OverRideUrlEncodedContent.Encode(current.Value));
            }
            return Encoding.Default.GetBytes(stringBuilder.ToString());
        }
        private static string Encode(string data)
        {
            if (string.IsNullOrEmpty(data))
            {
                return string.Empty;
            }
            //return JsonConvert.SerializeObject(data);
            return System.Net.WebUtility.UrlEncode(data);
        }
    }
}
