/**
 * Radegast Metaverse Client
 * Copyright(c) 2009-2014, Radegast Development Team
 * Copyright(c) 2016-2020, Sjofn, LLC
 * All rights reserved.
 *  
 * Radegast is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Lesser General Public License as published
 * by the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU Lesser General Public License
 * along with this program.If not, see<https://www.gnu.org/licenses/>.
 */

using UniRx;
using System;
using System.Collections;
//using System.Drawing;
using System.Text;
using System.IO;
using Raindrop.Netcom;
using OpenMetaverse;
using OpenMetaverse.StructuredData;
using Newtonsoft.Json;
//using System.Web.Script.Serialization;
using System.Collections.Generic;
using UnityEngine;
using Color = SixLabors.ImageSharp.Color;

namespace Raindrop
{
    //this class offers the GUI to have the ability to handle messages, via composition.
    public class IMTextManager
    {
        public bool DingOnAllIncoming = false;

        RaindropInstance instance;
        RaindropNetcom netcom => instance.Netcom;
        IMTextManagerType Type;
        string sessionName;
        bool AutoResponseSent = false;
        ArrayList textBuffer;

        public ReactiveCollection<object> textBufferReactive;

        public static Dictionary<string, Settings.FontSetting> fontSettings = new Dictionary<string, Settings.FontSetting>();

        public IMTextManager(RaindropInstance instance, ITextPrinter textPrinter, IMTextManagerType type, UUID sessionID, string sessionName)
        {
            SessionID = sessionID;
            this.sessionName = sessionName;
            TextPrinter = textPrinter;
            textBuffer = new ArrayList();
            textBufferReactive = new ReactiveCollection<object>();
            Type = type;

            this.instance = instance;
            PrintLastLog();
            AddNetcomEvents();
            InitializeConfig();

        }

        private void InitializeConfig()
        {
            Settings s = instance.GlobalSettings;
            //var serializer = new JavaScriptSerializer();
            //var serializer = new JsonSerializer();

            if (s["im_timestamps"].Type == OSDType.Unknown)
            {
                s["im_timestamps"] = OSD.FromBoolean(true);
            }
            //if (s["chat_fonts"].Type == OSDType.Unknown)
            //{
            //    try
            //    {
            //        s["chat_fonts"] = JsonConvert.SerializeObject(Settings.DefaultFontSettings);
            //    }
            //    catch (Exception ex)
            //    {
            //        Debug.LogError("Failed to save default font settings: " + ex.Message);
            //        //System.Windows.Forms.MessageBox.Show("Failed to save default font settings: " + ex.Message);
            //    }
            //}

            try
            {
                fontSettings = JsonConvert.DeserializeObject<Dictionary<string, Settings.FontSetting>>(s["chat_fonts"]);
            }
            catch (Exception ex)
            {
                OpenMetaverse.Logger.Log("Failed to read chat font settings: " + ex.Message, Helpers.LogLevel.Error);

                //System.Windows.Forms.MessageBox.Show("Failed to read chat font settings: " + ex.Message);
            }

            ShowTimestamps = s["im_timestamps"].AsBoolean();

            s.OnSettingChanged += new Settings.SettingChangedCallback(s_OnSettingChanged);
        }

        void s_OnSettingChanged(object sender, SettingsEventArgs e)
        {
            if (e.Key == "im_timestamps" && e.Value != null)
            {
                ShowTimestamps = e.Value.AsBoolean();
                ReprintAllText();
            }
            else if (e.Key == "chat_fonts")
            {
                try
                {
                    //var serializer = new JavaScriptSerializer();
                    fontSettings = JsonConvert.DeserializeObject<Dictionary<string, Settings.FontSetting>>(e.Value);
                }
                catch (Exception ex)
                {
                    OpenMetaverse.Logger.Log("Failed to read new font settings: " + ex.Message , Helpers.LogLevel.Error);
                    //System.Windows.Forms.MessageBox.Show("Failed to read new font settings: " + ex.Message);
                }
                ReprintAllText();
            }
        }

        private void AddNetcomEvents()
        {
            netcom.InstantMessageReceived += new EventHandler<InstantMessageEventArgs>(netcom_InstantMessageReceived);
            netcom.InstantMessageSent += new EventHandler<InstantMessageSentEventArgs>(netcom_InstantMessageSent);
        }

        private void RemoveNetcomEvents()
        {
            netcom.InstantMessageReceived -= netcom_InstantMessageReceived;
            netcom.InstantMessageSent -= netcom_InstantMessageSent;
        }

        private void netcom_InstantMessageSent(object sender, InstantMessageSentEventArgs e)
        {
            if (e.SessionID != SessionID) return;

            textBuffer.Add(e);
            textBufferReactive.Add(e);
            ProcessIM(e, true);
        }

        private void netcom_InstantMessageReceived(object sender, InstantMessageEventArgs e)
        {
            if (e.IM.IMSessionID != SessionID) return;
            if (e.IM.Dialog == InstantMessageDialog.StartTyping ||
                e.IM.Dialog == InstantMessageDialog.StopTyping)
                return;

            if (instance.Client.Self.MuteList.Find(me => me.Type == MuteType.Resident && me.ID == e.IM.FromAgentID) != null)
            {
                return;
            }

            textBuffer.Add(e);
            textBufferReactive.Add(e);
            ProcessIM(e, true);
        }

        public void ProcessIM(object e, bool isNewMessage)
        {
            if (e is InstantMessageEventArgs)
                ProcessIncomingIM((InstantMessageEventArgs)e, isNewMessage);
            else if (e is InstantMessageSentEventArgs)
                ProcessOutgoingIM((InstantMessageSentEventArgs)e, isNewMessage);
        }

        private void ProcessOutgoingIM(InstantMessageSentEventArgs e, bool isNewMessage)
        {
            PrintIM(e.Timestamp, netcom.LoginOptions.FullName, instance.Client.Self.AgentID, e.Message, isNewMessage);
        }

        private void ProcessIncomingIM(InstantMessageEventArgs e, bool isNewMessage)
        {
            string msg = e.IM.Message;

            //if (instance.RLV.RestictionActive("recvim", e.IM.FromAgentID.ToString()))
            //{
            //    msg = "*** IM blocked by your viewer";

            //    if (Type == IMTextManagerType.Agent)
            //    {
            //        instance.Client.Self.InstantMessage(instance.Client.Self.Name,
            //                e.IM.FromAgentID,
            //                "***  The Resident you messaged is prevented from reading your instant messages at the moment, please try again later.",
            //                e.IM.IMSessionID,
            //                InstantMessageDialog.BusyAutoResponse,
            //                InstantMessageOnline.Offline,
            //                instance.Client.Self.RelativePosition,
            //                instance.Client.Network.CurrentSim.ID,
            //                null);
            //    }
            //}

            if (DingOnAllIncoming)
            {
                //instance.MediaManager.PlayUISound(UISounds.IM);
            }
            PrintIM(DateTime.Now, instance.Names.Get(e.IM.FromAgentID, e.IM.FromAgentName), e.IM.FromAgentID, msg, isNewMessage);

            //if (!AutoResponseSent && Type == IMTextManagerType.Agent && e.IM.FromAgentID != UUID.Zero && e.IM.FromAgentName != "Second Life")
            //{
            //    bool autoRespond = false;
            //    AutoResponseType art = (AutoResponseType)instance.GlobalSettings["auto_response_type"].AsInteger();

            //    switch (art)
            //    {
            //        case AutoResponseType.WhenBusy: autoRespond = instance.State.IsBusy; break;
            //        case AutoResponseType.WhenFromNonFriend: autoRespond = !instance.Client.Friends.FriendList.ContainsKey(e.IM.FromAgentID); break;
            //        case AutoResponseType.Always: autoRespond = true; break;
            //    }

            //    if (autoRespond)
            //    {
            //        AutoResponseSent = true;
            //        instance.Client.Self.InstantMessage(instance.Client.Self.Name,
            //            e.IM.FromAgentID,
            //            instance.GlobalSettings["auto_response_text"].AsString(),
            //            e.IM.IMSessionID,
            //            InstantMessageDialog.BusyAutoResponse,
            //            InstantMessageOnline.Online,
            //            instance.Client.Self.RelativePosition,
            //            instance.Client.Network.CurrentSim.ID,
            //            null);

            //        PrintIM(DateTime.Now, instance.Client.Self.Name, instance.Client.Self.AgentID, instance.GlobalSettings["auto_response_text"].AsString(), isNewMessage);
            //    }
            //}
        }

        public void DisplayNotification(string message)
        {
            notifyObservers("notification", message);
            //if (instance.MainForm.InvokeRequired)
            //{
            //    instance.MainForm.Invoke(new System.Windows.Forms.MethodInvoker(() => DisplayNotification(message)));
            //    return;
            //}

            //todo

            //if (ShowTimestamps)
            //{
            //    if (fontSettings.ContainsKey("Timestamp"))
            //    {
            //        var fontSetting = fontSettings["Timestamp"];
            //        TextPrinter.ForeColor = fontSetting.ForeColor;
            //        TextPrinter.BackColor = fontSetting.BackColor;
            //        TextPrinter.Font = fontSetting.Font;
            //        TextPrinter.PrintText(DateTime.Now.ToString("[HH:mm] "));
            //    }
            //    else
            //    {
            //        TextPrinter.ForeColor = SystemColors.GrayText;
            //        TextPrinter.BackColor = Color.Transparent;
            //        TextPrinter.Font = Settings.FontSetting.DefaultFont;
            //        TextPrinter.PrintText(DateTime.Now.ToString("[HH:mm] "));
            //    }
            //}

            //if (fontSettings.ContainsKey("Notification"))
            //{
            //    var fontSetting = fontSettings["Notification"];
            //    TextPrinter.ForeColor = fontSetting.ForeColor;
            //    TextPrinter.BackColor = fontSetting.BackColor;
            //    TextPrinter.Font = fontSetting.Font;
            //}
            //else
            //{
            //    TextPrinter.ForeColor = Color.DarkCyan;
            //    TextPrinter.BackColor = Color.Transparent;
            //    TextPrinter.Font = Settings.FontSetting.DefaultFont;
            //}

            instance.LogClientMessage(sessionName + ".txt", message);
            TextPrinter.PrintTextLine(message);
        }

        private void notifyObservers(string v, string message)
        {
            throw new NotImplementedException();
        }

        private void PrintIM(DateTime timestamp, string fromName, UUID fromID, string message, bool isNewMessage)
        {
            if (ShowTimestamps)
            {
                if (fontSettings.ContainsKey("Timestamp"))
                {
                    var fontSetting = fontSettings["Timestamp"];
                    //TextPrinter.ForeColor = fontSetting.ForeColor;
                    //TextPrinter.BackColor = fontSetting.BackColor;
                    //TextPrinter.Font = fontSetting.Font;
                    TextPrinter.PrintText(DateTime.Now.ToString("[HH:mm] "));
                }
                else
                {
                    //TextPrinter.ForeColor = SystemColors.GrayText;
                    //TextPrinter.BackColor = Color.Transparent;
                    //TextPrinter.Font = Settings.FontSetting.DefaultFont;
                    TextPrinter.PrintText(DateTime.Now.ToString("[HH:mm] "));
                }
            }

            if (fontSettings.ContainsKey("Name"))
            {
                var fontSetting = fontSettings["Name"];
                //TextPrinter.ForeColor = fontSetting.ForeColor;
                //TextPrinter.BackColor = fontSetting.BackColor;
                //TextPrinter.Font = fontSetting.Font;
            }
            else
            {
                //TextPrinter.ForeColor = SystemColors.WindowText;
                //TextPrinter.BackColor = Color.Transparent;
                //TextPrinter.Font = Settings.FontSetting.DefaultFont;
            }

            if (instance.GlobalSettings["av_name_link"])
            {
                TextPrinter.InsertLink(fromName, $"secondlife:///app/agent/{fromID}/about");
            }
            else
            {
                TextPrinter.PrintText(fromName);
            }

            StringBuilder sb = new StringBuilder();

            if (message.StartsWith("/me "))
            {
                if (fontSettings.ContainsKey("Emote"))
                {
                    var fontSetting = fontSettings["Emote"];
                    //TextPrinter.ForeColor = fontSetting.ForeColor;
                    //TextPrinter.BackColor = fontSetting.BackColor;
                    //TextPrinter.Font = fontSetting.Font;
                }
                else
                {
                    //TextPrinter.ForeColor = SystemColors.WindowText;
                    //TextPrinter.BackColor = Color.Transparent;
                    //TextPrinter.Font = Settings.FontSetting.DefaultFont;
                }

                sb.Append(message.Substring(3));
            }
            else
            {
                if (fromID == instance.Client.Self.AgentID)
                {
                    if (fontSettings.ContainsKey("OutgoingIM"))
                    {
                        var fontSetting = fontSettings["OutgoingIM"];
                        //TextPrinter.ForeColor = fontSetting.ForeColor;
                        //TextPrinter.BackColor = fontSetting.BackColor;
                        //TextPrinter.Font = fontSetting.Font;
                    }
                    else
                    {
                        //TextPrinter.ForeColor = SystemColors.WindowText;
                        //TextPrinter.BackColor = Color.Transparent;
                        //TextPrinter.Font = Settings.FontSetting.DefaultFont;
                    }
                }
                else
                {
                    if (fontSettings.ContainsKey("IncomingIM"))
                    {
                        var fontSetting = fontSettings["IncomingIM"];
                        //TextPrinter.ForeColor = fontSetting.ForeColor;
                        //TextPrinter.BackColor = fontSetting.BackColor;
                        //TextPrinter.Font = fontSetting.Font;
                    }
                    else
                    {
                        //TextPrinter.ForeColor = SystemColors.WindowText;
                        //TextPrinter.BackColor = Color.Transparent;
                        //TextPrinter.Font = Settings.FontSetting.DefaultFont;
                    }
                }

                sb.Append(": ");
                sb.Append(message);
            }

            if (isNewMessage)
            {
                instance.LogClientMessage(sessionName + ".txt", fromName + sb);
            }

            TextPrinter.PrintTextLine(sb.ToString());
        }

        public static string ReadEndTokens(string path, Int64 numberOfTokens, Encoding encoding, string tokenSeparator)
        {

            int sizeOfChar = encoding.GetByteCount("\n");
            byte[] buffer = encoding.GetBytes(tokenSeparator);


            using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                Int64 tokenCount = 0;
                Int64 endPosition = fs.Length / sizeOfChar;

                for (Int64 position = sizeOfChar; position < endPosition; position += sizeOfChar)
                {
                    fs.Seek(-position, SeekOrigin.End);
                    fs.Read(buffer, 0, buffer.Length);

                    if (encoding.GetString(buffer) == tokenSeparator)
                    {
                        tokenCount++;
                        if (tokenCount == numberOfTokens)
                        {
                            byte[] returnBuffer = new byte[fs.Length - fs.Position];
                            fs.Read(returnBuffer, 0, returnBuffer.Length);
                            return encoding.GetString(returnBuffer);
                        }
                    }
                }

                // handle case where number of tokens in file is less than numberOfTokens
                fs.Seek(0, SeekOrigin.Begin);
                buffer = new byte[fs.Length];
                fs.Read(buffer, 0, buffer.Length);
                return encoding.GetString(buffer);
            }
        }

        private void PrintLastLog()
        {
            string last = string.Empty;
            try
            {
                last = ReadEndTokens(instance.ChatFileName(sessionName + ".txt"), 20, Encoding.UTF8, Environment.NewLine);
            }
            catch { }

            if (string.IsNullOrEmpty(last))
            {
                return;
            }

            string[] lines = last.Split(Environment.NewLine.ToCharArray());
            foreach (var line in lines)
            {
                string msg = line.Trim();
                if (!string.IsNullOrEmpty(msg))
                {
                    if (fontSettings.ContainsKey("History"))
                    {
                        var fontSetting = fontSettings["History"];
                        //TextPrinter.ForeColor = fontSetting.ForeColor;
                        //TextPrinter.BackColor = fontSetting.BackColor;
                        //TextPrinter.Font = fontSetting.Font;
                    }
                    else
                    {
                        //TextPrinter.ForeColor = SystemColors.GrayText;
                        //TextPrinter.BackColor = Color.Transparent;
                        //TextPrinter.Font = Settings.FontSetting.DefaultFont;
                    }
                    TextPrinter.PrintTextLine(msg);
                }
            }

            if (fontSettings.ContainsKey("History"))
            {
                var fontSetting = fontSettings["History"];
                //TextPrinter.ForeColor = fontSetting.ForeColor;
                //TextPrinter.BackColor = fontSetting.BackColor;
                //TextPrinter.Font = fontSetting.Font;
            }
            else
            {
                //TextPrinter.ForeColor = SystemColors.GrayText;
                //TextPrinter.BackColor = Color.Transparent;
                //TextPrinter.Font = Settings.FontSetting.DefaultFont;
            }
            TextPrinter.PrintTextLine("====");
        }

        public void ReprintAllText()
        {
            TextPrinter.ClearText();
            PrintLastLog();
            foreach (object obj in textBuffer)
                ProcessIM(obj, false);
        }

        public void ClearInternalBuffer()
        {
            textBuffer.Clear();
            textBufferReactive.Clear();
        }

        public void CleanUp()
        {
            RemoveNetcomEvents();

            textBuffer.Clear();
            textBufferReactive.Clear();
            textBuffer = null;
            textBufferReactive.Dispose();
            textBufferReactive = null;

            TextPrinter = null;
        }


        public ITextPrinter TextPrinter { get; set; } //a reference to a generic text printer (whose job is to print into a tectbox nicely.)

        public bool ShowTimestamps { get; set; }

        public UUID SessionID { get; set; }
    }

    public enum IMTextManagerType
    {
        Agent,
        Group,
        Conference
    }
}
