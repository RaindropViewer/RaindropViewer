// 
// Radegast Metaverse Client
// Copyright (c) 2009-2014, Radegast Development Team
// All rights reserved.
// 
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are met:
// 
//     * Redistributions of source code must retain the above copyright notice,
//       this list of conditions and the following disclaimer.
//     * Redistributions in binary form must reproduce the above copyright
//       notice, this list of conditions and the following disclaimer in the
//       documentation and/or other materials provided with the distribution.
//     * Neither the name of the application "Radegast", nor the names of its
//       contributors may be used to endorse or promote products derived from
//       this software without specific prior written permission.
// 
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
// DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE
// FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
// DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
// SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER
// CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY,
// OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
// OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
//
// $Id$
//
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;
using Disk;
//using Radegast.Commands;
using Raindrop.Netcom;
using Raindrop.Media;
using Raindrop.UI.Notification;
using OpenMetaverse;
using Raindrop.ServiceLocator;
using UnityEngine;
using Logger = OpenMetaverse.Logger;

namespace Raindrop
{
    // The singleton instance of the game client.
    public class RaindropInstance : IGameService
    {
        //composition of many things...
        
        private GridClient _client; //backend monolith API
        private RaindropNetcom _netcom;

        private StateManager _state;

        //private frmMain mainForm; //frmMain is a class that inherits RadegastForm. It seems to be the code-behind of the overall UI, that includes the view and buttons.
        
        // Singleton, there can be only one instance


        // managed the chats that are loaded in memory. (including local chat.)
        public ChatManager ChatManger { get { return chatManger; } }
        private ChatManager chatManger;



        /// <summary>
        /// Manages retrieving avatar names
        /// </summary>
        public NameManager Names { get { return _names; } }
        private NameManager _names;

        /// <summary>
        /// When was Radegast started (UTC)
        /// </summary>
        public readonly DateTime StartupTimeUtc;

        /// <summary>
        /// Time zone of the current world (currently hard coded to US Pacific time)
        /// </summary>
        public TimeZoneInfo WordTimeZone;

        /// <summary>
        /// System (not grid!) user's dir
        /// </summary>
        public string UserDir { get; private set; }

        /// <summary>
        /// Grid client's user dir for settings and logs
        /// ala the path combined with the account's full name
        /// </summary>
        public string ClientDir
        {
            get
            {
                if (_client != null && _client.Self != null && !string.IsNullOrEmpty(_client.Self.Name))
                {
                    return Path.Combine(UserDir, _client.Self.Name);
                }
                else
                {
                    return Environment.CurrentDirectory;
                }
            }
        }

        public string InventoryCacheFileName { get { return Path.Combine(ClientDir, "inventory.cache"); } }

        private string _globalLogFile;
        public string GlobalLogFile { get { return _globalLogFile; } }

        private bool _monoRuntime;
        public bool MonoRuntime { get { return _monoRuntime; } }

        public Dictionary<UUID, Group> Groups { get; private set; } = new Dictionary<UUID, Group>();

        private Settings _globalSettings;
        /// <summary>
        /// Global settings for the entire application
        /// </summary>
        public Settings GlobalSettings { get { return _globalSettings; } }

        /// <summary>
        /// Per client settings
        /// </summary>
        public Settings ClientSettings { get; private set; }

        public const string IncompleteName = "Loading...";

        public readonly bool AdvancedDebugging = false;

        //private PluginManager pluginManager;
        ///// <summary> Handles loading plugins and scripts</summary>
        //public PluginManager PluginManager { get { return pluginManager; } }

        /// <summary>
        /// Radegast media manager for playing streams and in world sounds
        /// </summary>
        public MediaManager MediaManager { get; private set; }


        //private CommandsManager commandsManager;
        ///// <summary>
        ///// Radegast command manager for executing textual console commands
        ///// </summary>
        //public CommandsManager CommandsManager { get { return commandsManager; } }

        /// <summary>
        /// Radegast ContextAction manager for context sensitive actions
        /// </summary>
        //public ContextActionsManager ContextActionManager { get; private set; }

        private RaindropMovement _movement;
        /// <summary>
        /// Allows key emulation for moving avatar around
        /// </summary>
        public RaindropMovement Movement { get { return _movement; } }

        //private InventoryClipboard inventoryClipboard;
        ///// <summary>
        ///// The last item that was cut or copied in the inventory, used for pasting
        ///// in a different place on the inventory, or other places like profile
        ///// that allow sending copied inventory items
        ///// </summary>
        //public InventoryClipboard InventoryClipboard
        //{
        //    get { return inventoryClipboard; }
        //    set
        //    {
        //        inventoryClipboard = value;
        //        OnInventoryClipboardUpdated(EventArgs.Empty);
        //    }
        //}

        //private RLVManager rlv;

        ///// <summary>
        ///// Manager for RLV functionality
        ///// </summary>
        //public RLVManager RLV { get { return rlv; } }

        private GridManager _gridManager;
        /// <summary>Manages default params for different grids</summary>
        public GridManager GridManger { get { return _gridManager; } }


        /// <summary>
        /// Current Outfit Folder (appearnce) manager
        /// </summary>
        public CurrentOutfitFolder Cof;

        /// <summary>
        /// Did we report crash to the grid login service
        /// </summary>
        public bool ReportedCrash = false;

        private string CrashMarkerFileName
        {
            get
            {
                return Path.Combine(UserDir, "crash_marker");
            }
        }

        #region Events

        #region ClientChanged event
        /// <summary>The event subscribers, null of no subscribers</summary>
        private EventHandler<ClientChangedEventArgs> _mClientChanged;

        ///<summary>Raises the ClientChanged Event</summary>
        /// <param name="e">A ClientChangedEventArgs object containing
        /// the old and the new client</param>
        protected virtual void OnClientChanged(ClientChangedEventArgs e)
        {
            EventHandler<ClientChangedEventArgs> handler = _mClientChanged;
            if (handler != null)
                handler(this, e);
        }

        /// <summary>Thread sync lock object</summary>
        private readonly object _mClientChangedLock = new object();

        /// <summary>Raised when the GridClient object in the main Radegast instance is changed</summary>
        public event EventHandler<ClientChangedEventArgs> ClientChanged
        {
            add { lock (_mClientChangedLock) { _mClientChanged += value; } }
            remove { lock (_mClientChangedLock) { _mClientChanged -= value; } }
        }
        #endregion ClientChanged event

        #region InventoryClipboardUpdated event
        /// <summary>The event subscribers, null of no subscribers</summary>
        private EventHandler<EventArgs> _mInventoryClipboardUpdated;

        ///<summary>Raises the InventoryClipboardUpdated Event</summary>
        /// <param name="e">A EventArgs object containing
        /// the old and the new client</param>
        protected virtual void OnInventoryClipboardUpdated(EventArgs e)
        {
            EventHandler<EventArgs> handler = _mInventoryClipboardUpdated;
            if (handler != null)
                handler(this, e);
        }

        /// <summary>Thread sync lock object</summary>
        private readonly object _mInventoryClipboardUpdatedLock = new object();

        /// <summary>Raised when the GridClient object in the main Radegast instance is changed</summary>
        public event EventHandler<EventArgs> InventoryClipboardUpdated
        {
            add { lock (_mInventoryClipboardUpdatedLock) { _mInventoryClipboardUpdated += value; } }
            remove { lock (_mInventoryClipboardUpdatedLock) { _mInventoryClipboardUpdated -= value; } }
        }
        #endregion InventoryClipboardUpdated event


        #endregion Events

        public RaindropInstance(GridClient client)
        {
            InitializeLoggingAndConfig();

            _client = client;

            // Initialize current time zone, and mark when we started
            GetWorldTimeZone();
            StartupTimeUtc = DateTime.UtcNow;

            // Are we running mono?
            _monoRuntime = Type.GetType("Mono.Runtime") != null;
            if (_monoRuntime)
            {
                Logger.Log("Mono runtime is detected. This should not happen except in the editor.", Helpers.LogLevel.Warning);
            }

            //Keyboard = new Keyboard();
            //Application.AddMessageFilter(Keyboard);

            _netcom = new RaindropNetcom(this);
            _state = new StateManager(this);
            MediaManager = new MediaManager(this);
            
            //commandsManager = new CommandsManager(this);
            //ContextActionManager = new ContextActionsManager(this);
            //RegisterContextActions();
            _movement = new RaindropMovement(this);

            InitializeClient(_client);

            //rlv = new RLVManager(this);
            _gridManager = new GridManager(UserDir);

            _names = new NameManager(this);
            Cof = new CurrentOutfitFolder(this);
            

            //ui_manager = new UIManager(this);
            //mainCanvas.InitializeControls();

            //mainCanvas.Load += new EventHandler(mainForm_Load);
            //pluginManager = new PluginManager(this);
            //pluginManager.ScanAndLoadPlugins();

            //chatManger = new ChatManager(this);
        }

        private void InitializeClient(GridClient client)
        {
            client.Settings.MULTIPLE_SIMS = false;

            client.Settings.USE_INTERPOLATION_TIMER = false;
            client.Settings.ALWAYS_REQUEST_OBJECTS = true;
            client.Settings.ALWAYS_DECODE_OBJECTS = true;
            client.Settings.OBJECT_TRACKING = true;
            client.Settings.ENABLE_SIMSTATS = true;
            // client.Settings.FETCH_MISSING_INVENTORY = true;
            client.Settings.SEND_AGENT_THROTTLE = true;
            client.Settings.SEND_AGENT_UPDATES = true;
            client.Settings.STORE_LAND_PATCHES = true;

            client.Settings.USE_ASSET_CACHE = true;
            client.Settings.ASSET_CACHE_DIR = Path.Combine(UserDir, "cache");
            client.Assets.Cache.AutoPruneEnabled = false;
            client.Assets.Cache.ComputeAssetCacheFilename = ComputeCacheName;

            client.Throttle.Total = 5000000f; //0.625 megabytes.
            client.Settings.THROTTLE_OUTGOING_PACKETS = true; //please throttle to be good boy.
            client.Settings.LOGIN_TIMEOUT = 120 * 1000;
            client.Settings.SIMULATOR_TIMEOUT = 180 * 1000;
            client.Settings.MAX_CONCURRENT_TEXTURE_DOWNLOADS = 20;

            client.Self.Movement.AutoResetControls = false;
            client.Self.Movement.UpdateInterval = 2500; //2.5 seconds?
            client.Settings.DISABLE_AGENT_UPDATE_DUPLICATE_CHECK = false; //lets be not-rude.

            RegisterClientEvents(client);
        }

        public string ComputeCacheName(string cacheDir, UUID assetID)
        {
            string fileName = assetID.ToString();
            string dir = cacheDir
                + Path.DirectorySeparatorChar + fileName.Substring(0, 1)
                + Path.DirectorySeparatorChar + fileName.Substring(1, 1);
            try
            {
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }
            }
            catch
            {
                return Path.Combine(cacheDir, fileName);
            }
            return Path.Combine(dir, fileName);
        }

        private void RegisterClientEvents(GridClient client)
        {
            //log
            //OpenMetaverse.Logger.OnLogMessage += Logger_OnLogMessage;

            client.Groups.CurrentGroups += new EventHandler<CurrentGroupsEventArgs>(Groups_CurrentGroups);
            client.Groups.GroupLeaveReply += new EventHandler<GroupOperationEventArgs>(Groups_GroupsChanged);
            client.Groups.GroupDropped += new EventHandler<GroupDroppedEventArgs>(Groups_GroupsChanged);
            client.Groups.GroupJoinedReply += new EventHandler<GroupOperationEventArgs>(Groups_GroupsChanged);
            if (_netcom != null)
                _netcom.ClientConnected += new EventHandler<EventArgs>(netcom_ClientConnected);
            client.Network.LoginProgress += new EventHandler<LoginProgressEventArgs>(Network_LoginProgress);
        }

        private void UnregisterClientEvents(GridClient client)
        {
            client.Groups.CurrentGroups -= new EventHandler<CurrentGroupsEventArgs>(Groups_CurrentGroups);
            client.Groups.GroupLeaveReply -= new EventHandler<GroupOperationEventArgs>(Groups_GroupsChanged);
            client.Groups.GroupDropped -= new EventHandler<GroupDroppedEventArgs>(Groups_GroupsChanged);
            client.Groups.GroupJoinedReply -= new EventHandler<GroupOperationEventArgs>(Groups_GroupsChanged);
            if (_netcom != null)
                _netcom.ClientConnected -= new EventHandler<EventArgs>(netcom_ClientConnected);
            client.Network.LoginProgress -= new EventHandler<LoginProgressEventArgs>(Network_LoginProgress);
        }

        private void GetWorldTimeZone()
        {
            try
            {
                foreach (TimeZoneInfo tz in TimeZoneInfo.GetSystemTimeZones())
                {
                    if (tz.Id == "Pacific Standard Time" || tz.Id == "America/Los_Angeles")
                    {
                        WordTimeZone = tz;
                        break;
                    }
                }
            }
            catch (Exception) { }
        }

        public DateTime GetWorldTime()
        {
            DateTime now;

            try
            {
                if (WordTimeZone != null)
                    now = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, WordTimeZone);
                else
                    now = DateTime.UtcNow.AddHours(-7);
            }
            catch (Exception)
            {
                now = DateTime.UtcNow.AddHours(-7);
            }

            return now;
        }


        public void Reconnect()
        {
            Notification.DisplayNotification("Attempting to reconnect...", ChatBufferTextStyle.StatusDarkBlue);
            Logger.Log("Attempting to reconnect", Helpers.LogLevel.Info, _client);
            GridClient oldClient = _client;
            _client = new GridClient();
            UnregisterClientEvents(oldClient);
            InitializeClient(_client);
            OnClientChanged(new ClientChangedEventArgs(oldClient, _client));
            _netcom.Login();
        }

        // invoke dispose pattern on all my objects
        public void CleanUp()
        {
            MarkEndExecution();

            if (Cof != null)
            {
                Cof.Dispose();
                Cof = null;
            }

            if (_names != null)
            {
                _names.Dispose();
                _names = null;
            }

            if (_gridManager != null)
            {
                _gridManager.Dispose();
                _gridManager = null;
            }

            //if (rlv != null)
            //{
            //    rlv.Dispose();
            //    rlv = null;
            //}

            if (_client != null)
            {
                UnregisterClientEvents(_client);
            }

            //if (pluginManager != null)
            //{
            //    pluginManager.Dispose();
            //    pluginManager = null;
            //}

            if (_movement != null)
            {
                _movement.Dispose();
                _movement = null;
            }
            //if (commandsManager != null)
            //{
            //    commandsManager.Dispose();
            //    commandsManager = null;
            //}
            //if (ContextActionManager != null)
            //{
            //    ContextActionManager.Dispose();
            //    ContextActionManager = null;
            //}
            if (MediaManager != null)
            {
                MediaManager.Dispose();
                MediaManager = null;
            }
            if (_state != null)
            {
                _state.Dispose();
                _state = null;
            }
            if (_netcom != null)
            {
                _netcom.Dispose();
                _netcom = null;
            }
            //if (mainCanvas != null)
            //{
            //    mainCanvas.Load -= new EventHandler(mainForm_Load);
            //}
            Logger.Log("RaindropInstance finished cleaning up.", Helpers.LogLevel.Debug);
        }
        
        
        
        void netcom_ClientConnected(object sender, EventArgs e)
        {
            _client.Self.RequestMuteList();
        }

        void Network_LoginProgress(object sender, LoginProgressEventArgs e)
        {
            if (e.Status == LoginStatus.ConnectingToSim)
            {
                try
                {
                    if (!Directory.Exists(ClientDir))
                    {
                        Directory.CreateDirectory(ClientDir);
                    }
                    ClientSettings = new Settings(Path.Combine(ClientDir, "client_settings.xml"));
                }
                catch (Exception ex)
                {
                    Logger.Log("Failed to create client directory", Helpers.LogLevel.Warning, ex);
                }
            }
        }


        /// <summary>
        /// Fetches avatar name
        /// </summary>
        /// <param name="key">Avatar UUID</param>
        /// <param name="blocking">Should we wait until the name is retrieved</param>
        /// <returns>Avatar name</returns>
        [Obsolete("Use Instance.Names.Get() instead")]
        public string GetAvatarName(UUID key, bool blocking)
        {
            return Names.Get(key, blocking);
        }

        /// <summary>
        /// Fetches avatar name from cache, if not in cache will request name from the server
        /// </summary>
        /// <param name="key">Avatar UUID</param>
        /// <returns>Avatar name</returns>
        [Obsolete("Use Instance.Names.Get() instead")]
        public string GetAvatarName(UUID key)
        {
            return Names.Get(key);
        }

        void Groups_GroupsChanged(object sender, EventArgs e)
        {
            _client.Groups.RequestCurrentGroups();
        }

        public static string SafeFileName(string fileName)
        {
            foreach (char lDisallowed in Path.GetInvalidFileNameChars())
            {
                fileName = fileName.Replace(lDisallowed.ToString(), "_");
            }

            return fileName;
        }

        //obtains the relevant filepath/name.
        public string ChatFileName(string session)
        {
            string fileName = session;

            foreach (char lDisallowed in System.IO.Path.GetInvalidFileNameChars())
            {
                fileName = fileName.Replace(lDisallowed.ToString(), "_");
            }

            return Path.Combine(ClientDir, fileName);
        }

        //this method will log all messages to the relevant file.
        public void LogClientMessage(string sessionName, string message)
        {
            if (_globalSettings["disable_chat_im_log"]) return;

            lock (this)
            {
                try
                {
                    File.AppendAllText(ChatFileName(sessionName),
                        DateTime.Now.ToString("yyyy-MM-dd [HH:mm:ss] ") + message + Environment.NewLine);
                }
                catch (Exception) { }
            }
        }

        void Groups_CurrentGroups(object sender, CurrentGroupsEventArgs e)
        {
            this.Groups = e.Groups;
        }

        // for simplicity, we will always use internal dir for settings.
        private void InitializeLoggingAndConfig()
        {
            try
            {
                var internalDir = DirectoryHelpers.GetInternalStorageDir();
                UserDir = internalDir;
                if (!Directory.Exists(UserDir))
                {
                    Directory.CreateDirectory(UserDir);
                }
            }
            catch (Exception)
            {
                Logger.Log("App termination due to: unable to create UserDir: " + UserDir, Helpers.LogLevel.Error);
                #if !UNITY_EDITOR
                Application.Quit();
                #endif
            };

            _globalLogFile = Path.Combine(UserDir, _programname + ".log");
            _globalSettings = new Settings(Path.Combine(UserDir, "settings.xml"));
            //frmSettings.InitSettigs(globalSettings, monoRuntime);
        }

        public GridClient Client
        {
            get { return _client; }
        }

        public RaindropNetcom Netcom
        {
            get { return _netcom; }
        }

        public StateManager State
        {
            get { return _state; }
        }

        //public TabsConsole TabConsole
        //{
        //    get { return mainForm.TabConsole; }
        //}

        public void HandleThreadException(object sender, ThreadExceptionEventArgs e)
        {
            Logger.Log("Unhandled thread exception: "
                + e.Exception.Message + Environment.NewLine
                + e.Exception.StackTrace + Environment.NewLine,
                Helpers.LogLevel.Error,
                _client);
        }

        #region Crash reporting
        FileStream _markerLock = null;
        private string _programname = "RaindropViewer";
        private readonly string _extAppDataDir;

        public bool AnotherInstanceRunning()
        {
            // We have successfuly obtained lock
            if (_markerLock != null && _markerLock.CanWrite)
            {
                Logger.Log("No other instances detected, marker file already locked", Helpers.LogLevel.Debug);
                return false || MonoRuntime;
            }

            try
            {
                _markerLock = new FileStream(CrashMarkerFileName, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
                Logger.Log(string.Format("Successfully created and locked marker file {0}", CrashMarkerFileName), Helpers.LogLevel.Debug);
                return false || MonoRuntime;
            }
            catch
            {
                _markerLock = null;
                Logger.Log(string.Format("Another instance detected, marker fils {0} locked", CrashMarkerFileName), Helpers.LogLevel.Debug);
                return true;
            }
        }

        public LastExecStatus GetLastExecStatus()
        {
            // Crash marker file found and is not locked by us
            if (File.Exists(CrashMarkerFileName) && _markerLock == null)
            {
                Logger.Log(string.Format("Found crash marker file {0}", CrashMarkerFileName), Helpers.LogLevel.Debug);
                return LastExecStatus.OtherCrash;
            }
            else
            {
                Logger.Log(string.Format("No crash marker file {0} found", CrashMarkerFileName), Helpers.LogLevel.Debug);
                return LastExecStatus.Normal;
            }
        }

        public void MarkStartExecution()
        {
            Logger.Log(string.Format("Marking start of execution run, creating file: {0}", CrashMarkerFileName), Helpers.LogLevel.Debug);
            try
            {
                File.Create(CrashMarkerFileName).Dispose();
            }
            catch { }
        }

        public void MarkEndExecution()
        {
            Logger.Log(string.Format("Marking end of execution run, deleting file: {0}", CrashMarkerFileName), Helpers.LogLevel.Debug);
            try
            {
                if (_markerLock != null)
                {
                    _markerLock.Close();
                    _markerLock.Dispose();
                    _markerLock = null;
                }

                File.Delete(CrashMarkerFileName);
            }
            catch { }
        }

        internal void SetAppDataDir(string appDataPath)
        {
            throw new NotImplementedException();
        }

        #endregion Crash reporting

        #region Context Actions
        //void RegisterContextActions()
        //{
        //    ContextActionManager.RegisterContextAction(typeof(Primitive), "Save as DAE...", ExportDAEHander);
        //    ContextActionManager.RegisterContextAction(typeof(Primitive), "Copy UUID to clipboard", CopyObjectUUIDHandler);
        //}

        //void DeregisterContextActions()
        //{
        //    ContextActionManager.DeregisterContextAction(typeof(Primitive), "Save as DAE...");
        //    ContextActionManager.DeregisterContextAction(typeof(Primitive), "Copy UUID to clipboard");
        //}

        //void ExportDAEHander(object sender, EventArgs e)
        //{
        //    MainForm.DisplayColladaConsole((Primitive)sender);
        //}

        //void CopyObjectUUIDHandler(object sender, EventArgs e)
        //{
        //    if (mainForm.InvokeRequired)
        //    {
        //        if (mainForm.IsHandleCreated || !MonoRuntime)
        //        {
        //            mainForm.Invoke(new MethodInvoker(() => CopyObjectUUIDHandler(sender, e)));
        //        }
        //        return;
        //    }

        //    Clipboard.SetText(((Primitive)sender).ID.ToString());
        //}

        #endregion Context Actions


        //no longer using this as we have configured log4net :)
        private void Logger_OnLogMessage(object message, Helpers.LogLevel level)
        {
            if (level == Helpers.LogLevel.Debug)
            {
                Debug.Log(message);
            }
            else if (level == Helpers.LogLevel.Error)
            {
                Debug.LogError(message);
            }
            else if (level == Helpers.LogLevel.Info)
            {
                Debug.Log(message);
            }
            else if (level == Helpers.LogLevel.Warning)
            {
                Debug.LogWarning(message);
            }
            else
            {
                Debug.LogError(message);
            }
        }


    }

    #region Event classes
    public class ClientChangedEventArgs : EventArgs
    {
        private GridClient m_OldClient;
        private GridClient m_Client;

        public GridClient OldClient { get { return m_OldClient; } }
        public GridClient Client { get { return m_Client; } }

        public ClientChangedEventArgs(GridClient OldClient, GridClient Client)
        {
            m_OldClient = OldClient;
            m_Client = Client;
        }
    }
    
    #endregion Event classes
}
