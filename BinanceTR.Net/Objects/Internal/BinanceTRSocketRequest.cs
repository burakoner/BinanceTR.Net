using Newtonsoft.Json;
using System;

namespace BinanceTR.Net.Objects.Internal
{
    internal class BinanceTRSocketRequest
    {
        [JsonProperty("method")]
        public string Method { get; set; } = "";
        [JsonProperty("params")]
        public string[] Params { get; set; } = Array.Empty<string>();
        [JsonProperty("id")]
        public int Id { get; set; }
    }
}
