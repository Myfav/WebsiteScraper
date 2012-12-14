using System.Collections.Generic;
using HtmlAgilityPack;

namespace WebsiteScraper.ExtractHelpers
{
    public abstract class ExtractHelperBase
    {
        protected static readonly List<HtmlNode> VisitedNodes = new List<HtmlNode>();
    }
}