using System.Collections.Generic;
using Newtonsoft.Json;
using BinanceTR.Net.Interfaces;
using System;
using CryptoExchange.Net.ExchangeInterfaces;
using CryptoExchange.Net.Interfaces;

namespace BinanceTR.Net.Objects.RestApi
{
    /// <summary>
    /// The order book for a asset
    /// </summary>
    public class BinanceTROrderBook : IBinanceTROrderBook
    {
        /// <summary>
        /// The symbol of the order book 
        /// </summary>
        [JsonProperty("s")]
        public string Symbol { get; set; } = string.Empty;

        /// <summary>
        /// The ID of the last update
        /// </summary>
        [JsonProperty("lastUpdateId")]
        public long LastUpdateId { get; set; }

        /// <summary>
        /// The list of bids
        /// </summary>
        public IEnumerable<BinanceTROrderBookEntry> Bids { get; set; } = Array.Empty<BinanceTROrderBookEntry>();

        /// <summary>
        /// The list of asks
        /// </summary>
        public IEnumerable<BinanceTROrderBookEntry> Asks { get; set; } = Array.Empty<BinanceTROrderBookEntry>();
    }
}
