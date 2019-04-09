using HtmlAgilityPack;

namespace AliBuu.Models.Includes.Extensions
{
    public static class HtmlDocumentExtension
    {
        public static InnerTextValue GetInnerText(this HtmlDocument document, string xPath)
        {
            var result = new InnerTextValue();
            var temp = document.DocumentNode.SelectSingleNode(xPath);
            if(temp != null)
            {
                result.Value = temp.InnerText.Trim();
                result.HasValue = true;
            }
            return result;
        }
    }
}
