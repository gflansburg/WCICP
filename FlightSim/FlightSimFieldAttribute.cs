using System;

namespace FlightSim
{
    public enum FieldPadAlign
    {
        Left,
        Right,
        Center
    }

    /// <summary>
    /// Metadata for fields exposed by FlightSimProviderBase, used to drive custom ICP layouts.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public sealed class FlightSimFieldAttribute : Attribute
    {
        public FlightSimFieldAttribute(string friendlyName)
        {
            FriendlyName = friendlyName ?? throw new ArgumentNullException(nameof(friendlyName));
        }

        /// <summary>Human-friendly display name (e.g., "COM 1 Active Frequency").</summary>
        public string FriendlyName { get; }

        /// <summary>
        /// Optional formatting. Use either standard .NET format strings for numeric types,
        /// or custom patterns you interpret yourself (e.g., "000.00").
        /// </summary>
        public string Format { get; set; } = "";

        /// <summary>Maximum number of characters the rendered value should occupy.</summary>
        public int MaxLength { get; set; } = 0;

        /// <summary>
        /// How to pad when the formatted value is shorter than MaxLength.
        /// Default is Right for non-string/non-bool values, Left for strings.
        /// If not set explicitly, provider will choose a sensible default.
        /// </summary>
        public FieldPadAlign PadAlign { get; set; } = FieldPadAlign.Right;

        /// <summary>
        /// Character used for padding. Defaults to space.
        /// </summary>
        public char PadChar { get; set; } = ' ';

        public string TrueText { get; set; } = "YES";
        public string FalseText { get; set; } = "NO";

        /// <summary>
        /// If true and the underlying value is a bool that evaluates true,
        /// the rendered text should use the "inverted" glyph set (UI/DED highlight).
        /// </summary>
        public bool InvertWhenTrue { get; set; } = false;
    }
}
