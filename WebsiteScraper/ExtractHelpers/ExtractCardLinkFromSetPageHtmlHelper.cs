using System;
using System.Collections.Generic;
using HtmlAgilityPack;
using WebsiteScraper.IntermediateData;

namespace WebsiteScraper.ExtractHelpers
{
    public class ExtractCardLinkFromSetPageHtmlHelper
    {
        public static CardLink ExtractCardLink(string html)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            // Extract name for card
            var resultNames = new List<string>();
            HtmlNodeHelpers.GetInnerTextOfNodeWithName(doc.DocumentNode, "#document", new List<HtmlNode>(), resultNames);

            // Find URL for card
            var resultNodes = new List<HtmlNode>();
            HtmlNodeHelpers.FindNodesWithNameAndAttribute(doc.DocumentNode, "a", "href", new List<HtmlNode>(),
                                                          resultNodes);

            HtmlNode linkNode = resultNodes[0];
            var completeUri = new Uri(new Uri(CommonHelpers.BaseCardUrl), HtmlNodeHelpers.GetAttributeValue(linkNode, "href"));
            string linkPath = completeUri.AbsoluteUri;

            return new CardLink {CardName = resultNames[0], Url = linkPath};
        }
    }
}