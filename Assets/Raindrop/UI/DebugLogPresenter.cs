using log4net.Core;
using OpenMetaverse;
using Raindrop.Netcom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Raindrop.Presenters
{

    public class DebugLogPresenter : MonoBehaviour
    {
        //main pane: a list of debug messages.
        private RaindropInstance instance { get { return RaindropInstance.GlobalInstance; } }
        private RaindropNetcom netcom { get { return instance.Netcom; } }

        private GridClient client => instance.Client;

        public TMP_Text text;

        //public void OnPointerClick(PointerEventData eventData)
        //{
        //    int linkIndex = TMP_TextUtilities.FindIntersectingLink(pTextMeshPro, Input.mousePosition, pCamera);
        //    if (linkIndex != -1)
        //    { // was a link clicked?
        //        TMP_LinkInfo linkInfo = pTextMeshPro.textInfo.linkInfo[linkIndex];

        //        // open the link id as a url, which is the metadata we added in the text field
        //        Application.OpenURL(linkInfo.GetLinkID());
        //    }
        //}



        private void Awake()
        {
            //Disposed += new EventHandler(DebugConsole_Disposed);
            RaindropAppender.Log += new EventHandler<LogEventArgs>(RaindropAppender_Log);
        }

        void RaindropAppender_Log(object sender, LogEventArgs e)
        {
            Debug.Log("yep the log function was called!");
            //if (!IsHandleCreated) return;

            //if (InvokeRequired)
            //{
            //    BeginInvoke(new MethodInvoker(() => RadegastAppender_Log(sender, e)));
            //    return;
            //}

            string _thenewline = "";

            //rtbLog.SelectionColor = Color.FromKnownColor(KnownColor.WindowText);
            _thenewline += (string.Format("{0:HH:mm:ss} [", e.LogEntry.TimeStamp));

            if (e.LogEntry.Level == Level.Error)
            {
                //rtbLog.SelectionColor = Color.Red;

                string prepend = "< color = \"red\" > ";
                _thenewline = prepend + _thenewline;
            }
            else if (e.LogEntry.Level == Level.Warn)
            {
                //rtbLog.SelectionColor = Color.Yellow;

                string prepend = "< color = \"yellow\" > ";
                _thenewline = prepend + _thenewline;
            }
            else if (e.LogEntry.Level == Level.Info)
            {

                string prepend = "< color = \"green\" > ";
                _thenewline = prepend + _thenewline;

                //rtbLog.SelectionColor = Color.Green;
            }
            else
            {

                string prepend = "< color = \"gray\" > ";
                _thenewline = prepend + _thenewline;
            }

            _thenewline += (e.LogEntry.Level.Name);


            //rtbLog.SelectionColor = Color.FromKnownColor(KnownColor.WindowText);
            
            _thenewline += (string.Format("]: - {0}{1}", e.LogEntry.MessageObject, Environment.NewLine));

            text.text = text.text + _thenewline;
        }

        //private void rtbLog_LinkClicked(object sender, LinkClickedEventArgs e)
        //{
        //    instance.MainForm.ProcessLink(e.LinkText);
        //}



    }



}
