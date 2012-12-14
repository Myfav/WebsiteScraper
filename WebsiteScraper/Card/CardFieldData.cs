using System;
using System.Xml.Serialization;

namespace WebsiteScraper.Card
{
    [Serializable]
    [XmlInclude(typeof(CardFieldDataString))]
    [XmlInclude(typeof(CardFieldDataLink))]
    public abstract class CardFieldData
    {
        public string Data { get; set; }
    }
}
