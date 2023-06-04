using System;
using BepInEx.Logging;
using UnityEngine;
using UnityEngine.UIElements;
using static SpaceWarp.UI.Console.SpaceWarpConsoleLogListener;

namespace SpaceWarp.UI.Console
{

    internal class LogEntry : BindableElement
    {
        private const string USSClassName = "spacewarp-logEntry";
        private const string USSLogEntryElementClassName = USSClassName + "-element";
        private const string USSHeaderGrouperClassName = USSClassName + "__headerContent";
        private const string USSTimeDateClassName = USSClassName + "__timeDate";
        private const string USSLogLevelClassName = USSClassName + "__logLevel";
        private const string USSLogSourceClassName = USSClassName + "__logSource";
        private const string USSLogMessageHeaderClassName = USSClassName + "__logMessage";

        private const string USSMessageGrouperClassName = USSClassName + "__messageContent";
        private const string USSMessageClassName = USSClassName + "__messageLabel";

        // internal static int MaxHeaderLength = 117;

        private readonly Label _timeDateLabel;

        internal DateTime TimeDate
        {
            get => _timeDate;
            set
            {
                _timeDate = value;
                if (SpaceWarpPlugin.Instance.ConfigShowTimeStamps.Value)
                    _timeDateLabel.text = value.ToString(SpaceWarpPlugin.Instance.ConfigTimeStampFormat.Value);
                else
                    _timeDateLabel.style.display = DisplayStyle.None;
            }
        }
        private DateTime _timeDate;

        private readonly Label _logLevelLabel;

        internal LogLevel LogLevel
        {
            get => _logLevel;
            private set
            {
                if (value == _logLevel) return;
                _logLevel = value;
                _logLevelLabel.text = $"[{_logLevel.ToString()}\t:";
            }
        }
        private LogLevel _logLevel = LogLevel.All;

        private readonly Label _logSourceLabel;
        public ILogSource LogSource
        {
            get => _logSource;
            private set
            {
                if (value == _logSource) return;
                _logSource = value;
                _logSourceLabel.text = $"{_logSource.SourceName}]";
            }
        }
        private ILogSource _logSource;

        private readonly Label _logMessageHeaderLabel;

        private string LogMessageHeader => LogMessage;
        private object _logData;

        private readonly Label _logMessageLabel;

        public string LogMessage => LogData.ToString();

        private object LogData
        {
            get => _logData;
            set
            {
                _logData = value;
                _logMessageLabel.text = $" {LogMessage}";
                _logMessageHeaderLabel.text = LogMessageHeader;
            }
        }

        private bool Expanded
        {
            get => _expanded;
            set
            {
                _expanded = value;
                if (value)
                {
                    _logMessageLabel.style.display = DisplayStyle.Flex;
                }
                else
                {
                    _logMessageLabel.style.display = DisplayStyle.None;
                }
                _logMessageHeaderLabel.text = LogMessageHeader;
            }
        }
        private bool _expanded;

        public Color TextColor
        {
            get => _textColor;
            set
            {
                    _textColor = value;
                    UpdateLabels();
            }
        }

        private void UpdateLabels()
        {
            //TimeDateLabel.style.color = TextColor;
            _logLevelLabel.style.color = TextColor;
            _logSourceLabel.style.color = TextColor;
            _logMessageHeaderLabel.style.color = TextColor;
            _logMessageLabel.style.color = TextColor;
        }

        private Color _textColor = Color.white;

        public LogEntry(LogInfo logInfo, bool startCollapsed = true)
        {
            AddToClassList(USSClassName);

            var headerGrouper = new VisualElement()
            {
                name = "console-LogEntry-headerGroup"
            };
            headerGrouper.AddToClassList(USSHeaderGrouperClassName);
            headerGrouper.AddToClassList(USSLogEntryElementClassName);

            hierarchy.Add(headerGrouper);

            _timeDateLabel = new Label()
            {
                name = "console-LogEntry-timeDate"
            };

            _timeDateLabel.AddToClassList(USSTimeDateClassName);
            _timeDateLabel.AddToClassList(USSLogEntryElementClassName);
            _logLevelLabel = new Label()
            {
                name = "console-LogEntry-logLevel"
            };

            _logLevelLabel.AddToClassList(USSLogLevelClassName);
            _logLevelLabel.AddToClassList(USSLogEntryElementClassName);

            _logSourceLabel = new Label()
            {
                name = "console-LogEntry-logSource"
            };

            _logSourceLabel.AddToClassList(USSLogSourceClassName);
            _logSourceLabel.AddToClassList(USSLogEntryElementClassName);

            _logMessageHeaderLabel = new Label()
            {
                name = "console-LogEntry-logMessageHeader"
            };

            _logMessageHeaderLabel.AddToClassList(USSLogMessageHeaderClassName);
            _logMessageHeaderLabel.AddToClassList(USSLogEntryElementClassName);

            var messageGrouper = new VisualElement()
            {
                name = "console-LogEntry-messageGroup"
            };
            messageGrouper.AddToClassList(USSMessageGrouperClassName);
            messageGrouper.AddToClassList(USSLogEntryElementClassName);
            hierarchy.Add(messageGrouper);

            _logMessageLabel = new Label()
            {
                name = "console-LogEntry-logMessage"
            };

            _logMessageLabel.AddToClassList(USSMessageClassName);
            _logMessageLabel.AddToClassList(USSLogEntryElementClassName);

            headerGrouper.hierarchy.Add(_timeDateLabel);
            headerGrouper.hierarchy.Add(_logLevelLabel);
            headerGrouper.hierarchy.Add(_logSourceLabel);
            headerGrouper.hierarchy.Add(_logMessageHeaderLabel);

            messageGrouper.hierarchy.Add(_logMessageLabel);

            TimeDate = logInfo.DateTime;
            LogSource = logInfo.Source;
            LogLevel = logInfo.Level;
            LogData = logInfo.Data;
            Expanded = !startCollapsed;
        }

        public override void HandleEvent(EventBase evt)
        {
            if(evt is ClickEvent)
            {
                Expanded = !Expanded;
            }
        }
    }
}