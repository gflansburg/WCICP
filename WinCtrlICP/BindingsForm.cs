using FlightSim;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace WinCtrlICP
{
    public partial class BindingsForm : Form
    {
        private bool _capturingBinding = false;

        Button _activeButton = null!;
        Label _activeLabel = null!;

        private List<UserIcpDisplay> UserIcpDisplays { get; set; } = new List<UserIcpDisplay>();

        public BindingsForm(List<UserIcpDisplay> userIcpDisplays)
        {
            UserIcpDisplays = userIcpDisplays;
            InitializeComponent();
            F16DEDWriterForm.Instance.Joysticks.JoystickEvent += Joysticks_JoystickEvent;
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private static JoystickBinding? GetBinding(Button btn) => btn.Tag as JoystickBinding;

        private void btnOK_Click(object sender, EventArgs e)
        {
            var form = F16DEDWriterForm.Instance;
            form.CNIBinding = GetBinding(btnCNI);
            form.COM1Binding = GetBinding(btnCOM1);
            form.COM2Binding = GetBinding(btnCOM2);
            form.IFFBinding = GetBinding(btnIFF);
            form.NAV1Binding = GetBinding(btnNAV1);
            form.NAV2Binding = GetBinding(btnNAV2);
            form.GPSBinding = GetBinding(btnGPS);
            form.FLPNBinding = GetBinding(btnFLPN);
            form.APBinding = GetBinding(btnAP);
            form.SWCHBinding = GetBinding(btnSWCH);
            form.LGHTBinding = GetBinding(btnLGHT);
            form.WXBinding = GetBinding(btnWX);
            form.TIMEBinding = GetBinding(btnTIME);
            form.INFOBinding = GetBinding(btnINFO);
            form.HROTBinding = GetBinding(btnHROT);
            form.HDISBinding = GetBinding(btnHDIS);
            form.HCTLBinding = GetBinding(btnHCTL);
            form.CycleSystemUpBinding = GetBinding(btnCycleSystemUp);
            form.CycleSystemDownBinding = GetBinding(btnCycleSystemDown);
            form.CycleCustomUpBinding = GetBinding(btnCycleCustomUp);
            form.CycleCustomDownBinding = GetBinding(btnCycleCustomDown);
            form.BUGBinding = GetBinding(btnBUG);
            form.KollsmanBinding = GetBinding(btnKollsman);
            DialogResult = DialogResult.OK;
            Close();
        }

        private void DisableButtons()
        {
            var buttons = Controls.OfType<Button>();
            foreach (var button in buttons)
            {
                button.Enabled = button == _activeButton;
            }
        }

        private void EnableButtons()
        {
            var buttons = Controls.OfType<Button>();
            foreach (var button in buttons)
            {
                button.Enabled = true;
            }
        }

        private void ToggleCaptureBinding(Button btn, Label lbl)
        {
            if (_capturingBinding)
            {
                F16DEDWriterForm.Instance.CaptureJoystickEvents = true;
                btn.Text = btn.Tag != null ? "&Unbind" : "&Bind";
                lbl.Text = btn.Tag != null ? btn.Tag.ToString() : string.Empty;
                _capturingBinding = false;
                EnableButtons();
            }
            else if (btn.Tag is JoystickBinding binding)
            {
                if (MessageBox.Show(this, $"Are you sure you wish to remove the binding for '{GetButtonName(btn)}'?", "Confirm Unbind", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    btn.Tag = null;
                    lbl.Text = string.Empty;
                    btn.Text = "&Bind";
                }
            }
            else
            {
                F16DEDWriterForm.Instance.CaptureJoystickEvents = false;
                _capturingBinding = true;
                lbl.Text = "Press a controller button…";
                btn.Text = "&Cancel";
                _activeButton = btn;
                _activeLabel = lbl;
                DisableButtons();
            }
        }

        private void btnCNI_Click(object sender, EventArgs e)
        {
            ToggleCaptureBinding(btnCNI, lblCNI);
        }

        private void btnCOM1_Click(object sender, EventArgs e)
        {
            ToggleCaptureBinding(btnCOM1, lblCOM1);
        }

        private void btnCOM2_Click(object sender, EventArgs e)
        {
            ToggleCaptureBinding(btnCOM2, lblCOM2);
        }

        private void btnIFF_Click(object sender, EventArgs e)
        {
            ToggleCaptureBinding(btnIFF, lblIFF);
        }

        private void btnNAV1_Click(object sender, EventArgs e)
        {
            ToggleCaptureBinding(btnNAV1, lblNAV1);
        }

        private void btnNAV2_Click(object sender, EventArgs e)
        {
            ToggleCaptureBinding(btnNAV2, lblNAV2);
        }

        private void btnGPS_Click(object sender, EventArgs e)
        {
            ToggleCaptureBinding(btnGPS, lblGPS);
        }

        private void btnFLPN_Click(object sender, EventArgs e)
        {
            ToggleCaptureBinding(btnFLPN, lblFLPN);
        }

        private void btnAP_Click(object sender, EventArgs e)
        {
            ToggleCaptureBinding(btnAP, lblAP);
        }

        private void btnSWCH_Click(object sender, EventArgs e)
        {
            ToggleCaptureBinding(btnSWCH, lblSWCH);
        }

        private void btnLGHT_Click(object sender, EventArgs e)
        {
            ToggleCaptureBinding(btnLGHT, lblLGHT);
        }

        private void btnWX_Click(object sender, EventArgs e)
        {
            ToggleCaptureBinding(btnWX, lblWX);
        }

        private void btnTIME_Click(object sender, EventArgs e)
        {
            ToggleCaptureBinding(btnTIME, lblTIME);
        }

        private void btnINFO_Click(object sender, EventArgs e)
        {
            ToggleCaptureBinding(btnINFO, lblINFO);
        }

        private void btnHROT_Click(object sender, EventArgs e)
        {
            ToggleCaptureBinding(btnHROT, lblHROT);
        }

        private void btnHDIS_Click(object sender, EventArgs e)
        {
            ToggleCaptureBinding(btnHDIS, lblHDIS);
        }

        private void btnHCTL_Click(object sender, EventArgs e)
        {
            ToggleCaptureBinding(btnHCTL, lblHCTL);
        }

        private void btnCycleSystemUp_Click(object sender, EventArgs e)
        {
            ToggleCaptureBinding(btnCycleSystemUp, lblCycleSystemUp);
        }

        private void btnCycleSystemDown_Click(object sender, EventArgs e)
        {
            ToggleCaptureBinding(btnCycleSystemDown, lblCycleSystemDown);
        }

        private void btnCycleCustomUp_Click(object sender, EventArgs e)
        {
            ToggleCaptureBinding(btnCycleCustomUp, lblCycleCustomUp);
        }

        private void btnCycleCustomDown_Click(object sender, EventArgs e)
        {
            ToggleCaptureBinding(btnCycleCustomDown, lblCycleCustomDown);
        }

        private void btnBUG_Click(object sender, EventArgs e)
        {
            ToggleCaptureBinding(btnBUG, lblBUG);
        }

        private void btnKollsman_Click(object sender, EventArgs e)
        {
            ToggleCaptureBinding(btnKollsman, lblKollsman);
        }

        private void SetButtonText(Button btn)
        {
            btn.Text = btn.Tag != null ? "&Unbind" : "&Bind";
        }

        private void BindingsForm_Load(object sender, EventArgs e)
        {
            btnCNI.Tag = F16DEDWriterForm.Instance.CNIBinding;
            btnCOM1.Tag = F16DEDWriterForm.Instance.COM1Binding;
            btnCOM2.Tag = F16DEDWriterForm.Instance.COM2Binding;
            btnIFF.Tag = F16DEDWriterForm.Instance.IFFBinding;
            btnNAV1.Tag = F16DEDWriterForm.Instance.NAV1Binding;
            btnNAV2.Tag = F16DEDWriterForm.Instance.NAV2Binding;
            btnGPS.Tag = F16DEDWriterForm.Instance.GPSBinding;
            btnFLPN.Tag = F16DEDWriterForm.Instance.FLPNBinding;
            btnAP.Tag = F16DEDWriterForm.Instance.APBinding;
            btnSWCH.Tag = F16DEDWriterForm.Instance.SWCHBinding;
            btnLGHT.Tag = F16DEDWriterForm.Instance.LGHTBinding;
            btnWX.Tag = F16DEDWriterForm.Instance.WXBinding;
            btnTIME.Tag = F16DEDWriterForm.Instance.TIMEBinding;
            btnINFO.Tag = F16DEDWriterForm.Instance.INFOBinding;
            btnHROT.Tag = F16DEDWriterForm.Instance.HROTBinding;
            btnHDIS.Tag = F16DEDWriterForm.Instance.HDISBinding;
            btnHCTL.Tag = F16DEDWriterForm.Instance.HCTLBinding;
            btnCycleSystemUp.Tag = F16DEDWriterForm.Instance.CycleSystemUpBinding;
            btnCycleSystemDown.Tag = F16DEDWriterForm.Instance.CycleSystemDownBinding;
            btnCycleCustomUp.Tag = F16DEDWriterForm.Instance.CycleCustomUpBinding;
            btnCycleCustomDown.Tag = F16DEDWriterForm.Instance.CycleCustomDownBinding;
            btnBUG.Tag = F16DEDWriterForm.Instance.BUGBinding;
            btnKollsman.Tag = F16DEDWriterForm.Instance.KollsmanBinding;

            SetButtonText(btnCNI);
            SetButtonText(btnCOM1);
            SetButtonText(btnCOM2);
            SetButtonText(btnIFF);
            SetButtonText(btnNAV1);
            SetButtonText(btnNAV2);
            SetButtonText(btnGPS);
            SetButtonText(btnFLPN);
            SetButtonText(btnAP);
            SetButtonText(btnSWCH);
            SetButtonText(btnLGHT);
            SetButtonText(btnWX);
            SetButtonText(btnTIME);
            SetButtonText(btnINFO);
            SetButtonText(btnHROT);
            SetButtonText(btnHDIS);
            SetButtonText(btnHCTL);
            SetButtonText(btnCycleSystemUp);
            SetButtonText(btnCycleSystemDown);
            SetButtonText(btnCycleCustomUp);
            SetButtonText(btnCycleCustomDown);
            SetButtonText(btnBUG);
            SetButtonText(btnKollsman);

            lblCNI.Text = F16DEDWriterForm.Instance.CNIBinding?.ToString();
            lblCOM1.Text = F16DEDWriterForm.Instance.COM1Binding?.ToString();
            lblCOM2.Text = F16DEDWriterForm.Instance.COM2Binding?.ToString();
            lblIFF.Text = F16DEDWriterForm.Instance.IFFBinding?.ToString();
            lblNAV1.Text = F16DEDWriterForm.Instance.NAV1Binding?.ToString();
            lblNAV2.Text = F16DEDWriterForm.Instance.NAV2Binding?.ToString();
            lblGPS.Text = F16DEDWriterForm.Instance.GPSBinding?.ToString();
            lblFLPN.Text = F16DEDWriterForm.Instance.FLPNBinding?.ToString();
            lblAP.Text = F16DEDWriterForm.Instance.APBinding?.ToString();
            lblSWCH.Text = F16DEDWriterForm.Instance.SWCHBinding?.ToString();
            lblLGHT.Text = F16DEDWriterForm.Instance.LGHTBinding?.ToString();
            lblWX.Text = F16DEDWriterForm.Instance.WXBinding?.ToString();
            lblTIME.Text = F16DEDWriterForm.Instance.TIMEBinding?.ToString();
            lblINFO.Text = F16DEDWriterForm.Instance.INFOBinding?.ToString();
            lblHROT.Text = F16DEDWriterForm.Instance.HROTBinding?.ToString();
            lblHDIS.Text = F16DEDWriterForm.Instance.HDISBinding?.ToString();
            lblHCTL.Text = F16DEDWriterForm.Instance.HCTLBinding?.ToString();
            lblCycleSystemUp.Text = F16DEDWriterForm.Instance.CycleSystemUpBinding?.ToString();
            lblCycleSystemDown.Text = F16DEDWriterForm.Instance.CycleSystemDownBinding?.ToString();
            lblCycleCustomUp.Text = F16DEDWriterForm.Instance.CycleCustomUpBinding?.ToString();
            lblCycleCustomDown.Text = F16DEDWriterForm.Instance.CycleCustomDownBinding?.ToString();
            lblBUG.Text = F16DEDWriterForm.Instance.BUGBinding?.ToString();
            lblKollsman.Text = F16DEDWriterForm.Instance.KollsmanBinding?.ToString();
        }

        private void BindingsForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            _capturingBinding = false;
            F16DEDWriterForm.Instance.CaptureJoystickEvents = true;
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
                    if (userIcpDisplay.Binding.Equals(binding))
                    {
                        return userIcpDisplay;
                    }
                }
            }
            return null!;
        }

        private string GetButtonName(Button button)
        {
            if (Equals(button, btnCNI))
            {
                return SystemButton.CNI.ToString().SplitTitleCase();
            }
            if (Equals(button, btnCOM1))
            {
                return SystemButton.COM1.ToString().SplitTitleCase();
            }
            if (Equals(button, btnCOM2))
            {
                return SystemButton.COM2.ToString().SplitTitleCase();
            }
            if (Equals(button, btnIFF))
            {
                return SystemButton.IFF.ToString().SplitTitleCase();
            }
            if (Equals(button, btnNAV1))
            {
                return SystemButton.NAV1.ToString().SplitTitleCase();
            }
            if (Equals(button, btnNAV2))
            {
                return SystemButton.NAV2.ToString().SplitTitleCase();
            }
            if (Equals(button, btnGPS))
            {
                return SystemButton.GPS.ToString().SplitTitleCase();
            }
            if (Equals(button, btnFLPN))
            {
                return SystemButton.FLPN.ToString().SplitTitleCase();
            }
            if (Equals(button, btnAP))
            {
                return SystemButton.AP.ToString().SplitTitleCase();
            }
            if (Equals(button, btnSWCH))
            {
                return SystemButton.SWCH.ToString().SplitTitleCase();
            }
            if (Equals(button, btnLGHT))
            {
                return SystemButton.LGHT.ToString().SplitTitleCase();
            }
            if (Equals(button, btnWX))
            {
                return SystemButton.WX.ToString().SplitTitleCase();
            }
            if (Equals(button, btnTIME))
            {
                return SystemButton.TIME.ToString().SplitTitleCase();
            }
            if (Equals(button, btnINFO))
            {
                return SystemButton.INFO.ToString().SplitTitleCase();
            }
            if (Equals(button, btnHROT))
            {
                return SystemButton.HROT.ToString().SplitTitleCase();
            }
            if (Equals(button, btnHDIS))
            {
                return SystemButton.HDIS.ToString().SplitTitleCase();
            }
            if (Equals(button, btnHCTL))
            {
                return SystemButton.HCTL.ToString().SplitTitleCase();
            }
            if (Equals(button, btnCycleSystemUp))
            {
                return SystemButton.CycleSystemUp.ToString().SplitTitleCase();
            }
            if (Equals(button, btnCycleSystemDown))
            {
                return SystemButton.CycleSystemDown.ToString().SplitTitleCase();
            }
            if (Equals(button, btnCycleCustomUp))
            {
                return SystemButton.CycleCustomUp.ToString().SplitTitleCase();
            }
            if (Equals(button, btnCycleCustomDown))
            {
                return SystemButton.CycleCustomDown.ToString().SplitTitleCase();
            }
            if (Equals(button, btnBUG))
            {
                return SystemButton.SyncHeadingBug.ToString().SplitTitleCase();
            }
            if (Equals(button, btnKollsman))
            {
                return SystemButton.SyncKollsman.ToString().SplitTitleCase();
            }
            return string.Empty;
        }

        private SystemButton IsJoystickButtonBoundToSystem(JoystickBinding binding)
        {
            if (binding.Equals(F16DEDWriterForm.Instance.CNIBinding) && _activeButton != btnCNI)
            {
                return SystemButton.CNI;
            }
            if (binding.Equals(F16DEDWriterForm.Instance.COM1Binding) && _activeButton != btnCOM1)
            {
                return SystemButton.COM2;
            }
            if (binding.Equals(F16DEDWriterForm.Instance.COM2Binding) && _activeButton != btnCOM2)
            {
                return SystemButton.COM1;
            }
            if (binding.Equals(F16DEDWriterForm.Instance.IFFBinding) && _activeButton != btnIFF)
            {
                return SystemButton.IFF;
            }
            if (binding.Equals(F16DEDWriterForm.Instance.NAV1Binding) && _activeButton != btnNAV1)
            {
                return SystemButton.NAV1;
            }
            if (binding.Equals(F16DEDWriterForm.Instance.NAV2Binding) && _activeButton != btnNAV2)
            {
                return SystemButton.NAV2;
            }
            if (binding.Equals(F16DEDWriterForm.Instance.GPSBinding) && _activeButton != btnGPS)
            {
                return SystemButton.GPS;
            }
            if (binding.Equals(F16DEDWriterForm.Instance.FLPNBinding) && _activeButton != btnFLPN)
            {
                return SystemButton.FLPN;
            }
            if (binding.Equals(F16DEDWriterForm.Instance.APBinding) && _activeButton != btnAP)
            {
                return SystemButton.AP;
            }
            if (binding.Equals(F16DEDWriterForm.Instance.SWCHBinding) && _activeButton != btnSWCH)
            {
                return SystemButton.SWCH;
            }
            if (binding.Equals(F16DEDWriterForm.Instance.LGHTBinding) && _activeButton != btnLGHT)
            {
                return SystemButton.LGHT;
            }
            if (binding.Equals(F16DEDWriterForm.Instance.WXBinding) && _activeButton != btnWX)
            {
                return SystemButton.WX;
            }
            if (binding.Equals(F16DEDWriterForm.Instance.TIMEBinding) && _activeButton != btnTIME)
            {
                return SystemButton.TIME;
            }
            if (binding.Equals(F16DEDWriterForm.Instance.INFOBinding) && _activeButton != btnINFO)
            {
                return SystemButton.INFO;
            }
            if (binding.Equals(F16DEDWriterForm.Instance.HROTBinding) && _activeButton != btnHROT)
            {
                return SystemButton.HROT;
            }
            if (binding.Equals(F16DEDWriterForm.Instance.HDISBinding) && _activeButton != btnHDIS)
            {
                return SystemButton.HDIS;
            }
            if (binding.Equals(F16DEDWriterForm.Instance.HCTLBinding) && _activeButton != btnHCTL)
            {
                return SystemButton.HCTL;
            }
            if (binding.Equals(F16DEDWriterForm.Instance.CycleSystemUpBinding) && _activeButton != btnCycleSystemUp)
            {
                return SystemButton.CycleSystemUp;
            }
            if (binding.Equals(F16DEDWriterForm.Instance.CycleSystemDownBinding) && _activeButton != btnCycleSystemDown)
            {
                return SystemButton.CycleSystemDown;
            }
            if (binding.Equals(F16DEDWriterForm.Instance.CycleCustomUpBinding) && _activeButton != btnCycleCustomUp)
            {
                return SystemButton.CycleCustomUp;
            }
            if (binding.Equals(F16DEDWriterForm.Instance.CycleCustomDownBinding) && _activeButton != btnCycleCustomDown)
            {
                return SystemButton.CycleCustomDown;
            }
            if (binding.Equals(F16DEDWriterForm.Instance.BUGBinding) && _activeButton != btnBUG)
            {
                return SystemButton.SyncHeadingBug;
            }
            if (binding.Equals(F16DEDWriterForm.Instance.KollsmanBinding) && _activeButton != btnKollsman)
            {
                return SystemButton.SyncKollsman;
            }
            return SystemButton.None;
        }

        private void Joysticks_JoystickEvent(object sender, JoystickEventArgs e)
        {
            Ui(() =>
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
                            Action clear = alreadyBoundSystem switch
                            {
                                SystemButton.CNI => () => F16DEDWriterForm.Instance.CNIBinding = null,
                                SystemButton.COM1 => () => F16DEDWriterForm.Instance.COM1Binding = null,
                                SystemButton.COM2 => () => F16DEDWriterForm.Instance.COM2Binding = null,
                                SystemButton.IFF => () => F16DEDWriterForm.Instance.IFFBinding = null,
                                SystemButton.NAV1 => () => F16DEDWriterForm.Instance.NAV1Binding = null,
                                SystemButton.NAV2 => () => F16DEDWriterForm.Instance.NAV2Binding = null,
                                SystemButton.GPS => () => F16DEDWriterForm.Instance.GPSBinding = null,
                                SystemButton.FLPN => () => F16DEDWriterForm.Instance.FLPNBinding = null,
                                SystemButton.AP => () => F16DEDWriterForm.Instance.APBinding = null,
                                SystemButton.SWCH => () => F16DEDWriterForm.Instance.SWCHBinding = null,
                                SystemButton.LGHT => () => F16DEDWriterForm.Instance.LGHTBinding = null,
                                SystemButton.WX => () => F16DEDWriterForm.Instance.WXBinding = null,
                                SystemButton.TIME => () => F16DEDWriterForm.Instance.TIMEBinding = null,
                                SystemButton.INFO => () => F16DEDWriterForm.Instance.INFOBinding = null,
                                SystemButton.HROT => () => F16DEDWriterForm.Instance.HROTBinding = null,
                                SystemButton.HDIS => () => F16DEDWriterForm.Instance.HDISBinding = null,
                                SystemButton.HCTL => () => F16DEDWriterForm.Instance.HCTLBinding = null,
                                SystemButton.CycleSystemUp => () => F16DEDWriterForm.Instance.CycleSystemUpBinding = null,
                                SystemButton.CycleSystemDown => () => F16DEDWriterForm.Instance.CycleSystemDownBinding = null,
                                SystemButton.CycleCustomUp => () => F16DEDWriterForm.Instance.CycleCustomUpBinding = null,
                                SystemButton.CycleCustomDown => () => F16DEDWriterForm.Instance.CycleCustomDownBinding = null,
                                SystemButton.SyncHeadingBug => () => F16DEDWriterForm.Instance.BUGBinding = null,
                                SystemButton.SyncKollsman => () => F16DEDWriterForm.Instance.KollsmanBinding = null,
                                _ => () => { }
                            };
                            clear();
                        }
                        _capturingBinding = false;
                        _activeLabel.Text = binding.ToString();
                        _activeButton.Tag = binding;
                        _activeButton.Text = "&Unbind";
                        EnableButtons();
                        F16DEDWriterForm.Instance.CaptureJoystickEvents = true;
                    }
                }
             });
        }
    }
}
