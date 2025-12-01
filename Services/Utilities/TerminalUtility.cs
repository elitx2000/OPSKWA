using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace OPSKWA
{
    public class TerminalUtility
    {
        private System.Windows.Controls.RichTextBox _terminal;
        private string _prompt = "=> ";
        private int _commandStartPosition;

        public event Action<string>? OnCommandEntered;
        public event Action<string>? OnLLMPromptEntered;

        private System.Windows.Media.SolidColorBrush _systemColor;
        private System.Windows.Media.SolidColorBrush _userColor;
        private System.Windows.Media.SolidColorBrush _llmColor;
        private System.Windows.Media.SolidColorBrush _errBrushColor;

        public TerminalUtility(System.Windows.Controls.RichTextBox terminal)
        {
            _terminal = terminal;
            SetupEventHandlers();
            SetupColorsFromConfig();
        }
        public void Initialize()
        {
            ShowPrompt();
        }

        private void SetupEventHandlers()
        {
            _terminal.PreviewKeyDown += new System.Windows.Input.KeyEventHandler(OnPreviewKeyDown);
        }
        private void SetupColorsFromConfig()
        {
            var tempSysColor = ConfigurationManager.AppSettings["SystemColor"];
            var tempUserColor = ConfigurationManager.AppSettings["UserColor"];
            var tempLLMColor = ConfigurationManager.AppSettings["LLMColor"];
            var tempSystemColor = ParseRgbToColor(tempSysColor ?? string.Empty);
            var tempUsrColor = ParseRgbToColor(tempUserColor ?? string.Empty);
            var tempLlmColor = ParseRgbToColor(tempLLMColor ?? string.Empty);

            _systemColor = new SolidColorBrush(tempSystemColor);
            _userColor = new SolidColorBrush(tempUsrColor);
            _llmColor = new SolidColorBrush(tempLlmColor);
            _errBrushColor = new SolidColorBrush(Colors.Red);
        }
        private void OnPreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                e.Handled = true;
                var userInput = GetCurrentCommand();

                if (string.IsNullOrWhiteSpace(userInput))
                {
                    ShowPromptWithoutClear();
                    return;
                }

                if (userInput.StartsWith("/"))
                {
                    ProcessCommand(userInput);
                }
                else
                {
                    ProcessLLMPrompt(userInput);
                }
                return;
            }
            if (e.Key == Key.Back)
            {
                if (IsCaretAtPrompt())
                {
                    e.Handled = true;
                    return;
                }
            }
            if (e.Key == Key.Left)
            {
                if (IsCaretAtPrompt())
                {
                    e.Handled = true;
                    return;
                }
            }
            EnsureCaretAfterPrompt();
        }
        public void ShowPrompt()
        {
            if (_terminal.Document.Blocks.Count > 0)
            {
                _terminal.Document.Blocks.Clear();
            }
            var paragraph = new Paragraph();
            paragraph.Margin = new System.Windows.Thickness(0);
            var promptRun = new Run(_prompt) { Foreground = _systemColor};
            paragraph.Inlines.Add(promptRun);

            _terminal.Document.Blocks.Add(paragraph);
            _terminal.CaretPosition = _terminal.Document.ContentEnd;
            _commandStartPosition = _terminal.Document.ContentStart.GetOffsetToPosition(_terminal.CaretPosition);
        }
        public void ShowPromptWithoutClear()
        {
            var paragraph = new Paragraph();
            paragraph.Margin = new System.Windows.Thickness(0);
            var promptRun = new Run(_prompt) { Foreground = _systemColor};
            paragraph.Inlines.Add(promptRun);

            _terminal.Document.Blocks.Add(paragraph);
            _terminal.CaretPosition = _terminal.Document.ContentEnd;
            _commandStartPosition = _terminal.Document.ContentStart.GetOffsetToPosition(_terminal.CaretPosition);
        }
        private string GetCurrentCommand()
        {
            var paragraph = _terminal.Document.Blocks.LastBlock as Paragraph;
            if (paragraph == null) return string.Empty;

            var text = new TextRange(paragraph.ContentStart, paragraph.ContentEnd).Text;

            if (text.StartsWith(_prompt))
            {
                return text.Substring(_prompt.Length).Trim();
            }

            return text.Trim();
        }
        private void ProcessCommand(string command)
        {
            if (string.IsNullOrWhiteSpace(command))
            {
                ShowPromptWithoutClear();
                return;
            }

            OnCommandEntered?.Invoke(command);

            ShowPromptWithoutClear();
        }
        private void ProcessLLMPrompt(string prompt)
        {
            OnLLMPromptEntered?.Invoke(prompt);
        }
        public void WriteOutput(string text, System.Windows.Media.SolidColorBrush? color = null)
        {
            var paragraph = new Paragraph();
            paragraph.Margin = new System.Windows.Thickness(0);
            var run = new Run(text)
            {
                Foreground = color
            };
            paragraph.Inlines.Add(run);

            _terminal.Document.Blocks.Add(paragraph);

            _terminal.CaretPosition = _terminal.Document.ContentEnd;
        }
        public void WriteError(string text)
        {
            WriteOutput(text, _errBrushColor);
        }
        public void WriteSuccess(string text)
        {
            WriteOutput(text, _systemColor);
        }
        public void WriteInfo(string text)
        {
            WriteOutput(text, _systemColor);
        }
        public void WriteLLM(string text)
        {
            WriteOutput(text, _llmColor);
        }
        public void Clear()
        {
            _terminal.Document.Blocks.Clear();
        }
        private bool IsCaretAtPrompt()
        {
            var paragraph = _terminal.Document.Blocks.LastBlock as Paragraph;
            if (paragraph == null) return false;

            var caretOffset = paragraph.ContentStart.GetOffsetToPosition(_terminal.CaretPosition);
            return caretOffset <= _prompt.Length;
        }
        private void EnsureCaretAfterPrompt()
        {
            if (IsCaretAtPrompt())
            {
                var paragraph = _terminal.Document.Blocks.LastBlock as Paragraph;
                if (paragraph != null)
                {
                    _terminal.CaretPosition = paragraph.ContentStart.GetPositionAtOffset(_prompt.Length);
                }
            }
        }
        private System.Windows.Media.Color ParseRgbToColor(string rgbString)
        {
            if (string.IsNullOrEmpty(rgbString))
                return Colors.White;

            var rgb = rgbString.Split(',').Select(c => byte.Parse(c.Trim())).ToArray();

            if (rgb.Length == 3)
            {
                return System.Windows.Media.Color.FromRgb(rgb[0], rgb[1], rgb[2]);
            }

            return Colors.White; 
        }
    }
}