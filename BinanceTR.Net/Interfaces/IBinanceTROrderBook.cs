using System;
using BinanceTR.Net.Objects;
using System.Collections.Generic;
using BinanceTR.Net.Objects.RestApi;
using Binance.Net.Objects;

namespace BinanceTR.Net.Interfaces
{
    /// <summary>
    /// The order book for a asset
    /// </summary>
    public interface IBinanceTROrderBook
    {
        /// <summary>
        /// The symbol of the order book (only filled from stream updates)
        /// </summary>
        string Symbol { get; set; }

        /// <summary>
        /// The ID of the last update
        /// </summary>
        long LastUpdateId { get; set; }

        /// <summary>
        /// The list of bids
        /// </summary>
        IEnumerable<BinanceTROrderBookEntry> Bids { get; set; }

        /// <summary>
        /// The list of asks
        /// </summary>
        IEnumerable<BinanceTROrderBookEntry> Asks { get; set; }
    }

    /// <summary>
    /// Order book update event
    /// </summary>
    public interface IBinanceEventOrderBook : IBinanceTROrderBook
    {
        /// <summary>
        /// The ID of the first update
        /// </summary>
        long? FirstUpdateId { get; set; }
        /// <summary>
        /// Timestamp of the event
        /// </summary>
        DateTime EventTime { get; set; }
    }

    /// <summary>
    /// Futures order book update event
    /// </summary>
    public interface IBinanceFuturesEventOrderBook : IBinanceEventOrderBook
    {
        /// <summary>
        /// Transaction time
        /// </summary>
        DateTime TransactionTime { get; set; }
        /// <summary>
        /// Last update id of the previous update
        /// </summary>
        public long LastUpdateIdStream { get; set; }
    }
}
