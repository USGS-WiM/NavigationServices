//------------------------------------------------------------------------------
//----- OverrideUrlEncodedContent ----------------------------------------------
//------------------------------------------------------------------------------

//-------1---------2---------3---------4---------5---------6---------7---------8
//       01234567890123456789012345678901234567890123456789012345678901234567890
//-------+---------+---------+---------+---------+---------+---------+---------+

// copyright:   2017 WIM - USGS

//    authors:  Jeremy K. Newson USGS Web Informatics and Mapping
//              
//  
//   purpose:   Overrides the url encoded content in order to overcome
//              the encoding limitation.
//
//discussion:   This override was implemented in order to pass a request to 
//              an ESRI geoprocessing services, which uses form urlencoded 
//              strings for POST bodies. However When Bodies are large FromUrlEncodedContent
//              throws an 'Invalid URI: The uri string is too long' based on a character limit of 2083.
//
//              an adaption of https://stackoverflow.com/questions/23703735/how-to-set-large-string-inside-httpcontent-when-using-httpclient#answer-23740338
//              solution seemed to work for this usecase.
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
            return System.Net.WebUtility.UrlEncode(data);
        }
    }
}
