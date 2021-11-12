using BinanceTR.Net.Converters;
using BinanceTR.Net.Enums;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BinanceTR.Net.Objects.RestApi
{
    public class BinanceTRSymbol
    {
        [JsonProperty("type"), JsonConverter(typeof(SymbolTypeConverter))]
        public SymbolType Type { get; set; }

        [JsonProperty("symbol")]
        public string Symbol { get; set; }

        [JsonProperty("baseAsset")]
        public string BaseAsset { get; set; }

        [JsonProperty("basePrecision")]
        public int BaseAssetPrecision { get; set; }

        [JsonProperty("quoteAsset")]
        public string QuoteAsset { get; set; }

        [JsonProperty("quotePrecision")]
        public int QuoteAssetPrecision { get; set; }

        [JsonProperty("icebergEnable"), JsonConverter(typeof(BooleanConverter))]
        public bool IcebergEnable { get; set; }

        [JsonProperty("ocoEnable"), JsonConverter(typeof(BooleanConverter))]
        public bool OcoEnable { get; set; }

        [JsonProperty("spotTradingEnable"), JsonConverter(typeof(BooleanConverter))]
        public bool SpotTradingEnable { get; set; }

        [JsonProperty("marginTradingEnable"), JsonConverter(typeof(BooleanConverter))]
        public bool MarginTradingEnable { get; set; }

        [JsonProperty("orderTypes", ItemConverterType = typeof(OrderTypeConverter))]
        public IEnumerable<OrderType> OrderTypes { get; set; } = Array.Empty<OrderType>();

        [JsonProperty("filters")]
        public IEnumerable<BinanceTRSymbolFilter> Filters { get; set; } = Array.Empty<BinanceTRSymbolFilter>();

        [JsonIgnore]
        public BinanceSymbolPriceFilter PriceFilter => Filters.OfType<BinanceSymbolPriceFilter>().FirstOrDefault();

        [JsonIgnore]
        public BinanceSymbolPercentPriceFilter PricePercentFilter => Filters.OfType<BinanceSymbolPercentPriceFilter>().FirstOrDefault();

        [JsonIgnore]
        public BinanceSymbolLotSizeFilter LotSizeFilter => Filters.OfType<BinanceSymbolLotSizeFilter>().FirstOrDefault();

        [JsonIgnore]
        public BinanceSymbolMinNotionalFilter MinNotionalFilter => Filters.OfType<BinanceSymbolMinNotionalFilter>().FirstOrDefault();

        [JsonIgnore]
        public BinanceSymbolIcebergPartsFilter IceBergPartsFilter => Filters.OfType<BinanceSymbolIcebergPartsFilter>().FirstOrDefault();

        [JsonIgnore]
        public BinanceSymbolMarketLotSizeFilter MarketLotSizeFilter => Filters.OfType<BinanceSymbolMarketLotSizeFilter>().FirstOrDefault();

        [JsonIgnore]
        public BinanceSymbolMaxAlgorithmicOrdersFilter MaxAlgorithmicOrdersFilter => Filters.OfType<BinanceSymbolMaxAlgorithmicOrdersFilter>().FirstOrDefault();

        [JsonIgnore]
        public BinanceSymbolMaxOrdersFilter MaxOrdersFilter => Filters.OfType<BinanceSymbolMaxOrdersFilter>().FirstOrDefault();

        [JsonIgnore]
        public BinanceSymbolMaxPositionFilter MaxPositionFilter => Filters.OfType<BinanceSymbolMaxPositionFilter>().FirstOrDefault();
    }
}
