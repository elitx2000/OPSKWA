using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;

namespace OPSKWA
{
    class MarketFeedService
    {
        private string _marketDataRefreshInterval;
        public MarketFeedService()
        {
            _marketDataRefreshInterval = ConfigurationManager.AppSettings["MarketFeedService.DefaultInterval"];
        }
    }
}
