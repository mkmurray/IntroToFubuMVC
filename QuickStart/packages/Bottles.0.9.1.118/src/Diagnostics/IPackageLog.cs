﻿using System;

namespace Bottles.Diagnostics
{
    public interface IPackageLog
    {
        void Trace(ConsoleColor color, string text, params object[] parameters);
        void Trace(string text, params object[] parameters);

        void MarkFailure(Exception exception);
        void MarkFailure(string text);
        string FullTraceText();
        string Description { get; }
        bool Success { get; }
        long TimeInMilliseconds { get; }
    }
}