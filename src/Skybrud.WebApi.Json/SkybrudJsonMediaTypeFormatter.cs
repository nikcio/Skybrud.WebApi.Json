using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Net.Http.Headers;
using System;
using System.Text;
using System.Threading.Tasks;

namespace Skybrud.WebApi.Json
{

    public class SkybrudJsonMediaTypeFormatter : TextOutputFormatter {

        private string _callbackQueryParameter;

        public SkybrudJsonMediaTypeFormatter(HttpContext context) {
            SupportedMediaTypes.Add(MediaTypeHeaderValue.Parse("text/vcard"));
            SupportedMediaTypes.Add(new MediaTypeHeaderValue("text/javascript"));
            Context = context;

            //MediaTypeMappings.Add(new UriPathExtensionMapping("jsonp", DefaultMediaType));
        }

        public string CallbackQueryParameter {
            get { return _callbackQueryParameter ?? "callback"; }
            set { _callbackQueryParameter = value; }
        }

        public HttpContext Context { get; }

        public override Task WriteResponseBodyAsync(OutputFormatterWriteContext context, Encoding selectedEncoding)
        {
            string callback;
            if (IsJsonpRequest(out callback)) {
                return Task.Factory.StartNew(async () => {
                    StringBuilder writer = new();
                    writer.Append(callback + "()");
                    await context.HttpContext.Response.WriteAsync(writer.ToString(), selectedEncoding);

                });
            }
            return null;
        }

        private bool IsJsonpRequest(out string callback) {
            callback = null;
            if (Context.Request.Method != "GET") return false;
            callback = Context.Request.Query[CallbackQueryParameter];
            return !String.IsNullOrEmpty(callback);
        }

    }

}