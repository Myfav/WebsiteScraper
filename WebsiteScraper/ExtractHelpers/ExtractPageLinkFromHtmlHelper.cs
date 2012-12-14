using System;
using System.Collections.Generic;
using System.Diagnostics;
using HtmlAgilityPack;
using WebsiteScraper.IntermediateData;

namespace WebsiteScraper.ExtractHelpers
{
    public class ExtractPageLinkFromHtmlHelper : ExtractHelperBase
    {
        public static void ExtractLinkInfoAtLevel(HtmlNode node, int levelToPrint, List<PageLink> pageLinks)
        {
            if (VisitedNodes.Contains(node))
                return;

            VisitedNodes.Add(node);

            if (levelToPrint == 0)
            {
                if (node.Name.Trim() == "a" &&
                    CommonHelpers.GetTrimmedInnerText(node.InnerText) != "&nbsp;&gt;" &&    // Angle bracket
                    CommonHelpers.GetTrimmedInnerText(node.InnerText) != "&gt;&gt;" &&      // Double angle bracket greater than
                    CommonHelpers.GetTrimmedInnerText(node.InnerText) != "&lt;&lt;" &&      // Double angle bracket less than 
                    CommonHelpers.GetTrimmedInnerText(node.InnerText) != "&lt;&nbsp;")       // Angle bracket less than
                {
                    int pageNumber;
                    var pageLink = new PageLink();
                    if (TryGetPageNumberText(node, out pageNumber))
                    {
                        pageLink.PageNumber = pageNumber;
                    }
                    else
                    {
                        Debug.Assert(false, "Failed to find page number, when we expect it to be there");
                    }

                    var relativeUrl = HtmlNodeHelpers.GetAttributeValue(node, "href");
                    var completeUri = new Uri(new Uri(CommonHelpers.BaseUrl), relativeUrl);
                    pageLink.Url = completeUri.AbsoluteUri;
                    pageLinks.Add(pageLink);
                }
                else if (node.NextSibling != null)
                {
                    ExtractLinkInfoAtLevel(node.NextSibling, levelToPrint, pageLinks);
                }
            }

            // Drill down into each child branch to find the label and the value
            foreach (HtmlNode child in node.ChildNodes)
            {
                // If child node has "Card Name:" text
                ExtractLinkInfoAtLevel(child, levelToPrint - 1, pageLinks);
            }
        }

        private static bool TryGetPageNumberText(HtmlNode node, out int pageNumber)
        {
            if (Int32.TryParse(node.InnerText, out pageNumber))
            {
                return true;
            }
            return false;
        }
    }
}