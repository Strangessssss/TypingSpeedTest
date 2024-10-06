
﻿using System.Globalization;
using System.Net.Mime;
using System.IO;
using System.Windows; // Namespace for basic WPF infrastructure
using System.Windows.Controls; // Namespace for all controls
using System.Windows.Documents;
using System.Windows.Input; // Namespace for all document-related controls
using System.Windows.Media; // Namespace for colors and brushes
using System.Diagnostics;


namespace TypingTest
{
    public partial class MainWindow : Window
    {
        private bool _isOn = false;
        private string _text;
        private List<string> _texts;
        private int _mistakes = 0;
        private int _startLength = 0;
        private int _currentLetterIndex = 0;
        private DateTime _startTime;
        private DateTime _endTime;
        private Stopwatch _stopwatch = new Stopwatch();
        private Brush _lastColor = new DrawingBrush();
        private Button _lastButton = new Button();
        
        Dictionary<Key, List<char>> _keyCdDictionary = new Dictionary<Key, List<char>>()
        {
            { Key.Space, [' '] },
            { Key.D0, ['0', ')'] },
            { Key.D1, ['1', '!'] },
            { Key.D2, ['2', '@'] },
            { Key.D3, ['3', '#'] },
            { Key.D4, ['4', '$'] },
            { Key.D5, ['5', '%'] },
            { Key.D6, ['6', '^'] },
            { Key.D7, ['7', '&'] },
            { Key.D8, ['8', '*'] },
            { Key.D9, ['9', '('] },
            { Key.OemMinus, ['-', '_'] },
            { Key.OemPlus, ['=', '+'] },
            { Key.OemOpenBrackets, ['[', '{'] },
            { Key.OemCloseBrackets, [']', '}'] },
            { Key.OemPipe, ['\\', '|'] },
            { Key.OemSemicolon, [';', ':'] },
            { Key.OemQuotes, ['\'', '\"'] },
            { Key.OemComma, [',', '<'] },
            { Key.OemPeriod, ['.', '>'] },
            { Key.OemQuestion, ['/', '?'] }
        };

        public MainWindow()
        {
            InitializeComponent();
            
            const string path = "Texts";
            var content = File.ReadAllText(path);
            _texts = content.Split(new[] { Environment.NewLine }, StringSplitOptions.None).ToList();
            
            _stopwatch.Restart();
        }
        
        
        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            LightButton(e.Key);
            if (_currentLetterIndex == _startLength)
            {
                Stop();
            }
            if (!_isOn) return;
            
            var curLetter = _text[_currentLetterIndex].ToString();

            switch (e.Key)
            {
                case Key.CapsLock:
                    CapsLockCheckBox.IsChecked = !CapsLockCheckBox.IsChecked;
                    return;
                case Key.LeftShift or Key.RightShift:
                    CapsLockCheckBox.IsChecked = true;
                    return;
            }

            if (!CaseSensitiveCheckBox.IsChecked!.Value)
            {
                CapsLockCheckBox.IsChecked = false;
            }
            
            if (_keyCdDictionary.ContainsKey(e.Key))
            {
                if (curLetter == " " || _keyCdDictionary[e.Key][!CapsLockCheckBox.IsChecked!.Value ? 0 : 1].ToString() == curLetter)
                    UpdateDisplay(true);
                else
                {
                    UpdateDisplay(false);
                    _mistakes++;
                }
            }
            else
            {
                if (CaseSensitiveCheckBox.IsChecked!.Value)
                {
                    if (curLetter == (CapsLockCheckBox.IsChecked!.Value ? e.Key.ToString() : e.Key.ToString().ToLower()))
                        UpdateDisplay(true);
                    else
                    {
                        UpdateDisplay(false);
                        _mistakes++;
                    }
                }
                else
                {
                    if (curLetter.ToUpper() == e.Key.ToString())
                        UpdateDisplay(true);
                    else
                    {
                        UpdateDisplay(false);
                        _mistakes++;
                    }
                }
            }
        }

        protected override void OnPreviewKeyUp(KeyEventArgs e)
        {
            if (e.Key is Key.LeftShift or Key.RightShift)
            {
                CapsLockCheckBox.IsChecked = false;
            }
        }

        private void UpdateDisplay(bool correct)
        {
            if (correct)
                _currentLetterIndex++;
            var textBlock = new TextBlock();

            const int letterSkipCount = 20;
            var run1 = new Run(_text.Substring(_currentLetterIndex >= letterSkipCount ? _currentLetterIndex-letterSkipCount: 0 , _currentLetterIndex-(_currentLetterIndex >= letterSkipCount ? _currentLetterIndex-letterSkipCount: 0)))
            {
                Foreground = (correct ? Brushes.Black: Brushes.Red)
            };
            var run2 = new Run(_text.Substring(_currentLetterIndex, _startLength-_currentLetterIndex))
            {
                Foreground = Brushes.Gray
            };
            textBlock.Inlines.Add(run1);
            textBlock.Inlines.Add(run2);
            TextLabel.Content = textBlock;
        }
        
        private void ButtonStart_OnClick(object sender, RoutedEventArgs e)
        {
            Start();
        }

        private void Start()
        {
            if (_isOn) return;
            _isOn = true;
            _mistakes = 0;
            _startTime = DateTime.Now;
            _currentLetterIndex = 0;
            _text = _texts[int.Parse(DifficultyTextBlock.Text)];
            _startLength = _text.Length;
            TextLabel.Content = _text;
            CapsLockCheckBox.IsEnabled = false;
            CaseSensitiveCheckBox.IsEnabled = false;
            DifficultySlider.IsEnabled = false;
        }

        private void ButtonStop_OnClick(object sender, RoutedEventArgs e)
        {
            Stop();
        }

        private void Stop()
        {
            if (!_isOn) return;
            _isOn = false;
            _stopwatch.Stop();

            var elapsedTime = _stopwatch.Elapsed;
            var timeInSeconds = elapsedTime.TotalSeconds;

            SpeedTextBlock.Text = (_text.Length / timeInSeconds).ToString("F2");
            FailsTextBlock.Text = _mistakes.ToString();

            CapsLockCheckBox.IsEnabled = true;
            CaseSensitiveCheckBox.IsEnabled = true;
            DifficultySlider.IsEnabled = true;
        }

        private void RangeBase_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            DifficultyTextBlock.Text = ((int)e.NewValue).ToString();
        }
        
        private void LightButton(Key key)
        {
            foreach (var grid in MainGrid.Children)
            {
                foreach (var button in ((Grid)grid).Children)
                {
                    
                    if (((Button)button).Content.ToString()!.ToUpper() == key.ToString())
                    {
                        _lastButton.Background = _lastColor;
                        _lastColor = ((Button)button).Background;
                        _lastButton = (Button)button;
                        _lastButton.Background = Brushes.White;
                    }
                    else if ("D" + ((Button)button).Content.ToString()!.ToUpper() == key.ToString())
                    {
                        _lastButton.Background = _lastColor;
                        _lastColor = ((Button)button).Background;
                        _lastButton = (Button)button;
                        _lastButton.Background = Brushes.White;
                    }
                }
                
            }
        }
    }
}
