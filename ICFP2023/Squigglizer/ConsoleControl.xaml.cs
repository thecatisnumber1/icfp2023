using System;
using System.Collections.Concurrent;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace ICFP2023
{
    public partial class ConsoleControl : UserControl
    {
        public TextWriter Out { get; private set; }
        public TextWriter Error { get; private set; }
        public TextReader In => inReader;

        private readonly ConcurrentQueue<OutMessage> messages;
        private readonly TextBoxReader inReader;

        public ConsoleControl()
        {
            InitializeComponent();

            Focusable = true;
            GotFocus += (sender, e) => InputBox.Focus();

            messages = new ConcurrentQueue<OutMessage>();
            Out = new QueueWriter(messages, Brushes.White, UpdateText);
            Error = new QueueWriter(messages, Brushes.Red, UpdateText);
            inReader = new TextBoxReader(this);

            Console.SetOut(Out);
            Console.SetError(Error);
            Console.SetIn(In);

            _ = new DispatcherTimer(
                    TimeSpan.FromSeconds(1.0 / 30),
                    DispatcherPriority.Normal,
                    (sender, e) => UpdateText(),
                    Dispatcher);
        }

        public string ReadLine() => In.ReadLine();

        public void UpdateText()
        {
            RunOnUIThreadSynchronous(() =>
            {
                bool updated = false;
                while (messages.TryDequeue(out OutMessage message))
                {
                    updated = true;
                    WriteFormattedText(message.text, message.color);
                }

                if (updated)
                {
                    RTBox.ScrollToEnd();
                }
            });
        }

        public void WriteFormattedText(string text, Brush color)
        {
            Paragraph.Inlines.Add(new Run(text) { Foreground = color });
        }

        private void RunOnUIThreadSynchronous(Action action)
        {
            if (Dispatcher.CheckAccess())
            {
                action();
            }
            else
            {
                Dispatcher.Invoke(action);
            }
        }

        private void InputBox_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                e.Handled = true;
                inReader.OnEnter();
            }
            else if (e.Key == Key.Escape
                || e.Key == Key.Back
                || (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.Z))
            {
                e.Handled = true;
                inReader.CancelEnter();
            }
        }

        private class QueueWriter : TextWriter
        {
            private readonly ConcurrentQueue<OutMessage> queue;
            private readonly Brush color;
            private readonly Action flusher;

            public  QueueWriter(ConcurrentQueue<OutMessage> queue, Brush color, Action flusher)
            {
                this.queue = queue;
                this.color = color;
                this.flusher = flusher;
            }

            public override Encoding Encoding => Encoding.UTF8;

            public override void Write(char message) => Write(message.ToString());

            public override void Write(string message)
            {
                queue.Enqueue(new OutMessage() { text = message, color = color});
            }

            public override void Write(char[] buffer, int index, int count)
            {
                Write(new string(buffer, index, count));
            }

            public override void Flush()
            {
                flusher();
            }
        }

        // I'm enqueueing these rather then Runs directly so that the Runs get constructed in the UI thread.
        private struct OutMessage
        {
            public string text;
            public Brush color;
        }

        private class TextBoxReader : TextReader
        {
            private readonly ConsoleControl consoleControl;
            private readonly TextBox inputBox;
            private readonly object readMutex, readWaitLock;
            private string readInput, enteredText;
            private bool waitingForEnter;
            private DispatcherTimer spinnerTimer;
            private int spinnerState;

            public TextBoxReader(ConsoleControl consoleControl)
            {
                this.consoleControl = consoleControl;
                inputBox = consoleControl.InputBox;
                readMutex = new object();
                readWaitLock = new object();
                enteredText = null;
                waitingForEnter = false;
                readInput = null;
                spinnerTimer = null;
                spinnerState = 0;
            }

            public override string ReadLine()
            {
                consoleControl.UpdateText();

                if (consoleControl.Dispatcher.CheckAccess())
                {
                    throw new Exception("Cannot block the UI thread while using the UI to get input!");
                }

                // Only one call of ReadLine can be "active" at a time
                lock (readMutex)
                {
                    lock (readWaitLock)
                    {
                        consoleControl.Dispatcher.BeginInvoke(() =>
                        {
                            lock (readWaitLock)
                            {
                                if (enteredText != null)
                                {
                                    RegisterTextEntered();
                                }
                                else
                                {
                                    waitingForEnter = true;
                                    inputBox.Focus();
                                }
                            }
                        });

                        while (readInput == null)
                        {
                            Monitor.Wait(readWaitLock);
                        }

                        string result = readInput;
                        readInput = null;
                        return result;
                    }
                }
            }

            private void RegisterTextEntered()
            {
                waitingForEnter = false;
                readInput = enteredText;
                enteredText = null;
                inputBox.Clear();
                consoleControl.WriteFormattedText(readInput + '\n', Brushes.Gray);
                AllowEdits();
                Monitor.Pulse(readWaitLock);
            }

            public void OnEnter()
            {
                consoleControl.Dispatcher.Invoke(() =>
                {
                    lock (readWaitLock)
                    {
                        if (enteredText != null)
                        {
                            return;
                        }

                        enteredText = inputBox.Text;

                        if (waitingForEnter)
                        {
                            RegisterTextEntered();
                        }
                        else
                        {
                            PreventEdits();
                        }
                    }
                });
            }

            private void PreventEdits()
            {
                inputBox.IsReadOnly = true;
                inputBox.CaretBrush = Brushes.Transparent;
                inputBox.Background = Brushes.DarkSlateGray;
                StartSpinner();
            }

            private void AllowEdits()
            {
                inputBox.IsReadOnly = false;
                inputBox.CaretBrush = Brushes.White;
                inputBox.Background = Brushes.Transparent;
                inputBox.CaretIndex = inputBox.Text.Length;
                StopSpinner();
            }

            public void CancelEnter()
            {
                lock (readWaitLock)
                {
                    if (enteredText == null)
                    {
                        return;
                    }

                    inputBox.Text = enteredText;
                    enteredText = null;
                    AllowEdits();
                }
            }

            private void StartSpinner()
            {
                spinnerState = -1;
                AdvanceSpinner();
                spinnerTimer = new DispatcherTimer(
                    TimeSpan.FromSeconds(0.1),
                    DispatcherPriority.Normal,
                    (object sender, EventArgs e) => AdvanceSpinner(),
                    consoleControl.Dispatcher);
            }

            private void AdvanceSpinner()
            {
                spinnerState = (spinnerState + 1) % 4;
                inputBox.Text = enteredText + ' ' + "|/-\\"[spinnerState];
            }

            private void StopSpinner()
            {
                spinnerTimer?.Stop();
                spinnerTimer = null;
            }
        }
    }
}
