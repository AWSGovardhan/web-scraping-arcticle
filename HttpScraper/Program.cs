﻿using System.Text;
using System;
using System.Net;
using System.IO;
using System.IO.Compression;
using HtmlAgilityPack;
using Jurassic;
using System.Collections.Generic;

namespace HttpScraper
{
    internal static class Program
    {
        private static readonly List<string> XPathList = new List<string>
        {
            "//td",
            "//tr",
            "//tr[@class]",
            "//tr[@class='odd']",
            "//*[@class='odd']",
            "//tr[starts-with(@class, 'e')]"
        };

        private static void Main()
        {
            TestXPath();
            TestJavascript();
        }

        private static void TestXPath()
        {
            var content = GetHtmlContent("http://localhost:5000/scraper/table");

            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(content);

            foreach (var xpath in XPathList)
            {
                var nodes = htmlDocument.DocumentNode.SelectNodes(xpath);

                Console.WriteLine($"Results for {xpath}");
                foreach (var node in nodes)
                {
                    Console.WriteLine(node.InnerText.Replace("\r\n", string.Empty));
                }
                Console.WriteLine();
            }
        }

        private static void TestJavascript()
        {
            var content = GetHtmlContent("http://localhost:5000/scraper/link");

            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(content);

            var scriptNode = htmlDocument.DocumentNode.SelectSingleNode("//script");

            var scriptEngine = new ScriptEngine();
            scriptEngine.Evaluate(scriptNode.InnerHtml);

            var javascriptLink = scriptEngine.GetGlobalValue<string>("secretLink");
            Console.WriteLine($"Link generated by javascript: {javascriptLink}");
        }

        public static string GetHtmlContent(string url)
        {
            var request = (HttpWebRequest)WebRequest.Create(url);

            request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; rv:68.0) Gecko/20100101 Firefox/68.0";
            request.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8";
            request.Headers.Set(HttpRequestHeader.AcceptLanguage, "en-us,en;q=0.5");
            request.Headers.Set(HttpRequestHeader.AcceptEncoding, "gzip,deflate");
            request.Method = "GET";

            var response = (HttpWebResponse)request.GetResponse();
            var responseStream = response.GetResponseStream();

            if (response.ContentEncoding?.IndexOf("gzip", StringComparison.InvariantCultureIgnoreCase) >= 0)
            {
                responseStream = new GZipStream(responseStream, CompressionMode.Decompress);
            }
            else if (response.ContentEncoding?.IndexOf("deflate", StringComparison.InvariantCultureIgnoreCase) >= 0)
            {
                responseStream = new DeflateStream(responseStream, CompressionMode.Decompress);
            }

            using var ms = new MemoryStream();
            responseStream?.CopyTo(ms);

            return Encoding.UTF8.GetString(ms.ToArray());
        }
    }
}