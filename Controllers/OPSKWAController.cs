using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace OPSKWA
{
    class OPSKWAController
    {
        private MainWindow _mainWindow;
        private LLMCMDService _llmcmdService;
        private MarketFeedService _marketFeedService;
        private LogFeedService _logFeedService;

        public OPSKWAController(MainWindow startUpView)
        {
            _mainWindow = startUpView as MainWindow;

            _llmcmdService = new LLMCMDService(startUpView.LlmCmd_RTX);
            _marketFeedService = new MarketFeedService(startUpView.MarketFeed_RTX);
            _logFeedService = new LogFeedService(startUpView.Log_RTX);

            _llmcmdService.ConnectRequested += OnConnectRequested;
            _llmcmdService.DisconnectRequested += OnDisconnectRequested;
            _marketFeedService.MarketDataUpdated += OnMarketDataUpdated;
            _marketFeedService.UpdateError += OnMarketUpdateError;
        }
        public void Initialize()
        {
            // Any synchronous initialization
        }

        private async void OnConnectRequested(object sender, EventArgs e)
        {
            try
            {
                bool success = await _marketFeedService.ConnectAsync();
                if (success)
                {
                    _llmcmdService.OnConnectSuccess();
                }
            }
            catch (Exception ex)
            {
                _llmcmdService.OnConnectFailed(ex.Message);
            }
        }

        private void OnDisconnectRequested(object sender, EventArgs e)
        {
            _marketFeedService.Disconnect();
            _llmcmdService.OnDisconnectSuccess();
        }

        private void OnMarketDataUpdated(object sender, MarketDataEventArgs e)
        {
            // TODO: Update UI with market data
            // parse the JsonDocuments here and update your MainWindow
            // For example:
            // _mainWindow.UpdateMarketDisplay(e.BtcData, e.EthData, e.XrpData);
        }

        private void OnMarketUpdateError(object sender, Exception e)
        {
            // TODO: Log errors to LogFeedService
            // For example:
            // _logFeedService.LogError($"Market update error: {e.Message}");
        }
    }
}
