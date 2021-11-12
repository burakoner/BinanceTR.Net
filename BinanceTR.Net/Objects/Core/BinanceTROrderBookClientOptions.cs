using System;
using System.Net.Http;
using BinanceTR.Net.Enums;
using BinanceTR.Net.Interfaces;
using CryptoExchange.Net.Objects;

namespace BinanceTR.Net.Objects.Core
{
    /// <summary>
    /// Binance symbol order book options
    /// </summary>
    public class BinanceTROrderBookClientOptions : OrderBookOptions
    {
        /// <summary>
        /// The rest client to use for requesting the initial order book
        /// </summary>
        public IBinanceTRClient RestClient { get; set; }

        /// <summary>
        /// The client to use for the socket connection. When using the same client for multiple order books the connection can be shared.
        /// </summary>
        public IBinanceTRSocketClient SocketClient { get; set; }

        /// <summary>
        /// The top amount of results to keep in sync. If for example limit=10 is used, the order book will contain the 10 best bids and 10 best asks. Leaving this null will sync the full order book
        /// </summary>
        public int? Limit { get; set; }

        /// <summary>
        /// Update interval in milliseconds, either 100 or 1000. Defaults to 1000
        /// </summary>
        public int? UpdateInterval { get; set; }

        /// <summary>
        /// Create new options
        /// </summary>
        /// <param name="limit">The top amount of results to keep in sync. If for example limit=10 is used, the order book will contain the 10 best bids and 10 best asks. Leaving this null will sync the full order book</param>
        /// <param name="updateInterval">Update interval in milliseconds, either 100 or 1000. Defaults to 1000</param>
        /// <param name="socketClient">The client to use for the socket connection. When using the same client for multiple order books the connection can be shared.</param>
        /// <param name="restClient">The rest client to use for requesting the initial order book.</param>
        public BinanceTROrderBookClientOptions(int? limit = null, int? updateInterval = null, IBinanceTRSocketClient socketClient = null, IBinanceTRClient restClient = null) : base("BinanceTR", limit == null, false)
        {
            Limit = limit;
            UpdateInterval = updateInterval;
            SocketClient = socketClient;
            RestClient = restClient;
        }
    }
}
