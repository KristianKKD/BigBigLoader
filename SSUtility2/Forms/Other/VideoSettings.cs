﻿using System;
using System.Windows.Forms;

namespace SSUtility2 {
    public partial class VideoSettings : Form {

        public Detached originalDetached;
        public bool isSecondary = false;

        public VideoSettings() {
            InitializeComponent();
        }

        private void VideoSettings_FormClosing(object sender, FormClosingEventArgs e) {
            if (e.CloseReason == CloseReason.UserClosing) {
                e.Cancel = true;
                Hide();
            }
        }

        private void checkB_PlayerD_Manual_CheckedChanged(object sender, EventArgs e) {
            if (checkB_PlayerD_Manual.Checked) {
                p_PlayerD_Simple.Hide();
                p_PlayerD_Extended.Show();
            } else {
                p_PlayerD_Extended.Hide();
                p_PlayerD_Simple.Show();
            }
        }

        public void Copy(VideoSettings sets, bool originalCopy = true) {
            tB_PlayerD_Adr.Text = sets.tB_PlayerD_Adr.Text;
            tB_PlayerD_Port.Text = sets.tB_PlayerD_Port.Text;
            tB_PlayerD_Username.Text = sets.tB_PlayerD_Username.Text;
            tB_PlayerD_Password.Text = sets.tB_PlayerD_Password.Text;
            cB_PlayerD_Type.Text = sets.cB_PlayerD_Type.Text;

            if (originalCopy) {
                tB_PlayerD_Name.Text = sets.tB_PlayerD_Name.Text + " 2";

                if (sets.cB_PlayerD_Type.Text.Contains("Daylight"))
                    tB_PlayerD_RTSP.Text = "videoinput_2:0/h264_1/onvif.stm";
                else
                    tB_PlayerD_RTSP.Text = "videoinput_1:0/h264_1/onvif.stm";

                tB_PlayerD_SimpleAdr.Text = originalDetached.secondView.GetCombined().ToString();
            } else {
                    tB_PlayerD_RTSP.Text = sets.tB_PlayerD_RTSP.Text;
            }

        }

        private void cB_PlayerD_Type_SelectedIndexChanged(object sender, EventArgs e) {
            string enc = cB_PlayerD_Type.Text;
            string username = "";
            string password = "";
            string rtsp = "";

            if (enc == "Daylight") {
                cB_PlayerD_Type.Text = "IONodes - Daylight";
                enc = cB_PlayerD_Type.Text;
            }

            if (enc == "IONodes - Daylight") {
                username = "admin";
                password = "admin";
                rtsp = "videoinput_1:0/h264_1/onvif.stm";
                if (originalDetached.secondView != null)
                    originalDetached.secondView.settings.tB_PlayerD_RTSP.Text = "videoinput_2:0/h264_1/onvif.stm";
            } else if (enc == "IONodes - Thermal") {
                username = "admin";
                password = "admin";
                rtsp = "videoinput_2:0/h264_1/onvif.stm";
                if (originalDetached.secondView != null)
                    originalDetached.secondView.settings.tB_PlayerD_RTSP.Text = "videoinput_1:0/h264_1/onvif.stm";
            } else if (enc == "VIVOTEK") {
                username = "root";
                password = "root1234";
                rtsp = "live.sdp";
            } else if (enc == "BOSCH") {
                username = "service";
                password = "Service123!";
                rtsp = "";
            }


            tB_PlayerD_RTSP.Text = rtsp;
            tB_PlayerD_Username.Text = username;
            tB_PlayerD_Password.Text = password;
        }

        private void b_Play_Click(object sender, EventArgs e) {
            if(isSecondary)
                originalDetached.Play(true, originalDetached.secondView);
            else
                originalDetached.StartPlaying(true);
        }

        private void VideoSettings_VisibleChanged(object sender, EventArgs e) {
            if (isSecondary)
                b_Hide.Show();
        }

        private void b_Hide_Click(object sender, EventArgs e) {
            MainForm.m.mainPlayer.secondView.VLCPlayer_D.playlist.stop();
            MainForm.m.mainPlayer.sP_Player.Hide();
            Hide();
            MainForm.m.Menu_Video_EnableSecondary.Visible = true;

        }
    }
}
