using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.XPath;
using HtmlAgilityPack;
using Newtonsoft.Json.Linq;
using SimpleBox.Models;

namespace SimpleBox.Puller
{
    public sealed class PeingPuller : MallowPuller
    {
        public override string Name { get; } = "Peing";
        public override string Address { get; } = @"https://peing.net/zh-CN/box?page=1";

        private readonly XPathExpression _extractEx
            = XPathExpression.Compile("//div[@data-questions]");
        private readonly XPathExpression _findLoadLastPageEx = XPathExpression.Compile("//span[contains(@class, 'last')]//a");

        protected override async Task<Mallow[]> Pull(Mallow[] existingMallows)
        {
            List<Mallow> mallows = new List<Mallow>();

            int totalPages = 1;
            for (int i = 1; i <= totalPages; i++)
            {
                HttpWebRequest req = await CreateWebRequest($"https://peing.net/zh-CN/box?page={i}");
                try
                {
                    HttpWebResponse resp = (HttpWebResponse)req.GetResponse();
                    if (resp.StatusCode != HttpStatusCode.OK
                        || !resp.ResponseUri.ToString().Contains("box"))
                    {
                        MessageBox.Show(
                            $"无法登录到{Name}服务，因此无法拉取。",
                            "错误",
                            MessageBoxButton.OK,
                            MessageBoxImage.Error,
                            MessageBoxResult.OK);

                        return null;
                    }

                    Encoding encoding;
                    try
                    {
                        encoding = Encoding.GetEncoding(string.IsNullOrWhiteSpace(resp.CharacterSet) ? resp.ContentEncoding : resp.CharacterSet);
                    }
                    catch (ArgumentException)
                    {
                        encoding = Encoding.UTF8;
                    }

                    HtmlDocument doc = new HtmlDocument();
                    using (Stream stream = resp.GetResponseStream())
                        if (stream != null)
                            doc.Load(stream, encoding);
                        else
                        {
                            MessageBox.Show(
                                "获取页面时出现问题，因此无法拉取。",
                                "错误",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error,
                                MessageBoxResult.OK);

                            return null;
                        }

                    HtmlNodeCollection nodes = doc.DocumentNode.SelectNodes(_extractEx);
                    if (nodes == null || nodes.Count == 0) continue;

                    string json = nodes[0].GetAttributeValue("data-questions", "[]");
                    json = WebUtility.HtmlDecode(json);
                    List<Mallow> items = new List<Mallow>();
                    foreach (JToken token in JArray.Parse(json))
                    {
                        if (!(token is JObject obj)) continue;
                        if (!obj.ContainsKey("uuid_hash")
                            || !obj.ContainsKey("body")) continue;
                        items.Add(new Mallow
                        {
                            OriginalMessage = obj.GetValue("body")?.ToString()
                        });
                    }

                    foreach (Mallow mallow in items)
                    {
                        if (existingMallows.Any(exMallow => exMallow.OriginalMessage == mallow.OriginalMessage))
                            return mallows.ToArray();
                        mallows.Add(mallow);
                    }

                    HtmlNodeCollection lastPageNodes = doc.DocumentNode.SelectNodes(_findLoadLastPageEx);
                    if (lastPageNodes == null || lastPageNodes.Count == 0) break;
                    HtmlNode lastPageNode = lastPageNodes[0];
                    string lastUriRelative = lastPageNode.GetAttributeValue("href", "null");
                    if (lastUriRelative == "null") break;
                    int pagePos = lastUriRelative.LastIndexOf('=') + 1;
                    if (pagePos < lastUriRelative.Length)
                        int.TryParse(lastUriRelative.Substring(pagePos), out totalPages);
                }
                catch (Exception)
                {
                    MessageBox.Show(
                        "拉取时出现问题。",
                        "错误",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error,
                        MessageBoxResult.OK);

                    return null;
                }
            }

            return mallows.ToArray();
        }
    }
}
