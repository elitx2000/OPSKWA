using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Windows.Controls;

namespace OPSKWA
{
    class LLMCMDService
    {
        private List<String> _commands;
        private TerminalUtility _terminalUtility;
        public LLMCMDService(System.Windows.Controls.RichTextBox llmCmd_RTX) 
        {
            _commands = new List<String>();
            _terminalUtility = new TerminalUtility(llmCmd_RTX);
            setCommands();

        }

        public void setCommands()
        {
            var cmds = ConfigurationManager.AppSettings["LLMCMDService.Commands"];
            if (!string.IsNullOrEmpty(cmds))
            {
                _commands = cmds.Split('|')
                                .Select(c => c.Trim())
                                .ToList();
            }
        }
        public List<String> getCommands()
        {
            return _commands;
        }
    }
}
