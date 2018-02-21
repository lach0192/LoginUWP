/*
 *      FILE:           BingImageOfTheDay.cs
 *      PROGRAMMER:     Eric Lachapelle
 * 
 *      INFO:           Simple UWP Login app with random background image.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace LoginUWP
{
    class BingImageOfTheDay
    {
        string result = string.Empty;
        string market = string.Empty;
        string description = string.Empty;
        Uri uri = null;

        public enum Resolution
        {
            Default, _800x600, _1024x768,
            _1366x768, _1920x1080, _1920x1200
        }

        Resolution resolution =
            Resolution.Default; // usually 1366x768
        public BingImageOfTheDay(
            Resolution resolution = Resolution.Default,
            string market = "en-ww")
        {
            this.resolution = resolution;
            this.market = market;

        }

        public async Task<Uri> GetImage()
        {
            var request = new Uri(string.Format
                ("http://www.bing.com/hpimagearchive.aspx?n=1&mkt={0}"
                , market));

            using (HttpClient httpClient = new HttpClient())
            {
                result = await httpClient.GetStringAsync(request);
            }

            // if no resolution provided (Default) we use the url value which contains a default resolution
            // else we use the urlBase value and then add our desired resolution (note this may not always work if we request a resolution that is not available)

            var targetElement = resolution
                == Resolution.Default ? "url" : "urlBase";

            // now we get either the url or urlBase value
            var pathString =
                XDocument.Parse(result).
                Descendants(targetElement).First().Value;

            // for the resolution if we are using the default (url) then we having nothing to add
            // else we need to add the resolution followed by .jpg
            var resolutionString = resolution
                == Resolution.Default ? "" :
                string.Format("{0}{1}", resolution, ".jpg");

            uri = new Uri(string.Format(
                "http://www.bing.com{0}{1}",
                pathString, resolutionString)); // store in the member field

            // now get the copyright value which should contain an image description too and store in the description member
            description = XDocument.Parse(result).
                Descendants("copyright").First().Value;

            return uri;
        }


        public string GetDescription() { return description; }

        public Uri GetURI() { return uri; }
    }
}