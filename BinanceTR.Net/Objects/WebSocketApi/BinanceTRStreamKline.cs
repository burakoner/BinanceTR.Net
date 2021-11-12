﻿using BinanceTR.Net.Converters;
using Newtonsoft.Json;
using System;
using CryptoExchange.Net.Converters;
using BinanceTR.Net.Interfaces;
using BinanceTR.Net.Enums;
using BinanceTR.Net.Objects.RestApi;
using BinanceTR.Net.Objects.Base;

namespace BinanceTR.Net.Objects.WebSocketApi
{
    /// <summary>
    /// Wrapper for kline information for a symbol
    /// </summary>
    public class BinanceStreamKlineData: BinanceTRStreamEvent, IBinanceTRStreamKlineData
    {
        /// <summary>
        /// The symbol the data is for
        /// </summary>
        [JsonProperty("s")]
        public string Symbol { get; set; } = string.Empty;

        /// <summary>
        /// The data
        /// </summary>
        [JsonProperty("k")]
        [JsonConverter(typeof(InterfaceConverter<BinanceTRStreamKline>))]
        public IBinanceTRStreamKline Data { get; set; } = default!;
    }

    /// <summary>
    /// The kline data
    /// </summary>
    public class BinanceTRStreamKline: BinanceTRKlineBase, IBinanceTRStreamKline
    {
        /// <summary>
        /// The open time of this candlestick
        /// </summary>
        [JsonProperty("t"), JsonConverter(typeof(TimestampConverter))]
        public new DateTime OpenTime { get; set; }

        /// <inheritdoc />
        [JsonProperty("v")]
        public override decimal BaseVolume { get; set; }

        /// <summary>
        /// The close time of this candlestick
        /// </summary>
        [JsonProperty("T"), JsonConverter(typeof(TimestampConverter))]
        public new DateTime CloseTime { get; set; }

        /// <inheritdoc />
        [JsonProperty("q")]
        public override decimal QuoteVolume { get; set; }

        /// <summary>
        /// The symbol this candlestick is for
        /// </summary>
        [JsonProperty("s")]
        public string Symbol { get; set; } = string.Empty;
        /// <summary>
        /// The interval of this candlestick
        /// </summary>
        [JsonProperty("i"), JsonConverter(typeof(KlineIntervalConverter))]
        public KlineInterval Interval { get; set; }
        /// <summary>
        /// The first trade id in this candlestick
        /// </summary>
        [JsonProperty("f")]
        public long FirstTrade { get; set; }
        /// <summary>
        /// The last trade id in this candlestick
        /// </summary>
        [JsonProperty("L")]
        public long LastTrade { get; set; }
        /// <summary>
        /// The open price of this candlestick
        /// </summary>
        [JsonProperty("o")]
        public new decimal Open { get; set; }
        /// <summary>
        /// The close price of this candlestick
        /// </summary>
        [JsonProperty("c")]
        public new decimal Close { get; set; }
        /// <summary>
        /// The highest price of this candlestick
        /// </summary>
        [JsonProperty("h")]
        public new decimal High { get; set; }
        /// <summary>
        /// The lowest price of this candlestick
        /// </summary>
        [JsonProperty("l")]
        public new decimal Low { get; set; }
        /// <summary>
        /// The amount of trades in this candlestick
        /// </summary>
        [JsonProperty("n")]
        public new int TradeCount { get; set; }

        /// <inheritdoc />
        [JsonProperty("V")]
        public override decimal TakerBuyBaseVolume { get; set; }
        /// <inheritdoc />
        [JsonProperty("Q")]
        public override decimal TakerBuyQuoteVolume { get; set; }

        /// <summary>
        /// Boolean indicating whether this candlestick is closed
        /// </summary>
        [JsonProperty("x")]
        public bool Final { get; set; }

        /// <summary>
        /// Casts this object to a <see cref="BinanceSpotKline"/> object
        /// </summary>
        /// <returns></returns>
        public BinanceTRKline ToKline()
        {
            return new BinanceTRKline
            {
                Open = Open,
                Close = Close,
                BaseVolume = BaseVolume,
                CloseTime = CloseTime,
                High = High,
                Low = Low,
                OpenTime = OpenTime,
                QuoteVolume = QuoteVolume,
                TakerBuyBaseVolume = TakerBuyBaseVolume,
                TakerBuyQuoteVolume = TakerBuyQuoteVolume,
                TradeCount = TradeCount
            };
        }
    }
}