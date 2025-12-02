using OPSKWA.Services.Utilities;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace OPSKWA
{
    class LLMCMDService
    {
        private List<String> _commands; 
        private TerminalUtility _terminalUtility;
        private OllamaClient _ollamaClient;

        public event EventHandler ConnectRequested;
        public event EventHandler DisconnectRequested;
        public LLMCMDService(System.Windows.Controls.RichTextBox llmCmd_RTX)
        {
            _commands = new List<String>();
            _terminalUtility = new TerminalUtility(llmCmd_RTX);
            _terminalUtility.Initialize();
            setCommands();

            _ollamaClient = new OllamaClient();

            _terminalUtility.OnCommandEntered += HandleCommand;
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
                case "exit":
                    _terminalUtility.WriteInfo("Exiting application...");
                    System.Threading.Thread.Sleep(500);
                    Environment.Exit(0);
                    break;
                case "help":
                    _terminalUtility.WriteInfo("AVAILABLE COMMANDS:");
                    foreach (var cmd in _commands)
                    {
                        _terminalUtility.WriteSuccess($"  {cmd}");
                    }
                    break;

                case "clearfeed":
                    _terminalUtility.WriteSuccess("Feed cleared");
                    break;

                case "clear":
                    _terminalUtility.Clear();
                    return; // Clear() already calls ShowPrompt, so return early

                case "clearlog":
                    _terminalUtility.WriteSuccess("Logs cleared");
                    break;

                case "setupdate":
                    _terminalUtility.WriteSuccess("Running setup update...");
                    break;

                case "connect":
                    _terminalUtility.WriteInfo("Connecting to Binance market feed...");
                    ConnectRequested?.Invoke(this, EventArgs.Empty);
                    break;

                case "disconnect":
                    _terminalUtility.WriteInfo("Disconnecting from market feed...");
                    DisconnectRequested?.Invoke(this, EventArgs.Empty);
                    break;

                default:
                    _terminalUtility.WriteError($"Command '{command}' recognized but not implemented yet");
                    break;
            }
            //_terminalUtility.ShowPromptWithoutClear();
        }
        private async void HandlePrompt(string prompt)
        {
            //_terminalUtility.WriteInfo($"LLM Prompt received: {prompt}");
            try
            {
                // Get AI response from Ollama
                string response = await _ollamaClient.Generate(prompt);

                // Display the AI response
                _terminalUtility.WriteLLM($"AI: {response}");
            }
            catch (HttpRequestException httpEx)
            {
                _terminalUtility.WriteError(httpEx.Message);
                _terminalUtility.WriteError($"Connection Error: Unable to reach Ollama at localhost:11434");
                _terminalUtility.WriteError($"Make sure Ollama is running.");
            }
            catch (Exception ex)
            {
                _terminalUtility.WriteError($"Error: {ex.Message}");
            }

            _terminalUtility.ShowPromptWithoutClear();
        }
        public void OnConnectSuccess()
        {
            _terminalUtility.WriteSuccess("✓ Connected to Binance market feed");
            _terminalUtility.WriteInfo("Receiving live market data for BTC, ETH, XRP");
        }

        public void OnConnectFailed(string error)
        {
            _terminalUtility.WriteError($"✗ Connection failed: {error}");
        }

        public void OnDisconnectSuccess()
        {
            _terminalUtility.WriteSuccess("✓ Disconnected from market feed");
        }
    }
}
