using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using Newtonsoft.Json.Linq;

namespace ConsoleApp1
{
    public class Program
    {
        public static string xmlURL      = "https://apekelgit.github.io/cse445_a4/Hotels.xml";
        public static string xmlErrorURL = "https://apekelgit.github.io/cse445_a4/HotelsErrors.xml";
        public static string xsdURL      = "https://apekelgit.github.io/cse445_a4/Hotels.xsd";

        public static void Main(string[] args)
        {
            string result = Verification(xmlURL, xsdURL);
            Console.WriteLine(result);

            result = Verification(xmlErrorURL, xsdURL);
            Console.WriteLine(result);

            result = Xml2Json(xmlURL);
            Console.WriteLine(result);
        }

        public static string Verification(string xmlUrl, string xsdUrl)
        {
            var errors = new List<string>();

            try
            {
                var settings = new XmlReaderSettings
                {
                    ValidationType = ValidationType.Schema,
                    DtdProcessing = DtdProcessing.Prohibit,
                    IgnoreComments = true,
                    IgnoreWhitespace = false
                };

                settings.Schemas.Add(null, xsdUrl);
                settings.ValidationFlags |= XmlSchemaValidationFlags.ReportValidationWarnings
                                          | XmlSchemaValidationFlags.ProcessInlineSchema
                                          | XmlSchemaValidationFlags.ProcessSchemaLocation;

                settings.ValidationEventHandler += (sender, e) =>
                {
                    errors.Add($"{e.Severity}: {e.Message}");
                };

                using (var reader = XmlReader.Create(xmlUrl, settings))
                {
                    while (reader.Read())
                }
            }
            catch (XmlException xe)
            {
                errors.Add($"XmlException: {xe.Message} (line {xe.LineNumber}, pos {xe.LinePosition})");
            }
            catch (Exception ex)
            {
                errors.Add($"Exception: {ex.Message}");
            }

            return errors.Count == 0 ? "No errors are found" : string.Join(Environment.NewLine, errors);
        }

        public static string Xml2Json(string xmlUrl)
        {
            XDocument doc;
            using (var client = new WebClient())
            using (var stream = client.OpenRead(xmlUrl))
            {
                doc = XDocument.Load(stream);
            }
          
            var hotelsArray = new JArray();

            foreach (var h in doc.Root.Elements("Hotel"))
            {
                var o = new JObject
                {
                    ["Name"] = (string)h.Element("Name") ?? ""
                };

                var phones = h.Elements("Phone")
                              .Select(p => (string)p)
                              .Where(s => !string.IsNullOrWhiteSpace(s))
                              .ToList();
                o["Phone"] = new JArray(phones);

                var addr = h.Element("Address");
                if (addr != null)
                {
                    var a = new JObject
                    {
                        ["Number"] = (string)addr.Element("Number") ?? "",
                        ["Street"] = (string)addr.Element("Street") ?? "",
                        ["City"] = (string)addr.Element("City") ?? "",
                        ["State"] = (string)addr.Element("State") ?? "",
                        ["Zip"] = (string)addr.Element("Zip") ?? "",
                        ["NearestAirport"] = (string)addr.Element("NearestAirport") ?? ""
                    };
                    o["Address"] = a;
                }

                var ratingAttr = (string)h.Attribute("Rating");
                if (!string.IsNullOrWhiteSpace(ratingAttr))
                    o["_Rating"] = ratingAttr;

                hotelsArray.Add(o);
            }

            var root = new JObject
            {
                ["Hotels"] = new JObject
                {
                    ["Hotel"] = hotelsArray
                }
            };

            return root.ToString();
        }
    }
}
