using Newtonsoft.Json;
using System;
using System.Globalization;
using CryptoExchange.Net.Converters;
using CryptoExchange.Net.ExchangeInterfaces;
using System.Collections.Generic;

namespace BinanceTR.Net.Objects.RestApi
{
    /// <summary>
    /// Information about a trade
    /// </summary>
    public class BinanceTRTrade
    {
        [JsonIgnore]
        public string Symbol { get; set; }

        /// <summary>
        /// The order id the trade belongs to
        /// </summary>
        [JsonProperty("id")]
        public long OrderId { get; set; }

        /// <summary>
        /// The price of the trade
        /// </summary>
        [JsonProperty("price")]
        public decimal Price { get; set; }

        /// <summary>
        /// The quantity of the trade
        /// </summary>
        [JsonProperty("qty")]
        public decimal Quantity { get; set; }

        /// <summary>
        /// The time the trade was made
        /// </summary>
        [JsonProperty("time"), JsonConverter(typeof(TimestampConverter))]
        public DateTime TradeTime { get; set; }

        [JsonProperty("isBuyerMaker")]
        public bool IsBuyerMaker { get; set; }

        [JsonProperty("isBestMatch")]
        public bool IsBestMatch { get; set; }
    }
}
