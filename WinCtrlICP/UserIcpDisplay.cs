using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace WinCtrlICP
{
    public enum IcpItemKind
    {
        BoundField = 0,
        Label = 1
    }

    public sealed class UserIcpDisplay : INotifyPropertyChanged
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>Menu/display name (e.g., "COM/NAV", "Landing").</summary>
        private string _displayName = "New Display";

        public string DisplayName
        {
            get => _displayName;
            set
            {
                if (_displayName != value)
                {
                    _displayName = value;
                    OnPropertyChanged(nameof(DisplayName));
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        public JoystickBinding? Binding { get; set; }

        public int PageIndex { get; set; } = 0;

        public List<UserIcpDisplayItem> Items { get; set; } = new List<UserIcpDisplayItem>();

        private static (string visible, List<(int start, int len)> spans) ParseInversionMarkers(string s)
        {
            // spans are measured in VISIBLE character coordinates (markers are zero-width)
            var spans = new List<(int start, int len)>();
            if (string.IsNullOrEmpty(s)) return (string.Empty, spans);

            var sb = new System.Text.StringBuilder(s.Length);
            int visiblePos = 0;

            int? openAt = null;

            foreach (char ch in s)
            {
                if (ch == '⟦')
                {
                    // start span at current visible position
                    openAt = visiblePos;
                    continue;
                }
                if (ch == '⟧')
                {
                    if (openAt.HasValue)
                    {
                        int len = visiblePos - openAt.Value;
                        if (len > 0) spans.Add((openAt.Value, len));
                        openAt = null;
                    }
                    continue;
                }

                sb.Append(ch);
                visiblePos++;
            }

            // If someone forgot a closing marker, just ignore dangling open.
            return (sb.ToString(), spans);
        }

        public static string[] BuildIcpLines(FlightSim.FlightSimProviderBase provider, List<UserIcpDisplayItem> items)
        {
            const int ROWS = 5;
            const int COLS = 25;

            // Fixed-width buffer that contains ONLY visible characters.
            // (Do NOT write ⟦ ⟧ into the buffer; the ICP renderer treats them as zero-width markers.)
            char[][] buffer = new char[ROWS][];
            for (int y = 0; y < ROWS; y++)
                buffer[y] = Enumerable.Repeat(' ', COLS).ToArray();

            // Track inverted spans so we can inject ⟦ ⟧ after layout.
            // Stored as (row, startX, visibleLen).
            var invertedSpans = new List<(int y, int x, int len)>();

            foreach (var item in items)
            {
                if (item.Y < 0 || item.Y >= ROWS) continue;
                if (item.X < 0 || item.X >= COLS) continue;

                string raw = item.Kind switch
                {
                    IcpItemKind.Label => item.LabelText ?? string.Empty,
                    IcpItemKind.BoundField => provider.GetFormattedValue(item.AttributeName),
                    _ => string.Empty
                };

                if (string.IsNullOrEmpty(raw))
                    continue;

                // Convert marker-rich text into visible text + inversion spans
                var (text, spansFromText) = ParseInversionMarkers(raw);

                if (string.IsNullOrEmpty(text))
                    continue;

                int visibleLen = Math.Min(text.Length, COLS - item.X);
                if (visibleLen <= 0)
                    continue;

                // Write ONLY visible characters into the fixed grid
                for (int i = 0; i < visibleLen; i++)
                    buffer[item.Y][item.X + i] = text[i];

                // Spans coming from GetFormattedValue (invert-when-true style)
                foreach (var sp in spansFromText)
                {
                    int start = sp.start;
                    int len = sp.len;

                    // Clip span to what actually fit on the row
                    if (start >= visibleLen) continue;
                    if (start + len > visibleLen) len = visibleLen - start;
                    if (len <= 0) continue;

                    invertedSpans.Add((item.Y, item.X + start, len));
                }

                // Also support explicit item-level inversion (whole field)
                if (item.Inverted)
                    invertedSpans.Add((item.Y, item.X, visibleLen));
            }

            string[] lines = new string[ROWS];

            for (int y = 0; y < ROWS; y++)
            {
                string line = new string(buffer[y]);

                // Inject inversion markers right-to-left so indexes stay valid as we insert.
                var spans = invertedSpans
                    .Where(s => s.y == y)
                    .OrderByDescending(s => s.x)
                    .ToList();

                foreach (var s in spans)
                {
                    // Insert closing then opening
                    line = line.Insert(s.x + s.len, "⟧");
                    line = line.Insert(s.x, "⟦");
                }

                lines[y] = line.PadRight(COLS);
            }
            return lines;
        }

        public string[] BuildIcpLines(FlightSim.FlightSimProviderBase provider)
        {
            return BuildIcpLines(provider, Items);
        }
    }

    public sealed class UserIcpDisplayItem : INotifyPropertyChanged
    {
        /// <summary>Text-grid column (0-based).</summary>
        public int X { get; set; }

        /// <summary>Text-grid row (0-based).</summary>
        public int Y { get; set; }

        public bool Inverted { get; set; }

        public IcpItemKind Kind { get; set; } = IcpItemKind.BoundField;

        /// <summary>
        /// For BoundField: provider property name (e.g., "Com1Frequency").
        /// Ignored for Label.
        /// </summary>
        public string AttributeName { get; set; } = "";

        /// <summary>
        /// For Label: literal text to render.
        /// Ignored for BoundField.
        /// </summary>
        private string _labelText = "";

        public string LabelText
        {
            get => _labelText;
            set
            {
                if (_labelText != value)
                {
                    _labelText = value;
                    OnPropertyChanged(nameof(ItemFriendlyName));
                }
            }
        }

        /// <summary>
        /// Derived UI-friendly name for editor lists.
        /// </summary>
        public string ItemFriendlyName
        {
            get
            {
                if (Kind == IcpItemKind.Label)
                {
                    return !string.IsNullOrWhiteSpace(LabelText) ? LabelText : "Label";
                }
                if (!string.IsNullOrWhiteSpace(AttributeName) && FlightSim.FlightSimFieldCatalog.GetFields().TryGetValue(AttributeName, out var meta))
                {
                    return meta.FriendlyName;
                }
                return AttributeName;
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
