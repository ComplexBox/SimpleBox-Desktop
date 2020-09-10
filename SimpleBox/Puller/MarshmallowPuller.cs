using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.XPath;
using HtmlAgilityPack;
using SimpleBox.Models;

namespace SimpleBox.Puller
{
    public sealed class MarshmallowPuller : MallowPuller
    {
        public override string Name { get; } = "Marshmallow";
        public override string Address { get; } = @"https://marshmallow-qa.com/messages/personal";

        private readonly XPathExpression _findAllMarshmallowsEx
            = XPathExpression.Compile(
                "//li[contains(@class, 'list-group-item') and not(@id='sample-message') and not(contains(@class, 'tip'))]//a[contains(@data-target, 'message.content')]");
        private readonly XPathExpression _findLoadNextPageEx =
            XPathExpression.Compile("//a[contains(@class, 'load-more')]");
        private readonly Regex _extractIdEx = new Regex("/messages/([a-zA-Z0-9\\-]+)");

        protected override Mallow[] Pull(Mallow[] existingMallows)
        {
            List<Mallow> mallows = new List<Mallow>();

            string nextUri = Address;
            while (true)
            {
                HttpWebRequest req = CreateWebRequest(nextUri);
                req.AllowAutoRedirect = false;
                try
                {
                    HttpWebResponse resp = (HttpWebResponse) req.GetResponse();
                    if (resp.StatusCode != HttpStatusCode.OK)
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
                        encoding = Encoding.GetEncoding(string.IsNullOrWhiteSpace(resp.CharacterSet)
                            ? resp.ContentEncoding
                            : resp.CharacterSet);
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

                    HtmlNodeCollection nodes = doc.DocumentNode.SelectNodes(_findAllMarshmallowsEx);
                    var items = (
                        from node in nodes
                        let href = node.GetAttributeValue("href", "null")
                        where !string.IsNullOrEmpty(node.InnerText) && href != "null"
                        let idMatch = _extractIdEx.Match(href)
                        where idMatch.Success
                        let id = idMatch.Groups[1].Captures[0].Value
                        select new Mallow
                        {
                            OriginalMessage = node.InnerText
                        }).ToList();

                    foreach (Mallow mallow in items)
                    {
                        if (existingMallows.Any(exMallow => exMallow.OriginalMessage == mallow.OriginalMessage))
                            return mallows.ToArray();
                        mallows.Add(mallow);
                    }

                    HtmlNodeCollection nextPageNodes = doc.DocumentNode.SelectNodes(_findLoadNextPageEx);
                    if (nextPageNodes == null || nextPageNodes.Count == 0) break;
                    HtmlNode nextPageNode = nextPageNodes[0];
                    string nextUriRelative = nextPageNode.GetAttributeValue("href", "null");
                    if (nextUriRelative == "null") break;
                    nextUri = "https://marshmallow-qa.com" + nextUriRelative;
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
