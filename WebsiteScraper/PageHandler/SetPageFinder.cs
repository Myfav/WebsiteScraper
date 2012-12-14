using System.Collections.Generic;
using System.Linq;
using HtmlAgilityPack;
using WebsiteScraper.ExtractHelpers;
using WebsiteScraper.IntermediateData;

namespace WebsiteScraper.PageHandler
{
    public class SetPageFinder
    {
        private const string PagingControl =
            "ctl00_ctl00_ctl00_MainContent_SubContent_bottomPagingControlsContainer";

        private static readonly List<HtmlNode> VisitedNodes = new List<HtmlNode>();

        public static void Handle(HtmlNode node, int level, List<PageLink> pageLinks)
        {
            VisitedNodes.Clear();
            ProcessPage(node, level, pageLinks);
        }

        private static void ProcessPage(HtmlNode node, int level, List<PageLink> pageLinks)
        {
            if (VisitedNodes.Contains(node))
                return;
            VisitedNodes.Add(node);

            if (NodeHasPagingControlAttributeOfInterest(node))
            {
                ExtractPageLinkFromHtmlHelper.ExtractLinkInfoAtLevel(node, 2, pageLinks);
            }

            foreach (HtmlNode child in node.ChildNodes)
            {
                ProcessPage(child, level + 1, pageLinks);
            }

            if (node.NextSibling != null)
            {
                ProcessPage(node.NextSibling, level, pageLinks);
            }
        }

        private static bool NodeHasPagingControlAttributeOfInterest(HtmlNode node)
        {
            if (node.Attributes.Any(a => (a.Value.Contains(PagingControl))))
            {
                return true;
            }
            return false;
        }
    }
}