﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using JetBrains.Annotations;

namespace MigSharp.Core
{
    internal enum LogCategory
    {
        General,
        Sql,
        Performance,
    }

    internal static class Log
    {
        private const SourceLevels DefaultSourceLevel = SourceLevels.Warning;

        private static readonly Dictionary<LogCategory, TraceSource> Sources = new Dictionary<LogCategory, TraceSource>();

        [SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline")]
        static Log()
        {
            foreach (LogCategory category in Enum.GetValues(typeof(LogCategory)))
            {
                var source = new TraceSource(string.Format(CultureInfo.InvariantCulture, "MigSharp.{0}", category), SourceLevels.All);
                Sources.Add(category, source);
                SetTraceLevel(category, DefaultSourceLevel);

                // copy the existing listeners from Trace (this helps for example with ReSharper and TeamCity integration, as these add their own listeners to Trace)
                source.Listeners.Clear();
                source.Listeners.AddRange(Trace.Listeners);
            }
        }

        [StringFormatMethod("format")]
        public static void Error(LogCategory category, string format, params object[] args)
        {
            TraceEvent(category, TraceEventType.Error, format, args);
        }

        [StringFormatMethod("format")]
        public static void Error(string format, params object[] args)
        {
            Error(LogCategory.General, format, args);
        }

        [StringFormatMethod("format")]
        public static void Warning(LogCategory category, string format, params object[] args)
        {
            TraceEvent(category, TraceEventType.Warning, format, args);
        }

        [StringFormatMethod("format")]
        public static void Warning(string format, params object[] args)
        {
            Warning(LogCategory.General, format, args);
        }

        [StringFormatMethod("format")]
        public static void Info(LogCategory category, string format, params object[] args)
        {
            TraceEvent(category, TraceEventType.Information, format, args);
        }

        [StringFormatMethod("format")]
        public static void Info(string format, params object[] args)
        {
            Info(LogCategory.General, format, args);
        }

        [StringFormatMethod("format")]
        public static void Verbose(LogCategory category, string format, params object[] args)
        {
            TraceEvent(category, TraceEventType.Verbose, format, args);
        }

        [StringFormatMethod("format")]
        public static void Verbose(string format, params object[] args)
        {
            Verbose(LogCategory.General, format, args);
        }

        [StringFormatMethod("format")]
        private static void TraceEvent(LogCategory category, TraceEventType traceEventType, string format, object[] args)
        {
            string msg = format;
            if (args.Length > 0) // only using string.Format when there are 'args' to inject; otherwise it might be wrong as 'format' might contain curly brackets which are part of the message itself and not place-holders
            {
                msg = string.Format(CultureInfo.CurrentCulture, format, args);
            }
            Sources[category].TraceEvent(traceEventType, 0, msg);
        }

        public static void SetTraceLevel(LogCategory category, SourceLevels sourceLevel)
        {
            var sourceSwitch = new SourceSwitch(string.Format(CultureInfo.InvariantCulture, "MigSharp.{0}.Switch", category))
            {
                Level = sourceLevel
            };
            Sources[category].Switch = sourceSwitch;
        }
    }
}