using System;
using System.Linq;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using MongoDB.Driver.Linq;

namespace IQ.IR.ContinuousDeployment.Server.Persistence
{
    //TODO: Copied and stripped down from IQ.IR.Core.  Common location to prevent duplication?
    public class MongoRepository<T> : IDocumentRepository<T> where T : class
    {
        readonly MongoDatabase _db;
        readonly MongoCollection<T> _docs;

        public MongoRepository(MongoDatabase db)
            : this(db, SafeMode.True)
        { }

        public MongoRepository(MongoDatabase db, SafeMode safeMode)
        {
            _db = db;
            _docs = GetOrCreateCollection(safeMode);
        }

        private MongoCollection<T> GetOrCreateCollection(SafeMode safeMode)
        {
            //TODO: Is this okay? Is there a big cost to doing it this way?  Should ask a Mongo expert
            var collectionName = typeof (T).Name;
            if (!_db.CollectionExists(collectionName))
                _db.CreateCollection(collectionName);
                
            return _db.GetCollection<T>(collectionName, safeMode);
        }

        public void Create(T resource) { _docs.Insert(resource); }

        public void Update(T resource) { _docs.Save(resource); }

        public void CreateOrUpdate(T resource) { _docs.Save(resource); }

        /// <summary>
        /// Allows for querying the document repository
        /// </summary>
        public IQueryable<T> CreateQuery() { return _docs.AsQueryable(); }

        public IQueryable<T> CreateQuery(IMongoQuery query) { return _docs.Find(query).AsQueryable(); }

        public virtual T Get(dynamic id)
        {
            if (id == null || string.IsNullOrEmpty(id.ToString()))
                return null;
            return _docs.FindOne(new QueryDocument("_id", id));
        }

        public void Update(dynamic id, IMongoUpdate update)
        {
            var builder = update as UpdateBuilder;
            if (builder != null)
            {
                if (builder.ToBsonDocument().Count() == 0)
                    throw new Exception("UpdateBuilder cannot be empty");
            }
            _docs.Update(new QueryDocument("_id", id), update);
        }

        /// <summary>
        /// Updates all documents that match the query
        /// </summary>
        /// <param name="query"></param>
        /// <param name="update"></param>
        /// <returns>Number of documents updated</returns>
        public long UpdateMany(IMongoQuery query, IMongoUpdate update)
        {
            var result = _docs.Update(query, update, UpdateFlags.Multi);
            return result == null
                ? 0
                : result.DocumentsAffected;
        }

        /// <summary>
        /// Deletes a document from the repository, will remove soft deleted documents as well
        /// </summary>
        public void Delete(dynamic id)
        {
            _docs.Remove(new QueryDocument("_id", id));
        }

        public void Delete(IMongoQuery query)
        {
            if (CreateQuery(query).Any()) _docs.Remove(query, RemoveFlags.None);
        }

        //try
        //{
        //    var product2 = _products.FindCurrent(Query.Null).FirstOrDefault(p => p.Assets.Select(a => a.Id).Contains(encodedVideo.SourceAssetId));
        //}
        //catch
        //{
        //    string s = "";
        //}

        //var asset = _assets.CreateQuery(Query.EQ("_id", encodedVideo.SourceAssetId)).ToList().FirstOrDefault();
        //var asset2 = _assets.CreateQuery().FirstOrDefault(a => a.Id == encodedVideo.SourceAssetId);

        //var encodedVideo2 = _encodedVideos.CreateQuery(Query.EQ("SourceAssetId", encodedVideo.SourceAssetId)).ToList().FirstOrDefault();
        //var encodedVideo3 = _encodedVideos.CreateQuery().FirstOrDefault(v => v.SourceAssetId == encodedVideo.SourceAssetId);
    }
}