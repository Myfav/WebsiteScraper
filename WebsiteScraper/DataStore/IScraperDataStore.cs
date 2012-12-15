using System.Collections.Generic;
using WebsiteScraper.Card;

namespace WebsiteScraper.DataStore
{
    interface IScraperDataStore
    {
        void Write(IEnumerable<CardDetails> cardDetails);
    }
}
