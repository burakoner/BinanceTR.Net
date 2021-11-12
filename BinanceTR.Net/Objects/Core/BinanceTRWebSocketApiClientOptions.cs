using System;
using System.Net.Http;
using BinanceTR.Net.Enums;
using BinanceTR.Net.Interfaces;
using CryptoExchange.Net.Objects;

namespace BinanceTR.Net.Objects.Core
{
    public class BinanceTRWebSocketApiClientOptions : SocketClientOptions
    {
        private string _socketCloudAddress;
        private string _socketLocalAddress;

        public string SocketCloudAddress
        {
            get => _socketCloudAddress;
            set
            {
                var newValue = value;
                if (newValue != null && !newValue.EndsWith("/"))
                    newValue += "/";
                _socketCloudAddress = newValue;
            }
        }

        public string SocketLocalAddress
        {
            get => _socketLocalAddress;
            set
            {
                var newValue = value;
                if (newValue != null && !newValue.EndsWith("/"))
                    newValue += "/";
                _socketLocalAddress = newValue;
            }
        }

        public BinanceTRWebSocketApiClientOptions() : this(BinanceTRApiAddresses.Default)
        {
        }

        public BinanceTRWebSocketApiClientOptions(BinanceTRApiAddresses addresses) : this(addresses.SocketCloudAddress, addresses.SocketLocalAddress)
        {
        }

        public BinanceTRWebSocketApiClientOptions(string socketCloudAddress, string socketLocalAddress) : base(socketCloudAddress)
        {
            SocketCloudAddress = socketCloudAddress;
            SocketLocalAddress = socketLocalAddress;
            SocketSubscriptionsCombineTarget = 10;
        }

        public BinanceTRWebSocketApiClientOptions Copy()
        {
            var copy = Copy<BinanceTRWebSocketApiClientOptions>();
            copy.SocketCloudAddress = SocketCloudAddress;
            copy.SocketLocalAddress = SocketLocalAddress;
            return copy;
        }
    }
}
