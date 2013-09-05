using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using HtmlAgilityPack;
using System.Web;

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
            //var url = "http://www.avito.ru/moskva/kvartiry/prodam/vtorichka?s=1";
            var url = "http://www.avito.ru/moskva/kvartiry/prodam/studii/vtorichka?s=1";
            var toDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day).AddDays(-7);

            List<AdvertHeader> headers = new List<AdvertHeader>();
            int oldCount = -1;
            int index = 0;

            while (headers.Count != oldCount && headers.Count < MAXCOUNT)
            {
                oldCount = headers.Count;
                index++;

                var result = this.DownloadPage(url + "&p=" + index, UserAgents.GetRandomUserAgent(), null, CancellationToken.None);

                Console.WriteLine("Downloaded");
                HtmlDocument page = new HtmlDocument();
                page.LoadHtml(result);

                Console.WriteLine("parsed");



                foreach (HtmlNode tier in page.DocumentNode.SelectNodes(@"//div[contains(@class,'t_i_i')]"))
                {
                    var link = ParseLinkToFullDescription(tier);
                    var date = ParseDate(tier);

                    //Console.WriteLine(date);

                    if (date > toDate)
                        headers.Add(new AdvertHeader()
                        {
                            DateSite = date,
                            Url = link
                        });
                }
            }

            headers.ForEach(s => Console.WriteLine(s.Url)); 

        }

        private string ParseLinkToFullDescription(HtmlNode tier)
        {
            var link = tier.SelectSingleNode(@".//h3[@class='t_i_h3']/a");
            if (link != null && link.Attributes.Contains("href"))
                return "http://www.avito.ru" + Normalize(link.Attributes["href"].Value);

            throw new Exception("Can't get link to full description");
        }

        private DateTime ParseDate(HtmlNode tier)
        {
            var whenNode = tier.SelectSingleNode(@".//div[contains(@class,'t_i_date')]");
            if (whenNode != null && !string.IsNullOrEmpty(whenNode.InnerText))
            {
                var whenItems = new List<string>(whenNode.InnerText.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries));

                for (int i = 0; i < whenItems.Count; i++)
                    whenItems[i] = whenItems[i].Trim().ToLower();

                whenItems.RemoveAll(s => String.IsNullOrEmpty(s));

                if (whenItems.Count == 2)
                {
                    var dateString = whenItems[0];
                    var timeString = whenItems[1];

                    DateTime dt = DateTime.MinValue;

                    if (dateString == "сегодня")
                        dt = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
                    else if (dateString == "вчера")
                        dt = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day).AddDays(-1);
                    else
                    {
                        var dateS = dateString.Split(' ');
                        if (dateS != null && dateS.Count() == 2)
                        {
                            var month = 0;
                            switch (dateS[1])
                            {
                                case "янв.": month = 1; break;
                                case "фев.": month = 2; break;
                                case "мар.": month = 3; break;
                                case "апр.": month = 4; break;
                                case "мая": month = 5; break;
                                case "июня": month = 6; break;
                                case "июля": month = 7; break;
                                case "авг.": month = 8; break;
                                case "сен.": month = 9; break;
                                case "окт.": month = 10; break;
                                case "ноя.": month = 11; break;
                                case "дек.": month = 12; break;
                                default:
                                    throw new Exception("Can't parse date information");
                            }

                            dt = new DateTime(DateTime.Now.Year, month, Int32.Parse(dateS[0]));
                            if (DateTime.Now.Month < month)
                                dt = dt.AddYears(-1);
                        }
                    }

                    var timeS = timeString.Split(':');
                    if (timeS != null && timeS.Count() == 2)
                    {
                        dt = dt.AddHours(Int32.Parse(timeS[0]));
                        dt = dt.AddMinutes(Int32.Parse(timeS[1]));

                        return dt;
                    }
                    else
                        throw new Exception("Can't parse date information");
                }
            }

            throw new Exception("Can't parse date information");
        }

        private string Normalize(string htmlValue)
        {
            return HttpUtility.HtmlDecode(htmlValue).Trim();
        }


    }
}
