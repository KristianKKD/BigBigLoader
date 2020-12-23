﻿using System;
using System.Threading.Tasks;

namespace SSLUtility2 {
    public class ScriptCommand {
        public string[] names;
        public byte[] codeContent;
        public string description;
        public bool custom;
        public bool valueAccepting;

        public ScriptCommand(string[] n, byte[] code, string text, bool value = false, bool scriptCommand = false) {
            names = n;
            codeContent = code;
            description = text;
            custom = scriptCommand;
            valueAccepting = value;
        }
    }

    public class CustomScriptCommands {

        public static ScriptCommand[] cameraCommands = new ScriptCommand[]{
            new ScriptCommand(new string[] {"pause", "wait"}, PelcoD.pause, "Pause the script execution for X milliseconds", true, true),

            new ScriptCommand(new string[] {"stop"}, new byte[] { 0x00, 0x00, 0x00, 0x00 }, "Stops whatever the camera is doing"),
            new ScriptCommand(new string[] {"mono", "monocolour", "monocolor"}, new byte[] { 0x00, 0x07, 0x00, 0x03 }, "Camera video toggles between color and black/white pallete"),
            new ScriptCommand(new string[] {"panzero", "zeropan", "azimuth"}, new byte[] { 0x00, 0x49, 0x00, 0x00 }, "Sets camera pan to zero"),

            new ScriptCommand(new string[] {"setzoomspeed"}, new byte[] { 0x00, 0x25, 0x00, 0x00 }, "Sets camera zoom speed to X (DATA 2)", true),
            new ScriptCommand(new string[] {"setpantiltspeed"}, new byte[] { 0x00, 0x4B, 0x00, 0x00 }, "Sets camera pan and tilt speed to X (DATA 2)", true),
            new ScriptCommand(new string[] {"setpanpos"}, new byte[] { 0x00, 0x4B, 0x00, 0x00 }, "Sets camera pan position to X (DATA 1 & DATA 2)", true),
            new ScriptCommand(new string[] {"settiltpos"}, new byte[] { 0x00, 0x4D, 0x00, 0x00 }, "Sets camera tilt position to X (DATA 1 & DATA 2)", true),

            new ScriptCommand(new string[] {"querytilt"}, new byte[] { 0x00, 0x51, 0x00, 0x00 }, "Returns camera tilt position"),
            new ScriptCommand(new string[] {"querypan"}, new byte[] { 0x00, 0x53, 0x00, 0x00 }, "Returns camera pan position"),
            new ScriptCommand(new string[] {"queryzoom", "queryzoom"}, new byte[] { 0x00, 0x55, 0x00, 0x00 }, "Returns camera FOV"),
            new ScriptCommand(new string[] {"queryfocus"}, new byte[] { 0x01, 0x55, 0x00, 0x00 }, "Returns camera focus value"),
            new ScriptCommand(new string[] {"querypost"}, new byte[] { 0x07, 0x6B, 0x00, 0x00 }, "Returns camera test data"),
            new ScriptCommand(new string[] {"queryconfig"}, new byte[] { 0x03, 0x6B, 0x00, 0x00 }, "Returns camera config"),
        };

        public static async Task<byte[]> CheckForCommands(string line, uint adr) {
            byte[] code = CheckForPresets(line).Result;
            code = RefineCode(code, adr, CheckForVal(line)).Result;

            return code;
        }

        static int CheckForVal(string line) {
            int value = 0;
            int marker = line.IndexOf(" ");

            if (marker != -1) {
                value = int.Parse(line.Substring(marker + 1));
            }

            return value;
        }

        static async Task<byte[]> RefineCode(byte[] code, uint adr, int value) {
            if (code == PelcoD.pause) {
                MainForm.m.WriteToResponses("Waiting: " + value.ToString() + "ms", true);
                await Task.Delay(value).ConfigureAwait(false);
            }

            if (code == PelcoD.noCommand || code == PelcoD.pause) {
                return code;
            }

            if (value > 255) { //need to test
                code[2] = 0x01;
                value -= 255;
            }
            if (code[3] == 0x00) {
                code[3] = Convert.ToByte(value);
            }

            uint checksum = GetCheckSum(code, adr, value);

            code = new byte[] { 0xFF, (byte)adr, code[0], code[1], code[2], code[3], (byte)checksum };

            return code;
        }

        public static uint GetCheckSum(byte[] code, uint adr, int value) {
            uint checksum = 0;
            for (int i = 0; i < code.Length; i++) {
                checksum += code[i];
            }
            checksum += adr;
            checksum += Convert.ToByte(value);
            checksum = checksum % 256;

            return checksum;
        }

        public static async Task<byte[]> CheckForPresets(string line) {
            string start = line;

            int markerPos = line.IndexOf(" ");

            if (markerPos > 0) {
                start = line.Substring(0, markerPos);
                start = start.Trim();
            }

            for (int i = 0; i < cameraCommands.Length; i++) {
                for (int x = 0; x < cameraCommands[i].names.Length; x++) {
                    if (cameraCommands[i].names[x] == start) {
                        return cameraCommands[i].codeContent;
                    }
                }
            }

            return PelcoD.noCommand;
        }

        public static async Task QuickCommand(string command) {
            if (CameraCommunicate.Connect(MainForm.m.ipCon.l_IPCon_Adr.Text, MainForm.m.ipCon.l_IPCon_Port.Text, MainForm.m.ipCon.l_IPCon_Connected, true).Result) {
                byte[] code = CheckForCommands(command, MainForm.m.MakeAdr(MainForm.m.ipCon.cB_IPCon_Selected)).Result;
                CameraCommunicate.sendtoIPAsync(code);
            }
        }


    }
}
