﻿using System;
using System.Drawing;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Reflection.Emit;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SSLUtility2 {

    class CameraCommunicate {
        public static MainForm mainRef;

        static string failedConnectMsg = "Issue connecting to TCP Port\n" +
                    "Would you like to see more information?";
        static string failedConnectCaption = "Error";

        static IPAddress serverAddr = null;
        static Socket sock = new Socket(AddressFamily.Unspecified, SocketType.Stream, ProtocolType.Tcp);
        static IPEndPoint endPoint = new IPEndPoint(0, 0);

        public static string lastIPPort = "1";

        public static async Task<bool> sendtoIPAsync(byte[] code, Control lab, string ip = null, string port = null) {
            try {
                //if (ip == null) {
                //    ip = mainRef.tB_IPCon_Adr.Text;
                //    port = mainRef.tB_IPCon_Port.Text;
                //}
                if (!sock.Connected) {
                    bool ableToConnect = Connect(ip, port, lab, false).Result;
                    if (!ableToConnect) {
                        return false;
                    }
                }
                lastIPPort = ip + port;
                
                bool success = SendToSocket(code).Result;
                return success;
            } catch (Exception e) {
                MainForm.ShowError(failedConnectMsg, failedConnectCaption, e.ToString());
                return false;
            }
        }

        public static async Task<bool> Connect(string ipAdr, string port, Control lCon, bool stopError = false) {
            LabelDisplay(false, lCon);

            if (sock.Connected) {
                CloseSock();
            }
            if (!PingAdr(ipAdr).Result) {
                if (!stopError) {
                    MainForm.ShowError(failedConnectMsg, failedConnectCaption, ipAdr + ":" + port + " ping timed out with no response.");
                }
                return false;
            }
            LabelDisplay(true, lCon);

            serverAddr = IPAddress.Parse(ipAdr);
            sock = new Socket(serverAddr.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            endPoint = new IPEndPoint(serverAddr, Convert.ToInt32(port));
            sock.Connect(endPoint); //used to be async (maybe i can get it back at some point)
            return true;
        }

        public static void LabelDisplay(bool connected, Control l) {
            if (connected) {
                l.Text = "✓";
                l.ForeColor = Color.Green;
            } else {
                l.Text = "❌";
                l.ForeColor = Color.Red;
            }
        }

        public static async Task<bool> PingAdr(string address) {
            Ping pinger = null;

            if (address == null) {
                return false;
            }

            try {
                pinger = new Ping();
                PingReply reply = pinger.Send(address, 2);
                if (reply.Status == IPStatus.Success) {
                    return true;
                }
            } catch {
            } finally {
                pinger.Dispose();
            }
            return false;
        }

        private static async Task<bool> SendToSocket(byte[] code) {
            if (code != null) {
                sock.SendTo(code, endPoint);
                return true;
            }
            return false;
        }

        private static async Task ComWithSocket(byte[] code) {
            sock.SendTo(code, endPoint);
            //sock.BeginRe
        }


        public static void CloseSock() {
            if (sock != null) {
                sock.Shutdown(SocketShutdown.Both);
                sock.Close();
            }
        }

    }
}
