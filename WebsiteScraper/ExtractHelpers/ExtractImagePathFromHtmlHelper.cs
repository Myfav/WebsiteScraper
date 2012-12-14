using System;
using System.Collections.Generic;
using System.Diagnostics;
using HtmlAgilityPack;
using WebsiteScraper.Card;

namespace WebsiteScraper.ExtractHelpers
{
    public class ExtractImagePathFromHtmlHelper
    {
        public static CardDetailEntry ExtractImagePath(string html)
        {
            HtmlDocument doc = CommonHelpers.GetHtmlDocumentFromHtml(html);

            var results = new List<HtmlNode>();
            HtmlNodeHelpers.FindNodesWithNameAndAttribute(doc.DocumentNode, "img", "src", new List<HtmlNode>(), results);

            Debug.Assert(results.Count > 0);
            HtmlNode imageNode = results[0];
            var completeUri = new Uri(new Uri(CommonHelpers.BaseCardUrl),
                                      HtmlNodeHelpers.GetAttributeValue(imageNode, "src"));
            var detailEntry = new CardDetailEntryString {FieldName = CardFieldName.Image};
            detailEntry.AddData(new CardFieldDataLink {Data = completeUri.AbsoluteUri});
            return detailEntry;
        }
    }
}
