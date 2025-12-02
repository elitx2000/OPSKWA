using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace OPSKWA
{
    public class MarketDataEventArgs : EventArgs
    {
        public JsonDocument XrpData { get; set; }
    }
    class MarketFeedService
    {
        private string _marketDataRefreshInterval;
        private CryptoService _cryptoService;
        private bool _isConnected;
        private System.Threading.Timer _updateTimer;
        private string _tokenTickerSymbol = "XRPUSDT";
        private readonly System.Windows.Controls.RichTextBox _marketTerminal;
        public event EventHandler<MarketDataEventArgs> MarketDataUpdated;
        public event EventHandler<Exception> UpdateError;

        public MarketFeedService(System.Windows.Controls.RichTextBox marketTerminal)
        {
            _marketTerminal = marketTerminal;
            _marketDataRefreshInterval = ConfigurationManager.AppSettings["MarketFeedService.DefaultInterval"];
            _tokenTickerSymbol = ConfigurationManager.AppSettings["MarketFeedService.TokenTickerSymbol"] ?? "XRPUSDT";
            _cryptoService = new CryptoService();
        }
        protected virtual void OnMarketDataUpdated(JsonDocument xrp)
        {
            MarketDataUpdated?.Invoke(this, new MarketDataEventArgs { XrpData = xrp });
        }

        protected virtual void OnUpdateError(Exception ex)
        {
            UpdateError?.Invoke(this, ex);
        }

        public async Task<bool> ConnectAsync()
        {
            if (_isConnected)
            {
                return true;
            }

            try
            {
                var testData = await _cryptoService.GetTickerAsync(_tokenTickerSymbol);
                _isConnected = true;

                // Start periodic updates
                int intervalMs = int.Parse(_marketDataRefreshInterval ?? "10000");
                _updateTimer = new System.Threading.Timer(
                    async _ => await UpdateMarketDataAsync(),
                    null,
                    0,
                    intervalMs
                );

                return true;
            }
            catch (Exception ex)
            {
                _isConnected = false;
                throw new Exception($"Failed to connect to Binance: {ex.Message}", ex);
            }
        }
        private async Task UpdateMarketDataAsync()
        {
            if (!_isConnected) return;

            try
            {
                var tokenData = await _cryptoService.GetTickerAsync(_tokenTickerSymbol);

                WriteMarketUpdate(tokenData);

                OnMarketDataUpdated(tokenData);
            }
            catch (Exception ex)
            {
                // Handle update errors without disconnecting
                WriteError($"Market update failed: {ex.Message}");
                OnUpdateError(ex);
            }
        }

        public void Disconnect()
        {
            _updateTimer?.Dispose();
            _updateTimer = null;
            _isConnected = false;
        }

        private void WriteMarketUpdate(JsonDocument data)
        {
            _marketTerminal.Dispatcher.Invoke(() =>
            {
                try
                {
                    var root = data.RootElement;

                    string symbol = root.GetProperty("symbol").GetString();
                    string lastPrice = root.GetProperty("lastPrice").GetString();
                    string priceChange = root.GetProperty("priceChange").GetString();
                    string priceChangePercent = root.GetProperty("priceChangePercent").GetString();
                    string volume = root.GetProperty("volume").GetString();
                    string highPrice = root.GetProperty("highPrice").GetString();
                    string lowPrice = root.GetProperty("lowPrice").GetString();

                    // Parse for color coding
                    decimal change = decimal.Parse(priceChange);
                    bool isPositive = change >= 0;

                    // Create formatted output
                    var timestamp = DateTime.Now.ToString("HH:mm:ss");
                    var paragraph = new Paragraph();

                    // Timestamp
                    paragraph.Inlines.Add(new Run($"[{timestamp}] ") { Foreground = System.Windows.Media.Brushes.Gray });

                    // Symbol
                    paragraph.Inlines.Add(new Run($"{symbol} ") { Foreground = System.Windows.Media.Brushes.Cyan, FontWeight = FontWeights.Bold });

                    // Price
                    paragraph.Inlines.Add(new Run($"${lastPrice} ") { Foreground = System.Windows.Media.Brushes.White, FontWeight = FontWeights.Bold });

                    // Price change with color
                    var changeColor = isPositive ? System.Windows.Media.Brushes.LimeGreen : System.Windows.Media.Brushes.Red;
                    var changeSymbol = isPositive ? "▲" : "▼";
                    paragraph.Inlines.Add(new Run($"{changeSymbol} {priceChangePercent}% ") { Foreground = changeColor });

                    // 24h High/Low
                    paragraph.Inlines.Add(new Run($"H: ${highPrice} L: ${lowPrice} ") { Foreground = System.Windows.Media.Brushes.Gray });

                    // Volume
                    paragraph.Inlines.Add(new Run($"Vol: {decimal.Parse(volume):N2}") { Foreground = System.Windows.Media.Brushes.Yellow });

                    _marketTerminal.Document.Blocks.Add(paragraph);

                    // Auto-scroll to bottom
                    _marketTerminal.ScrollToEnd();

                    // Optional: Limit number of lines to prevent memory issues
                    if (_marketTerminal.Document.Blocks.Count > 100)
                    {
                        _marketTerminal.Document.Blocks.Remove(_marketTerminal.Document.Blocks.FirstBlock);
                    }
                }
                catch (Exception ex)
                {
                    WriteError($"Failed to parse market data: {ex.Message}");
                }
            });
        }
        private void WriteError(string message)
        {
            _marketTerminal.Dispatcher.Invoke(() =>
            {
                var timestamp = DateTime.Now.ToString("HH:mm:ss");
                var paragraph = new Paragraph();
                paragraph.Inlines.Add(new Run($"[{timestamp}] ") { Foreground = System.Windows.Media.Brushes.Gray });
                paragraph.Inlines.Add(new Run($"Error: {message}") { Foreground = System.Windows.Media.Brushes.Red });
                _marketTerminal.Document.Blocks.Add(paragraph);
                _marketTerminal.ScrollToEnd();
            });
        }
    }
}
