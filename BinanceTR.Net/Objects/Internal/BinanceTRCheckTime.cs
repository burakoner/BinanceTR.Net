using System;
using CryptoExchange.Net.Converters;
using Newtonsoft.Json;

namespace BinanceTR.Net.Objects.Internal
{
    internal class BinanceTRCheckTime
    {
        [JsonProperty("timestamp"), JsonConverter(typeof(TimestampConverter))]
        public DateTime ServerTime { get; set; }
    }
}
