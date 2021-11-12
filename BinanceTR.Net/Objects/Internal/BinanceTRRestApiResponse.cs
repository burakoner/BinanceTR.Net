using BinanceTR.Net.Converters;
using BinanceTR.Net.Enums;
using CryptoExchange.Net.Attributes;
using CryptoExchange.Net.Converters;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace BinanceTR.Net.Objects.Internal
{
    internal class BinanceTRRestApiResponse<T>
    {
        [JsonProperty("code")]
        public int ErrorCode { get; set; }

        [JsonProperty("msg")]
        public string Message { get; set; } = string.Empty;

        [JsonProperty("data"), JsonOptionalProperty]
        public T Data { get; set; }

        [JsonProperty("timestamp"), JsonConverter(typeof(TimestampConverter))]
        public DateTime Timestamp { get; set; }
    }

    internal class BinanceTRListResponse<T>
    {
        [JsonProperty("list")]
        public IEnumerable<T> List { get; set; }
    }
}
