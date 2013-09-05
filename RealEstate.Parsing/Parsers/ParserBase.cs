using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HtmlAgilityPack;

namespace RealEstate.Parsing.Parsers
{
    public abstract class ParserBase
    {
        protected abstract Advert ParseAdvertHtml(HtmlNode advertNode);

        protected abstract List<HtmlNode> GetAdvertsNode(HtmlNode pageNode);

        public abstract List<Advert> ParsePage(string url);

        protected const int MAXCOUNT = 3;
    }
}
