using System.Threading.Tasks;
using BinanceTR.Net.Interfaces;
using BinanceTR.Net.Objects;
using BinanceTR.Net.Objects.Core;
using CryptoExchange.Net.Objects;
using CryptoExchange.Net.OrderBook;
using CryptoExchange.Net.Sockets;

namespace BinanceTR.Net
{
    /// <summary>
    /// Implementation for a synchronized order book. After calling Start the order book will sync itself and keep up to date with new data. It will automatically try to reconnect and resync in case of a lost/interrupted connection.
    /// Make sure to check the State property to see if the order book is synced.
    /// </summary>
    public class BinanceTROrderBookClient : SymbolOrderBook
    {
        private readonly IBinanceTRClient _restClient;
        private readonly IBinanceTRSocketClient _socketClient;
        private readonly bool _restOwner;
        private readonly bool _socketOwner;
        private readonly int? _updateInterval;

        /// <summary>
        /// Create a new instance
        /// </summary>
        /// <param name="symbol">The symbol of the order book</param>
        /// <param name="options">The options for the order book</param>
        public BinanceTROrderBookClient(string symbol, BinanceTROrderBookClientOptions options = null) : base(symbol, options ?? new Objects.Core.BinanceTROrderBookClientOptions())
        {
            symbol.ValidateBinanceSymbol();
            Levels = options?.Limit;
            _updateInterval = options?.UpdateInterval;
            _socketClient = options?.SocketClient ?? new BinanceTRSocketClient();
            _restClient = options?.RestClient ?? new BinanceTRClient();
            _restOwner = options?.RestClient == null;
            _socketOwner = options?.SocketClient == null;
        }

        protected override async Task<CallResult<UpdateSubscription>> DoStartAsync()
        {
            CallResult<UpdateSubscription> subResult;
            if (Levels == null) subResult = await _socketClient.SubscribeToOrderBookUpdatesAsync(Symbol, _updateInterval, HandleUpdate).ConfigureAwait(false);
            else subResult = await _socketClient.SubscribeToPartialOrderBookUpdatesAsync(Symbol, Levels.Value, _updateInterval, HandleUpdate).ConfigureAwait(false);

            if (!subResult)
                return new CallResult<UpdateSubscription>(null, subResult.Error);

            Status = OrderBookStatus.Syncing;
            if (Levels == null)
            {
                // Small delay to make sure the snapshot is from after our first stream update
                await Task.Delay(200).ConfigureAwait(false);
                var bookResult = await _restClient.GetOrderBookAsync(Symbol, Levels ?? 5000).ConfigureAwait(false);
                if (!bookResult)
                {
                    log.Write(Microsoft.Extensions.Logging.LogLevel.Debug, $"{Id} order book {Symbol} failed to retrieve initial order book");
                    await _socketClient.UnsubscribeAsync(subResult.Data).ConfigureAwait(false);
                    return new CallResult<UpdateSubscription>(null, bookResult.Error);
                }

                SetInitialOrderBook(bookResult.Data.LastUpdateId, bookResult.Data.Bids, bookResult.Data.Asks);
            }
            else
            {
                var setResult = await WaitForSetOrderBookAsync(10000).ConfigureAwait(false);
                return setResult ? subResult : new CallResult<UpdateSubscription>(null, setResult.Error);
            }

            return new CallResult<UpdateSubscription>(subResult.Data, null);
        }

        private void HandleUpdate(DataEvent<IBinanceEventOrderBook> data)
        {
            if (data.Data.FirstUpdateId != null)
                UpdateOrderBook(data.Data.FirstUpdateId.Value, data.Data.LastUpdateId, data.Data.Bids, data.Data.Asks);
            else
                UpdateOrderBook(data.Data.LastUpdateId, data.Data.Bids, data.Data.Asks);
        }


        private void HandleUpdate(DataEvent<IBinanceTROrderBook> data)
        {
            if (Levels == null) UpdateOrderBook(data.Data.LastUpdateId, data.Data.Bids, data.Data.Asks);
            else SetInitialOrderBook(data.Data.LastUpdateId, data.Data.Bids, data.Data.Asks);
        }

        protected override void DoReset()
        {
        }

        protected override async Task<CallResult<bool>> DoResyncAsync()
        {
            if (Levels != null)
                return await WaitForSetOrderBookAsync(10000).ConfigureAwait(false);

            var bookResult = await _restClient.GetOrderBookAsync(Symbol, Levels ?? 5000).ConfigureAwait(false);
            if (!bookResult)
                return new CallResult<bool>(false, bookResult.Error);

            SetInitialOrderBook(bookResult.Data.LastUpdateId, bookResult.Data.Bids, bookResult.Data.Asks);
            return new CallResult<bool>(true, null);
        }

        public override void Dispose()
        {
            processBuffer.Clear();
            asks.Clear();
            bids.Clear();

            if (_restOwner) _restClient?.Dispose();
            if (_socketOwner) _socketClient?.Dispose();
        }
    }
}
