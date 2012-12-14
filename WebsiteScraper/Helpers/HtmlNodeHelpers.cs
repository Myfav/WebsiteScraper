using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using HtmlAgilityPack;

namespace WebsiteScraper
{
    public static class HtmlNodeHelpers
    {
        private static readonly Dictionary<string, Func<string, string>> _filters = new Dictionary
            <string, Func<string, string>>
                                                                                        {
                                                                                            {"src", ProcessSrc},
                                                                                            {"href", ProcessSrc}
                                                                                        };

        public static string Src = "src";

        public static string GetAttributeValue(HtmlNode node, string attributeName)
        {
            IEnumerable<HtmlAttribute> matchingAttributes = node.Attributes.Where(a => a.Name.Contains(attributeName));
            string matchedValue = matchingAttributes.First().Value;
            if (_filters.ContainsKey(attributeName.ToLower()))
            {
                matchedValue = _filters[attributeName.ToLower()].Invoke(matchedValue);
            }
            return matchedValue;
        }

        private static string ProcessSrc(string input)
        {
            string result = input;
            if (input.Contains("&amp;"))
            {
                result = input.Replace("&amp;", "&");
            }
            return result;
        }

        public static string GenerateNodeString(HtmlNode node)
        {
            string attributeString = "";
            foreach (HtmlAttribute attr in node.Attributes)
            {
                attributeString += " ID: " + attr.Name + " Value: " + attr.Value;
            }
            string nameString = "Name:" + node.Name;
            string textString = "InnerText:" + node.InnerHtml;
            string lineString = "Line:" + node.Line + "LinePosition:" + node.LinePosition;

            string typeString = "Type: " + node.NodeType;
            string attribString = "Attribute:" + attributeString;

            return nameString + " " + attribString + " " + typeString + " " + lineString + " " + textString;
        }


        private static bool NodeHasAttributeName(HtmlNode node, string attributeName)
        {
            if (node.Attributes.Any(a => a.Name.Contains(attributeName)))
            {
                return true;
            }
            return false;
        }

        public static void FindNodesWithNameAndAttribute(HtmlNode node, string name, string attributeName,
                                                         List<HtmlNode> visitedNodes, List<HtmlNode> resultNodes)
        {
            if (visitedNodes.Contains(node))
                return;

            visitedNodes.Add(node);

            if (node.Name.Contains(name) && NodeHasAttributeName(node, attributeName))
            {
                resultNodes.Add(node);
            }

            foreach (HtmlNode child in node.ChildNodes)
            {
                // If child node has "Card Name:" text
                FindNodesWithNameAndAttribute(child, name, attributeName, visitedNodes, resultNodes);
            }

            if (node.NextSibling != null)
            {
                FindNodesWithNameAndAttribute(node.NextSibling, name, attributeName, visitedNodes, resultNodes);
            }
        }

        // To get everything below parent, specify level = 1
        public static void FindChildNodesInOrderBelowLevel(HtmlNode node, int level, List<HtmlNode> visitedNodes,
                                                           List<HtmlNode> resultNodes)
        {
            if (visitedNodes.Contains(node))
                return;

            visitedNodes.Add(node);

            if (level <= 0)
            {
                resultNodes.Add(node);
            }

            foreach (HtmlNode child in node.ChildNodes)
            {
                // If child node has "Card Name:" text
                FindChildNodesInOrderBelowLevel(child, level - 1, visitedNodes, resultNodes);
            }

            if (node.NextSibling != null)
            {
                FindChildNodesInOrderBelowLevel(node.NextSibling, level - 1, visitedNodes, resultNodes);
            }
        }

        public static void GetInnerTextOfNodeWithName(HtmlNode node, string nodeName, List<HtmlNode> visitedNodes,
                                                      List<string> resultText)
        {
            if (visitedNodes.Contains(node))
                return;

            visitedNodes.Add(node);

            if (node.Name.Contains(nodeName))
            {
                resultText.Add(CommonHelpers.GetTrimmedInnerText(node.InnerText));
            }

            foreach (HtmlNode child in node.ChildNodes)
            {
                // If child node has "Card Name:" text
                GetInnerTextOfNodeWithName(child, nodeName, visitedNodes, resultText);
            }

            if (node.NextSibling != null)
            {
                GetInnerTextOfNodeWithName(node.NextSibling, nodeName, visitedNodes, resultText);
            }
        }
    }
}