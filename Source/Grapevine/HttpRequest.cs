using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Grapevine
{
    public class HttpRequest
    {
        public HttpMethod Method
        {
            get;
            set;
        }

        public string Url
        {
            get;
            set;
        }

        public string HttpVersion
        {
            get;
            set;
        }

        public HttpHeaderCollection Headers
        {
            get;
            private set;
        }

        public string Body
        {
            get;
            set;
        }

        public HttpRequest()
        {
            this.Headers = new HttpHeaderCollection();
        }
    }
}
