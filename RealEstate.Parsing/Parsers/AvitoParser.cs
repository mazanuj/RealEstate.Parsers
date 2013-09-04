using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using HtmlAgilityPack;

namespace RealEstate.Parsing.Parsers
{
    public class AvitoParser : ParserBase
    {
        protected override Advert ParseAdvertHtml(HtmlAgilityPack.HtmlNode advertNode)
        {
            throw new NotImplementedException();
        }

        protected override List<HtmlAgilityPack.HtmlNode> GetAdvertsNode(HtmlAgilityPack.HtmlNode pageNode)
        {
            throw new NotImplementedException();
        }

        public override List<Advert> ParsePage(string url)
        {
            throw new NotImplementedException();
        }

        public string DownloadPage(string url, string userAgent, WebProxy proxy, CancellationToken cs)
        {
            Trace.WriteLine("Sending request to " + url);

            string HtmlResult = null;

            HttpWebRequest myHttpWebRequest = (HttpWebRequest)WebRequest.Create(url);
            myHttpWebRequest.AllowAutoRedirect = false;
            myHttpWebRequest.Proxy = proxy ?? WebRequest.DefaultWebProxy;
            myHttpWebRequest.UserAgent = userAgent;

            var asyncResult = myHttpWebRequest.BeginGetResponse(null, null);

            WaitHandle.WaitAny(new[] { asyncResult.AsyncWaitHandle, cs.WaitHandle });
            if (cs.IsCancellationRequested)
            {
                myHttpWebRequest.Abort();
                throw new OperationCanceledException();
            }

            var myHttpWebResponse = myHttpWebRequest.EndGetResponse(asyncResult);
            System.IO.StreamReader sr = new System.IO.StreamReader(myHttpWebResponse.GetResponseStream());
            HtmlResult = sr.ReadToEnd();
            sr.Close();
            Trace.WriteLine("Response is received");

            return HtmlResult;

        }

        public void Test()
        {
            var url = "http://www.avito.ru/moskva/kvartiry/prodam/vtorichka?s=1";
            var result = this.DownloadPage(url, UserAgents.GetRandomUserAgent(), null, CancellationToken.None);

            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(result);


        }
    }
}
