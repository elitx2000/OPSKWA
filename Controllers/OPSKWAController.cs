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
            _marketFeedService = new MarketFeedService();
            _logFeedService = new LogFeedService();
        }
        public void Initilize()
        {
        }
    }
}
