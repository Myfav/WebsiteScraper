using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Xml.Serialization;
using HtmlAgilityPack;
using WebsiteScraper.Card;

namespace WebsiteScraper
{
    public static class CommonHelpers
    {
        public static string BaseUrl = "http://gatherer.wizards.com/";
        public static string BaseCardUrl = BaseUrl + "pages/Card/";
        public static string ImagesFolder = "CardImages";
        public static string DetailsImagesFolder = "DetailsImages";
        public static string XmlFolder = "XmlData";

        public static string GenerateLevelString(int level)
        {
            string levString = "";
            for (int i = 0; i < level; i++)
            {
                levString += " ";
            }
            return levString;
        }

        private static string[] _webUrlTokens = new[]
                                                    {
                                                        "size=",
                                                        "name=",
                                                        "type=",
                                                    };

        public static string ConvertWebImageUrlToLocalFileName(string webUrl)
        {
            string decodedUrl = WebUtility.HtmlDecode(webUrl);

            Debug.Assert(decodedUrl.Contains(BaseUrl));
            Debug.Assert(decodedUrl.Length > BaseUrl.Length);
            string shortenedUrl = decodedUrl.Remove(0, BaseUrl.Length);
            return RemoveInvalidFilenameCharacters(shortenedUrl);
        }

        public static string RemoveInvalidFilenameCharacters(string url)
        {
            string invalid = new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars());

            foreach (char c in invalid)
            {
                url = url.Replace(c.ToString(), "");
            }
            url = url.Replace(":", "");
            url = url.Replace(".", "");
            return url;
        }

        public static void PrintHtmlHierarchy(HtmlNode node, int level, List<HtmlNode> visitedNodes)
        {
            if (visitedNodes.Contains(node))
                return;

            visitedNodes.Add(node);

            //Debug.WriteLine(level + ":" + GenerateLevelString(level) + "\nNode.Name: " + node.Name +
            //                "\nNode.InnerText: " +
            //                GetTrimmedInnerText(node.InnerText) +
            //                "\nGenerateNodeString: " + HtmlNodeHelpers.GenerateNodeString(node));

            foreach (HtmlNode child in node.ChildNodes)
            {
                // If child node has "Card Name:" text
                PrintHtmlHierarchy(child, level + 1, visitedNodes);
            }

            if (node.NextSibling != null)
            {
                PrintHtmlHierarchy(node.NextSibling, level, visitedNodes);
            }
        }

        public static string GetTrimmedInnerText(string text)
        {
            string trimmedText = text ?? "";
            trimmedText = trimmedText.Replace("\r", "");
            trimmedText = trimmedText.Replace("\n", "");
            //trimmedText = trimmedText.Replace("&nbsp;", " ");
            trimmedText = trimmedText.Trim();
            return trimmedText;
        }

        public static HtmlDocument GetHtmlDocumentFromLink(string link)
        {
            var document = new HtmlDocument();
            var reader = new StreamReader(System.Net.WebRequest.Create(link).GetResponse().GetResponseStream(),
                                          Encoding.UTF8); //put your encoding            
            document.Load(reader);
            return document;
        }

        public static HtmlDocument GetHtmlDocumentFromHtml(string html)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(html);
            return doc;
        }

        public static void SerializeToXML(CardDetails details)
        {
            var serializer = new XmlSerializer(typeof (CardDetails));

            string path = Path.Combine(Directory.GetCurrentDirectory(),
                                       XmlFolder + Path.DirectorySeparatorChar +
                                       RemoveInvalidFilenameCharacters(details.Expansion.ToString()) + " " +
                                       RemoveInvalidFilenameCharacters(details.CardName.Value) + ".xml");

            if (File.Exists(path))
            {
                File.Delete(path);
            }
            string directoryName = Path.GetDirectoryName(path);
            if (!Directory.Exists(directoryName))
            {
                Directory.CreateDirectory(directoryName);
            }

            TextWriter textWriter = new StreamWriter(path, false);
            serializer.Serialize(textWriter, details);
            textWriter.Close();
        }

        public static List<CardFieldData> FilterNodeList(List<HtmlNode> nodeList)
        {
            // Remove duplicates, remove blank entries
            var newNodeList = new List<CardFieldData>();
            foreach (HtmlNode node in nodeList)
            {
            }
            return newNodeList;
        }

        public static string ConvertWebToUtf8(string webEncodedString)
        {
            Encoding iso = Encoding.GetEncoding("ISO-8859-1");
            Encoding unicode = Encoding.Unicode;
            Encoding utf8 = Encoding.UTF8;

            byte[] isoBytes = iso.GetBytes(webEncodedString);
            byte[] unicodeBytes = Encoding.Convert(iso, unicode, isoBytes);
            string msg = unicode.GetString(unicodeBytes);
            return msg;
        }
    }
}