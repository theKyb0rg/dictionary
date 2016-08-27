using Dictionary.Models;
using HtmlAgilityPack;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.Mvc;
namespace Dictionary.Controllers
{
    public static class StringExtensions
    {
        public static string SafeReplace(this string input, string find, string replace, bool matchWholeWord)
        {
            string textToFind = matchWholeWord ? string.Format(@"\b{0}\b", find) : find;
            return Regex.Replace(input, textToFind, replace, RegexOptions.IgnoreCase);
        }
    }

    public class HomeController : Controller
    {
        // url to search from
        private static string _url = "http://www.dictionary.com/browse/";

        public ActionResult Index()
        {
            return View();
        }

        public async Task<string> Search(string searchTerm)
        {
            // Get http client for grabbing html data from the web
            HttpClient http = new HttpClient();

            // grab the byte array from the url
            var response = await http.GetByteArrayAsync(_url + searchTerm);

            // encode it 
            string source = Encoding.GetEncoding("utf-8").GetString(response, 0, response.Length - 1);

            // decode into an html file
            source = WebUtility.HtmlDecode(source);

            // load it in a 'fake' html document
            HtmlDocument html = new HtmlDocument();
            html.LoadHtml(source);

            // grab word definitions
            List<HtmlNode> content = html.DocumentNode
                .Descendants()
                .Where(x => (x.Name == "div" && x.Attributes["class"] != null && x.Attributes["class"].Value.Contains("def-content")))
                .ToList();

            if (content.Count == 0)
            {
                return "-1";
            }
            else
            {
                // declare list for holding definitions
                List<Definition> definitions = new List<Definition>();

                // loop through the content and assign the category and value to the object
                foreach (var text in content)
                {
                    // clean the string
                    string value = Regex.Replace(text.InnerText, @"\s+", " ").Trim();

                    // words to append fuckin' to
                    string[] words = { "a ", "any ", "he ", "she ", "it ", "the " };

                    // add fuckin'
                    value = AddFuckin(words, value);

                    // store words in definitions and send to browser
                    definitions.Add(new Definition
                    {
                        Url = _url + searchTerm,
                        Category = Regex.Replace(text.ParentNode
                            .ParentNode
                            .FirstChild
                            .NextSibling
                            .InnerText, @"\s+", " ")
                            .Trim(),
                        Value = value
                    });
                }
                return JsonConvert.SerializeObject(definitions, new JsonSerializerSettings
                {
                    ContractResolver = new CamelCasePropertyNamesContractResolver()
                });
            }
        }

        public string AddFuckin(string[] words, string phrase)
        {
            for (int i = 0; i < words.Length; i++)
            {
                if (phrase.IndexOf(words[i]) != -1)
                {
                    phrase = phrase.SafeReplace(words[i], words[i] + " <b>fuckin'</b> ", true);
                }
            }
            return phrase;
        }
    }
}