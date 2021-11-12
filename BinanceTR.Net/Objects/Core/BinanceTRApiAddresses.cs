namespace BinanceTR.Net.Objects.Core
{
    public class BinanceTRApiAddresses
    {
        public string RestApiMainAddress { get; set; }
        public string RestApiNextAddress { get; set; }
        public string SocketCloudAddress { get; set; }
        public string SocketLocalAddress { get; set; }

        public static BinanceTRApiAddresses Default = new BinanceTRApiAddresses
        {
            RestApiMainAddress = "https://www.trbinance.com",
            RestApiNextAddress = "https://api.binance.me",

            SocketCloudAddress = "wss://stream-cloud.trbinance.com/",
            SocketLocalAddress = "wss://www.trbinance.com/",
        };
    }
}
