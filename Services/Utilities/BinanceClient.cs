using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace OPSKWA.Services.Utilities
{
    internal class BinanceClient
    {
        private const string BASE_URL = "https://api.binance.us/api/v3";
        private readonly HttpClient _client;

        public BinanceClient()
        {
            _client = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(30)
            };
        }

        public async Task<JsonDocument> GetTicker(string symbol)
        {
            return await Fetch($"/ticker/24hr?symbol={symbol}");
        }

        // Candlestick/kline data - interval can be 1m, 5m, 15m, 1h, etc.
        public async Task<JsonDocument> GetKlines(string symbol, string interval, int limit)
        {
            return await Fetch($"/klines?symbol={symbol}&interval={interval}&limit={limit}");
        }

        public async Task<JsonDocument> GetOrderBook(string symbol, int limit)
        {
            return await Fetch($"/depth?symbol={symbol}&limit={limit}");
        }

        public async Task<JsonDocument> GetRecentTrades(string symbol, int limit)
        {
            return await Fetch($"/trades?symbol={symbol}&limit={limit}");
        }

        private async Task<JsonDocument> Fetch(string endpoint)
        {
            HttpResponseMessage response = await _client.GetAsync(BASE_URL + endpoint);

            if (!response.IsSuccessStatusCode)
            {
                string errorBody = await response.Content.ReadAsStringAsync();
                throw new Exception($"Binance API error: {errorBody}");
            }

            string responseBody = await response.Content.ReadAsStringAsync();
            return JsonDocument.Parse(responseBody);
        }
    }
}
