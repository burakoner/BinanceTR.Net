using System.Collections.Generic;
using CryptoExchange.Net.Converters;
using BinanceTR.Net.Enums;

namespace BinanceTR.Net.Converters
{
    internal class SymbolTypeConverter : BaseConverter<SymbolType>
    {
        public SymbolTypeConverter(): this(true) { }
        public SymbolTypeConverter(bool quotes) : base(quotes) { }

        protected override List<KeyValuePair<SymbolType, string>> Mapping => new List<KeyValuePair<SymbolType, string>>
        {
            new KeyValuePair<SymbolType, string>(SymbolType.Main, "1"),
            new KeyValuePair<SymbolType, string>(SymbolType.Next, "2"),
        };
    }
}
