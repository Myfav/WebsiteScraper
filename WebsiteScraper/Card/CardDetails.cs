using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using MongoDB.Bson;

namespace WebsiteScraper.Card
{
    public class CardDetails
    {
        public CardDetailEntryString CardName;
        public CardDetailEntrySequence ManaCost;
        public int ConvertedManaCost;
        public CardDetailEntryString Types;
        public CardDetailEntrySequence CardText;
        public string CardTextInnerHtml;
        public string CardTextOuterHtml;
        public CardDetailEntryString Watermark;
        public string Power;
        public string Toughness;
        public CardDetailEntrySequence Expansion;
        public CardDetailEntryString Rarity;
        public CardDetailEntryString CardNumber;
        public CardDetailEntryString Artist;
        public CardDetailEntryString CommunityRating;
        public CardDetailEntryString ImageUrl;

        public CardDetailEntryString LocalImagePath;

        private readonly List<CardDetailEntry> _entries = new List<CardDetailEntry>();

        public void AddEntry(CardDetailEntry entry)
        {
            Debug.Assert(!_entries.Contains(entry));
            _entries.Add(entry);
            AssignFieldFromEntry(entry);
        }

        public CardDetailEntry GetEntry(CardFieldName fieldName)
        {
            return _entries.First(e => e.FieldName == fieldName);
        }

        private void AssignFieldFromEntry(CardDetailEntry entry)
        {
            switch (entry.FieldName)
            {
                case CardFieldName.CardName:
                    CardName = (CardDetailEntryString) entry;
                    break;
                case CardFieldName.ManaCost:
                    ManaCost = (CardDetailEntrySequence) entry;
                    break;
                case CardFieldName.ConvertedManaCost:
                    if (!Int32.TryParse(((CardDetailEntryString) entry).Value, out ConvertedManaCost))
                    {
                        Debug.Assert(false);
                    }
                    break;
                case CardFieldName.Types:
                    Types = (CardDetailEntryString) entry;
                    break;
                case CardFieldName.CardText:
                    CardText = (CardDetailEntrySequence) entry;
                    CardTextInnerHtml = entry.InnerHtmlCode;
                    CardTextOuterHtml = entry.OuterHtmlCode;
                    break;
                case CardFieldName.Watermark:
                    Watermark = (CardDetailEntryString) entry;
                    break;
                case CardFieldName.PowerToughness:
                    ExtractPowerToughnessOrLoyalty(((CardDetailEntryString) entry).Value, out Power, out Toughness);
                    break;
                case CardFieldName.Expansion:
                    Expansion = (CardDetailEntrySequence) entry;
                    break;
                case CardFieldName.Rarity:
                    Rarity = (CardDetailEntryString) entry;
                    break;
                case CardFieldName.CardNumber:
                    CardNumber = (CardDetailEntryString) entry;
                    break;
                case CardFieldName.Artist:
                    Artist = (CardDetailEntryString) entry;
                    break;
                case CardFieldName.CommunityRating:
                    CommunityRating = (CardDetailEntryString) entry;
                    break;
                case CardFieldName.Image:
                    ImageUrl = (CardDetailEntryString) entry;
                    break;
            }
        }

        private void ExtractPowerToughnessOrLoyalty(string ptString, out string power, out string toughness)
        {
            string[] tokens = ptString.Split('/');
            var trimmedTokens = new List<string>();
            foreach (string token in tokens)
            {
                trimmedTokens.Add(token.Trim());
            }

            Debug.Assert(trimmedTokens.Count > 0 && trimmedTokens.Count <= 2);
            power = trimmedTokens[0];
            toughness = "";
            if (trimmedTokens.Count > 1)
            {
                toughness = trimmedTokens[1];
            }
        }

        private void ExtractCommnityRating(string ptString, out float rating, out float totalRating)
        {
            string[] tokens = ptString.Split('/');
            Debug.Assert(tokens.Length == 2);
            bool ratingResult = float.TryParse(tokens[0].Trim(), out rating);
            bool totalRatingResult = float.TryParse(tokens[1].Trim(), out totalRating);
            Debug.Assert(ratingResult && totalRatingResult);
        }

        public BsonDocument CreateBsonDocument()
        {
            var doc = new BsonDocument();

            doc.Add("CardName", CardName.GetBsonValue());
            if (ManaCost != null)
                doc.Add("ManaCost", ManaCost.GetBsonValue());

            doc.Add("ConvertedManaCost", ConvertedManaCost);
            doc.Add("Types", Types.GetBsonValue());
            if (CardText != null)
                doc.Add("CardText", CardText.GetBsonValue());

            doc.Add("CardTextInnerHtml", CardTextInnerHtml);
            doc.Add("CardTextOuterHtml", CardTextOuterHtml);
            if (Watermark != null)
                doc.Add("Watermark", Watermark.GetBsonValue());

            doc.Add("Power", Power);
            doc.Add("Toughness", Toughness);

            doc.Add("Expansion", Expansion.GetBsonValue());
            doc.Add("Rarity", Rarity.GetBsonValue());
            doc.Add("CardNumber", CardNumber.GetBsonValue());
            doc.Add("Artist", Artist.GetBsonValue());
            doc.Add("CommunityRating", CommunityRating.GetBsonValue());
            doc.Add("ImageUrl", ImageUrl.GetBsonValue());
            doc.Add("LocalImagePath", LocalImagePath.GetBsonValue());

            return doc;
        }
    }
}