using System.Collections.Generic;
using WebsiteScraper.Card;
using WebsiteScraper.Mongo;

namespace WebsiteScraper.DataStore
{
    public class MongoDbDataStore : IScraperDataStore
    {
        public void Write(IEnumerable<CardDetails> cardDetails)
        {
            var mongoDb = MongoWrapper.GetMongoDatabase();
            if (!mongoDb.CollectionExists("CardDetails"))
            {
                mongoDb.CreateCollection("CardDetails");
            }
            var mongoCollection = mongoDb.GetCollection("CardDetails");
            foreach (CardDetails detail in cardDetails)
            {
                mongoCollection.Save(detail.CreateBsonDocument());
            }
        }
    }
}
