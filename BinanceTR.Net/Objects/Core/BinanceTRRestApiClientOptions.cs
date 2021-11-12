using System;
using System.Net.Http;
using BinanceTR.Net.Enums;
using BinanceTR.Net.Interfaces;
using CryptoExchange.Net.Objects;

namespace BinanceTR.Net.Objects.Core
{
    public class BinanceTRRestApiClientOptions : RestClientOptions
    {
        private string _restApiMainAddress;
        private string _restApiNextAddress;

        public string RestApiMainAddress
        {
            get => _restApiMainAddress;
            set
            {
                var newValue = value;
                if (newValue != null && !newValue.EndsWith("/"))
                    newValue += "/";
                _restApiMainAddress = newValue;
            }
        }

        public string RestApiNextAddress
        {
            get => _restApiNextAddress;
            set
            {
                var newValue = value;
                if (newValue != null && !newValue.EndsWith("/"))
                    newValue += "/";
                _restApiNextAddress = newValue;
            }
        }

        /// <summary>
        /// Whether or not to automatically sync the local time with the server time
        /// </summary>
        public bool AutoTimestamp { get; set; } = true;

        /// <summary>
        /// Interval for refreshing the auto timestamp calculation
        /// </summary>
        public TimeSpan AutoTimestampRecalculationInterval { get; set; } = TimeSpan.FromHours(3);

        /// <summary>
        /// A manual offset for the timestamp. Should only be used if AutoTimestamp and regular time synchronization on the OS is not reliable enough
        /// </summary>
        public TimeSpan TimestampOffset { get; set; } = TimeSpan.Zero;

        /// <summary>
        /// Whether to check the trade rules when placing new orders and what to do if the trade isn't valid
        /// </summary>
        public TradeRulesBehaviour TradeRulesBehaviour { get; set; } = TradeRulesBehaviour.None;
        /// <summary>
        /// How often the trade rules should be updated. Only used when TradeRulesBehaviour is not None
        /// </summary>
        public TimeSpan TradeRulesUpdateInterval { get; set; } = TimeSpan.FromMinutes(60);

        /// <summary>
        /// The default receive window for requests
        /// </summary>
        public TimeSpan ReceiveWindow { get; set; } = TimeSpan.FromSeconds(5);
        
        /// <summary>
        /// Constructor with default endpoints
        /// </summary>
        public BinanceTRRestApiClientOptions(): this(BinanceTRApiAddresses.Default)
        {
        }

        /// <summary>
        /// Constructor with default endpoints
        /// </summary>
        /// <param name="client">HttpClient to use for requests from this client</param>
        public BinanceTRRestApiClientOptions(HttpClient client) : this(BinanceTRApiAddresses.Default, client)
        {
        }

        /// <summary>
        /// Constructor with custom endpoints
        /// </summary>
        /// <param name="addresses">The base addresses to use</param>
        public BinanceTRRestApiClientOptions(BinanceTRApiAddresses addresses) : this(addresses.RestApiMainAddress, addresses.RestApiNextAddress, null)
        {
        }


        /// <summary>
        /// Constructor with custom endpoints
        /// </summary>
        /// <param name="addresses">The base addresses to use</param>
        /// <param name="client">HttpClient to use for requests from this client</param>
        public BinanceTRRestApiClientOptions(BinanceTRApiAddresses addresses, HttpClient client) : this(addresses.RestApiMainAddress, addresses.RestApiNextAddress, client)
        {
        }

        public BinanceTRRestApiClientOptions(string restApiBaseAddress, string restApiSomeAddress) : this(restApiBaseAddress, restApiSomeAddress, null)
        {
        }

        public BinanceTRRestApiClientOptions(string restApiBaseAddress, string restApiSomeAddress, HttpClient client) : base(restApiBaseAddress)
        {
            HttpClient = client;
            RestApiMainAddress = restApiBaseAddress;
            RestApiNextAddress = restApiSomeAddress;
        }

        /// <summary>
        /// Return a copy of these options
        /// </summary>
        /// <returns></returns>
        public BinanceTRRestApiClientOptions Copy()
        {
            var copy = Copy<BinanceTRRestApiClientOptions>();
            copy.AutoTimestamp = AutoTimestamp;
            copy.AutoTimestampRecalculationInterval = AutoTimestampRecalculationInterval;
            copy.TimestampOffset = TimestampOffset;
            copy.TradeRulesBehaviour = TradeRulesBehaviour;
            copy.TradeRulesUpdateInterval = TradeRulesUpdateInterval;
            copy.ReceiveWindow = ReceiveWindow;
            copy.RestApiMainAddress = RestApiMainAddress;
            copy.RestApiNextAddress = RestApiNextAddress;
            return copy;
        }
    }
}
