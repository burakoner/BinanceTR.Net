using BinanceTR.Net.Enums;
using BinanceTR.Net.Objects.WebSocketApi;
using CryptoExchange.Net.Interfaces;
using CryptoExchange.Net.Objects;
using CryptoExchange.Net.Sockets;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BinanceTR.Net.Interfaces
{
    public interface IBinanceTRSocketClient: ISocketClient
    {
        void SetApiCredentials(string apiKey, string apiSecret);

        CallResult<UpdateSubscription> SubscribeToAggregatedTradeUpdates(string symbol, Action<DataEvent<BinanceTRStreamAggregatedTrade>> onMessage);
        CallResult<UpdateSubscription> SubscribeToAggregatedTradeUpdates(IEnumerable<string> symbols, Action<DataEvent<BinanceTRStreamAggregatedTrade>> onMessage);
        Task<CallResult<UpdateSubscription>> SubscribeToAggregatedTradeUpdatesAsync(IEnumerable<string> symbols, Action<DataEvent<BinanceTRStreamAggregatedTrade>> onMessage);
        Task<CallResult<UpdateSubscription>> SubscribeToAggregatedTradeUpdatesAsync(string symbol, Action<DataEvent<BinanceTRStreamAggregatedTrade>> onMessage);
        Task<CallResult<UpdateSubscription>> SubscribeToAllSymbolMiniTickerUpdatesAsync(Action<DataEvent<IEnumerable<IBinanceTRMiniTicker>>> onMessage);
        Task<CallResult<UpdateSubscription>> SubscribeToKlineUpdatesAsync(IEnumerable<string> symbols, IEnumerable<KlineInterval> intervals, Action<DataEvent<IBinanceTRStreamKlineData>> onMessage);
        Task<CallResult<UpdateSubscription>> SubscribeToKlineUpdatesAsync(IEnumerable<string> symbols, KlineInterval interval, Action<DataEvent<IBinanceTRStreamKlineData>> onMessage);
        Task<CallResult<UpdateSubscription>> SubscribeToKlineUpdatesAsync(string symbol, IEnumerable<KlineInterval> intervals, Action<DataEvent<IBinanceTRStreamKlineData>> onMessage);
        Task<CallResult<UpdateSubscription>> SubscribeToKlineUpdatesAsync(string symbol, KlineInterval interval, Action<DataEvent<IBinanceTRStreamKlineData>> onMessage);
        Task<CallResult<UpdateSubscription>> SubscribeToOrderBookUpdatesAsync(IEnumerable<string> symbols, int? updateInterval, Action<DataEvent<IBinanceEventOrderBook>> onMessage);
        Task<CallResult<UpdateSubscription>> SubscribeToOrderBookUpdatesAsync(string symbol, int? updateInterval, Action<DataEvent<IBinanceEventOrderBook>> onMessage);
        Task<CallResult<UpdateSubscription>> SubscribeToPartialOrderBookUpdatesAsync(IEnumerable<string> symbols, int levels, int? updateInterval, Action<DataEvent<IBinanceTROrderBook>> onMessage);
        Task<CallResult<UpdateSubscription>> SubscribeToPartialOrderBookUpdatesAsync(string symbol, int levels, int? updateInterval, Action<DataEvent<IBinanceTROrderBook>> onMessage);
        Task<CallResult<UpdateSubscription>> SubscribeToSymbolMiniTickerUpdatesAsync(IEnumerable<string> symbols, Action<DataEvent<IBinanceTRMiniTicker>> onMessage);
        Task<CallResult<UpdateSubscription>> SubscribeToSymbolMiniTickerUpdatesAsync(string symbol, Action<DataEvent<IBinanceTRMiniTicker>> onMessage);
        Task<CallResult<UpdateSubscription>> SubscribeToTradeUpdatesAsync(IEnumerable<string> symbols, Action<DataEvent<BinanceTRStreamTrade>> onMessage);
        Task<CallResult<UpdateSubscription>> SubscribeToTradeUpdatesAsync(string symbol, Action<DataEvent<BinanceTRStreamTrade>> onMessage);
    }
}