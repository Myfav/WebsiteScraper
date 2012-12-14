using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using HtmlAgilityPack;
using WebsiteScraper.ExtractHelpers;
using WebsiteScraper.IntermediateData;

namespace WebsiteScraper.PageHandler
{
    public class SetHandler
    {
        // Sample card title link "ctl00_ctl00_ctl00_MainContent_SubContent_SubContent_ctl00_listRepeater_ctl24_cardTitle"
        private const string CardTitleLinkStart =
            "ctl00_ctl00_ctl00_MainContent_SubContent_SubContent_ctl00_listRepeater_ct";

        private static readonly string CardTitleLinkEnd = "_cardTitle";

        // LinkStart + pageNumber + cardNumberOnPage + LinkEnd
        private static readonly List<HtmlNode> VisitedNodes = new List<HtmlNode>();

        public static void Handle(HtmlNode node, int level, List<CardLink> cardLinks)
        {
            ProcessPage(node, level, cardLinks);
        }

        private static void ProcessPage(HtmlNode node, int level, List<CardLink> cardLinks)
        {
            if (VisitedNodes.Contains(node))
                return;
            VisitedNodes.Add(node);

            if (NodeHasCardTitleAttributeOfInterest(node))
            {
                CardLink cardLink = ExtractCardLinkFromSetPageHtmlHelper.ExtractCardLink(node.OuterHtml);
                cardLinks.Add(cardLink);
            }

            foreach (HtmlNode child in node.ChildNodes)
            {
                ProcessPage(child, level + 1, cardLinks);
            }

            if (node.NextSibling != null)
            {
                ProcessPage(node.NextSibling, level, cardLinks);
            }
        }

        private static bool NodeHasCardTitleAttributeOfInterest(HtmlNode node)
        {
            if (node.Attributes.Any(a => (a.Value.Contains(CardTitleLinkStart)
                                          && a.Value.Contains(CardTitleLinkEnd))))
            {
                return true;
            }
            return false;
        }
    }
}