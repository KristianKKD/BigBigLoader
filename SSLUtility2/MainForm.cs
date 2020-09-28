﻿/*
 * Created by SharpDevelop.
 * User: Alistair
 * Date: 17/03/2018
 * Time: 16:58
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
/*
 * Based on ManagePTZ2 by Alistair Windram, with amendments by Filip Ionita
 * Form layout modelled on SSUTILITY Firmware tab for continuity
 */
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SSLUtility2
{
    public partial class MainForm : Form {

        public const string version = "v1.2.5.0";
        D protocol = new D();
        public static MainForm m;
        public bool lite = false;
        Control lab;

        public static Control[] saveList = new Control[0];

        const string defaultIP = "192.168.1.71";

        ControlPanel ipCon;

        public async Task StartupStuff() {
            m = this;
            lite = false;
            CameraCommunicate.mainRef = m;
            l_Version.Text = l_Version.Text + version;

            tC_Main.TabPages[1].Dispose(); //remove the firmware page

            AttachControlPanel();
            Detached playerL = AttachDetached(10);
            Detached playerR = AttachDetached(690);

            saveList = new Control[]{
                ipCon.cB_IPCon_Type,
                ipCon.tB_IPCon_Adr,
                ipCon.tB_IPCon_Port,
                ipCon.cB_IPCon_Selected,

                playerL.cB_PlayerD_Type,
                playerL.tB_PlayerD_Adr,
                playerL.tB_PlayerD_Port,
                playerL.tB_PlayerD_RTSP,
                playerL.tB_PlayerD_Buffering,
                playerL.tB_PlayerD_Username,
                playerL.tB_PlayerD_Password,
                playerL.tB_PlayerD_SimpleAdr,

                playerR.cB_PlayerD_Type,
                playerR.tB_PlayerD_Adr,
                playerR.tB_PlayerD_Port,
                playerR.tB_PlayerD_RTSP,
                playerR.tB_PlayerD_Buffering,
                playerR.tB_PlayerD_Username,
                playerR.tB_PlayerD_Password,
                playerR.tB_PlayerD_SimpleAdr,
            };

            FileStuff();

            PopulateSettingText();
            SetFeatureToAllControls(m.Controls);

            if (ipCon.tB_IPCon_Adr.Text != defaultIP) {
                CameraCommunicate.Connect(ipCon.tB_IPCon_Adr.Text, ipCon.tB_IPCon_Port.Text, lab, true);
            }
            
        }
        async Task FindVars() {
            foreach (ConfigVar v in ConfigControl.stringVarList) {
                if (v.value.ToLower() == "false" || v.value.ToLower() == "true") {
                    bool val = ConfigControl.CheckVal(v.value);
                    switch (v.name) {
                        case ConfigControl.subnetNotifVar:
                            ConfigControl.subnetNotif = val;
                            break;
                        case ConfigControl.configNotifVar:
                            ConfigControl.configNotif = val;
                            break;
                    }
                } else {
                    switch (v.name) {
                        case ConfigControl.screenshotFolderVar:
                            ConfigControl.scFolder = v.value;
                            break;
                        case ConfigControl.videoFolderVar:
                            ConfigControl.vFolder = v.value;
                            break;
                        case ConfigControl.scFileNVar:
                            ConfigControl.scFileName = v.value;
                            break;
                        case ConfigControl.videoFileNVar:
                            ConfigControl.vFileName = v.value;
                            break;
                        case ConfigControl.recQualVar:
                            ConfigControl.recQual = v.value;
                            break;
                        case ConfigControl.recFPSVar:
                            ConfigControl.recFPS = v.value;
                            break;
                    }
                }
            }
        }

        public static OpenFileDialog OpenTxt() {
            OpenFileDialog fileDlg = new OpenFileDialog();
            fileDlg.InitialDirectory = ConfigControl.savedFolder;
            fileDlg.Multiselect = false;
            fileDlg.DefaultExt = ".txt";
            fileDlg.Filter = "Text File (*.txt)|*.txt|All files (*.*)|*.*";
            fileDlg.FilterIndex = 1;
            fileDlg.RestoreDirectory = true;
            fileDlg.Title = "Select Text File";
            return fileDlg;
        }

        public static SaveFileDialog SaveTxt(string name) {
            SaveFileDialog fileDlg = new SaveFileDialog();
            fileDlg.InitialDirectory = ConfigControl.savedFolder;
            fileDlg.DefaultExt = ".txt";
            fileDlg.Filter = "Text File (*.txt)|*.txt|All files (*.*)|*.*";
            fileDlg.FilterIndex = 1;
            fileDlg.RestoreDirectory = true;
            fileDlg.Title = "Select Text File";
            fileDlg.FileName = name;
            return fileDlg;
        }

        async Task FileStuff() {

            ConfigControl.SetToDefaults();

            CheckCreateFile(ConfigControl.config, ConfigControl.appFolder);
            CheckCreateFile(ConfigControl.autoSave, ConfigControl.appFolder);
            CheckCreateFile(null, ConfigControl.scFolder);
            CheckCreateFile(null, ConfigControl.vFolder);
            CheckCreateFile(null, ConfigControl.savedFolder);

            await ConfigControl.SearchForVarsAsync(ConfigControl.appFolder + ConfigControl.config);
            FindVars();
            AutoSave.LoadAuto(ConfigControl.appFolder + ConfigControl.autoSave);
        }

        void AttachControlPanel() {
            TabPage tp = tC_Control.TabPages[0];
            ipCon = SpawnControlPanel(tp, false);
            ipCon.mainRef = m;
            PresetPanel pp = AttachPresetPanel(tp, ipCon);
            pp.cp = ipCon;

            ipCon.isOriginal = true;
            lab = ipCon.l_IPCon_Connected;
        }

        Detached AttachDetached(int xOffset) {
            TabPage tp = tC_Control.TabPages[0];
            GroupBox gb = new GroupBox();
            Detached d = DetachVid(false);

            var c = GetAllType(d, typeof(GroupBox));
            var c2 = GetAllType(d, typeof(Button));
            var c3 = GetAllType(d, typeof(Label));
            var c4 = GetAllType(d, typeof(AxAXVLC.AxVLCPlugin2));
            var c5 = GetAllType(d, typeof(CheckBox));

            gb.Controls.AddRange(c.ToArray());
            gb.Controls.AddRange(c2.ToArray());
            gb.Controls.AddRange(c3.ToArray());
            gb.Controls.AddRange(c4.ToArray());
            gb.Controls.AddRange(c5.ToArray());

            d.VLCPlayer_D.Location = new Point(d.VLCPlayer_D.Location.X, d.VLCPlayer_D.Location.Y + 5);
            gb.Size = new Size(d.Width - 18, d.Height - 40);
            gb.Location = new Point(ipCon.Location.X + ipCon.Size.Width + xOffset, ipCon.Location.Y + 25);

            tp.Controls.Add(gb);

            return d;
        }
       
        public void InitLiteMode() {
            TabPage tp = LiteMode();
            ControlPanel cp = SpawnControlPanel(tp);
            PresetPanel pp = AttachPresetPanel(tp, cp);
            pp.cp = cp;
            cp.isOriginal = false;
            cp.b_IPCon_LayoutMode.Text = "Swap To: Dual Mode";
            foreach (TabPage tab in tC_Control.TabPages) {
                if (tab != tp) {
                    tab.Dispose();
                }
            }
        }

        public TabPage LiteMode() {
            TabPage tp = new TabPage();
            tp.Text = "Camera Control";
            tC_Control.Controls.Add(tp);
            m.Size = new Size(300, 1000);
            AutoSave.SaveAuto(ConfigControl.appFolder + ConfigControl.autoSave);
            lite = true;
            return tp;
        }

        public Detached DetachVid(bool show) {
            Detached dv = new Detached();
            if (show) {
                dv.Show();
            }
            dv.mainRef = m;
            SetFeatureToAllControls(dv.Controls);
            return dv;
        }

        ControlPanel SpawnControlPanel(TabPage tp, bool makeLite = true) {
            GroupBox gb = new GroupBox();
            ControlPanel cp = new ControlPanel();

            if (makeLite) {
                cp.mainRef = m;
                cp.pathToAuto = ConfigControl.appFolder + ConfigControl.autoSave;
                cp.cB_IPCon_Type.Text = ipCon.cB_IPCon_Type.Text;
                cp.tB_IPCon_Adr.Text = ipCon.tB_IPCon_Adr.Text;
                cp.tB_IPCon_Port.Text = ipCon.tB_IPCon_Port.Text;
                cp.cB_IPCon_Selected.Text = ipCon.cB_IPCon_Selected.Text;
            }


            SetFeatureToAllControls(cp.Controls);

            cp.StartConnect();

            gb.Size = new Size(cp.Size.Width, cp.Size.Height - 30);
            tp.Controls.Add(gb);

            AddControls(gb, cp);
            return cp;
        }

        PresetPanel AttachPresetPanel(TabPage tp, ControlPanel panel) {
            GroupBox gb = new GroupBox();
            PresetPanel pp = new PresetPanel();
            
            SetFeatureToAllControls(pp.Controls);
            pp.mainRef = m;
            panel.SendToBack();

            gb.Location = new Point(0, panel.Size.Height - 40);
            gb.Size = pp.Size;

            tp.Controls.Add(gb);

            var c = GetAllType(pp, typeof(TabControl));
            var cTwo = GetAllType(pp, typeof(Label));
            gb.Controls.AddRange(c.ToArray());
            gb.Controls.AddRange(cTwo.ToArray());
            return pp;
        }

        void AddControls(GroupBox g, Control panel) {
            var c = GetAll(panel);
            g.Controls.AddRange(c.ToArray());
        }

        public IEnumerable<Control> GetAll(Control control) {
            var controls = control.Controls.Cast<Control>();

            return controls.SelectMany(ctrl => GetAll(ctrl))
                                      .Concat(controls);
        }

        public IEnumerable<Control> GetAllType(Control control, Type type) {
            var controls = control.Controls.Cast<Control>();

            return controls.SelectMany(ctrl => GetAllType(ctrl, type))
                                      .Concat(controls)
                                      .Where(c => c.GetType() == type);
        }


        async Task<bool> CheckFinishedTypingPath(TextBox tb, Label linkLabel) {
            if (tb.Text.Length < 1) {
                tb.Text = ConfigControl.appFolder;
                return false;
            }
            if (ConfigControl.CheckIfExists(tb, linkLabel).Result) {
                return true;
            }
            return false;
        }

        public static bool ShowError(string message, string caption, string error, bool showError = true) {
            bool res = false;
            DialogResult d = MessageBox.Show(message, caption,
                                MessageBoxButtons.YesNo,
                                MessageBoxIcon.Question);
            if (d == DialogResult.Yes) {
                res = true;
                if (showError) {
                    MessageBox.Show(error, caption, MessageBoxButtons.OK);
                }
            }
            return res;
        }

        public Recorder StartRec(AxAXVLC.AxVLCPlugin2 player) {
            Recorder rec = new Recorder(new Record(tB_Paths_vFolder.Text + ConfigControl.vFileName +
                (Directory.GetFiles(ConfigControl.vFolder).Length + 1) + ".avi", int.Parse(ConfigControl.recFPS),
                 SharpAvi.KnownFourCCs.Codecs.MotionJpeg, int.Parse(ConfigControl.recQual), player));//add quality and framerate changing too 
            return rec;
        }

        public void StopRec(Recorder r) {
            r.Dispose();
        }

        public (bool, Recorder) StopStartRec(bool isPlaying, AxAXVLC.AxVLCPlugin2 player, Button control, Recorder r) {
            if (isPlaying) {
                control.Text = "START Recording";
                isPlaying = false;
                StopRec(r);
                MessageBox.Show("Saved recording to: " + ConfigControl.appFolder + ConfigControl.vFolder);
                return (isPlaying, null);
            } else {
                control.Text = "STOP Recording";
                isPlaying = true;
                return (isPlaying, StartRec(player));
            }
        }

        public uint MakeAdr(Control comboBox = null) {
            if (comboBox == null) {
                comboBox = ipCon.cB_IPCon_Selected;
            }
            if (comboBox.Text == "Daylight") {
                return 1;
            } else {
                return 2;
            }
        }

        public void Play(AxAXVLC.AxVLCPlugin2 player, string combinedUrl, TextBox linkedTB, string buffering) {
            if (combinedUrl == "") {
                MessageBox.Show("Address is invalid!");
                return;
            }
            if (CameraCommunicate.PingAdr(combinedUrl).Result) {
                MessageBox.Show("Address could not be pinged!");
                return;
            }
            linkedTB.Text = combinedUrl;
            Replay(player, combinedUrl, buffering);
        }

        public void Replay(AxAXVLC.AxVLCPlugin2 player, string combinedUrl, string buffering) {
            if (player.playlist.isPlaying) {
                player.playlist.stop();
                player.playlist.items.clear();
            }

            player.playlist.add(combinedUrl, null, ":network-caching=" +buffering);
            player.playlist.next();
            player.playlist.play();
        }

        public void ExtendOptions(bool check, GroupBox gbExt, GroupBox gbSim) {
            if (check) {
                gbSim.Hide();
                gbExt.Show();
            } else {
                gbExt.Hide();
                gbSim.Show();
            }
        }

        private void BrowseFolderButton(TextBox tb) {
            FolderBrowserDialog folderDlg = new FolderBrowserDialog();
            folderDlg.ShowNewFolderButton = true;
            DialogResult result = folderDlg.ShowDialog();
            if (result == DialogResult.OK) {
                tb.Text = folderDlg.SelectedPath;
            }
        }

        private async Task ApplyAll() { //
            ConfigControl.CreateConfig(ConfigControl.appFolder + ConfigControl.config);
            MessageBox.Show("Applied settings to: " + ConfigControl.appFolder + ConfigControl.config);
        }
        private void b_Settings_Default_Click(object sender, EventArgs e) {
            DialogResult d = MessageBox.Show("Are you sure you want to reset all settings? \n" +
                "Settings will not automatically be applied so the user may edit the defaults before applying.",
                "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (d == DialogResult.Yes) {
                ConfigControl.SetToDefaults();
                PopulateSettingText();
            }
        }

        public void SetFeatureToAllControls(Control.ControlCollection cc) {
            if (cc != null) {
                foreach (Control control in cc) {
                    if (control != tC_Control) {
                        control.PreviewKeyDown += new PreviewKeyDownEventHandler(control_PreviewKeyDown);
                    }
                    SetFeatureToAllControls(control.Controls);
                }
            }
        }

        public void control_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e) {
            if (e.KeyCode == Keys.Up || e.KeyCode == Keys.Down ||
                e.KeyCode == Keys.Left || e.KeyCode == Keys.Right ||
                e.KeyCode == Keys.Escape || e.KeyCode == Keys.Enter) {
                e.IsInputKey = true;
            }
        }

        public void PTZMove(bool IsTilt, uint address, uint speed,
            D.Tilt tilt = D.Tilt.Up, D.Pan pan = D.Pan.Left,
            string ip = null, string port = null, Control lcon = null) {
            byte[] code;

            if (CameraCommunicate.lastIPPort != ip + port) {
                CameraCommunicate.Connect(ip, port, lcon);
            }

            if (IsTilt) {
                code = protocol.CameraTilt(address, tilt, speed);
            } else {
                code = protocol.CameraPan(address, pan, speed);
            }

            CameraCommunicate.sendtoIPAsync(code, lab, ip, port);
        }

        public void PTZZoom(D.Zoom dir, uint address, string ip, string port, Control lcon) {
            if (CameraCommunicate.lastIPPort != ip + port) {
                CameraCommunicate.Connect(ip, port, lcon);
            }

            CameraCommunicate.sendtoIPAsync(protocol.CameraZoom(address, dir), lcon, ip, port);
        }


        public async Task SaveSnap(AxAXVLC.AxVLCPlugin2 player) {
            var imagePath = ConfigControl.scFolder + @"\" + ConfigControl.scFileName + (Directory.GetFiles(ConfigControl.scFolder).Length + 1) + ".jpg";
            
            Image bmp = new Bitmap(player.Width, player.Height);
            Graphics gfx = Graphics.FromImage(bmp);
            Rectangle rec = player.RectangleToScreen(player.ClientRectangle);
            gfx.CopyFromScreen(rec.Location, Point.Empty, player.Size);

            bmp.Save(imagePath, ImageFormat.Jpeg);

            MessageBox.Show("Image saved : " + ConfigControl.scFolder);
        }

        static async Task<bool> CheckIfNameValid(char[] nameArray) {
            foreach (Char c in nameArray) {
                foreach (Char symbol in Path.GetInvalidFileNameChars()) {
                    if (c == symbol && c.ToString() != ":" && c.ToString() != "\\") {
                        ShowError("Invalid character detected, Show more?", "Cannot create file",
                        "Do not use invalid symbols in file names.\nInvalid character found: " + c);
                        return false;
                    }
                }
            }
            return true;
        }

        static async Task CheckCreateFile(string fileName, string folderName = null) {
            CheckIfNameValid((fileName + folderName).ToCharArray());

            if (folderName != null) {
                if (!Directory.Exists(folderName)) {
                    Directory.CreateDirectory(folderName);
                }
            }
            if (fileName != null) {
                if (!File.Exists(ConfigControl.appFolder + fileName)) {
                    if (ConfigControl.appFolder + fileName == ConfigControl.appFolder + ConfigControl.config) {
                        ConfigControl.CreateConfig(ConfigControl.appFolder + fileName);
                    } else {
                        var newFile = File.Create(ConfigControl.appFolder + fileName);
                        newFile.Close();
                    }
                }
            }
        }

        public async Task PopulateSettingText() {
            tB_Paths_sCFolder.Text = ConfigControl.scFolder;
            tB_Paths_vFolder.Text = ConfigControl.vFolder;

            tB_Rec_vFileN.Text = ConfigControl.vFileName;
            tB_Rec_scFileN.Text = ConfigControl.scFileName;

            cB_Rec_Quality.Text = ConfigControl.recQual;
            cB_Rec_FPS.Text = ConfigControl.recFPS;

            check_Not_Subnet.Checked = ConfigControl.subnetNotif;
            check_Not_Config.Checked = ConfigControl.configNotif;
        }

        public PelcoD OpenPelco(string ip, string port, string selected) {
            PelcoD pd = new PelcoD();
            pd.mainRef = m;
            pd.tB_IPCon_Adr.Text = ip;
            pd.tB_IPCon_Port.Text = port;
            pd.cB_IPCon_Selected.Text = selected;
            pd.Show();
            SetFeatureToAllControls(pd.Controls);
            return pd;
        }

        private void OnFinishedTypingScFolder(object sender, EventArgs e) {
            if (CheckFinishedTypingPath(tB_Paths_sCFolder, l_Paths_sCCheck).Result) {
                ConfigControl.scFolder = tB_Paths_sCFolder.Text;
            }
        }

        private void OnFinishedTypingVFolder(object sender, EventArgs e) {
            if (CheckFinishedTypingPath(tB_Paths_vFolder, l_Paths_vCheck).Result) {
                ConfigControl.vFolder = tB_Paths_vFolder.Text;
            }
        }

        private void check_Not_Subnet_CheckedChanged(object sender, EventArgs e) {
            ConfigControl.subnetNotif = check_Not_Subnet.Checked;
        }

        private void check_Not_Config_CheckedChanged(object sender, EventArgs e) {
            ConfigControl.configNotif = check_Not_Config.Checked;
        }

      
        private void b_Paths_vBrowse_Click(object sender, EventArgs e) {
            BrowseFolderButton(tB_Paths_vFolder);
        }

        private void tB_Rec_vFileN_TextChanged(object sender, EventArgs e) {
            if (CheckIfNameValid(tB_Rec_vFileN.Text.ToCharArray()).Result) {
                ConfigControl.vFileName = tB_Rec_vFileN.Text;
            } else {
                tB_Rec_vFileN.Text = ConfigControl.vFileName;
            }
        }

        private void tB_Rec_sCFileN_TextChanged(object sender, EventArgs e) {
            if (CheckIfNameValid(tB_Rec_scFileN.Text.ToCharArray()).Result) {
                ConfigControl.scFileName = tB_Rec_scFileN.Text;
            } else {
                tB_Rec_vFileN.Text = ConfigControl.scFileName;
            }
        }

        private void cB_Rec_Quality_TextChanged(object sender, EventArgs e) {
            if (!int.TryParse(cB_Rec_Quality.Text, out int q)) {
                cB_Rec_Quality.Text = q.ToString();
                return;
            }
            if (q > 100) {
                cB_Rec_Quality.Text = "100";
            } else if (q < 1) {
                cB_Rec_Quality.Text = "1";
            }

            ConfigControl.recQual = cB_Rec_Quality.Text;
        }

        private void cB_Rec_FPS_TextChanged(object sender, EventArgs e) {
            if (!int.TryParse(cB_Rec_Quality.Text, out int fps)) {
                cB_Rec_FPS.Text = fps.ToString();
                return;
            }
            if (fps < 1) {
                cB_Rec_FPS.Text = "1";
            }

            ConfigControl.recFPS = cB_Rec_FPS.Text;
        }

        private void b_Paths_sCBrowse_Click(object sender, EventArgs e) {
            BrowseFolderButton(tB_Paths_sCFolder);
        }

        private void b_Settings_Apply_Click(object sender, EventArgs e) {
            ApplyAll();
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e) {
            CameraCommunicate.CloseSock();
            if (!lite) {
                AutoSave.SaveAuto(ConfigControl.appFolder + ConfigControl.autoSave);
            }
        }

    } // end of class MainForm
} // end of namespace SSLUtility2
