using BinanceTR.Net.Converters;
using BinanceTR.Net.Objects;
using CryptoExchange.Net;
using CryptoExchange.Net.Authentication;
using CryptoExchange.Net.Objects;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using BinanceTR.Net.Enums;
using BinanceTR.Net.Interfaces;
using CryptoExchange.Net.ExchangeInterfaces;
using Microsoft.Extensions.Logging;
using BinanceTR.Net.Objects.RestApi;
using BinanceTR.Net.Objects.Core;
using BinanceTR.Net.Objects.Internal;

namespace BinanceTR.Net
{
    public class BinanceTRClient : RestClient, IBinanceTRClient
    {
        #region Private Fields
        private static BinanceTRRestApiClientOptions _defaultOptions = new BinanceTRRestApiClientOptions();
        private static BinanceTRRestApiClientOptions DefaultOptions => _defaultOptions.Copy();
        #endregion

        #region Endpoints
        private const string mainApi = "open";
        private const string mainPublicVersion = "1";
        private const string mainCommonTimeEndpoint = "common/time";
        private const string mainCommonSymbolsEndpoint = "common/symbols";
        private const string mainMarketDepthEndpoint = "market/depth";
        private const string mainMarketTradesEndpoint = "market/trades";
        private const string mainMarketAggTradesEndpoint = "market/agg-trades";
        private const string mainMarketKlinesEndpoint = "market/klines";

        private readonly string _mainAddress;
        private readonly string _nextAddress;
        private const string nextApi = "api";
        private const string nextPublicVersion = "3";
        private const string nextMarketDepthEndpoint = "depth";
        private const string nextMarketTradesEndpoint = "trades";
        private const string nextMarketAggTradesEndpoint = "aggTrades";
        private const string nextMarketKlinesEndpoint = "klines";
        #endregion

        #region Internal Fields
        internal readonly bool AutoTimestamp;
        internal readonly TimeSpan AutoTimestampRecalculationInterval;
        internal readonly TimeSpan TimestampOffset;
        internal readonly TradeRulesBehaviour TradeRulesBehaviour;
        internal readonly TimeSpan TradeRulesUpdateInterval;
        internal readonly TimeSpan DefaultReceiveWindow;

        internal static double CalculatedTimeOffset;
        internal static bool TimeSynced;
        internal static DateTime LastTimeSync;

        internal IEnumerable<BinanceTRSymbol> ExchangeSymbols;
        internal DateTime? LastExchangeInfoUpdate;
        #endregion

        #region Event Actions
        /// <summary>
        /// Event triggered when an order is placed via this client. 
        /// Only available for Spot orders
        /// </summary>
        public event Action<ICommonOrderId> OnOrderPlaced;

        /// <summary>
        /// Event triggered when an order is cancelled via this client. 
        /// Note that this does not trigger when using CancelAllOrdersAsync. 
        /// Only available for Spot orders
        /// </summary>
        public event Action<ICommonOrderId> OnOrderCanceled;
        #endregion

        #region Constructor
        /// <summary>
        /// Create a new instance of BinanceClient using the default options
        /// </summary>
        public BinanceTRClient() : this(DefaultOptions)
        {
        }

        /// <summary>
        /// Create a new instance of BinanceClient using provided options
        /// </summary>
        /// <param name="options">The options to use for this client</param>
        public BinanceTRClient(BinanceTRRestApiClientOptions options) : base("BinanceTR", options, options.ApiCredentials == null ? null : new BinanceTRAuthenticationProvider(options.ApiCredentials))
        {
            AutoTimestamp = options.AutoTimestamp;
            TradeRulesBehaviour = options.TradeRulesBehaviour;
            TradeRulesUpdateInterval = options.TradeRulesUpdateInterval;
            AutoTimestampRecalculationInterval = options.AutoTimestampRecalculationInterval;
            TimestampOffset = options.TimestampOffset;
            DefaultReceiveWindow = options.ReceiveWindow;
            _mainAddress = options.RestApiMainAddress;
            _nextAddress = options.RestApiNextAddress;

            arraySerialization = ArrayParametersSerialization.MultipleValues;
            requestBodyFormat = RequestBodyFormat.FormData;
            requestBodyEmptyContent = string.Empty;
        }
        #endregion

        #region General Methods
        /// <summary>
        /// Set the default options to be used when creating new clients
        /// </summary>
        /// <param name="options"></param>
        public static void SetDefaultOptions(BinanceTRRestApiClientOptions options)
        {
            _defaultOptions = options;
        }

        /// <summary>
        /// Set the API key and secret
        /// </summary>
        /// <param name="apiKey">The api key</param>
        /// <param name="apiSecret">The api secret</param>
        public void SetApiCredentials(string apiKey, string apiSecret)
        {
            SetAuthenticationProvider(new BinanceTRAuthenticationProvider(new ApiCredentials(apiKey, apiSecret)));
        }
        #endregion

        #region General Endpoints
        public virtual WebCallResult<DateTime> GetServerTime(bool resetAutoTimestamp = false, CancellationToken ct = default) => GetServerTimeAsync(resetAutoTimestamp, ct).Result;
        public virtual async Task<WebCallResult<DateTime>> GetServerTimeAsync(bool resetAutoTimestamp = false, CancellationToken ct = default)
        {
            var url = GetMainUrl(mainCommonTimeEndpoint, mainApi, mainPublicVersion);
            if (!AutoTimestamp)
            {
                var result = await SendRequestInternal<BinanceTRCheckTime>(url, HttpMethod.Get, ct).ConfigureAwait(false);
                return result.As(result.Data?.ServerTime ?? default);
            }
            else
            {
                var localTime = DateTime.UtcNow;
                var result = await SendRequestInternal<BinanceTRCheckTime>(url, HttpMethod.Get, ct).ConfigureAwait(false);
                if (!result)
                    return new WebCallResult<DateTime>(result.ResponseStatusCode, result.ResponseHeaders, default, result.Error);

                if (BinanceTRClient.TimeSynced && !resetAutoTimestamp)
                    return result.As(result.Data.ServerTime);

                if (TotalRequestsMade == 1)
                {
                    // If this was the first request make another one to calculate the offset since the first one can be slower
                    localTime = DateTime.UtcNow;
                    result = await SendRequestInternal<BinanceTRCheckTime>(url, HttpMethod.Get, ct).ConfigureAwait(false);
                    if (!result)
                        return new WebCallResult<DateTime>(result.ResponseStatusCode, result.ResponseHeaders, default, result.Error);
                }

                // Calculate time offset between local and server
                var offset = (result.Data.ServerTime - localTime).TotalMilliseconds;
                if (offset >= 0 && offset < 500)
                {
                    // Small offset, probably mainly due to ping. Don't adjust time
                    BinanceTRClient.CalculatedTimeOffset = 0;
                    BinanceTRClient.TimeSynced = true;
                    BinanceTRClient.LastTimeSync = DateTime.UtcNow;
                    log.Write(LogLevel.Information, $"Time offset between 0 and 500ms ({offset}ms), no adjustment needed");
                    return result.As(result.Data.ServerTime);
                }

                BinanceTRClient.CalculatedTimeOffset = (result.Data.ServerTime - localTime).TotalMilliseconds;
                BinanceTRClient.TimeSynced = true;
                BinanceTRClient.LastTimeSync = DateTime.UtcNow;
                log.Write(LogLevel.Information, $"Time offset set to {BinanceTRClient.CalculatedTimeOffset}ms");
                return result.As(result.Data.ServerTime);
            }
        }

        public virtual WebCallResult<IEnumerable<BinanceTRSymbol>> GetTradingSymbols(CancellationToken ct = default) => GetTradingSymbolsAsync(ct).Result;
        public virtual async Task<WebCallResult<IEnumerable<BinanceTRSymbol>>> GetTradingSymbolsAsync(CancellationToken ct = default)
        {
            var result = await SendRequestInternal<BinanceTRRestApiResponse<BinanceTRListResponse<BinanceTRSymbol>>>(GetMainUrl(mainCommonSymbolsEndpoint, mainApi, mainPublicVersion), HttpMethod.Get, ct).ConfigureAwait(false);
            if (!result.Success) return WebCallResult<IEnumerable<BinanceTRSymbol>>.CreateErrorResult(result.ResponseStatusCode, result.ResponseHeaders, result.Error);
            if (result.Data.ErrorCode > 0) return WebCallResult<IEnumerable<BinanceTRSymbol>>.CreateErrorResult(result.ResponseStatusCode, result.ResponseHeaders, new ServerError(result.Data.ErrorCode.ToString(), result.Data.Message));

            ExchangeSymbols = result.Data.Data.List;
            LastExchangeInfoUpdate = DateTime.UtcNow;
            log.Write(LogLevel.Information, "Trade rules updated");

            return new WebCallResult<IEnumerable<BinanceTRSymbol>>(result.ResponseStatusCode, result.ResponseHeaders, result.Data.Data.List, null);
        }
        #endregion

        #region Market Data Endpoints
        public virtual WebCallResult<BinanceTROrderBook> GetOrderBook(string symbol, int limit = 100, CancellationToken ct = default) => GetOrderBookAsync(symbol, limit, ct).Result;
        public virtual async Task<WebCallResult<BinanceTROrderBook>> GetOrderBookAsync(string symbol, int limit = 100, CancellationToken ct = default)
        {
            symbol = symbol.Replace("_", "");
            symbol.ValidateBinanceSymbol();
            limit.ValidateIntValues(nameof(limit), 5, 10, 20, 50, 100, 500, 1000, 5000);

            var parameters = new Dictionary<string, object> { { "symbol", symbol } };
            parameters.AddOptionalParameter("limit", limit.ToString(CultureInfo.InvariantCulture));

            var url = GetNextUrl(nextMarketDepthEndpoint, nextApi, nextPublicVersion);
            var result = await SendRequestInternal<BinanceTROrderBook>(url, HttpMethod.Get, ct, parameters).ConfigureAwait(false);
            if (!result.Success) return WebCallResult<BinanceTROrderBook>.CreateErrorResult(result.ResponseStatusCode, result.ResponseHeaders, result.Error);

            result.Data.Symbol = symbol;
            return new WebCallResult<BinanceTROrderBook>(result.ResponseStatusCode, result.ResponseHeaders, result.Data, null);
        }

        public virtual WebCallResult<IEnumerable<BinanceTRTrade>> GetTrades(string symbol, long? fromId = null, int limit = 100, CancellationToken ct = default) => GetTradesAsync(symbol, fromId, limit, ct).Result;
        public virtual async Task<WebCallResult<IEnumerable<BinanceTRTrade>>> GetTradesAsync(string symbol, long? fromId = null, int limit = 100, CancellationToken ct = default)
        {
            symbol = symbol.Replace("_", "");
            symbol.ValidateBinanceSymbol();
            limit.ValidateIntBetween(nameof(limit), 1, 1000);

            var parameters = new Dictionary<string, object> { { "symbol", symbol } };
            parameters.AddOptionalParameter("fromId", fromId?.ToString(CultureInfo.InvariantCulture));
            parameters.AddOptionalParameter("limit", limit.ToString(CultureInfo.InvariantCulture));

            var url = GetNextUrl(nextMarketTradesEndpoint, nextApi, nextPublicVersion);
            var result = await SendRequestInternal<IEnumerable<BinanceTRTrade>>(url, HttpMethod.Get, ct, parameters).ConfigureAwait(false);
            if (!result.Success) return WebCallResult<IEnumerable<BinanceTRTrade>>.CreateErrorResult(result.ResponseStatusCode, result.ResponseHeaders, result.Error);

            foreach (var row in result.Data) row.Symbol = symbol;
            return new WebCallResult<IEnumerable<BinanceTRTrade>>(result.ResponseStatusCode, result.ResponseHeaders, result.Data, null);
        }

        public virtual WebCallResult<IEnumerable<BinanceTRAggregatedTrade>> GetAggregatedTrades(string symbol, long? fromId = null, DateTime? startTime = null, DateTime? endTime = null, int limit = 100, SymbolType type = SymbolType.Main, CancellationToken ct = default) => GetAggregatedTradesAsync(symbol, fromId, startTime, endTime, limit, type, ct).Result;
        public virtual async Task<WebCallResult<IEnumerable<BinanceTRAggregatedTrade>>> GetAggregatedTradesAsync(string symbol, long? fromId = null, DateTime? startTime = null, DateTime? endTime = null, int limit = 100, SymbolType type = SymbolType.Main, CancellationToken ct = default)
        {
            symbol = symbol.Replace("_", "");
            symbol.ValidateBinanceSymbol();
            limit.ValidateIntBetween(nameof(limit), 1, 1000);

            var parameters = new Dictionary<string, object> { { "symbol", symbol } };
            parameters.AddOptionalParameter("fromId", fromId?.ToString(CultureInfo.InvariantCulture));
            parameters.AddOptionalParameter("startTime", startTime != null ? BinanceTRClient.ToUnixTimestamp(startTime.Value).ToString(CultureInfo.InvariantCulture) : null);
            parameters.AddOptionalParameter("endTime", endTime != null ? BinanceTRClient.ToUnixTimestamp(endTime.Value).ToString(CultureInfo.InvariantCulture) : null);
            parameters.AddOptionalParameter("limit", limit.ToString(CultureInfo.InvariantCulture));

            var url = GetNextUrl(nextMarketAggTradesEndpoint, nextApi, nextPublicVersion);
            var result = await SendRequestInternal<IEnumerable<BinanceTRAggregatedTrade>>(url, HttpMethod.Get, ct, parameters).ConfigureAwait(false);
            if (!result.Success) return WebCallResult<IEnumerable<BinanceTRAggregatedTrade>>.CreateErrorResult(result.ResponseStatusCode, result.ResponseHeaders, result.Error);

            foreach (var row in result.Data) row.Symbol = symbol;
            return new WebCallResult<IEnumerable<BinanceTRAggregatedTrade>>(result.ResponseStatusCode, result.ResponseHeaders, result.Data, null);
        }

        public virtual WebCallResult<IEnumerable<BinanceTRKline>> GetKlines(string symbol, KlineInterval interval, DateTime? startTime = null, DateTime? endTime = null, int limit = 100, CancellationToken ct = default) => GetKlinesAsync(symbol, interval, startTime, endTime, limit, ct).Result;
        public virtual async Task<WebCallResult<IEnumerable<BinanceTRKline>>> GetKlinesAsync(string symbol, KlineInterval interval, DateTime? startTime = null, DateTime? endTime = null, int limit = 100, CancellationToken ct = default)
        {
            symbol = symbol.Replace("_", "");
            symbol.ValidateBinanceSymbol();
            limit.ValidateIntBetween(nameof(limit), 1, 1000);

            var parameters = new Dictionary<string, object> {
                { "symbol", symbol },
                { "interval", JsonConvert.SerializeObject(interval, new KlineIntervalConverter(false)) }
            };
            parameters.AddOptionalParameter("startTime", startTime != null ? BinanceTRClient.ToUnixTimestamp(startTime.Value).ToString(CultureInfo.InvariantCulture) : null);
            parameters.AddOptionalParameter("endTime", endTime != null ? BinanceTRClient.ToUnixTimestamp(endTime.Value).ToString(CultureInfo.InvariantCulture) : null);
            parameters.AddOptionalParameter("limit", limit.ToString(CultureInfo.InvariantCulture));

            var url = GetNextUrl(nextMarketKlinesEndpoint, nextApi, nextPublicVersion);
            var result = await SendRequestInternal<IEnumerable<BinanceTRKline>>(url, HttpMethod.Get, ct, parameters).ConfigureAwait(false);
            if (!result.Success) return WebCallResult<IEnumerable<BinanceTRKline>>.CreateErrorResult(result.ResponseStatusCode, result.ResponseHeaders, result.Error);

            foreach (var row in result.Data) row.Symbol = symbol;
            return new WebCallResult<IEnumerable<BinanceTRKline>>(result.ResponseStatusCode, result.ResponseHeaders, result.Data, null);
        }
        #endregion

        #region Account Endpoints
        #endregion

        #region Wallet Endpoints
        #endregion

        #region Core Methods
        protected override Error ParseErrorResponse(JToken error)
        {
            if (!error.HasValues)
                return new ServerError(error.ToString());

            if (error["msg"] == null && error["code"] == null)
                return new ServerError(error.ToString());

            if (error["msg"] != null && error["code"] == null)
                return new ServerError((string)error["msg"]!);

            var err = new ServerError((int)error["code"]!, (string)error["msg"]!);
            if (err.Code == -1021)
            {
                if (AutoTimestamp)
                    _ = GetServerTimeAsync(true);
            }
            return err;
        }

        internal Error ParseErrorResponseInternal(JToken error) => ParseErrorResponse(error);

        internal Uri GetMainUrl(string endpoint, string api, string version = null)
        {
            var result = $"{BaseAddress}{api}/";

            if (!string.IsNullOrEmpty(version))
                result += $"v{version}/";

            result += endpoint;
            return new Uri(result);
        }

        internal Uri GetNextUrl(string endpoint, string api, string version = null)
        {
            var result = $"{_nextAddress}{api}/";

            if (!string.IsNullOrEmpty(version))
                result += $"v{version}/";

            result += endpoint;
            return new Uri(result);
        }

        internal static long ToUnixTimestamp(DateTime time)
        {
            return (long)(time - new DateTime(1970, 1, 1)).TotalMilliseconds;
        }

        internal string GetTimestamp()
        {
            var offset = AutoTimestamp ? CalculatedTimeOffset : 0;
            offset += TimestampOffset.TotalMilliseconds;
            return ToUnixTimestamp(DateTime.UtcNow.AddMilliseconds(offset)).ToString(CultureInfo.InvariantCulture);
        }

        internal async Task<WebCallResult<DateTime>> CheckAutoTimestamp(CancellationToken ct)
        {
            if (AutoTimestamp && (!TimeSynced || DateTime.UtcNow - LastTimeSync > AutoTimestampRecalculationInterval))
                return await GetServerTimeAsync(TimeSynced, ct).ConfigureAwait(false);

            return new WebCallResult<DateTime>(null, null, default, null);
        }

        internal async Task<BinanceTRTradeRuleResult> CheckTradeRules(string symbol, decimal? quantity, decimal? price, decimal? stopPrice, OrderType? type, CancellationToken ct)
        {
            var outputQuantity = quantity;
            var outputPrice = price;
            var outputStopPrice = stopPrice;

            if (TradeRulesBehaviour == TradeRulesBehaviour.None)
                return BinanceTRTradeRuleResult.CreatePassed(outputQuantity, outputPrice, outputStopPrice);

            if (ExchangeSymbols == null || LastExchangeInfoUpdate == null || (DateTime.UtcNow - LastExchangeInfoUpdate.Value).TotalMinutes > TradeRulesUpdateInterval.TotalMinutes)
                await GetTradingSymbolsAsync(ct).ConfigureAwait(false);

            if (ExchangeSymbols == null)
                return BinanceTRTradeRuleResult.CreateFailed("Unable to retrieve trading rules, validation failed");

            var symbolData = ExchangeSymbols.SingleOrDefault(s => string.Equals(s.Symbol, symbol, StringComparison.CurrentCultureIgnoreCase));
            if (symbolData == null)
                return BinanceTRTradeRuleResult.CreateFailed($"Trade rules check failed: Symbol {symbol} not found");

            if (type != null)
            {
                if (!symbolData.OrderTypes.Contains(type.Value))
                    return BinanceTRTradeRuleResult.CreateFailed(
                        $"Trade rules check failed: {type} order type not allowed for {symbol}");
            }

            if (symbolData.LotSizeFilter != null || (symbolData.MarketLotSizeFilter != null && type == OrderType.Market))
            {
                var minQty = symbolData.LotSizeFilter?.MinQuantity;
                var maxQty = symbolData.LotSizeFilter?.MaxQuantity;
                var stepSize = symbolData.LotSizeFilter?.StepSize;
                if (type == OrderType.Market && symbolData.MarketLotSizeFilter != null)
                {
                    minQty = symbolData.MarketLotSizeFilter.MinQuantity;
                    if (symbolData.MarketLotSizeFilter.MaxQuantity != 0)
                        maxQty = symbolData.MarketLotSizeFilter.MaxQuantity;

                    if (symbolData.MarketLotSizeFilter.StepSize != 0)
                        stepSize = symbolData.MarketLotSizeFilter.StepSize;
                }

                if (minQty.HasValue && quantity.HasValue)
                {
                    outputQuantity = BinanceTRHelpers.ClampQuantity(minQty.Value, maxQty!.Value, stepSize!.Value, quantity.Value);
                    if (outputQuantity != quantity.Value)
                    {
                        if (TradeRulesBehaviour == TradeRulesBehaviour.ThrowError)
                        {
                            return BinanceTRTradeRuleResult.CreateFailed($"Trade rules check failed: LotSize filter failed. Original quantity: {quantity}, Closest allowed: {outputQuantity}");
                        }

                        log.Write(LogLevel.Information, $"Quantity clamped from {quantity} to {outputQuantity} based on lot size filter");
                    }
                }
            }

            if (price == null)
                return BinanceTRTradeRuleResult.CreatePassed(outputQuantity, null, outputStopPrice);

            if (symbolData.PriceFilter != null)
            {
                if (symbolData.PriceFilter.MaxPrice != 0 && symbolData.PriceFilter.MinPrice != 0)
                {
                    outputPrice = BinanceTRHelpers.ClampPrice(symbolData.PriceFilter.MinPrice, symbolData.PriceFilter.MaxPrice, price.Value);
                    if (outputPrice != price)
                    {
                        if (TradeRulesBehaviour == TradeRulesBehaviour.ThrowError)
                            return BinanceTRTradeRuleResult.CreateFailed($"Trade rules check failed: Price filter max/min failed. Original price: {price}, Closest allowed: {outputPrice}");

                        log.Write(LogLevel.Information, $"price clamped from {price} to {outputPrice} based on price filter");
                    }

                    if (stopPrice != null)
                    {
                        outputStopPrice = BinanceTRHelpers.ClampPrice(symbolData.PriceFilter.MinPrice,
                            symbolData.PriceFilter.MaxPrice, stopPrice.Value);
                        if (outputStopPrice != stopPrice)
                        {
                            if (TradeRulesBehaviour == TradeRulesBehaviour.ThrowError)
                                return BinanceTRTradeRuleResult.CreateFailed(
                                    $"Trade rules check failed: Stop price filter max/min failed. Original stop price: {stopPrice}, Closest allowed: {outputStopPrice}");

                            log.Write(LogLevel.Information,
                                $"Stop price clamped from {stopPrice} to {outputStopPrice} based on price filter");
                        }
                    }
                }

                if (symbolData.PriceFilter.TickSize != 0)
                {
                    var beforePrice = outputPrice;
                    outputPrice = BinanceTRHelpers.FloorPrice(symbolData.PriceFilter.TickSize, price.Value);
                    if (outputPrice != beforePrice)
                    {
                        if (TradeRulesBehaviour == TradeRulesBehaviour.ThrowError)
                            return BinanceTRTradeRuleResult.CreateFailed($"Trade rules check failed: Price filter tick failed. Original price: {price}, Closest allowed: {outputPrice}");

                        log.Write(LogLevel.Information, $"price floored from {beforePrice} to {outputPrice} based on price filter");
                    }

                    if (stopPrice != null)
                    {
                        var beforeStopPrice = outputStopPrice;
                        outputStopPrice = BinanceTRHelpers.FloorPrice(symbolData.PriceFilter.TickSize, stopPrice.Value);
                        if (outputStopPrice != beforeStopPrice)
                        {
                            if (TradeRulesBehaviour == TradeRulesBehaviour.ThrowError)
                                return BinanceTRTradeRuleResult.CreateFailed(
                                    $"Trade rules check failed: Stop price filter tick failed. Original stop price: {stopPrice}, Closest allowed: {outputStopPrice}");

                            log.Write(LogLevel.Information,
                                $"Stop price floored from {beforeStopPrice} to {outputStopPrice} based on price filter");
                        }
                    }
                }
            }

            if (symbolData.MinNotionalFilter == null || quantity == null || outputPrice == null)
                return BinanceTRTradeRuleResult.CreatePassed(outputQuantity, outputPrice, outputStopPrice);

            var currentQuantity = (outputQuantity.HasValue ? outputQuantity.Value : quantity.Value);
            var notional = currentQuantity * outputPrice.Value;
            if (notional < symbolData.MinNotionalFilter.MinNotional)
            {
                if (TradeRulesBehaviour == TradeRulesBehaviour.ThrowError)
                    return BinanceTRTradeRuleResult.CreateFailed(
                        $"Trade rules check failed: MinNotional filter failed. Order size: {notional}, minimal order size: {symbolData.MinNotionalFilter.MinNotional}");

                if (symbolData.LotSizeFilter == null)
                    return BinanceTRTradeRuleResult.CreateFailed("Trade rules check failed: MinNotional filter failed. Unable to auto comply because LotSizeFilter not present");

                var minQuantity = symbolData.MinNotionalFilter.MinNotional / outputPrice.Value;
                var stepSize = symbolData.LotSizeFilter!.StepSize;
                outputQuantity = BinanceTRHelpers.Floor(minQuantity + (stepSize - (minQuantity % stepSize)));
                log.Write(LogLevel.Information, $"Quantity clamped from {currentQuantity} to {outputQuantity} based on min notional filter");
            }

            return BinanceTRTradeRuleResult.CreatePassed(outputQuantity, outputPrice, outputStopPrice);
        }

        internal Task<WebCallResult<T>> SendRequestInternal<T>(Uri uri, HttpMethod method, CancellationToken cancellationToken, Dictionary<string, object> parameters = null, bool signed = false, bool checkResult = true, HttpMethodParameterPosition? postPosition = null, ArrayParametersSerialization? arraySerialization = null) where T : class
        {
            return base.SendRequestAsync<T>(uri, method, cancellationToken, parameters, signed, checkResult, postPosition, arraySerialization);
        }

        internal void InvokeOrderPlaced(ICommonOrderId id)
        {
            OnOrderPlaced?.Invoke(id);
        }

        internal void InvokeOrderCanceled(ICommonOrderId id)
        {
            OnOrderCanceled?.Invoke(id);
        }

        #endregion
    }
}
