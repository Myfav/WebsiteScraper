using System;
using System.Collections.Generic;
using System.Linq;
using HtmlAgilityPack;
using WebsiteScraper.Card;

namespace WebsiteScraper.ExtractHelpers
{
    public static class ExtractCardFieldFromNodeListHelper
    {
        private static readonly Dictionary<string, CardFieldName> LabelFieldMap = new Dictionary<string, CardFieldName>
                                                                                      {
                                                                                          {
                                                                                              "Card Name:",
                                                                                              CardFieldName.CardName
                                                                                              },
                                                                                          {
                                                                                              "Converted Mana Cost:",
                                                                                              CardFieldName.
                                                                                              ConvertedManaCost
                                                                                              },
                                                                                          {
                                                                                              "Mana Cost:",
                                                                                              CardFieldName.ManaCost
                                                                                              },
                                                                                          {
                                                                                              "Types:",
                                                                                              CardFieldName.Types
                                                                                              },
                                                                                          {
                                                                                              "Card Text:",
                                                                                              CardFieldName.CardText
                                                                                              },
                                                                                          {
                                                                                              "Watermark:",
                                                                                              CardFieldName.Watermark
                                                                                              },
                                                                                          {
                                                                                              "P/T:",
                                                                                              CardFieldName.
                                                                                              PowerToughness
                                                                                              },
                                                                                          {
                                                                                              "Loyalty:",
                                                                                              CardFieldName.
                                                                                              PowerToughness
                                                                                              },
                                                                                          {
                                                                                              "Expansion:",
                                                                                              CardFieldName.Expansion
                                                                                              },
                                                                                          {
                                                                                              "Rarity:",
                                                                                              CardFieldName.Rarity
                                                                                              },
                                                                                          {
                                                                                              "Card #:",
                                                                                              CardFieldName.CardNumber
                                                                                              },
                                                                                          {
                                                                                              "Artist:",
                                                                                              CardFieldName.Artist
                                                                                              },
                                                                                          {
                                                                                              "Community Rating:",
                                                                                              CardFieldName.
                                                                                              CommunityRating
                                                                                              },
                                                                                      };

        public static CardDetailEntry Extract(List<HtmlNode> nodeList)
        {
            // We have all child nodes in order, in a flat list, turn this data into a valid CardDetailEntry
            string label;
            CardFieldName field = CardFieldName.Default;
            List<HtmlNode> remainingNodes = GetAndStripLabelNodes(nodeList, out field, out label);
            return ExtractDataFromField(field, remainingNodes);
        }

        private static CardDetailEntry ExtractDataFromField(CardFieldName field, List<HtmlNode> nodeList)
        {
            CardDetailEntry entry;
            switch (field)
            {
                case CardFieldName.CardText:
                case CardFieldName.Expansion:
                    {
                        // Img and Text
                        entry = new CardDetailEntrySequence {FieldName = field};
                        List<HtmlNode> imgTextNodes = nodeList.Where(n => n.Name == "img" || n.Name == "#text").ToList();
                        var addedText = new List<string>();
                        foreach (HtmlNode node in imgTextNodes)
                        {
                            if (node.Name == "img")
                            {
                                var completeUri = new Uri(new Uri(CommonHelpers.BaseUrl),
                                                          HtmlNodeHelpers.GetAttributeValue(node, "src"));
                                entry.AddData(new CardFieldDataLink {Data = completeUri.AbsoluteUri});
                            }

                            else if (node.Name == "#text")

                            {
                                string nodeText = CommonHelpers.GetTrimmedInnerText(node.InnerText);
                                if (!addedText.Contains(nodeText) && !string.IsNullOrEmpty(nodeText))
                                {
                                    entry.AddData(new CardFieldDataString {Data = nodeText});
                                    addedText.Add(nodeText);
                                }
                            }
                        }
                    }
                    break;
                case CardFieldName.ManaCost:
                    {
                        // We are only interested in img nodes
                        entry = new CardDetailEntrySequence {FieldName = field};
                        List<HtmlNode> imgNodes = nodeList.Where(n => n.Name == "img").ToList();
                        foreach (HtmlNode node in imgNodes)
                        {
                            var completeUri = new Uri(new Uri(CommonHelpers.BaseUrl),
                                                      HtmlNodeHelpers.GetAttributeValue(node, "src"));
                            entry.AddData(new CardFieldDataLink {Data = completeUri.AbsoluteUri});
                        }
                    }
                    break;
                default:
                    {
                        // Add unique text
                        var valueList = new List<string>();
                        // Determine what field
                        foreach (HtmlNode node in nodeList)
                        {
                            string trimmedInnerText = CommonHelpers.GetTrimmedInnerText(node.InnerText);
                            if (!HasInfoOfInterest(trimmedInnerText))
                            {
                                valueList.Add(trimmedInnerText);
                            }
                        }
                        // Strip out all blank results
                        valueList = valueList.Where(s => s.Length > 0).Distinct().ToList();
                        entry = new CardDetailEntryString {FieldName = field};
                        foreach (string value in valueList)
                        {
                            entry.AddData(new CardFieldDataString {Data = value});
                        }
                        return entry;
                    }
                    break;
            }
            return entry;
        }

        private static List<HtmlNode> GetAndStripLabelNodes(List<HtmlNode> nodeList, out CardFieldName field,
                                                            out string label)
        {
            var removeList = new List<HtmlNode>();
            field = CardFieldName.Default;
            label = "Invalid";

            // Determine what field
            foreach (HtmlNode node in nodeList)
            {
                string trimmedInnerText = CommonHelpers.GetTrimmedInnerText(node.InnerText);
                if (HasInfoOfInterest(trimmedInnerText))
                {
                    field = MapLabelToField(trimmedInnerText);
                    label = trimmedInnerText;
                    removeList.Add(node);
                }
            }

            // Remove
            return nodeList.Except(removeList).ToList();
        }

        private static CardFieldName MapLabelToField(string label)
        {
            foreach (var entry in LabelFieldMap)
            {
                if (label.Contains(entry.Key))
                {
                    return entry.Value;
                }
            }
            return CardFieldName.Default;
        }

        private static bool HasInfoOfInterest(string text)
        {
            foreach (var str in LabelFieldMap)
            {
                if (text.Contains(str.Key) && text.Length == str.Key.Length)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
