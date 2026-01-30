using FlightSim;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Timer = System.Windows.Forms.Timer;

namespace WinCtrlICP
{
    public partial class UserIcpDisplayForm : Form
    {
        private readonly Dictionary<UserIcpDisplayItem, IcpDisplayControl> _overlays = new();

        private bool _dragging = false;
        private Point _dragMouseDownOffset; // mouse position inside the overlay
        private UserIcpDisplayItem? _dragItem;
        private IcpDisplayControl? _dragControl;

        private readonly Dictionary<UserIcpDisplayItem, string> _lastRenderedText = new();

        private bool _capturingBinding = false;

        JoystickBinding? _binding = null;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public UserIcpDisplay? UserIcpDisplay { get; set; } = null;

        private List<UserIcpDisplay> UserIcpDisplays { get; set; } = new List<UserIcpDisplay>();

        public UserIcpDisplayForm(List<UserIcpDisplay> userIcpDisplays)
        {
            UserIcpDisplays = userIcpDisplays;
            InitializeComponent();
            F16DEDWriterForm.Instance.Joysticks.JoystickEvent += Joysticks_JoystickEvent;
            FlightSimProviders.Preview.OnFlightDataReceived += Preview_OnFlightDataReceived;
        }

        private void Preview_OnFlightDataReceived(FlightSimProviderBase sender)
        {
            Ui(() =>
            {
                foreach (var kvp in _overlays)
                {
                    var item = kvp.Key;
                    var overlay = kvp.Value;
                    if (item.Kind != IcpItemKind.BoundField)
                        continue;
                    string newText = GetItemPreviewText(item);
                    if (item.Inverted)
                    {
                        newText = $"⟦{newText}⟧";
                    }
                    if (_lastRenderedText.TryGetValue(item, out var oldText) &&
                        oldText == newText)
                    {
                        // 🔇 No change → do nothing
                        continue;
                    }
                    // 🔄 Update only what changed
                    overlay.SetLines(new[] { newText });
                    overlay.Invalidate();
                    _lastRenderedText[item] = newText;
                }
            });
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            ChooseFieldForm form = new ChooseFieldForm();
            if (form.ShowDialog(this) == DialogResult.OK)
            {
                var selected = form.SelectedField;
                if (selected == null)
                {
                    return;
                }
                UserIcpDisplayItem item = new UserIcpDisplayItem()
                {
                    Kind = !string.IsNullOrEmpty(selected.PropertyName) ? IcpItemKind.BoundField : IcpItemKind.Label,
                    AttributeName = selected.PropertyName
                };
                int index = lstFields.Items.Add(item);
                lstFields.SelectedIndex = index;
                AddOrUpdateOverlay(item);
                UpdateOverlaySelection();
                btnDelete.Enabled = lstFields.SelectedIndex != -1;
                btnOk.Enabled = !string.IsNullOrEmpty(textName.Text) && lstFields.Items.Count > 0;
                if (item.Kind == IcpItemKind.Label)
                {
                    EditLabel(item);
                }
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (lstFields.SelectedItem is UserIcpDisplayItem item)
            {
                if (MessageBox.Show(this, $"Are you sure you waich to delete '{item.ItemFriendlyName}'?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    RemoveOverlay(item);
                    lstFields.Items.Remove(item);
                }
            }
            btnOk.Enabled = !string.IsNullOrEmpty(textName.Text) && lstFields.Items.Count > 0;
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            if (UserIcpDisplay == null)
            {
                UserIcpDisplay = new UserIcpDisplay();
            }
            UserIcpDisplay.DisplayName = textName.Text;
            UserIcpDisplay.Binding = _binding;
            UserIcpDisplay.Items = lstFields.Items.OfType<UserIcpDisplayItem>().ToList();
            DialogResult = DialogResult.OK;
            Close();
        }

        private void UserIcpDisplayForm_Load(object sender, EventArgs e)
        {
            if (UserIcpDisplay != null)
            {
                textName.Text = UserIcpDisplay.DisplayName;
                _binding = UserIcpDisplay.Binding;
                if (_binding != null)
                {
                    lblCaptureStatus.Text = _binding.ToString();
                    btnBind.Text = "&Unbind";
                }
                foreach (UserIcpDisplayItem item in UserIcpDisplay.Items)
                {
                    lstFields.Items.Add(item);
                    AddOrUpdateOverlay(item);
                }
            }
            btnOk.Enabled = !string.IsNullOrEmpty(textName.Text) && lstFields.Items.Count > 0;
        }

        private void overlayHost_LocationChanged(object sender, EventArgs e)
        {
            RebuildAllOverlays();
        }

        private void overlayHost_SizeChanged(object sender, EventArgs e)
        {
            RebuildAllOverlays();
        }

        private void UpdateOverlaySelection()
        {
            UserIcpDisplayItem? selected = lstFields.SelectedItem as UserIcpDisplayItem;
            foreach (var kvp in _overlays)
            {
                bool isSelected = (kvp.Key == selected);
                kvp.Value.ShowSelectionBorder = isSelected;
                kvp.Value.Invalidate(); if (isSelected)
                {
                    kvp.Value.BringToFront();
                }
            }
        }

        private const int TOP_MARGIN = 2;
        private const int LEFT_MARGIN = 13;

        private (int glyphW, int glyphH) GetBaseGlyphSize()
        {
            int baseGlyphH = ((overlayHost.ClientSize.Height - (TOP_MARGIN * 2)) / 5);
            int baseGlyphW = (baseGlyphH * (IcpDisplayControl.GlyphW - 2)) / IcpDisplayControl.GlyphH;
            return (baseGlyphW, baseGlyphH);
        }

        private static int Clamp(int v, int min, int max) => v < min ? min : (v > max ? max : v);

        private static int VisibleCharCount(string s)
        {
            if (string.IsNullOrEmpty(s)) return 0;

            int n = 0;
            foreach (char c in s)
            {
                if (c == '⟦' || c == '⟧') continue;
                n++;
            }
            return n;
        }

        private string GetItemPreviewText(UserIcpDisplayItem item)
        {
            // Use whatever you already use for preview; this is a safe default:
            if (item.Kind == IcpItemKind.Label)
            {
                return item.LabelText ?? "";
            }
            // BoundField: preview provider
            return FlightSim.FlightSimProviders.Preview.GetFormattedValue(item.AttributeName);
        }

        private void AddOrUpdateOverlay(UserIcpDisplayItem item)
        {
            var (glyphW, glyphH) = GetBaseGlyphSize();
            if (glyphW <= 0 || glyphH <= 0)
                return;

            string text = GetItemPreviewText(item);
            int remainingCols = Math.Max(0, 25 - item.X);

            int widthCells;

            if (item.Kind == IcpItemKind.Label)
            {
                // Labels: width equals label text
                widthCells = Math.Min(VisibleCharCount(text), remainingCols);
            }
            else
            {
                // Bound fields: width comes from FlightSimFieldAttribute.MaxLength
                int maxLen = 0;
                string? format = null;

                var fields = FlightSim.FlightSimFieldCatalog.GetFields();
                if (item.AttributeName != null &&
                    fields.TryGetValue(item.AttributeName, out var meta))
                {
                    maxLen = meta.MaxLength;
                    format = meta.Format;
                }

                if (maxLen <= 0)
                {
                    // Fallback if metadata missing
                    widthCells = Math.Min(VisibleCharCount(text), remainingCols);
                }
                else
                {
                    widthCells = Math.Min(maxLen, remainingCols);

                    int visible = VisibleCharCount(text);

                    // 🔑 Key rule:
                    // Only pad if there is NO format string
                    if (string.IsNullOrEmpty(format))
                    {
                        if (visible < widthCells)
                        {
                            text = new string(' ', widthCells - visible) + text;
                        }
                    }

                    // Defensive clip (keep rightmost chars)
                    if (VisibleCharCount(text) > widthCells)
                    {
                        text = text.Substring(text.Length - widthCells, widthCells);
                    }
                }
            }

            int overlayHeight = glyphH;
            int overlayWidth = Math.Max(1, widthCells * glyphW);

            if (!_overlays.TryGetValue(item, out var overlay))
            {
                overlay = new IcpDisplayControl
                {
                    Parent = overlayHost,
                    LineCount = 1,
                    BackColor = overlayHost.BackColor,
                    TopMargin = 0,
                    LeftMargin = 0,
                    Enabled = false
                };
                overlay.DoubleClick += lstFields_DoubleClick;
                _overlays[item] = overlay;
            }

            overlay.Size = new Size(overlayWidth, Math.Max(1, overlayHeight));

            overlay.Location = new Point(
                LEFT_MARGIN + (item.X * glyphW),
                TOP_MARGIN + (item.Y * glyphH)
            );

            overlay.MouseDown -= Overlay_MouseDown;
            overlay.MouseMove -= Overlay_MouseMove;
            overlay.MouseUp -= Overlay_MouseUp;
            overlay.MouseDown += Overlay_MouseDown;
            overlay.MouseMove += Overlay_MouseMove;
            overlay.MouseUp += Overlay_MouseUp;

            overlay.Enabled = !_capturingBinding;
            overlay.Cursor = _capturingBinding ? Cursors.Default : Cursors.SizeAll;
            overlay.Tag = item;

            overlay.SetLines(new[] { item.Inverted ? $"⟦{text}⟧" : text });
            overlay.BringToFront();
        }

        private void Overlay_MouseDown(object? sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left)
                return;

            if (sender is not IcpDisplayControl ctrl)
                return;

            if (!ctrl.Enabled)
                return;

            if (ctrl.Tag is not UserIcpDisplayItem item)
                return;

            // Select corresponding item in listbox
            lstFields.SelectedItem = item;

            _dragging = true;
            _dragControl = ctrl;
            _dragItem = item;

            _dragMouseDownOffset = e.Location; // where inside control the mouse grabbed

            ctrl.BringToFront();
        }

        private void Overlay_MouseMove(object? sender, MouseEventArgs e)
        {
            if (_capturingBinding) 
                return;

            if (!_dragging || _dragControl == null || _dragItem == null)
                return;

            var (glyphW, glyphH) = GetBaseGlyphSize();
            if (glyphW <= 0 || glyphH <= 0)
                return;

            // Mouse position relative to overlayHost
            Point mouseInHost = overlayHost.PointToClient(Cursor.Position);

            // Desired top-left in pixels (before snapping)
            int desiredXpx = mouseInHost.X - _dragMouseDownOffset.X;
            int desiredYpx = mouseInHost.Y - _dragMouseDownOffset.Y;

            // ***** KEY FIX: remove margins before snapping *****
            int contentX = desiredXpx - LEFT_MARGIN;
            int contentY = desiredYpx - TOP_MARGIN;

            int snappedX = (int)Math.Round(contentX / (double)glyphW, MidpointRounding.AwayFromZero);
            int snappedY = (int)Math.Round(contentY / (double)glyphH, MidpointRounding.AwayFromZero);

            // Clamp to grid bounds (respect item width)
            string text = GetItemPreviewText(_dragItem);
            int charCount = VisibleCharCount(text);
            int maxX = Math.Max(0, 25 - charCount);

            snappedX = Clamp(snappedX, 0, maxX);
            snappedY = Clamp(snappedY, 0, 4);

            // ***** KEY FIX: add margins back when applying pixels *****
            int snappedXpx = LEFT_MARGIN + (snappedX * glyphW);
            int snappedYpx = TOP_MARGIN + (snappedY * glyphH);

            _dragControl.Location = new Point(snappedXpx, snappedYpx);
        }

        private void Overlay_MouseUp(object? sender, MouseEventArgs e)
        {
            if (_capturingBinding)
                return;

            if (e.Button != MouseButtons.Left)
                return;

            if (!_dragging || _dragControl == null || _dragItem == null)
                return;

            var (glyphW, glyphH) = GetBaseGlyphSize();
            if (glyphW <= 0 || glyphH <= 0)
                return;

            string text = GetItemPreviewText(_dragItem);
            int charCount = VisibleCharCount(text);
            int maxX = Math.Max(0, 25 - charCount);

            // ***** KEY FIX: remove margins before converting pixels -> grid *****
            int contentLeft = _dragControl.Left - LEFT_MARGIN;
            int contentTop = _dragControl.Top - TOP_MARGIN;

            int snappedX = Clamp(
                (int)Math.Round(contentLeft / (double)glyphW, MidpointRounding.AwayFromZero),
                0,
                maxX
            );

            int snappedY = Clamp(
                (int)Math.Round(contentTop / (double)glyphH, MidpointRounding.AwayFromZero),
                0,
                4
            );

            _dragItem.X = snappedX;
            _dragItem.Y = snappedY;

            _dragging = false;
            _dragControl = null;
            _dragItem = null;
        }

        private void RemoveOverlay(UserIcpDisplayItem item)
        {
            if (_overlays.TryGetValue(item, out var overlay))
            {
                overlay.Parent?.Controls.Remove(overlay);
                overlay.Dispose();
                _overlays.Remove(item);
            }
        }

        private void RebuildAllOverlays()
        {
            foreach (var item in lstFields.Items.OfType<UserIcpDisplayItem>())
            {
                AddOrUpdateOverlay(item);
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            btnOk.Enabled = !string.IsNullOrEmpty(textName.Text) && lstFields.Items.Count > 0;
        }

        private void lstFields_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lstFields.SelectedItem is UserIcpDisplayItem item)
            {
                chkInverted.CheckedChanged -= chkInverted_CheckedChanged;

                bool allowInvert = false;

                if (item.Kind == IcpItemKind.Label)
                {
                    // Labels always allow manual inversion
                    allowInvert = true;
                }
                else if (item.Kind == IcpItemKind.BoundField)
                {
                    var meta = GetFieldAttribute(item.AttributeName);
                    allowInvert = meta?.InvertWhenTrue != true;
                }

                chkInverted.Visible = allowInvert;
                chkInverted.Enabled = allowInvert;

                chkInverted.Checked = allowInvert && item.Inverted;

                chkInverted.CheckedChanged += chkInverted_CheckedChanged;
            }
            else
            {
                chkInverted.CheckedChanged -= chkInverted_CheckedChanged;
                chkInverted.Visible = false;
                chkInverted.Enabled = false;
                chkInverted.Checked = false;
                chkInverted.CheckedChanged += chkInverted_CheckedChanged;
            }

            UpdateOverlaySelection();
            btnDelete.Enabled = lstFields.SelectedIndex != -1;
        }

        private void lstFields_DoubleClick(object? sender, EventArgs e)
        {
            if (lstFields.SelectedItem is not UserIcpDisplayItem display)
            {
                return;
            }
            if (display.Kind == IcpItemKind.Label)
            {
                EditLabel(display);
            }
        }

        private void EditLabel(UserIcpDisplayItem display)
        {
            EditLabelForm form = new EditLabelForm(display.LabelText);
            if (form.ShowDialog(this) == DialogResult.OK)
            {
                display.LabelText = form.LabelText;
                AddOrUpdateOverlay(display);
                lstFields.Items[lstFields.SelectedIndex] = lstFields.Items[lstFields.SelectedIndex];
            }
        }

        private void UserIcpDisplayForm_Shown(object sender, EventArgs e)
        {
            textName.Focus();
            textName.SelectAll();
        }

        private void chkInverted_CheckedChanged(object? sender, EventArgs e)
        {
            if (lstFields.SelectedItem is not UserIcpDisplayItem item)
            {
                return;
            }
            item.Inverted = chkInverted.Checked;
            AddOrUpdateOverlay(item);
        }

        private void btnBind_Click(object sender, EventArgs e)
        {
            if (_capturingBinding)
            {
                F16DEDWriterForm.Instance.CaptureJoystickEvents = true;
                btnBind.Text = _binding != null ? "&Unbind" : "&Bind";
                lblCaptureStatus.Text = _binding != null ? _binding.ToString() : string.Empty;
                _capturingBinding = false;
                EnableButtons();
            }
            else if (_binding != null)
            {
                if (MessageBox.Show(this, "Are you sure you wish to remove the binding?", "Confirm Unbind", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    _binding = null;
                    lblCaptureStatus.Text = string.Empty;
                    btnBind.Text = "&Bind";
                }
            }
            else
            {
                F16DEDWriterForm.Instance.CaptureJoystickEvents = false;
                _capturingBinding = true;
                lblCaptureStatus.Text = "Press a controller button…";
                btnBind.Text = "&Cancel";
                DisableButtons();
            }
        }

        private void Ui(Action action)
        {
            if (InvokeRequired)
            {
                BeginInvoke(action);
            }
            else
            {
                action();
            }
        }

        private UserIcpDisplay IsJoystickButtonBoundToCustom(JoystickBinding binding)
        {
            foreach (var userIcpDisplay in UserIcpDisplays)
            {
                if (userIcpDisplay.Binding != null)
                {
                    if ((UserIcpDisplay == null || userIcpDisplay.Id != UserIcpDisplay.Id) && userIcpDisplay.Binding.Equals(binding))
                    {
                        return userIcpDisplay;
                    }
                }
            }
            return null!;
        }

        private SystemButton IsJoystickButtonBoundToSystem(JoystickBinding binding)
        {
            if (binding.Equals(F16DEDWriterForm.Instance.CNIBinding))
            {
                return SystemButton.CNI;
            }
            if (binding.Equals(F16DEDWriterForm.Instance.COM1Binding))
            {
                return SystemButton.COM2;
            }
            if (binding.Equals(F16DEDWriterForm.Instance.COM2Binding))
            {
                return SystemButton.COM1;
            }
            if (binding.Equals(F16DEDWriterForm.Instance.IFFBinding))
            {
                return SystemButton.IFF;
            }
            if (binding.Equals(F16DEDWriterForm.Instance.NAV1Binding))
            {
                return SystemButton.NAV1;
            }
            if (binding.Equals(F16DEDWriterForm.Instance.NAV2Binding))
            {
                return SystemButton.NAV2;
            }
            if (binding.Equals(F16DEDWriterForm.Instance.GPSBinding))
            {
                return SystemButton.GPS;
            }
            if (binding.Equals(F16DEDWriterForm.Instance.FLPNBinding))
            {
                return SystemButton.FLPN;
            }
            if (binding.Equals(F16DEDWriterForm.Instance.APBinding))
            {
                return SystemButton.AP;
            }
            if (binding.Equals(F16DEDWriterForm.Instance.SWCHBinding))
            {
                return SystemButton.SWCH;
            }
            if (binding.Equals(F16DEDWriterForm.Instance.LGHTBinding))
            {
                return SystemButton.LGHT;
            }
            if (binding.Equals(F16DEDWriterForm.Instance.WXBinding))
            {
                return SystemButton.WX;
            }
            if (binding.Equals(F16DEDWriterForm.Instance.TIMEBinding))
            {
                return SystemButton.TIME;
            }
            if (binding.Equals(F16DEDWriterForm.Instance.INFOBinding))
            {
                return SystemButton.INFO;
            }
            if (binding.Equals(F16DEDWriterForm.Instance.HROTBinding))
            {
                return SystemButton.HROT;
            }
            if (binding.Equals(F16DEDWriterForm.Instance.HDISBinding))
            {
                return SystemButton.HDIS;
            }
            if (binding.Equals(F16DEDWriterForm.Instance.HCTLBinding))
            {
                return SystemButton.HCTL;
            }
            if (binding.Equals(F16DEDWriterForm.Instance.CycleSystemUpBinding))
            {
                return SystemButton.CycleSystemUp;
            }
            if (binding.Equals(F16DEDWriterForm.Instance.CycleSystemDownBinding))
            {
                return SystemButton.CycleSystemDown;
            }
            if (binding.Equals(F16DEDWriterForm.Instance.CycleCustomUpBinding))
            {
                return SystemButton.CycleCustomUp;
            }
            if (binding.Equals(F16DEDWriterForm.Instance.CycleCustomDownBinding))
            {
                return SystemButton.CycleCustomDown;
            }
            if (binding.Equals(F16DEDWriterForm.Instance.BUGBinding))
            {
                return SystemButton.SyncHeadingBug;
            }
            if (binding.Equals(F16DEDWriterForm.Instance.KollsmanBinding))
            {
                return SystemButton.SyncKollsman;
            }
            return SystemButton.None;
        }

        private void Joysticks_JoystickEvent(object sender, JoystickEventArgs e)
        {
            Ui(() =>
            {
                if (e.EventType == JoystickEventType.ButtonDown || e.EventType == JoystickEventType.PovDown || e.EventType == JoystickEventType.PovLeft || e.EventType == JoystickEventType.PovRight || e.EventType == JoystickEventType.PovUp)
                {
                    if (_capturingBinding)
                    {
                        if (e.EventType == JoystickEventType.ButtonDown || e.EventType == JoystickEventType.PovDown || e.EventType == JoystickEventType.PovLeft || e.EventType == JoystickEventType.PovRight || e.EventType == JoystickEventType.PovUp)
                        {
                            JoystickBinding binding = new JoystickBinding()
                            {
                                DeviceType = e.EventType == JoystickEventType.ButtonDown ? BindingDeviceType.JoystickButton : BindingDeviceType.JoystickPov,
                                JoystickGuid = e.Joystick.Information.InstanceGuid,
                                ButtonOrKey = e.ButtonOrAxis,
                                Name = e.Joystick.Information.InstanceName,
                                Direction = e.Direction
                            };
                            UserIcpDisplay alreadyBoundCustom = IsJoystickButtonBoundToCustom(binding);
                            SystemButton alreadyBoundSystem = IsJoystickButtonBoundToSystem(binding);
                            if (alreadyBoundCustom != null)
                            {
                                _capturingBinding = false;
                                if (MessageBox.Show(this, string.Format("This button is already bound to \"{0}\". Do you want remove the binding from \"{0}\"?", alreadyBoundCustom.DisplayName), "Duplicate Binding", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                                {
                                    _capturingBinding = true;
                                    return;
                                }
                                _capturingBinding = true;
                                alreadyBoundCustom.Binding = null;
                            }
                            else if (alreadyBoundSystem != SystemButton.None)
                            {
                                _capturingBinding = false;
                                if (MessageBox.Show(this, string.Format("This button is already bound to \"{0}\". Do you want remove the binding from \"{0}\"?", alreadyBoundSystem.ToString().SplitTitleCase()), "Duplicate Binding", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                                {
                                    _capturingBinding = true;
                                    return;
                                }
                                _capturingBinding = true;
                                switch (alreadyBoundSystem)
                                {
                                    case SystemButton.CNI:
                                        F16DEDWriterForm.Instance.CNIBinding = null;
                                        break;
                                    case SystemButton.COM1:
                                        F16DEDWriterForm.Instance.COM1Binding = null;
                                        break;
                                    case SystemButton.COM2:
                                        F16DEDWriterForm.Instance.COM2Binding = null;
                                        break;
                                    case SystemButton.IFF:
                                        F16DEDWriterForm.Instance.IFFBinding = null;
                                        break;
                                    case SystemButton.NAV1:
                                        F16DEDWriterForm.Instance.NAV1Binding = null;
                                        break;
                                    case SystemButton.NAV2:
                                        F16DEDWriterForm.Instance.NAV2Binding = null;
                                        break;
                                    case SystemButton.GPS:
                                        F16DEDWriterForm.Instance.GPSBinding = null;
                                        break;
                                    case SystemButton.FLPN:
                                        F16DEDWriterForm.Instance.FLPNBinding = null;
                                        break;
                                    case SystemButton.AP:
                                        F16DEDWriterForm.Instance.APBinding = null;
                                        break;
                                    case SystemButton.SWCH:
                                        F16DEDWriterForm.Instance.SWCHBinding = null;
                                        break;
                                    case SystemButton.LGHT:
                                        F16DEDWriterForm.Instance.LGHTBinding = null;
                                        break;
                                    case SystemButton.WX:
                                        F16DEDWriterForm.Instance.WXBinding = null;
                                        break;
                                    case SystemButton.TIME:
                                        F16DEDWriterForm.Instance.TIMEBinding = null;
                                        break;
                                    case SystemButton.INFO:
                                        F16DEDWriterForm.Instance.INFOBinding = null;
                                        break;
                                    case SystemButton.HROT:
                                        F16DEDWriterForm.Instance.HROTBinding = null;
                                        break;
                                    case SystemButton.HDIS:
                                        F16DEDWriterForm.Instance.HDISBinding = null;
                                        break;
                                    case SystemButton.HCTL:
                                        F16DEDWriterForm.Instance.HCTLBinding = null;
                                        break;
                                    case SystemButton.CycleSystemUp:
                                        F16DEDWriterForm.Instance.CycleSystemUpBinding = null;
                                        break;
                                    case SystemButton.CycleSystemDown:
                                        F16DEDWriterForm.Instance.CycleSystemDownBinding = null;
                                        break;
                                    case SystemButton.CycleCustomUp:
                                        F16DEDWriterForm.Instance.CycleCustomUpBinding = null;
                                        break;
                                    case SystemButton.CycleCustomDown:
                                        F16DEDWriterForm.Instance.CycleSystemDownBinding = null;
                                        break;
                                    case SystemButton.SyncHeadingBug:
                                        F16DEDWriterForm.Instance.BUGBinding = null;
                                        break;
                                    case SystemButton.SyncKollsman:
                                        F16DEDWriterForm.Instance.KollsmanBinding = null;
                                        break;
                                }
                            }
                            _capturingBinding = false;
                            _binding = binding;
                            lblCaptureStatus.Text = _binding.ToString();
                            btnBind.Text = "&Unbind";
                            EnableButtons();
                            F16DEDWriterForm.Instance.CaptureJoystickEvents = true;
                        }
                    }
                }
            });
        }

        private void UserIcpDisplayForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            FlightSimProviders.Preview.OnFlightDataReceived -= Preview_OnFlightDataReceived;
            _capturingBinding = false;
            F16DEDWriterForm.Instance.CaptureJoystickEvents = true;
        }

        private static FlightSim.FlightSimFieldAttribute? GetFieldAttribute(string? propertyName)
        {
            if (string.IsNullOrEmpty(propertyName))
                return null;

            var fields = FlightSim.FlightSimFieldCatalog.GetFields();
            return fields.TryGetValue(propertyName, out var meta) ? meta : null;
        }

        private bool _isInvertedEnabled;
        private void DisableButtons()
        {
            var buttons = Controls.OfType<Button>();
            foreach (var button in buttons)
            {
                button.Enabled = button == btnBind;
            }
            var overlays = overlayHost.Controls.OfType<IcpDisplayControl>();
            foreach (var overlay in overlays)
            {
                overlay.Enabled = false;
            }
            _isInvertedEnabled = chkInverted.Enabled;
            chkInverted.Enabled = false;
            textName.Enabled = false;
            lstFields.Enabled = false;
        }

        private void EnableButtons()
        {
            var buttons = Controls.OfType<Button>();
            foreach (var button in buttons)
            {
                button.Enabled = true;
            }
            var overlays = overlayHost.Controls.OfType<IcpDisplayControl>();
            foreach (var overlay in overlays)
            {
                overlay.Enabled = true;
            }
            textName.Enabled = true;
            lstFields.Enabled = true;
            chkInverted.Enabled = _isInvertedEnabled;
        }
    }
}
