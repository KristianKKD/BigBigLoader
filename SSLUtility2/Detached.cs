﻿using System;
using System.Windows.Forms;

namespace SSLUtility2 {

    public partial class Detached : Form {

        public MainForm MainRef { get;
            set; }

        public Detached() {
            InitializeComponent();
        }

        private void b_PlayerD_Play_Click(object sender, EventArgs e) {
            string combinedUrl;

            if (checkB_PlayerD_Manual.Checked) { //make a function to automatically grab these from gB_...
                string ipaddress = tB_PlayerD_Adr.Text; //is it possible? the variables need to be in an order
                string port = tB_PlayerD_Port.Text;
                string url = tB_PlayerD_RTSP.Text;
                string username = tB_PlayerD_Username.Text;
                string password = tB_PlayerD_Password.Text;

                combinedUrl = "rtsp://" + username + ":" + password + "@" + ipaddress + ":" + port + "/" + url;
            } else {
                combinedUrl = tB_PlayerD_SimpleAdr.Text;
            }

            MainRef.Play(VLCPlayer_D, combinedUrl, tB_PlayerD_SimpleAdr);
        }

        private void b_PlayerD_SaveSnap_Click(object sender, EventArgs e) {
            MainRef.SaveSnap(VLCPlayer_D);
        }

        private void checkB_PlayerD_Manual_CheckedChanged(object sender, EventArgs e) {
            MainRef.ExtendOptions(checkB_PlayerD_Manual.Checked, gB_PlayerD_Extended, gB_PlayerD_Simple);
        }

        bool Dplaying = false;
        Recorder recorderD;

        private void b_PlayerD_StartRec_Click(object sender, EventArgs e) {
            (bool, Recorder) vals = MainRef.StopStartRec(Dplaying, VLCPlayer_D, b_PlayerD_StartRec, recorderD);
            Dplaying = vals.Item1;
            recorderD = vals.Item2;
        }
        private void b_PlayerD_PauseRec_Click(object sender, EventArgs e) {
            //MainRef.Pause
        }

        private void cB_PlayerD_Type_SelectedIndexChanged(object sender, EventArgs e) {
            string enc = cB_PlayerD_Type.Text;
            string username = "";
            string password = "";
            string rtsp = "";

            if (enc == "IONodes - Daylight") {
                username = "admin";
                password = "admin";
                rtsp = "videoinput_1:0/h264_1/onvif.stm";
            } else if (enc == "IONodes - Thermal") {
                username = "admin";
                password = "admin";
                rtsp = "videoinput_2:0/h264_1/onvif.stm";
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
    }
}