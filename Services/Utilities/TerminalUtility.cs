using System;
using System.Collections.Generic;
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

        public TerminalUtility(System.Windows.Controls.RichTextBox terminal)
        {
            _terminal = terminal;
            SetupEventHandlers();
            ShowPrompt();
        }

        private void SetupEventHandlers()
        {
            _terminal.PreviewKeyDown += new System.Windows.Input.KeyEventHandler(OnPreviewKeyDown);
        }

        private void OnPreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                e.Handled = true;
                var command = GetCurrentCommand();
                ProcessCommand(command);
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
            var promptRun = new Run(_prompt) { Foreground = System.Windows.Media.Brushes.LimeGreen };
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
                ShowPrompt();
                return;
            }

            OnCommandEntered?.Invoke(command);

            ShowPrompt();
        }

        public void WriteOutput(string text, System.Windows.Media.Brush color = null)
        {
            var paragraph = new Paragraph();
            var run = new Run(text) { Foreground = color ?? System.Windows.Media.Brushes.White };
            paragraph.Inlines.Add(run);

            var lastBlock = _terminal.Document.Blocks.LastBlock;
            _terminal.Document.Blocks.InsertBefore(lastBlock, paragraph);

            _terminal.CaretPosition = _terminal.Document.ContentEnd;
        }

        public void WriteError(string text)
        {
            WriteOutput(text, System.Windows.Media.Brushes.Red);
        }

        public void WriteSuccess(string text)
        {
            WriteOutput(text, System.Windows.Media.Brushes.LimeGreen);
        }

        public void WriteInfo(string text)
        {
            WriteOutput(text, System.Windows.Media.Brushes.Cyan);
        }

        public void Clear()
        {
            _terminal.Document.Blocks.Clear();
            ShowPrompt();
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
    }
}