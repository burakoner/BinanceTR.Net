namespace BinanceTR.Net.Objects.Internal
{
    internal class BinanceTRTradeRuleResult
    {
        public bool Passed { get; set; }
        public decimal? Quantity { get; set; }
        public decimal? Price { get; set; }
        public decimal? StopPrice { get; set; }
        public string ErrorMessage { get; set; }

        public static BinanceTRTradeRuleResult CreatePassed(decimal? quantity, decimal? price, decimal? stopPrice)
        {
            return new BinanceTRTradeRuleResult
            {
                Passed = true,
                Quantity = quantity,
                Price = price,
                StopPrice = stopPrice
            };
        }

        public static BinanceTRTradeRuleResult CreateFailed(string message)
        {
            return new BinanceTRTradeRuleResult
            {
                Passed = false,
                ErrorMessage = message
            };
        }
    }
}
