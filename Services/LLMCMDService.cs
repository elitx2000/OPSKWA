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

            _terminalUtility.OnCommandEntered += HandleCommand;
            //Temp:
            _terminalUtility.OnLLMPromptEntered += HandlePrompt;
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
        private void HandleCommand(string userInput)
        {
            var command = userInput.TrimStart('/').ToLower();

            if (!_commands.Contains(userInput))
            {
                _terminalUtility.WriteError($"Unknown command: {userInput}");
                _terminalUtility.WriteInfo("Type /help for available commands");
                return;
            }

            switch (command)
            {
                case "help":
                    _terminalUtility.WriteInfo("Available commands:");
                    foreach (var cmd in _commands)
                    {
                        _terminalUtility.WriteSuccess($"  {cmd}");
                    }
                    break;

                case "clearfeed":
                    _terminalUtility.WriteSuccess("Feed cleared");
                    break;

                case "clearllm":
                    _terminalUtility.WriteSuccess("LLM output cleared");
                    break;

                case "clearcmd":
                    _terminalUtility.Clear();
                    return; // Clear() already calls ShowPrompt, so return early

                case "clearlog":
                    _terminalUtility.WriteSuccess("Logs cleared");
                    break;

                case "setupdate":
                    _terminalUtility.WriteSuccess("Running setup update...");
                    break;

                default:
                    _terminalUtility.WriteError($"Command '{command}' recognized but not implemented yet");
                    break;
            }
            _terminalUtility.ShowPromptWithoutClear();
        }
        private void HandlePrompt(string prompt)
        {
            // Temp handler for LLM prompts
            _terminalUtility.WriteInfo($"LLM Prompt received: {prompt}");
            _terminalUtility.ShowPromptWithoutClear();
        }
    }
}
