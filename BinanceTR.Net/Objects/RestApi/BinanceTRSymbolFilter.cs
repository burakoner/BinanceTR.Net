using BinanceTR.Net.Converters;
using BinanceTR.Net.Enums;
using Newtonsoft.Json;

namespace BinanceTR.Net.Objects.RestApi
{
    [JsonConverter(typeof(SymbolFilterConverter))]
    public class BinanceTRSymbolFilter
    {
        [JsonProperty("applyToMarket")]
        public bool ApplyToMarket { get; set; }

        [JsonProperty("filterType")]
        public SymbolFilterType FilterType { get; set; }
    }

    public class BinanceSymbolPriceFilter: BinanceTRSymbolFilter
    {
        [JsonProperty("minPrice")]
        public decimal MinPrice { get; set; }

        [JsonProperty("maxPrice")]
        public decimal MaxPrice { get; set; }

        [JsonProperty("tickSize")]
        public decimal TickSize { get; set; }
    }

    public class BinanceSymbolPercentPriceFilter : BinanceTRSymbolFilter
    {
        [JsonProperty("multiplierUp")]
        public decimal MultiplierUp { get; set; }

        [JsonProperty("multiplierDown")]
        public decimal MultiplierDown { get; set; }

        [JsonProperty("avgPriceMins")]
        public int AveragePriceMinutes { get; set; }
    }

    public class BinanceSymbolLotSizeFilter : BinanceTRSymbolFilter
    {
        [JsonProperty("minQty")]
        public decimal MinQuantity { get; set; }

        [JsonProperty("maxQty")]
        public decimal MaxQuantity { get; set; }

        [JsonProperty("stepSize")]
        public decimal StepSize { get; set; }
    }

    public class BinanceSymbolMinNotionalFilter : BinanceTRSymbolFilter
    {
        /// <summary>
        /// The minimal total size of an order. This is calculated by Price * Quantity.
        /// </summary>
        [JsonProperty("minNotional")]
        public decimal MinNotional { get; set; }

        /// <summary>
        /// The amount of minutes the average price of trades is calculated over for market orders. 0 means the last price is used
        /// </summary>
        [JsonProperty("avgPriceMins")]
        public int AveragePriceMinutes { get; set; }
    }

    public class BinanceSymbolIcebergPartsFilter : BinanceTRSymbolFilter
    {
        [JsonProperty("limit")]
        public int Limit { get; set; }
    }

    public class BinanceSymbolMarketLotSizeFilter : BinanceTRSymbolFilter
    {
        [JsonProperty("minQty")]
        public decimal MinQuantity { get; set; }

        [JsonProperty("maxQty")]
        public decimal MaxQuantity { get; set; }

        [JsonProperty("stepSize")]
        public decimal StepSize { get; set; }
    }

    public class BinanceSymbolMaxAlgorithmicOrdersFilter : BinanceTRSymbolFilter
    {
        [JsonProperty("maxNumAlgoOrders")]
        public int MaxNumberAlgorithmicOrders { get; set; }
    }

    public class BinanceSymbolMaxOrdersFilter : BinanceTRSymbolFilter
    {
        /// <summary>
        /// The max number of orders for this symbol
        /// </summary>
        // [JsonProperty("maxNumOrders")]
        // public int MaxNumberOrders { get; set; }
    }

    public class BinanceSymbolMaxPositionFilter : BinanceTRSymbolFilter
    {
        /// <summary>
        /// The MaxPosition filter defines the allowed maximum position an account can have on the base asset of a symbol.
        /// </summary>
        // [JsonProperty("maxPosition")]
        // public decimal MaxPosition { get; set; }
    }
}
