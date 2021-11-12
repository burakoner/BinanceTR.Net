using BinanceTR.Net.Enums;

namespace BinanceTR.Net.Interfaces
{
    /// <summary>
    /// Stream kline data
    /// </summary>
    public interface IBinanceTRStreamKlineData
    {
        /// <summary>
        /// The symbol the data is for
        /// </summary>
        string Symbol { get; set; }

        /// <summary>
        /// The data
        /// </summary>
        IBinanceTRStreamKline Data { get; set; }
    }

    /// <summary>
    /// Stream kline data
    /// </summary>
    public interface IBinanceTRStreamKline: IBinanceTRKline
    {
        /// <summary>
        /// Interval
        /// </summary>
        KlineInterval Interval { get; set; }
        /// <summary>
        /// Is this kline final
        /// </summary>
        bool Final { get; set; }
        /// <summary>
        /// Id of the first trade in this kline
        /// </summary>
        long FirstTrade { get; set; }
        /// <summary>
        /// Id of the last trade in this kline
        /// </summary>
        long LastTrade { get; set; }
    }
}
