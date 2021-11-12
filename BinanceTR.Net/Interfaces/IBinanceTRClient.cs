using BinanceTR.Net.Enums;
using BinanceTR.Net.Objects.RestApi;
using CryptoExchange.Net.Interfaces;
using CryptoExchange.Net.Objects;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace BinanceTR.Net.Interfaces
{
    /// <summary>
    /// Binance interface
    /// </summary>
    public interface IBinanceTRClient : IRestClient
    {
        /// <summary>
        /// Set the API key and secret
        /// </summary>
        /// <param name="apiKey">The api key</param>
        /// <param name="apiSecret">The api secret</param>
        void SetApiCredentials(string apiKey, string apiSecret);

        #region General Endpoints
        WebCallResult<DateTime> GetServerTime(bool resetAutoTimestamp = false, CancellationToken ct = default);
        Task<WebCallResult<DateTime>> GetServerTimeAsync(bool resetAutoTimestamp = false, CancellationToken ct = default);

        WebCallResult<IEnumerable<BinanceTRSymbol>> GetTradingSymbols(CancellationToken ct = default);
        Task<WebCallResult<IEnumerable<BinanceTRSymbol>>> GetTradingSymbolsAsync(CancellationToken ct = default);
        #endregion

        #region Market Data Endpoints
        WebCallResult<BinanceTROrderBook> GetOrderBook(string symbol, int limit = 100, CancellationToken ct = default);
        Task<WebCallResult<BinanceTROrderBook>> GetOrderBookAsync(string symbol, int limit = 100, CancellationToken ct = default);

        WebCallResult<IEnumerable<BinanceTRTrade>> GetTrades(string symbol, long? fromId = null, int limit = 100, CancellationToken ct = default);
        Task<WebCallResult<IEnumerable<BinanceTRTrade>>> GetTradesAsync(string symbol, long? fromId = null, int limit = 100, CancellationToken ct = default);

        WebCallResult<IEnumerable<BinanceTRAggregatedTrade>> GetAggregatedTrades(string symbol, long? fromId = null, DateTime? startTime = null, DateTime? endTime = null, int limit = 100, SymbolType type = SymbolType.Main, CancellationToken ct = default);
        Task<WebCallResult<IEnumerable<BinanceTRAggregatedTrade>>> GetAggregatedTradesAsync(string symbol, long? fromId = null, DateTime? startTime = null, DateTime? endTime = null, int limit = 100, SymbolType type = SymbolType.Main, CancellationToken ct = default);

        WebCallResult<IEnumerable<BinanceTRKline>> GetKlines(string symbol, KlineInterval interval, DateTime? startTime = null, DateTime? endTime = null, int limit = 100, CancellationToken ct = default);
        Task<WebCallResult<IEnumerable<BinanceTRKline>>> GetKlinesAsync(string symbol, KlineInterval interval, DateTime? startTime = null, DateTime? endTime = null, int limit = 100, CancellationToken ct = default);
        #endregion

    }
}