using System;
using BepInEx.Logging;
using UnityEngine;
using UnityEngine.UIElements;
using static SpaceWarp.UI.Console.SpaceWarpConsoleLogListener;

namespace SpaceWarp.UI.Console
{

    public class LogEntry : BindableElement
    {
        public static readonly string ussClassName = "spacewarp-logEntry";
        public static readonly string ussLogEntryElementClassName = ussClassName + "-element";
        public static readonly string ussHeaderGrouperClassName = ussClassName + "__headerContent";
        public static readonly string ussTimeDateClassName = ussClassName + "__timeDate";
        public static readonly string ussLogLevelClassName = ussClassName + "__logLevel";
        public static readonly string ussLogSourceClassName = ussClassName + "__logSource";
        public static readonly string ussLogMessageHeaderClassName = ussClassName + "__logMessage";

        public static readonly string ussMessageGrouperClassName = ussClassName + "__messageContent";
        public static readonly string ussMessageClassName = ussClassName + "__messageLabel";

        public static int MaxHeaderLenght = 117;

        public VisualElement HeaderGrouper;
        public Label TimeDateLabel;
        public DateTime TimeDate
        {
            get => _timeDate;
            set
            {
                _timeDate = value;
                if (SpaceWarpPlugin.Instance.ConfigShowTimeStamps.Value)
                    TimeDateLabel.text = value.ToString(SpaceWarpPlugin.Instance.ConfigTimeStampFormat.Value);
                else
                    TimeDateLabel.style.display = DisplayStyle.None;
            }
        }
        private DateTime _timeDate;

        public Label LogLevelLabel;
        public LogLevel logLevel
        {
            get => _logLevel;
            set
            {
                if (value != _logLevel)
                {
                    _logLevel = value;
                    LogLevelLabel.text = $"[{_logLevel.ToString()}\t:";
                }
            }
        }
        private LogLevel _logLevel = LogLevel.All;

        public Label LogSourceLabel;
        public ILogSource LogSource
        {
            get => _logSource;
            set
            {
                if (value != _logSource)
                {
                    _logSource = value;
                    LogSourceLabel.text = $"{_logSource.SourceName}]";
                }
            }
        }
        private ILogSource _logSource;

        public Label LogMessageHeaderLabel;
        public string LogMessageHeader
        {
            get
            {
                    return LogMessage;
            }
        }
        private object _logData;

        public VisualElement MessageGrouper;
        public Label LogMessageLabel;

        public string LogMessage => LogData.ToString();

        public object LogData
        {
            get => _logData;
            set
            {
                _logData = value;
                LogMessageLabel.text = $" {LogMessage}";
                LogMessageHeaderLabel.text = LogMessageHeader;
            }
        }
        public bool Expanded
        {
            get => _expanded;
            set
            {
                _expanded = value;
                if (value)
                {
                    LogMessageLabel.style.display = DisplayStyle.Flex;
                }
                else
                {
                    LogMessageLabel.style.display = DisplayStyle.None;
                }
                LogMessageHeaderLabel.text = LogMessageHeader;
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
            LogLevelLabel.style.color = TextColor;
            LogSourceLabel.style.color = TextColor;
            LogMessageHeaderLabel.style.color = TextColor;
            LogMessageLabel.style.color = TextColor;
        }

        private Color _textColor = Color.white;

        public LogEntry(LogInfo logInfo, bool startCollapsed = true)
        {
            AddToClassList(ussClassName);

            HeaderGrouper = new VisualElement()
            {
                name = "console-LogEntry-headerGroup"
            };
            HeaderGrouper.AddToClassList(ussHeaderGrouperClassName);
            HeaderGrouper.AddToClassList(ussLogEntryElementClassName);

            hierarchy.Add(HeaderGrouper);

            TimeDateLabel = new Label()
            {
                name = "console-LogEntry-timeDate"
            };

            TimeDateLabel.AddToClassList(ussTimeDateClassName);
            TimeDateLabel.AddToClassList(ussLogEntryElementClassName);
            LogLevelLabel = new Label()
            {
                name = "console-LogEntry-logLevel"
            };

            LogLevelLabel.AddToClassList(ussLogLevelClassName);
            LogLevelLabel.AddToClassList(ussLogEntryElementClassName);

            LogSourceLabel = new Label()
            {
                name = "console-LogEntry-logSource"
            };

            LogSourceLabel.AddToClassList(ussLogSourceClassName);
            LogSourceLabel.AddToClassList(ussLogEntryElementClassName);

            LogMessageHeaderLabel = new Label()
            {
                name = "console-LogEntry-logMessageHeader"
            };

            LogMessageHeaderLabel.AddToClassList(ussLogMessageHeaderClassName);
            LogMessageHeaderLabel.AddToClassList(ussLogEntryElementClassName);

            MessageGrouper = new VisualElement()
            {
                name = "console-LogEntry-messageGroup"
            };
            MessageGrouper.AddToClassList(ussMessageGrouperClassName);
            MessageGrouper.AddToClassList(ussLogEntryElementClassName);
            hierarchy.Add(MessageGrouper);

            LogMessageLabel = new Label()
            {
                name = "console-LogEntry-logMessage"
            };

            LogMessageLabel.AddToClassList(ussMessageClassName);
            LogMessageLabel.AddToClassList(ussLogEntryElementClassName);

            HeaderGrouper.hierarchy.Add(TimeDateLabel);
            HeaderGrouper.hierarchy.Add(LogLevelLabel);
            HeaderGrouper.hierarchy.Add(LogSourceLabel);
            HeaderGrouper.hierarchy.Add(LogMessageHeaderLabel);

            MessageGrouper.hierarchy.Add(LogMessageLabel);

            this.TimeDate = logInfo.dateTime;
            this.LogSource = logInfo.Source;
            this.logLevel = logInfo.Level;
            this.LogData = logInfo.Data;
            this.Expanded = !startCollapsed;
        }

        public override void HandleEvent(EventBase evt)
        {
            if(evt is ClickEvent clickEvent)
            {
                this.Expanded = !this.Expanded;
            }
        }
    }
}