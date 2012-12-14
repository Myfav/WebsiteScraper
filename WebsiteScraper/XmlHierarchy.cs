using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Xml;

namespace WebsiteScraper
{
    public static class XmlHierarchy
    {
        private static readonly List<XmlNode> _visitedNodes = new List<XmlNode>();

        public static void XmlHelper(string html)
        {
            {
                try
                {
                    // Create an XML document instance.
                    // The same instance of DOM is used through out this code; this 
                    // may or may not be the actual case.
                    _visitedNodes.Clear();
                    var doc = new XmlDocument();
                    doc.LoadXml(html);

                    // Display the content of the DOM document.
                    Debug.Write("\n{0}\n", doc.OuterXml);
                    PrintXmlHierarchy(doc, 0);
                }
                catch (XmlException xmlEx) // Handle the XML exceptions here.   
                {
                    Console.WriteLine("{0}", xmlEx.Message);
                }
                catch (Exception ex) // Handle the generic exceptions here.
                {
                    Console.WriteLine("{0}", ex.Message);
                }
                finally
                {
                    // Finalize here.
                }
            }
        }

        private static void PrintXmlHierarchy(XmlNode node, int level)
        {
            if (_visitedNodes.Contains(node))
                return;

            _visitedNodes.Add(node);

            var trimChars = new[] {'\r', '\n'};
            string nodeValue = node.Value != null ? node.Value.Trim(trimChars) : "";
            nodeValue = nodeValue.Trim();
            //Debug.WriteLine(CommonHelpers.GenerateLevelString(level) + "Name: " + node.Name + " Value: " +
            //                nodeValue.Trim());

            foreach (XmlNode child in node.ChildNodes)
            {
                // If child node has "Card Name:" text
                PrintXmlHierarchy(child, level + 1);
            }

            if (node.NextSibling != null)
            {
                PrintXmlHierarchy(node.NextSibling, level);
            }
        }
    }
}