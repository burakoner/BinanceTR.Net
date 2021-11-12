using System.Collections.Generic;
using CryptoExchange.Net.Converters;
using BinanceTR.Net.Enums;

namespace BinanceTR.Net.Converters
{
    internal class BooleanConverter : BaseConverter<bool>
    {
        public BooleanConverter(): this(true) { }
        public BooleanConverter(bool quotes) : base(quotes) { }

        protected override List<KeyValuePair<bool, string>> Mapping => new List<KeyValuePair<bool, string>>
        {
            new KeyValuePair<bool, string>(true, "1"),
            new KeyValuePair<bool, string>(true, "T"),
            new KeyValuePair<bool, string>(true, "True"),
            new KeyValuePair<bool, string>(true, "Y"),
            new KeyValuePair<bool, string>(true, "Yes"),
            
            new KeyValuePair<bool, string>(false, "0"),
            new KeyValuePair<bool, string>(false, "F"),
            new KeyValuePair<bool, string>(false, "False"),
            new KeyValuePair<bool, string>(false, "N"),
            new KeyValuePair<bool, string>(false, "No"),
        };
    }
}
