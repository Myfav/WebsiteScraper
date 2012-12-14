using System.Collections.Generic;
using System.Linq;
using HtmlAgilityPack;
using WebsiteScraper.IntermediateData;

namespace WebsiteScraper.PageHandler
{
    public class SetPageSearcher
    {
        public static List<PageLink> CrawlSetPageForAllLinks(string pageOneLink)
        {
            HtmlDocument htmlDoc = CommonHelpers.GetHtmlDocumentFromLink(pageOneLink);
            var accumulatedLinks = new List<PageLink>();
            bool hasNewLinks = true;
            while (hasNewLinks)
            {
                // Sychronously accumulate of all page links (unique) by proccessing first page, 
                // then jumping to last page and continuing until there isn't any new links
                var newPageLinks = new List<PageLink>();
                SetPageFinder.Handle(htmlDoc.DocumentNode, 0, newPageLinks);

                hasNewLinks = AccumulateNewPages(newPageLinks, accumulatedLinks);
                if (hasNewLinks)
                {
                    accumulatedLinks = accumulatedLinks.OrderBy(l => l.PageNumber).ToList();
                    // Jump to last link
                    htmlDoc = CommonHelpers.GetHtmlDocumentFromLink(accumulatedLinks.Last().Url);
                }
                else
                {
                    break;
                }
            }
            return accumulatedLinks;
        }

        private static bool AccumulateNewPages(List<PageLink> newLinks, List<PageLink> accumulatedLinks)
        {
            bool addedNewLink = false;
            foreach (PageLink link in newLinks)
            {
                if (!accumulatedLinks.Any(existing => existing.PageNumber == link.PageNumber))
                {
                    addedNewLink = true;
                    accumulatedLinks.Add(link);
                }
            }
            return addedNewLink;
        }
    }
}