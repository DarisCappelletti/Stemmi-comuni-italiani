using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace StemmiComuniItalianiConsole.Models
{
    class WikiSearch
    {
        public class _1
        {
            public int ns { get; set; }
            public string title { get; set; }
            public string missing { get; set; }
            public string known { get; set; }
            public string imagerepository { get; set; }
            public List<Imageinfo> imageinfo { get; set; }
        }

        public class Imageinfo
        {
            public string url { get; set; }
            public string descriptionurl { get; set; }
            public string descriptionshorturl { get; set; }
        }

        public class Pages
        {
            [JsonProperty(PropertyName = "-1")]
            public _1 _1 { get; set; }

            [JsonProperty("-2")]
            public _1 _2
            { get; set; }

            [JsonProperty("-3")]
            public _1 _3
            { get; set; }

            [JsonProperty("-4")]
            public _1 _4
            { get; set; }

            [JsonProperty("-5")]
            public _1 _5
            { get; set; }
        }

        public class Query
        {
            public Pages pages { get; set; }
        }

        public class Root
        {
            public string batchcomplete { get; set; }
            public Query query { get; set; }
        }
    }
}
