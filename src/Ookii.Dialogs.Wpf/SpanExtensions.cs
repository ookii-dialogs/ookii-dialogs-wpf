using System;

namespace Ookii.Dialogs.Wpf
{
    internal static class SpanExtensions
    {
        /// <summary>
        /// Drops '\0' character padding before converting to string.
        /// </summary>
        /// <param name="span">Span to convert to string.</param>
        /// <returns>Resulting string.</returns>
        public static string ToCleanString(this Span<char> span) => span.Slice(0, span.IndexOf('\0')).ToString();
    }
}
