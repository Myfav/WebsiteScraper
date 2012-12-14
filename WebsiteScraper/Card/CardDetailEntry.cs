using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using MongoDB.Bson;

namespace WebsiteScraper.Card
{
    [Serializable]
    public enum CardFieldName
    {
        Default,
        CardName,
        ManaCost,
        ConvertedManaCost,
        Types,
        CardText,
        Watermark,
        PowerToughness,
        Expansion,
        Rarity,
        CardNumber,
        Artist,
        CommunityRating,
        Image,
        LocalImagePath,
    };

    public interface IDatabaseTypes
    {
        BsonValue GetBsonValue();
    }

    [Serializable]
    public abstract class CardDetailEntry : IDatabaseTypes
    {
        public CardFieldName FieldName;
        public List<CardFieldData> Data = new List<CardFieldData>();
        public string InnerHtmlCode;
        public string OuterHtmlCode;

        public virtual void AddData(CardFieldData data)
        {
            Data.Add(data);
        }


        public abstract BsonValue GetBsonValue();
    }

    [Serializable]
    public class CardDetailEntryString : CardDetailEntry
    {
        public string Value
        {
            get
            {
                return Data[0].Data;
            }
        }

        public override string ToString()
        {
            return Value;
        }

        public override void AddData(CardFieldData data)
        {
            var htmlDecodedString = WebUtility.HtmlDecode(data.Data);
            //var convertedString = CommonHelpers.ConvertWebToUtf8(htmlDecodedString);
            Data.Add(new CardFieldDataString { Data = htmlDecodedString });
        }

        public override BsonValue GetBsonValue()
        {
            if( Value != null)
            {
                return new BsonString(Value);
            }
            return null;
        }
    }

    [Serializable]
    public class CardDetailEntrySequence : CardDetailEntry
    {
        public string Value
        {
            get { return string.Join(",", (from d in Data select d.Data).ToArray()); }
        }

        public override string ToString()
        {
            return Data.First(a => a is CardFieldDataString).Data;
        }

        public override BsonValue GetBsonValue()
        {
            return new BsonArray((from d in Data select d.Data).ToArray());
        }
    }
}