using MongoDB.Driver;

namespace WebsiteScraper.Mongo
{
    public static class MongoWrapper
    {
        private const string MongoConnectionString = "MongoDB";
        private const string MongoDbName = "Friday";
        private static MongoDatabase _mongoDatabase;

        public static MongoDatabase GetMongoDatabase()
        {
            if (_mongoDatabase == null)
            {
                var server = MongoServer.Create("mongodb://localhost:27017/?safe=true");
                _mongoDatabase = server.GetDatabase(MongoDbName);
            }
            return _mongoDatabase;
        }
    }
}
