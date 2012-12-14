using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using HtmlAgilityPack;
using WebsiteScraper.Card;
using WebsiteScraper.ExtractHelpers;

namespace WebsiteScraper.PageHandler
{
    public class IndividualHandler
    {
        #region ProcessPass enum

        public enum ProcessPass
        {
            PreScan,
            Details,
            Image,
        };

        public enum CardType
        {
            Invalid,
            StandardCard,
            FlipCard,
        };

        public enum AttributeCardType
        {
            Standard,
            FlipCardDefault,
            FlipCardAlternate,
        }

        private string[] cardTypeAttributeMiddle = new string[]
        {
            "",         //standard card has no middle
            "_ctl05",    // flip card front
            "_ctl06",    // Flip card back
        };

        #endregion

        private static readonly string cardAttributeStart = "ctl00_ctl00_ctl00_MainContent_SubContent_SubContent";
        private static readonly string[] standardCardAttributes = new[]
                                                                    {
                                                                        "_nameRow"
                                                                        , // Name
                                                                        "_manaRow"
                                                                        , // Mana cost
                                                                        "_cmcRow"
                                                                        , // Converted mana cost
                                                                        "_typeRow"
                                                                        , // Creature type
                                                                        "_textRow"
                                                                        , // Rules text
                                                                        "_markRow"
                                                                        ,
                                                                        "_ptRow"
                                                                        , // Power toughness
                                                                        "_setRow"
                                                                        ,
                                                                        "_rarityRow"
                                                                        , // Rarity
                                                                        "_numberRow"
                                                                        ,
                                                                        "_artistRow"
                                                                        ,
                                                                        "_playerRatingRow"
                                                                        ,
                                                                    };

        private static readonly string[] standardCardAssetAttributes = new[]
                                                                         {
                                                                             "_cardImage"
                                                                             , // card image
                                                                         };

        private static readonly List<HtmlNode> VisitedNodes = new List<HtmlNode>();

        private bool NodeHasCardAttributeOfValue(HtmlNode node,AttributeCardType attrCardType)
        {
            foreach (string str in standardCardAttributes)
            {
                var compareAttribute = cardAttributeStart + cardTypeAttributeMiddle[(int)attrCardType] + str;
                if (node.Attributes.Any(a => a.Value.Contains(compareAttribute)))
                {
                    return true;
                }
            }
            return false;
        }

        private bool NodeHasAssetAttributeOfInterest(HtmlNode node,AttributeCardType attrCardType)
        {
            foreach (string str in standardCardAssetAttributes)
            {
                var compareAttribute = cardAttributeStart + cardTypeAttributeMiddle[(int)attrCardType] + str;
                if (node.Attributes.Any(a => a.Value.Contains(compareAttribute)))
                {
                    return true;
                }
            }
            return false;
        }

        public CardType DetermineCardType(HtmlNode node, int level)
        {
            VisitedNodes.Clear();
            CardType cardType = CardType.Invalid;
            DetermineIfStandardCardType(node, ref cardType);
            return cardType;
        }

        private void DetermineIfStandardCardType(HtmlNode node, ref CardType type)
        {
            if (VisitedNodes.Contains(node))
                return;
            VisitedNodes.Add(node);

            if (NodeHasCardAttributeOfValue(node,AttributeCardType.Standard))
            {
                type = CardType.StandardCard;
            }

            foreach (HtmlNode child in node.ChildNodes)
            {
                DetermineIfStandardCardType(child, ref type);
            }

            if (node.NextSibling != null)
            {
                DetermineIfStandardCardType(node.NextSibling, ref type);
            }
        }


        public void Handle(HtmlNode node, int level, CardDetails cardDetails, AttributeCardType attributeType)
        {
            VisitedNodes.Clear();

            ProcessPage(node, 0, ProcessPass.Details, cardDetails,attributeType);

            Debug.WriteLine("Processing: " + cardDetails.Expansion.Value + " " + cardDetails.CardName.Value);
            VisitedNodes.Clear();
            ProcessPage(node, 0, ProcessPass.Image, cardDetails,attributeType);

            DownloadAllImages(cardDetails);
        }

        private void DownloadAllImages(CardDetails details)
        {
            if (details.ManaCost != null) // Land won't have mana cost
            {
                DownloadImagesForDetailEntrySequence(details.ManaCost);
            }
            if (details.CardText != null) // Some cards don't have body text
            {
                DownloadImagesForDetailEntrySequence(details.CardText);
            }
            DownloadImagesForDetailEntrySequence(details.Expansion);

            // Image
            details.LocalImagePath = new CardDetailEntryString {FieldName = CardFieldName.LocalImagePath};
            details.LocalImagePath.AddData(
                new CardFieldDataLink
                    {
                        Data =
                            DownloadImageHelper.DownloadImageAsync(details.ImageUrl.Value,
                                                                   CommonHelpers.RemoveInvalidFilenameCharacters(details.Expansion.ToString()) + " " + CommonHelpers.RemoveInvalidFilenameCharacters(details.CardName.Value) +
                                                                   ".jpg",
                                                                   CommonHelpers.ImagesFolder)
                    });
        }

        private void DownloadImagesForDetailEntrySequence(CardDetailEntrySequence sequence)
        {
            foreach (CardFieldData element in sequence.Data)
            {
                if (element is CardFieldDataLink)
                {
                    string convertedImageName = CommonHelpers.ConvertWebImageUrlToLocalFileName(element.Data);
                    DownloadImageHelper.DownloadImage(element.Data,
                                                      convertedImageName + ".jpg",
                                                      CommonHelpers.DetailsImagesFolder);
                }
            }
        }

        private void ProcessPage(HtmlNode node, int level, ProcessPass pass, CardDetails cardDetails, AttributeCardType attributeType)
        {
            if (VisitedNodes.Contains(node))
                return;
            VisitedNodes.Add(node);
            switch (pass)
            {
                case ProcessPass.Details:
                    if (NodeHasCardAttributeOfValue(node,attributeType))
                    {
                        var resultNodes = new List<HtmlNode>();
                        HtmlNodeHelpers.FindChildNodesInOrderBelowLevel(node.ChildNodes.First(), 0, new List<HtmlNode>(),
                                                                        resultNodes);
                        CardDetailEntry entry = ExtractCardFieldFromNodeListHelper.Extract(resultNodes);
                        cardDetails.AddEntry(entry);
                    }
                    break;
                case ProcessPass.Image:
                    if (NodeHasAssetAttributeOfInterest(node,attributeType))
                    {
                        //((CardDetailEntryString)cardDetails.GetEntry(CardFieldName.CardName)).Value
                        CardDetailEntry entry = ExtractImagePathFromHtmlHelper.ExtractImagePath(node.OuterHtml);
                        cardDetails.AddEntry(entry);
                    }
                    break;
            }

            foreach (HtmlNode child in node.ChildNodes)
            {
                ProcessPage(child, level + 1, pass, cardDetails,attributeType);
            }

            if (node.NextSibling != null)
            {
                ProcessPage(node.NextSibling, level, pass, cardDetails,attributeType);
            }
        }
    }
}