﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using HtmlAgilityPack;
using System.Web;
using System.Text.RegularExpressions;
using System.Drawing;
using System.IO;

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

        public void Test()
        {
            //var url = "http://www.avito.ru/moskva/kvartiry/prodam/vtorichka?s=1";
            var url = "http://www.avito.ru/moskva/kvartiry/prodam/studii/vtorichka?s=1";
            //var url = "http://www.avito.ru/moskva/kvartiry/prodam/novostroyka?s=1";
            //var url = "http://www.avito.ru/moskva/kvartiry/prodam?s=1";

            var toDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day).AddDays(0);

            List<AdvertHeader> headers = LoadHeaders(url, toDate);

            List<Advert> adverts = new List<Advert>();

            foreach (var head in headers.Take(3))
            {
                adverts.Add(Parse(head));
            }

            adverts.ForEach(s => Console.WriteLine(s.ToString() + "\r\n"));

        }

        private List<AdvertHeader> LoadHeaders(string url, DateTime toDate)
        {
            List<AdvertHeader> headers = new List<AdvertHeader>();
            int oldCount = -1;
            int index = 0;

            do
            {
                oldCount = headers.Count;
                index++;

                string result;

                try
                {
                    result = this.DownloadPage(url + "&p=" + index, UserAgents.GetRandomUserAgent(), null, CancellationToken.None);

                }
                catch (Exception ex)
                {
                    Console.WriteLine("Can't dowload");
                    break;
                }

                Console.WriteLine("Downloaded");
                HtmlDocument page = new HtmlDocument();
                page.LoadHtml(result);

                Console.WriteLine("parsed");

                if (page.DocumentNode.SelectSingleNode(@"//h2[contains(@class,'nulus_h2')]") != null)
                    break;

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
            while (headers.Count != oldCount && headers.Count < MAXCOUNT);

            return headers;
        }

        private string ParseSeller(HtmlDocument full)
        {
            var seller = full.GetElementbyId("seller");
            if (seller != null)
            {
                var strong = seller.SelectSingleNode(@".//strong");
                if (strong != null)
                    return Normalize(strong.InnerText);
            }

            throw new Exception("Can't get seller");
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

        private void ParseTitle(HtmlDocument full, Advert advert)
        {
            var header = full.DocumentNode.SelectSingleNode(@".//h1[contains(@class,'item_title')]");
            if (header != null)
            {
                var title = Normalize(header.InnerText);
                advert.Title = title;

                var parts = title.Split(',');
                if (parts.Count() == 3)
                {
                    advert.Rooms = parts[0];

                    float area;
                    float.TryParse(parts[1].Replace(" м²", "").Trim(), out area);
                    advert.AreaFull = area;

                    var floors = parts[2].Replace(" эт.", "").Trim().Split('/');

                    int floor;
                    Int32.TryParse(floors[0], out floor);
                    advert.Floor = (short)floor;

                    int floorfull;
                    Int32.TryParse(floors[1], out floorfull);
                    advert.FloorTotal = (short)floorfull;

                }
                else
                {
                    Console.WriteLine(title);
                    throw new Exception("unknow header");
                }

            }
            else
                throw new Exception("none header");
        }

        private string ParseCity(HtmlDocument full)
        {
            var cityLabel = full.GetElementbyId("map");
            if (cityLabel != null)
            {
                var city = cityLabel.ChildNodes.First(s => s.Name == "a");
                if (city != null)
                    return Normalize(city.InnerText).TrimEnd(new[]{',',' '});
            }

            return null;
        }

        private void ParseAddress(HtmlDocument full, Advert advert)
        {
            var addressLabel = full.DocumentNode.SelectSingleNode(@"//dt[@class='description_term']/span[text() = 'Адрес']");
            if (addressLabel != null)
            {
                var nextBlock = addressLabel.ParentNode.NextSibling;
                if (nextBlock != null)
                {
                    var addressBlock = nextBlock.NextSibling;
                    if (addressBlock != null)
                    {
                        var link = addressBlock.SelectSingleNode(@"./a");
                        if (link != null)
                            link.Remove();

                        var metro = addressBlock.SelectSingleNode(@"./span");
                        if (metro != null)
                        {
                            advert.MetroStation = metro.InnerText.TrimEnd(new[] { ',' });
                            metro.Remove();
                        }

                        advert.Address = Normalize(addressBlock.InnerText).TrimEnd(new[] { ',' }).TrimStart(new[] { ',' });
                    }
                }
            }
        }

        private Int64 ParsePrice(HtmlDocument full)
        {
            var block = full.DocumentNode.SelectSingleNode(@".//span[contains(@class,'p_i_price t-item-price')]/strong/span");
            if (block != null)
            {
                return Int64.Parse(block.InnerText.Replace("&nbsp;", ""));
            }

            throw new Exception("Can't find price");
        }

        private static AdvertType MapType(string param)
        {
            var dealMap = new Dictionary<string, AdvertType> {
                { "201_1058", AdvertType.Buy },
                { "201_1060", AdvertType.Pass },
                { "201_1061", AdvertType.Rent },
                { "201_1059", AdvertType.Sell },
                { "202_1064", AdvertType.Sell },
                { "202_1065", AdvertType.Pass },
                { "202_1063", AdvertType.Buy },
                { "202_1066", AdvertType.Rent },
                { "203_1069", AdvertType.Sell },
                { "203_1068", AdvertType.Buy },
                { "204_1074", AdvertType.Sell },
                { "204_1075", AdvertType.Pass },
                { "204_1073", AdvertType.Buy },
                { "204_1076", AdvertType.Rent },
                { "536_5545", AdvertType.Sell },
                { "536_5546", AdvertType.Rent },
            };

            return dealMap.ContainsKey(param) ? dealMap[param] : AdvertType.All;
        }

        private static Usedtype MapUsedtype(string param)
        {
            var subTypeMap = new Dictionary<string, Usedtype> {
                { "499_5255", Usedtype.New },
            };

            return subTypeMap.ContainsKey(param) ? subTypeMap[param] : Usedtype.None;
        }

        private static RealEstateType MapRealEstateType(string param)
        {
            var typeMap = new Dictionary<string, RealEstateType> {
                { "201_1058", RealEstateType.Apartments },
                { "201_1060", RealEstateType.Apartments },
                { "201_1061", RealEstateType.Apartments },
                { "201_1059", RealEstateType.Apartments },
                { "202_1064", RealEstateType.House },
                { "202_1065", RealEstateType.House },
                { "202_1063", RealEstateType.House },
                { "202_1066", RealEstateType.House },
            };

            return typeMap.ContainsKey(param) ? typeMap[param] : RealEstateType.All;
        }

        private void ParseCategory(HtmlDocument full, Advert advert)
        {
            var nodes = full.DocumentNode.SelectNodes(@"//dl[@class='description description-expanded']/dd[@class='item-params c-1']/div/a");
            if (nodes != null && nodes.Count > 0)
            {
                foreach (var node in nodes)
                {
                    if (node.Attributes.Contains("href"))
                    {
                        var href = node.Attributes["href"].Value;

                        var paramsRegex = new Regex(@"params=([0-9\._]+)").Match(href);
                        if (paramsRegex.Success)
                        {
                            foreach (var param in paramsRegex.Groups[1].Value.Split('.'))
                            {
                                var type = MapType(param);
                                if (type != AdvertType.All)
                                {
                                    advert.AdvertType = type;
                                }

                                var usedType = MapUsedtype(param);
                                if (usedType != Usedtype.None)
                                {
                                    advert.Usedtype = usedType;
                                }

                                var realEstateType = MapRealEstateType(param);
                                if (realEstateType != RealEstateType.All)
                                {
                                    advert.RealEstateType = realEstateType;
                                }
                            }

                        }
                    }
                }
            }

        }

        private string ParseDescription(HtmlDocument full)
        {
            var desc = full.GetElementbyId("desc_text");
            if (desc != null)
                return Normalize(desc.InnerText);

            return "";
        }

        private List<Image> ParsePhotos(HtmlDocument full)
        {
            var result = new List<Image>();

            var gallery = full.DocumentNode.SelectSingleNode("//div[@class='gallery scrollable']/div[@class='items']");
            if (gallery != null)
            {
                foreach (var link in gallery.SelectNodes("//div[contains(@class, 'll fit')]/a"))
                {
                    if (result.Count > 5)
                        break;

                    try
                    {
                        var href = link.Attributes["href"];
                        if (href != null && href.Value != null)
                        {
                            var src = Normalize(href.Value).TrimStart('/');

                            result.Add(new Image() { URl = src });
                        }
                    }
                    catch (Exception exc)
                    {
                        if (result.Count > 0)
                            continue;

                        throw;
                    }
                }
            }

            return result;
        }

        private string ParsePhoneImageUrl(HtmlDocument full, string itemId)
        {
            var script = full.DocumentNode.SelectSingleNode("//div[@class='item']/script");
            if (script == null)
                return null;

            var urlRegex = new Regex(@"var item_url = '([^']+)").Match(script.InnerText);
            var itemRegex = new Regex(@"item_phone = '([^']+)").Match(script.InnerText);

            if (urlRegex.Success && itemRegex.Success)
            {
                var itemUrl = urlRegex.Groups[1].Value;
                var itemPhone = itemRegex.Groups[1].Value;

                var pre = new List<string>();
                foreach (Match item in new Regex("[0-9a-f]+", RegexOptions.IgnoreCase).Matches(itemPhone))
                    pre.Add(item.Value);

                if (Convert.ToInt64(itemId) % 2 == 0)
                    pre.Reverse();

                var mixed = string.Join("", pre.ToArray());
                var s = mixed.Length;
                var pkey = "";

                for (var k = 0; k < s; ++k)
                    if (k % 3 == 0)
                        pkey += mixed[k];

                return string.Format(@"http://www.avito.ru/items/phone/{0}?pkey={1}", itemUrl, pkey);
            }

            return null;
        }

        private void ParsePhone(HtmlDocument full, Advert advert)
        {
            var itemId = Normalize(full.GetElementbyId("item_id").InnerText);

            var phoneUrl = ParsePhoneImageUrl(full, itemId);
            if (phoneUrl != null)
            {
                var phoneImage = DownloadImage(phoneUrl, UserAgents.GetRandomUserAgent(), null, CancellationToken.None, advert.Url);
                var phoneText = new StringBuilder(new RealEstate.Parsing.Ocr.AvitoOcr().Recognize(phoneImage));

                //System.IO.File.WriteAllBytes(@"c:/test/" + phoneImage.GetHashCode() + ".png", phoneImage);

                phoneText.Replace("-", "");
                advert.PhoneNumber = phoneText.ToString();
            }
        }

        private Advert Parse(AdvertHeader header)
        {
            Advert advert = new Advert();

            advert.DateUpdate = DateTime.Now;

            advert.DateSite = header.DateSite;
            advert.Url = header.Url;

            string result;
            result = this.DownloadPage(advert.Url, UserAgents.GetRandomUserAgent(), null, CancellationToken.None);

            Console.WriteLine("Downloaded description");
            HtmlDocument page = new HtmlDocument();
            page.LoadHtml(result);

            Console.WriteLine("parsed description");

            ParseTitle(page, advert);

            advert.Name = ParseSeller(page);
            advert.City = ParseCity(page);
            ParseAddress(page, advert);

            ParseCategory(page, advert);

            advert.MessageFull = ParseDescription(page);
            advert.Price = ParsePrice(page);

            advert.Images = ParsePhotos(page);
            ParsePhone(page, advert);

            return advert;
        }

        private string Normalize(string htmlValue)
        {
            return HttpUtility.HtmlDecode(htmlValue).Trim();
        }


    }
}
