using OPSKWA.Services.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace OPSKWA
{
    class CryptoService
    {
        private BinanceClient _binanceClient;
        public CryptoService()
        {
            // Just instantiate - safe, no network calls
            _binanceClient = new BinanceClient();
        }
        public async Task<JsonDocument> GetTickerAsync(string symbol)
        {
            return await _binanceClient.GetTicker(symbol);
        }
    }
}
