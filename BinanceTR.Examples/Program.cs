using BinanceTR.Net;
using BinanceTR.Net.Objects.Core;
using System;
using System.Threading;

namespace BinanceTR.Examples
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var cli = new BinanceTRClient(new BinanceTRRestApiClientOptions
            {
                LogLevel = Microsoft.Extensions.Logging.LogLevel.Debug,
                ApiCredentials = new CryptoExchange.Net.Authentication.ApiCredentials("cfDC92B191b9B3Ca3D842Ae0e01108CBKI6BqEW6xr4NrPus3hoZ9Ze9YrmWwPFV", "f9AbA6a8AD6bC2a97294a212244dda04ETfl0kc4BSUGOtL7m7rNELpt3Jh25SiP")
            });



            //var servertime = cli.GetServerTime();
            //var symbols = cli.GetTradingSymbols();

            //var mainSymbols = symbols.Data.Where(x => x.Type == SymbolType.Main).ToList();
            //var nextSymbols = symbols.Data.Where(x => x.Type == SymbolType.Next).ToList();

            //var orderbook = cli.GetOrderBook("BTC_TRY");
            //var trades = cli.GetTrades("BTC_TRY");
            //var aggtrades = cli.GetAggregatedTrades("BTC_TRY");
            //var klines = cli.GetKlines("BTC_TRY", KlineInterval.OneHour);


            var ws = new BinanceTRSocketClient(new BinanceTRWebSocketApiClientOptions
            {
                LogLevel = Microsoft.Extensions.Logging.LogLevel.Debug,
                ApiCredentials = new CryptoExchange.Net.Authentication.ApiCredentials("cfDC92B191b9B3Ca3D842Ae0e01108CBKI6BqEW6xr4NrPus3hoZ9Ze9YrmWwPFV", "f9AbA6a8AD6bC2a97294a212244dda04ETfl0kc4BSUGOtL7m7rNELpt3Jh25SiP")
            });

            /*
            ws.SubscribeToAggregatedTradeUpdates("BTCTRY", (data) =>
            {
                if (data != null)
                {
                    Console.WriteLine($"S:{data.Data.Symbol} P:{data.Data.Price} Q:{data.Data.Quantity} ID:{data.Data.AggregateTradeId} FID:{data.Data.FirstTradeId} LID:{data.Data.LastTradeId}");
                }
            });
            */

            ws.SubscribeToTradeUpdates("BTCTRY", (data) =>
            {
                if (data != null)
                {
                    Console.WriteLine($"S:{data.Data.Symbol} P:{data.Data.Price} Q:{data.Data.Quantity}");
                }
            });

            var ob = new BinanceTROrderBookClient("BTCTRY", new BinanceTROrderBookClientOptions
            {
                RestClient = cli,
                SocketClient = ws,
            });
            var res = ob.StartAsync().Result;
            Console.WriteLine("Başladı");
            Console.ReadLine();

            while (true)
            {
                Console.WriteLine($"S:{ob.Symbol} BP:{ob.BestBid.Price} BQ:{ob.BestBid.Quantity} AP:{ob.BestAsk.Price} AQ:{ob.BestAsk.Quantity}");
                Thread.Sleep(1000);
            }

            Console.WriteLine("Done");
            Console.ReadLine();
        }
    }
}
