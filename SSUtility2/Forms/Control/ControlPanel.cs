﻿using System;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SSUtility2 {

    public partial class ControlPanel : Form {
    
        public Label l;

        public ControlPanel() {
            InitializeComponent();
            l = l_IPCon_Connected;
            Program.cp = this;
        }

        private void cB_IPCon_Type_SelectedIndexChanged(object sender, EventArgs e) {
            string control = cB_IPCon_Type.Text;
            string port = "";

            if (control == "Encoder") {
                port = "6791";
            } else if (control == "MOXA nPort") {
                port = "4001";
            }

            tB_IPCon_Port.Text = port;
        }

        private void b_Presets_GoTo_Click(object sender, EventArgs e) {
            if (!PresetsCheck())
                return;
            byte presetNumber = Convert.ToByte(tB_Presets_Number.Text);

            AsyncCamCom.SendNewCommand(D.protocol.Preset(MainForm.m.MakeAdr(), presetNumber, D.PresetAction.Goto));
        }

        private void b_Presets_Learn_Click(object sender, EventArgs e) { //can combine these 2 easily
            if (!PresetsCheck())
                return;
            byte presetNumber = Convert.ToByte(tB_Presets_Number.Text);

            AsyncCamCom.SendNewCommand(D.protocol.Preset(MainForm.m.MakeAdr(), presetNumber, D.PresetAction.Set));
        }

        bool PresetsCheck() {
            if (tB_Presets_Number.Text.ToString() == "") {
                return false;
            }
            if (!MainForm.CheckIfNameValid(tB_Presets_Number.Text, true)) {
                return false;
            }
            return true;
        }

        private void b_PTZ_Up_MouseDown(object sender, MouseEventArgs e) {
            PTZMove(D.Tilt.Up, D.Pan.Left);
        }

        private void b_PTZ_Down_MouseDown(object sender, MouseEventArgs e) {
            PTZMove(D.Tilt.Down, D.Pan.Left);
        }

        private void b_PTZ_Left_MouseDown(object sender, MouseEventArgs e) {
            PTZMove(D.Tilt.Null, D.Pan.Left);
        }

        private void b_PTZ_Right_MouseDown(object sender, MouseEventArgs e) {
            PTZMove(D.Tilt.Null, D.Pan.Right);
        }

        private void b_PTZ_ZoomPos_MouseDown(object sender, MouseEventArgs e) {
            PTZZoom(D.Zoom.Tele);
        }

        private void b_PTZ_ZoomNeg_MouseDown(object sender, MouseEventArgs e) {
            PTZZoom(D.Zoom.Wide);
        }
        private void b_PTZ_FocusPos_MouseDown(object sender, MouseEventArgs e) {
            AsyncCamCom.SendNonAsync(D.protocol.CameraFocus(MainForm.m.MakeAdr(cB_IPCon_Selected), D.Focus.Far));
        }

        private void b_PTZ_FocusNeg_MouseDown(object sender, MouseEventArgs e) {
            AsyncCamCom.SendNonAsync(D.protocol.CameraFocus(MainForm.m.MakeAdr(cB_IPCon_Selected), D.Focus.Near));
        }

        void PTZMove(D.Tilt tilt, D.Pan pan) {
            byte[] code;
            uint speed = Convert.ToUInt32(MainForm.m.ipCon.track_PTZ_PTSpeed.Value);

            if (tilt != D.Tilt.Null) {
                code = D.protocol.CameraTilt(MainForm.m.MakeAdr(), tilt, speed);
            } else {
                code = D.protocol.CameraPan(MainForm.m.MakeAdr(), pan, speed);
            }

            AsyncCamCom.SendNonAsync(code);
        }

        public void PTZZoom(D.Zoom dir) {
            AsyncCamCom.SendNonAsync(D.protocol.CameraZoom(MainForm.m.MakeAdr(), dir));
        }

        private void b_PTZ_Any_MouseUp(object sender, MouseEventArgs e) {
            DelayStop();
        }

        async Task DelayStop() {
            if (!AsyncCamCom.sock.Connected) {
                return;
            }
            AsyncCamCom.SendNonAsync(D.protocol.CameraStop(MainForm.m.MakeAdr()));
            await Task.Delay(100).ConfigureAwait(false);
            AsyncCamCom.SendNonAsync(D.protocol.CameraStop(MainForm.m.MakeAdr()));
        }

        private void tB_IPCon_Adr_Leave(object sender, EventArgs e) {
            AsyncCamCom.TryConnect(true);
        }

        private void tB_IPCon_Adr_KeyDown(object sender, KeyEventArgs e) { //make this better in future
            if (e.KeyCode == Keys.Enter) {
                AsyncCamCom.TryConnect(false);
            }
        }

        private void ControlPanel_Load(object sender, EventArgs e) {
            MainForm.m.setPage.PopulateSettingText();
        }

        public void StopCam() {
            if (cB_IPCon_KeyboardCon.Checked == true) {
                CustomScriptCommands.QuickCommand("stop");
            }
        }

        public void KeyControl(Keys k) { //test this
            if (cB_IPCon_KeyboardCon.Checked == true) {
                uint ptSpeed = Convert.ToUInt32(track_PTZ_PTSpeed.Value);
                byte[] code = null;
                uint address = MainForm.m.MakeAdr();

                switch (k) { //is there a command that accepts diagonal? // there is but its in byte codes // need to accept multiple keys
                    case Keys.Up:
                        code = D.protocol.CameraTilt(address, D.Tilt.Up, ptSpeed);
                        break;
                    case Keys.Down:
                        code = D.protocol.CameraTilt(address, D.Tilt.Down, ptSpeed);
                        break;
                    case Keys.Left:
                        code = D.protocol.CameraPan(address, D.Pan.Left, ptSpeed);
                        break;
                    case Keys.Right:
                        code = D.protocol.CameraPan(address, D.Pan.Right, ptSpeed);
                        break;
                    case Keys.Enter:
                        code = D.protocol.CameraZoom(address, D.Zoom.Tele);
                        break;
                    case Keys.Escape:
                        code = D.protocol.CameraZoom(address, D.Zoom.Wide);
                        break;
                }

                AsyncCamCom.SendNewCommand(code);
            }
        }

        private void track_IPCon_Zoom_MouseUp(object sender, MouseEventArgs e) {
            int zoomSpeed = track_IPCon_Zoom.Value;
            CustomScriptCommands.QuickCommand("setzoomspeed " + zoomSpeed.ToString());
        }

    }
}
