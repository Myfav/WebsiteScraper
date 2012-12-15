using System.Collections.Generic;
using WebsiteScraper.Card;

namespace WebsiteScraper.DataStore
{
    public class XmlDataStore : IScraperDataStore
    {
        public void Write(IEnumerable<CardDetails> cardDetails)
        {
            foreach (CardDetails detail in cardDetails)
            {
                CommonHelpers.SerializeToXML(detail);
            }
        }
    }
}
