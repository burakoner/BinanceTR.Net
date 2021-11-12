using Binance.Net.Objects.Spot.MarketData;
using BinanceTR.Net.Converters;
using BinanceTR.Net.Enums;
using BinanceTR.Net.Interfaces;
using BinanceTR.Net.Objects;
using BinanceTR.Net.Objects.Core;
using BinanceTR.Net.Objects.Internal;
using BinanceTR.Net.Objects.RestApi;
using BinanceTR.Net.Objects.WebSocketApi;
using CryptoExchange.Net;
using CryptoExchange.Net.Authentication;
using CryptoExchange.Net.Objects;
using CryptoExchange.Net.Sockets;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace BinanceTR.Net
{
    public class BinanceTRSocketClient : SocketClient, IBinanceTRSocketClient
    {
        #region Private Fields
        private BinanceTRWebSocketApiClientOptions ClientOptions;
        private static BinanceTRWebSocketApiClientOptions _defaultOptions = new BinanceTRWebSocketApiClientOptions();
        private static BinanceTRWebSocketApiClientOptions DefaultOptions => _defaultOptions.Copy();
        #endregion

        #region Endpoints
        private const string aggregatedTradesStreamEndpoint = "@aggTrade";
        private const string tradesStreamEndpoint = "@trade";
        private const string klineStreamEndpoint = "@kline";
        private const string symbolMiniTickerStreamEndpoint = "@miniTicker";
        private const string allSymbolMiniTickerStreamEndpoint = "!miniTicker@arr";
        private const string partialBookDepthStreamEndpoint = "@depth";
        private const string depthStreamEndpoint = "@depth";
        #endregion

        #region Constructor
        /// <summary>
        /// Create a new instance of BinanceSocketClient with default options
        /// </summary>
        public BinanceTRSocketClient() : this(DefaultOptions)
        {
        }

        /// <summary>
        /// Create a new instance of BinanceSocketClient using provided options
        /// </summary>
        /// <param name="options">The options to use for this client</param>
        public BinanceTRSocketClient(BinanceTRWebSocketApiClientOptions options) : base("BinanceTR", options, options.ApiCredentials == null ? null : new BinanceTRAuthenticationProvider(options.ApiCredentials))
        {
            this.ClientOptions = options;

            SetDataInterpreter((byte[] data) => { return string.Empty; }, null);
            RateLimitPerSocketPerSecond = 4;
        }
        #endregion 

        #region General Methods

        /// <summary>
        /// Set the default options to be used when creating new socket clients
        /// </summary>
        /// <param name="options"></param>
        public static void SetDefaultOptions(BinanceTRWebSocketApiClientOptions options)
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

        /// <summary>
        /// Set a function to interpret the data, used when the data is received as bytes instead of a string
        /// </summary>
        /// <param name="byteHandler">Handler for byte data</param>
        /// <param name="stringHandler">Handler for string data</param>
        public new void SetDataInterpreter(Func<byte[], string> byteHandler, Func<string, string> stringHandler)
        {
            base.SetDataInterpreter(byteHandler, stringHandler);
        }
        #endregion

        #region Aggregated Trade Streams
        public CallResult<UpdateSubscription> SubscribeToAggregatedTradeUpdates(string symbol, Action<DataEvent<BinanceTRStreamAggregatedTrade>> onMessage)
            => SubscribeToAggregatedTradeUpdatesAsync(new[] { symbol }, onMessage).Result;
        public CallResult<UpdateSubscription> SubscribeToAggregatedTradeUpdates(IEnumerable<string> symbols, Action<DataEvent<BinanceTRStreamAggregatedTrade>> onMessage)
            => SubscribeToAggregatedTradeUpdatesAsync(symbols, onMessage).Result;

        /// <summary>
        /// Subscribes to the aggregated trades update stream for the provided symbol
        /// </summary>
        /// <param name="symbol">The symbol</param>
        /// <param name="onMessage">The event handler for the received data</param>
        /// <returns>A stream subscription. This stream subscription can be used to be notified when the socket is disconnected/reconnected</returns>
        public async Task<CallResult<UpdateSubscription>> SubscribeToAggregatedTradeUpdatesAsync(string symbol, Action<DataEvent<BinanceTRStreamAggregatedTrade>> onMessage)
            => await SubscribeToAggregatedTradeUpdatesAsync(new[] { symbol }, onMessage).ConfigureAwait(false);
        /// <summary>
        /// Subscribes to the aggregated trades update stream for the provided symbols
        /// </summary>
        /// <param name="symbols">The symbols</param>
        /// <param name="onMessage">The event handler for the received data</param>
        /// <returns>A stream subscription. This stream subscription can be used to be notified when the socket is disconnected/reconnected</returns>
        public async Task<CallResult<UpdateSubscription>> SubscribeToAggregatedTradeUpdatesAsync(IEnumerable<string> symbols, Action<DataEvent<BinanceTRStreamAggregatedTrade>> onMessage)
        {
            symbols.ValidateNotNull(nameof(symbols));
            foreach (var symbol in symbols) symbol.ValidateBinanceSymbol();
            symbols = symbols.Select(a => a.ToLower(CultureInfo.InvariantCulture) + aggregatedTradesStreamEndpoint).ToArray();

            var handler = new Action<DataEvent<BinanceTRCombinedStream<BinanceTRStreamAggregatedTrade>>>(data => onMessage(data.As(data.Data.Data, data.Data.Data.Symbol)));
            return await Subscribe(symbols, handler).ConfigureAwait(false);
        }

        #endregion

        #region Trade Streams
        public CallResult<UpdateSubscription> SubscribeToTradeUpdates(string symbol, Action<DataEvent<BinanceTRStreamTrade>> onMessage)
            => SubscribeToTradeUpdatesAsync(new[] { symbol }, onMessage).Result;
        public CallResult<UpdateSubscription> SubscribeToTradeUpdates(IEnumerable<string> symbols, Action<DataEvent<BinanceTRStreamTrade>> onMessage)
            => SubscribeToTradeUpdatesAsync(symbols, onMessage).Result;

        /// <summary>
        /// Subscribes to the trades update stream for the provided symbol
        /// </summary>
        /// <param name="symbol">The symbol</param>
        /// <param name="onMessage">The event handler for the received data</param>
        /// <returns>A stream subscription. This stream subscription can be used to be notified when the socket is disconnected/reconnected</returns>
        public async Task<CallResult<UpdateSubscription>> SubscribeToTradeUpdatesAsync(string symbol, Action<DataEvent<BinanceTRStreamTrade>> onMessage) => await SubscribeToTradeUpdatesAsync(new[] { symbol }, onMessage).ConfigureAwait(false);

        /// <summary>
        /// Subscribes to the trades update stream for the provided symbols
        /// </summary>
        /// <param name="symbols">The symbols</param>
        /// <param name="onMessage">The event handler for the received data</param>
        /// <returns>A stream subscription. This stream subscription can be used to be notified when the socket is disconnected/reconnected</returns>
        public async Task<CallResult<UpdateSubscription>> SubscribeToTradeUpdatesAsync(IEnumerable<string> symbols, Action<DataEvent<BinanceTRStreamTrade>> onMessage)
        {
            symbols.ValidateNotNull(nameof(symbols));
            foreach (var symbol in symbols)                symbol.ValidateBinanceSymbol();
            symbols = symbols.Select(a => a.ToLower(CultureInfo.InvariantCulture) + tradesStreamEndpoint).ToArray();
            
            var handler = new Action<DataEvent<BinanceTRCombinedStream<BinanceTRStreamTrade>>>(data => onMessage(data.As(data.Data.Data, data.Data.Data.Symbol)));
            return await Subscribe(symbols, handler).ConfigureAwait(false);
        }
        #endregion

        #region Kline/Candlestick Streams
        public CallResult<UpdateSubscription> SubscribeToKlineUpdates(string symbol, KlineInterval interval, Action<DataEvent<IBinanceTRStreamKlineData>> onMessage)
            => SubscribeToKlineUpdatesAsync(new[] { symbol }, interval, onMessage).Result;
        public CallResult<UpdateSubscription> SubscribeToKlineUpdates(string symbol, IEnumerable<KlineInterval> intervals, Action<DataEvent<IBinanceTRStreamKlineData>> onMessage)
            => SubscribeToKlineUpdatesAsync(new[] { symbol }, intervals, onMessage).Result;
        public CallResult<UpdateSubscription> SubscribeToKlineUpdates(IEnumerable<string> symbols, KlineInterval interval, Action<DataEvent<IBinanceTRStreamKlineData>> onMessage)
            => SubscribeToKlineUpdatesAsync(symbols, new[] { interval }, onMessage).Result;
        public CallResult<UpdateSubscription> SubscribeToKlineUpdates(IEnumerable<string> symbols, IEnumerable<KlineInterval> intervals, Action<DataEvent<IBinanceTRStreamKlineData>> onMessage)
            => SubscribeToKlineUpdatesAsync(symbols, intervals, onMessage).Result;

        /// <summary>
        /// Subscribes to the candlestick update stream for the provided symbol
        /// </summary>
        /// <param name="symbol">The symbol</param>
        /// <param name="interval">The interval of the candlesticks</param>
        /// <param name="onMessage">The event handler for the received data</param>
        /// <returns>A stream subscription. This stream subscription can be used to be notified when the socket is disconnected/reconnected</returns>
        public async Task<CallResult<UpdateSubscription>> SubscribeToKlineUpdatesAsync(string symbol, KlineInterval interval, Action<DataEvent<IBinanceTRStreamKlineData>> onMessage)
            => await SubscribeToKlineUpdatesAsync(new[] { symbol }, interval, onMessage).ConfigureAwait(false);

        /// <summary>
        /// Subscribes to the candlestick update stream for the provided symbol and intervals
        /// </summary>
        /// <param name="symbol">The symbol</param>
        /// <param name="intervals">The intervals of the candlesticks</param>
        /// <param name="onMessage">The event handler for the received data</param>
        /// <returns>A stream subscription. This stream subscription can be used to be notified when the socket is disconnected/reconnected</returns>
        public async Task<CallResult<UpdateSubscription>> SubscribeToKlineUpdatesAsync(string symbol, IEnumerable<KlineInterval> intervals, Action<DataEvent<IBinanceTRStreamKlineData>> onMessage)
            => await SubscribeToKlineUpdatesAsync(new[] { symbol }, intervals, onMessage).ConfigureAwait(false);

        /// <summary>
        /// Subscribes to the candlestick update stream for the provided symbols and interval
        /// </summary>
        /// <param name="symbols">The symbols</param>
        /// <param name="interval">The interval of the candlesticks</param>
        /// <param name="onMessage">The event handler for the received data</param>
        /// <returns>A stream subscription. This stream subscription can be used to be notified when the socket is disconnected/reconnected</returns>
        public async Task<CallResult<UpdateSubscription>> SubscribeToKlineUpdatesAsync(IEnumerable<string> symbols, KlineInterval interval, Action<DataEvent<IBinanceTRStreamKlineData>> onMessage)
            => await SubscribeToKlineUpdatesAsync(symbols, new[] { interval }, onMessage).ConfigureAwait(false);

        /// <summary>
        /// Subscribes to the candlestick update stream for the provided symbols and intervals
        /// </summary>
        /// <param name="symbols">The symbols</param>
        /// <param name="intervals">The intervals of the candlesticks</param>
        /// <param name="onMessage">The event handler for the received data</param>
        /// <returns>A stream subscription. This stream subscription can be used to be notified when the socket is disconnected/reconnected</returns>
        public async Task<CallResult<UpdateSubscription>> SubscribeToKlineUpdatesAsync(IEnumerable<string> symbols, IEnumerable<KlineInterval> intervals, Action<DataEvent<IBinanceTRStreamKlineData>> onMessage)
        {
            symbols.ValidateNotNull(nameof(symbols));
            foreach (var symbol in symbols) symbol.ValidateBinanceSymbol();
            symbols = symbols.SelectMany(a =>
                intervals.Select(i =>
                a.ToLower(CultureInfo.InvariantCulture) + klineStreamEndpoint + "_" +
                JsonConvert.SerializeObject(i, new KlineIntervalConverter(false)))).ToArray();

            var handler = new Action<DataEvent<BinanceTRCombinedStream<BinanceStreamKlineData>>>(data => onMessage(data.As<IBinanceTRStreamKlineData>(data.Data.Data, data.Data.Data.Symbol)));
            return await Subscribe(symbols, handler).ConfigureAwait(false);
        }
        #endregion

        #region Individual Symbol Mini Ticker Stream
        public  CallResult<UpdateSubscription> SubscribeToSymbolMiniTickerUpdates(string symbol, Action<DataEvent<IBinanceTRMiniTicker>> onMessage)
            => SubscribeToSymbolMiniTickerUpdatesAsync(new[] { symbol }, onMessage).Result;
        public  CallResult<UpdateSubscription> SubscribeToSymbolMiniTickerUpdates(IEnumerable<string> symbols, Action<DataEvent<IBinanceTRMiniTicker>> onMessage)
            => SubscribeToSymbolMiniTickerUpdatesAsync(symbols, onMessage).Result;

        /// <summary>
        /// Subscribes to mini ticker updates stream for a specific symbol
        /// </summary>
        /// <param name="symbol">The symbol to subscribe to</param>
        /// <param name="onMessage">The event handler for the received data</param>
        /// <returns>A stream subscription. This stream subscription can be used to be notified when the socket is disconnected/reconnected</returns>
        public async Task<CallResult<UpdateSubscription>> SubscribeToSymbolMiniTickerUpdatesAsync(string symbol, Action<DataEvent<IBinanceTRMiniTicker>> onMessage)
            => await SubscribeToSymbolMiniTickerUpdatesAsync(new[] { symbol }, onMessage).ConfigureAwait(false);

        /// <summary>
        /// Subscribes to mini ticker updates stream for a specific symbol
        /// </summary>
        /// <param name="symbols">The symbols to subscribe to</param>
        /// <param name="onMessage">The event handler for the received data</param>
        /// <returns>A stream subscription. This stream subscription can be used to be notified when the socket is disconnected/reconnected</returns>
        public async Task<CallResult<UpdateSubscription>> SubscribeToSymbolMiniTickerUpdatesAsync(IEnumerable<string> symbols, Action<DataEvent<IBinanceTRMiniTicker>> onMessage)
        {
            symbols.ValidateNotNull(nameof(symbols));
            foreach (var symbol in symbols) symbol.ValidateBinanceSymbol();
            symbols = symbols.Select(a => a.ToLower(CultureInfo.InvariantCulture) + symbolMiniTickerStreamEndpoint).ToArray();

            var handler = new Action<DataEvent<BinanceTRCombinedStream<BinanceTRStreamMiniTick>>>(data => onMessage(data.As<IBinanceTRMiniTicker>(data.Data.Data, data.Data.Data.Symbol)));
            return await Subscribe(symbols, handler).ConfigureAwait(false);
        }
        #endregion

        #region All Market Mini Tickers Stream
        public CallResult<UpdateSubscription> SubscribeToAllSymbolMiniTickerUpdates(Action<DataEvent<IEnumerable<IBinanceTRMiniTicker>>> onMessage)
            => SubscribeToAllSymbolMiniTickerUpdatesAsync(onMessage).Result;

        /// <summary>
        /// Subscribes to mini ticker updates stream for all symbols
        /// </summary>
        /// <param name="onMessage">The event handler for the received data</param>
        /// <returns>A stream subscription. This stream subscription can be used to be notified when the socket is disconnected/reconnected</returns>
        public async Task<CallResult<UpdateSubscription>> SubscribeToAllSymbolMiniTickerUpdatesAsync(            Action<DataEvent<IEnumerable<IBinanceTRMiniTicker>>> onMessage)
        {
            var handler = new Action<DataEvent<BinanceTRCombinedStream<IEnumerable<BinanceStreamCoinMiniTick>>>>(data => onMessage(data.As<IEnumerable<IBinanceTRMiniTicker>>(data.Data.Data, data.Data.Stream)));
            return await Subscribe(new[] { allSymbolMiniTickerStreamEndpoint }, handler).ConfigureAwait(false);
        }
        #endregion

        #region Partial Book Depth Streams
        public CallResult<UpdateSubscription> SubscribeToPartialOrderBookUpdates(string symbol, int levels, int? updateInterval, Action<DataEvent<IBinanceTROrderBook>> onMessage)
            => SubscribeToPartialOrderBookUpdatesAsync(new[] { symbol }, levels, updateInterval, onMessage).Result;
        public CallResult<UpdateSubscription> SubscribeToPartialOrderBookUpdates(IEnumerable<string> symbols, int levels, int? updateInterval, Action<DataEvent<IBinanceTROrderBook>> onMessage)
            => SubscribeToPartialOrderBookUpdatesAsync(symbols, levels, updateInterval, onMessage).Result;

        /// <summary>
        /// Subscribes to the depth updates for the provided symbol
        /// </summary>
        /// <param name="symbol">The symbol to subscribe on</param>
        /// <param name="levels">The amount of entries to be returned in the update</param>
        /// <param name="updateInterval">Update interval in milliseconds, either 100 or 1000. Defaults to 1000</param>
        /// <param name="onMessage">The event handler for the received data</param>
        /// <returns>A stream subscription. This stream subscription can be used to be notified when the socket is disconnected/reconnected</returns>
        public async Task<CallResult<UpdateSubscription>> SubscribeToPartialOrderBookUpdatesAsync(string symbol, int levels, int? updateInterval, Action<DataEvent<IBinanceTROrderBook>> onMessage) 
            => await SubscribeToPartialOrderBookUpdatesAsync(new[] { symbol }, levels, updateInterval, onMessage).ConfigureAwait(false);

        /// <summary>
        /// Subscribes to the depth updates for the provided symbols
        /// </summary>
        /// <param name="symbols">The symbols to subscribe on</param>
        /// <param name="levels">The amount of entries to be returned in the update of each symbol</param>
        /// <param name="updateInterval">Update interval in milliseconds, either 100 or 1000. Defaults to 1000</param>
        /// <param name="onMessage">The event handler for the received data</param>
        /// <returns>A stream subscription. This stream subscription can be used to be notified when the socket is disconnected/reconnected</returns>
        public async Task<CallResult<UpdateSubscription>> SubscribeToPartialOrderBookUpdatesAsync(IEnumerable<string> symbols, int levels, int? updateInterval, Action<DataEvent<IBinanceTROrderBook>> onMessage)
        {
            symbols.ValidateNotNull(nameof(symbols));
            foreach (var symbol in symbols) symbol.ValidateBinanceSymbol();

            levels.ValidateIntValues(nameof(levels), 5, 10, 20);
            updateInterval?.ValidateIntValues(nameof(updateInterval), 100, 1000);

            symbols = symbols.Select(a =>
                a.ToLower(CultureInfo.InvariantCulture) + partialBookDepthStreamEndpoint + levels +
                (updateInterval.HasValue ? $"@{updateInterval.Value}ms" : "")).ToArray();

            var handler = new Action<DataEvent<BinanceTRCombinedStream<BinanceTROrderBook>>>(data =>
            {
                data.Data.Data.Symbol = data.Data.Stream.Split('@')[0];
                onMessage(data.As<IBinanceTROrderBook>(data.Data.Data, data.Data.Data.Symbol));
            });
            return await Subscribe(symbols, handler).ConfigureAwait(false);
        }
        #endregion

        #region Diff. Depth Stream
        public CallResult<UpdateSubscription> SubscribeToOrderBookUpdates(string symbol, int? updateInterval, Action<DataEvent<IBinanceEventOrderBook>> onMessage)
           => SubscribeToOrderBookUpdatesAsync(new[] { symbol }, updateInterval, onMessage).Result;
        public CallResult<UpdateSubscription> SubscribeToOrderBookUpdates(IEnumerable<string> symbols, int? updateInterval, Action<DataEvent<IBinanceEventOrderBook>> onMessage)
           => SubscribeToOrderBookUpdatesAsync(symbols, updateInterval, onMessage).Result;

        /// <summary>
        /// Subscribes to the order book updates for the provided symbol
        /// </summary>
        /// <param name="symbol">The symbol</param>
        /// <param name="updateInterval">Update interval in milliseconds, either 100 or 1000. Defaults to 1000</param>
        /// <param name="onMessage">The event handler for the received data</param>
        /// <returns>A stream subscription. This stream subscription can be used to be notified when the socket is disconnected/reconnected</returns>
        public async Task<CallResult<UpdateSubscription>> SubscribeToOrderBookUpdatesAsync(string symbol, int? updateInterval, Action<DataEvent<IBinanceEventOrderBook>> onMessage)
            => await SubscribeToOrderBookUpdatesAsync(new[] { symbol }, updateInterval, onMessage).ConfigureAwait(false);

        /// <summary>
        /// Subscribes to the depth update stream for the provided symbols
        /// </summary>
        /// <param name="symbols">The symbols</param>
        /// <param name="updateInterval">Update interval in milliseconds, either 100 or 1000. Defaults to 1000</param>
        /// <param name="onMessage">The event handler for the received data</param>
        /// <returns>A stream subscription. This stream subscription can be used to be notified when the socket is disconnected/reconnected</returns>
        public async Task<CallResult<UpdateSubscription>> SubscribeToOrderBookUpdatesAsync(IEnumerable<string> symbols, int? updateInterval, Action<DataEvent<IBinanceEventOrderBook>> onMessage)
        {
            symbols.ValidateNotNull(nameof(symbols));
            foreach (var symbol in symbols)
                symbol.ValidateBinanceSymbol();

            updateInterval?.ValidateIntValues(nameof(updateInterval), 100, 1000);
            symbols = symbols.Select(a =>
                a.ToLower(CultureInfo.InvariantCulture) + depthStreamEndpoint +
                (updateInterval.HasValue ? $"@{updateInterval.Value}ms" : "")).ToArray();

            var handler = new Action<DataEvent<BinanceTRCombinedStream<BinanceTROrderBookEvent>>>(data => onMessage(data.As<IBinanceEventOrderBook>(data.Data.Data, data.Data.Data.Symbol)));
            return await Subscribe(symbols, handler).ConfigureAwait(false);
        }

        #endregion

        #region Core Methods
        private async Task<CallResult<UpdateSubscription>> Subscribe<T>(IEnumerable<string> topics, Action<DataEvent<T>> onData)
        {
            return await SubscribeInternal(ClientOptions.BaseAddress + "stream", topics, onData).ConfigureAwait(false);
        }

        internal CallResult<T> DeserializeInternal<T>(JToken data, bool checkObject = true)
        {
            return Deserialize<T>(data, checkObject);
        }

        internal Task<CallResult<UpdateSubscription>> SubscribeInternal<T>(string url, IEnumerable<string> topics, Action<DataEvent<T>> onData)
        {
            var request = new BinanceTRSocketRequest
            {
                Method = "SUBSCRIBE",
                Params = topics.ToArray(),
                Id = NextId()
            };

            return SubscribeAsync(url, request, null, false, onData);
        }

        protected override bool HandleQueryResponse<T>(SocketConnection s, object request, JToken data, out CallResult<T> callResult)
        {
            throw new NotImplementedException();
        }

        protected override bool HandleSubscriptionResponse(SocketConnection s, SocketSubscription subscription, object request, JToken message, out CallResult<object> callResult)
        {
            callResult = null;
            if (message.Type != JTokenType.Object)
                return false;

            var id = message["id"];
            if (id == null)
                return false;

            var bRequest = (BinanceTRSocketRequest)request;
            if ((int)id != bRequest.Id)
                return false;

            var result = message["result"];
            if (result != null && result.Type == JTokenType.Null)
            {
                log.Write(Microsoft.Extensions.Logging.LogLevel.Trace, $"Socket {s.Socket.Id} Subscription completed");
                callResult = new CallResult<object>(null, null);
                return true;
            }

            var error = message["error"];
            if (error == null)
            {
                callResult = new CallResult<object>(null, new ServerError("Unknown error: " + message.ToString()));
                return true;
            }

            callResult = new CallResult<object>(null, new ServerError(error["code"]!.Value<int>(), error["msg"]!.ToString()));
            return true;
        }

        protected override bool MessageMatchesHandler(JToken message, object request)
        {
            if (message.Type != JTokenType.Object)
                return false;

            var bRequest = (BinanceTRSocketRequest)request;
            var stream = message["stream"];
            if (stream == null)
                return false;

            return bRequest.Params.Contains(stream.ToString());
        }

        protected override bool MessageMatchesHandler(JToken message, string identifier)
        {
            return true;
        }

        protected override Task<CallResult<bool>> AuthenticateSocketAsync(SocketConnection s)
        {
            throw new NotImplementedException();
        }

        protected override async Task<bool> UnsubscribeAsync(SocketConnection connection, SocketSubscription subscription)
        {
            var topics = ((BinanceTRSocketRequest)subscription.Request!).Params;
            var unsub = new BinanceTRSocketRequest { Method = "UNSUBSCRIBE", Params = topics, Id = NextId() };
            var result = false;

            if (!connection.Socket.IsOpen)
                return true;

            await connection.SendAndWaitAsync(unsub, ResponseTimeout, data =>
            {
                if (data.Type != JTokenType.Object)
                    return false;

                var id = data["id"];
                if (id == null)
                    return false;

                if ((int)id != unsub.Id)
                    return false;

                var result = data["result"];
                if (result?.Type == JTokenType.Null)
                {
                    result = true;
                    return true;
                }

                return true;
            }).ConfigureAwait(false);
            return result;
        }
        #endregion
    }
}
