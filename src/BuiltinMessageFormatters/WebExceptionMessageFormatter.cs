using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace Decoherence.ExceptionFormatting
{
    public class WebExceptionMessageFormatter : IMessageFormatter
    {
        public Type exceptionType => typeof(WebException);

        public string FormatterMessage(Exception ex)
        {
            WebException webEx = ex as WebException;

            string response = null;
            if (webEx.Response != null)
            {
                using (StreamReader sr = new StreamReader(webEx.Response.GetResponseStream()))
                {
                    response = sr.ReadToEnd();
                }
            }

            string message = $"{ex.Message}";
            if (!string.IsNullOrEmpty(message))
            {
                message += $"\nResponse as follows:\n{response}";
            }
            return message;
        }
    }
}
