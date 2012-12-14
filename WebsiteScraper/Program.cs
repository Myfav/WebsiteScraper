using System.Collections.Generic;
using System.Diagnostics;
using HtmlAgilityPack;
using WebsiteScraper.Card;
using WebsiteScraper.IntermediateData;
using WebsiteScraper.PageHandler;
using WebsiteScraper.Mongo;

namespace WebsiteScraper
{
    internal static class Program
    {
        private static Dictionary<string,string> Pages = new Dictionary<string, string>()
                                                             {
                                                                 //{"ScarsTestPage","http://gatherer.wizards.com/Pages/Search/Default.aspx?action=advanced&set=|[%22Scars%20of%20Mirrodin%22]"},
                                                                 //{"BesiegedTestPage","http://gatherer.wizards.com/Pages/Search/Default.aspx?action=advanced&set=|[%22Mirrodin%20Besieged%22]"},
                                                                 //{"NewPhyrexiaTestPage","http://gatherer.wizards.com/Pages/Search/Default.aspx?action=advanced&set=|[%22New%20Phyrexia%22]"},

                                                                 //{"PlaneswalkersTestPage","http://gatherer.wizards.com/Pages/Search/Default.aspx?action=advanced&type=+[%22Planeswalker%22]"},
                                                                 {"CoreSet11To13","http://gatherer.wizards.com/Pages/Search/Default.aspx?action=advanced&set=|[%22Magic%202011%22]|[%22Magic%202012%22]|[%22Magic%202013%22]"},
                                                             }; 


        private static void Main(string[] args)
        {
            //RunIndividualHandler(ArchdemonFlipTestPage);
            //TestSethandler(NewPhyrexiaTestPage);
            //TestSetPageFinder(InnistradTestPage);
            foreach (var page in Pages)
            {
                TestSetPageFinder(page.Value);
            }

            while (!AssetDownloader.IsIdle)
            {
            }
            while (true)
            {
            }
        }

        private static List<CardDetails> RunIndividualHandler(string link)
        {
            var indivHandler = new IndividualHandler();
            HtmlNode docNode = CommonHelpers.GetHtmlDocumentFromLink(link).DocumentNode;
            IndividualHandler.CardType cardType = indivHandler.DetermineCardType(docNode, 0);
            var detailsList = new List<CardDetails>();
            if (cardType == IndividualHandler.CardType.StandardCard)
            {
                var cardDetails = new CardDetails();
                indivHandler.Handle(CommonHelpers.GetHtmlDocumentFromLink(link).DocumentNode, 0, cardDetails,
                                    IndividualHandler.AttributeCardType.Standard);
                CommonHelpers.SerializeToXML(cardDetails);
                detailsList.Add(cardDetails);
            }
            else
            {
                var cardDetails = new CardDetails();
                indivHandler.Handle(CommonHelpers.GetHtmlDocumentFromLink(link).DocumentNode, 0, cardDetails,
                                    IndividualHandler.AttributeCardType.FlipCardDefault);
                CommonHelpers.SerializeToXML(cardDetails);
                detailsList.Add(cardDetails);

                var cardDetailsFlip = new CardDetails();
                indivHandler.Handle(CommonHelpers.GetHtmlDocumentFromLink(link).DocumentNode, 0, cardDetailsFlip,
                                    IndividualHandler.AttributeCardType.FlipCardAlternate);
                CommonHelpers.SerializeToXML(cardDetailsFlip);
                detailsList.Add(cardDetailsFlip);
            }
            return detailsList;
        }

        private static void TestSethandler(string link)
        {
            var cardLinks = new List<CardLink>();
            SetHandler.Handle(CommonHelpers.GetHtmlDocumentFromLink(link).DocumentNode, 0, cardLinks);
        }

        private static void TestSetPageFinder(string link)
        {
            var webClient = new WebClient();
            string downloadString =
                webClient.DownloadString(link);

            var doc = new HtmlDocument();
            doc.LoadHtml(downloadString);

            List<PageLink> pageLinks = SetPageSearcher.CrawlSetPageForAllLinks(link);
            var allCardLinks = new List<CardLink>();
            foreach (PageLink page in pageLinks)
            {
                var cardLinks = new List<CardLink>();
                Debug.WriteLine("Parsing page: " + page.Url + " for individual card links");
                SetHandler.Handle(CommonHelpers.GetHtmlDocumentFromLink(page.Url).DocumentNode, 0, cardLinks);

                allCardLinks.AddRange(cardLinks);
            }

            var allCardDetails = new List<CardDetails>();
            foreach (CardLink cardLink in allCardLinks)
            {
                allCardDetails.AddRange(RunIndividualHandler(cardLink.Url));
            }

            foreach (CardDetails detail in allCardDetails)
            {
                CommonHelpers.SerializeToXML(detail);
            }

            var mongoDb = MongoWrapper.GetMongoDatabase();
            if(!mongoDb.CollectionExists("CardDetails"))
            {
                mongoDb.CreateCollection("CardDetails");
            }
            var mongoCollection = mongoDb.GetCollection("CardDetails");
            foreach (CardDetails detail in allCardDetails)
            {
                mongoCollection.Save(detail.CreateBsonDocument());
            }
        }
    }
}