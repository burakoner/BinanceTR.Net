﻿using System;
using BinanceTR.Net.Converters;
using BinanceTR.Net.Enums;
using CryptoExchange.Net.Attributes;
using CryptoExchange.Net.Converters;
using Newtonsoft.Json;

namespace BinanceTR.Net.Objects.Base
{
    /// <summary>
    /// Order info
    /// </summary>
    public class BinanceTROrderBase
    {
        /// <summary>
        /// The symbol the order is for
        /// </summary>
        public string Symbol { get; set; } = string.Empty;
        /// <summary>
        /// The order id generated by Binance
        /// </summary>
        public long OrderId { get; set; }

        /// <summary>
        /// Id of the order list this order belongs to
        /// </summary>
        public long OrderListId { get; set; }

        /// <summary>
        /// Original order id
        /// </summary>
        [JsonOptionalProperty]
        [JsonProperty("origClientOrderId")]
        public string OriginalClientOrderId { get; set; } = string.Empty;

        /// <summary>
        /// The order id as assigned by the client
        /// </summary>
        public string ClientOrderId { get; set; } = string.Empty;

        private decimal _price;

        /// <summary>
        /// The price of the order
        /// </summary>
        public decimal Price
        {
            get
            {
                if (_price == 0 && Type == OrderType.Market && QuantityFilled != 0)
                    return QuoteQuantityFilled / QuantityFilled;
                return _price;
            }
            set => _price = value;
        }

        /// <summary>
        /// The original quantity of the order
        /// </summary>
        [JsonProperty("origQty")]
        public decimal Quantity { get; set; }
        /// <summary>
        /// The currently executed quantity of the order
        /// </summary>
        [JsonProperty("executedQty")]
        public decimal QuantityFilled { get; set; }
        /// <summary>
        /// Cummulative amount
        /// </summary>
        [JsonProperty("cummulativeQuoteQty")]
        public decimal QuoteQuantityFilled { get; set; }
        /// <summary>
        /// The original quote order quantity
        /// </summary>
        [JsonProperty("origQuoteOrderQty")]
        public decimal QuoteQuantity { get; set; }


        /// <summary>
        /// The status of the order
        /// </summary>
        [JsonConverter(typeof(OrderStatusConverter))]
        public OrderStatus Status { get; set; }

        /// <summary>
        /// How long the order is active
        /// </summary>
        [JsonConverter(typeof(TimeInForceConverter))]
        public TimeInForce TimeInForce { get; set; }
        /// <summary>
        /// The type of the order
        /// </summary>
        [JsonConverter(typeof(OrderTypeConverter))]
        public OrderType Type { get; set; }
        /// <summary>
        /// The side of the order
        /// </summary>
        [JsonConverter(typeof(OrderSideConverter))]
        public OrderSide Side { get; set; }
        /// <summary>
        /// The stop price
        /// </summary>
        public decimal? StopPrice { get; set; }

        /// <summary>
        /// The iceberg quantity
        /// </summary>
        [JsonProperty("icebergQty")]
        public decimal? IcebergQuantity { get; set; }
        /// <summary>
        /// The time the order was submitted
        /// </summary>
        [JsonProperty("time"), JsonConverter(typeof(TimestampConverter))]
        public DateTime CreateTime { get; set; }
        /// <summary>
        /// The time the order was last updated
        /// </summary>
        [JsonConverter(typeof(TimestampConverter))]
        public DateTime? UpdateTime { get; set; }
        /// <summary>
        /// Is working
        /// </summary>
        public bool? IsWorking { get; set; }


        /// <summary>
        /// Quantity which is still open to be filled
        /// </summary>
        public decimal QuantityRemaining => Quantity - QuantityFilled;

        /// <summary>
        /// The average price the order was filled
        /// </summary>
        public decimal? AverageFillPrice
        {
            get
            {
                if (QuantityFilled == 0)
                    return null;

                return QuoteQuantityFilled / QuantityFilled;
            }
        }
    }
}
