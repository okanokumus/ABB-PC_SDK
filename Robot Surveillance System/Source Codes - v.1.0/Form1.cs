using System;
using System.Linq;
using System.Windows.Forms;

using System.IO;
using System.IO.Compression;
using ABB.Robotics.Controllers;
using ABB.Robotics.Controllers.Discovery;
using ABB.Robotics.Controllers.RapidDomain;
using ABB.Robotics.Controllers.EventLogDomain;
using ABB.Robotics.Controllers.IOSystemDomain;
using System.Net.Mail;

namespace NetworkScanningWindow
{
    public partial class Form1 : Form
    {
        #region global variables
        // data decleration and initializition
        public NetworkScanner scanner = null; 
        public Controller controller = null;
        public Controller[] scannedController = new Controller[25] ; // to select all controller
        public ControllerInfo controllerInfo = null;
        public ControllerInfo[] scannedControllerInfo = new ControllerInfo[25]; // to select all controller
        public ListViewItem[] scanneditem = new ListViewItem[25];
        public Backup backup = null;
        public BackupEventArgs backupsuccess = null;
        public NetworkWatcher networkwatcher = null;
        public EventLog log = null;
        public EventLog[] scannedLog = new EventLog[25];// for selected all controller
        EventLogCategory cat; // get categories
        EventLogCategory[] scannedCat = new EventLogCategory[25]; // get categories for all controller
        ListViewItem item = null;
        string VirtualReal;
        string[] scannedVirtualReal = new string[25]; // to select all controller
        // create the path folder
        // Specify a name for your top-level folder.
        string FolderString = "";
        string EventLogPath = "";
        string localDir = "C:/ABB BACKUPS-EVENT LOG";
        string BackupComputerPath = "";
        string EventLogComputerPath = "";
        string[] eventLogBody = new string[600];
        string restorePath = "";
        // saving file
        SaveFileDialog save = new SaveFileDialog();
        string fileName;
        // ctr -> number of scanned controller
        int ctr = 1;
        // to controll all controllers are selected or single controller
        bool selectedAll = false;
        string PrezipFolder, zipFolder;
        #endregion

        #region FORM
        public Form1()
        {
            InitializeComponent();
            this.MaximizeBox = false;
            listView1.FullRowSelect = true;
            listView1.GridLines = true;
            listView1.View = View.Details;
            textBox3.Text = "";
            // backup main folder
            //3/19/18
            //BackupComputerPath = "/Robot Backups/";
            BackupComputerPath = localDir + "/Robot Backups/";
            // eventlog main folder 
            EventLogComputerPath = localDir + "/Robot Event Log";
            // BACKUP file dir is created
            System.IO.Directory.CreateDirectory(BackupComputerPath);
            // EVENT LOG file dir is created
            System.IO.Directory.CreateDirectory(EventLogComputerPath);
            Directory.SetCurrentDirectory(localDir);
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            comboBox1.SelectedItem = "ROBOT RESTART TYPES";
            comboBox2.SelectedItem = "STOP MODES";
            comboBox3.SelectedItem = "EVENT LOG";
            comboBox4.SelectedItem = "ROBOTS";
            comboBox5.SelectedItem = "SIGNALS";

        }
        #endregion

        #region selection of the controller(double click listview)
        private void listView1_DoubleClick(object sender, EventArgs e)
        {

            selectedAll = false;
            selectAllController.Checked = false; // assuming that only selected controller will be working on(not all controller)
            listBox1.Items.Clear();
            comboBox1.SelectedItem = "ROBOT RESTART TYPES";
            comboBox2.SelectedItem = "STOP MODES";
            comboBox3.SelectedItem = "EVENT LOG";
            comboBox5.SelectedItem = "SIGNALS";
            for (int i = 0;i<ctr;i++)
            {
                scannedController[i] = null;
            }
            EventLogPath = "";
            ListViewItem item = this.listView1.SelectedItems[0];
            textBox1.Text = item.Tag.ToString();
            if (item.Tag != null)
            {
                ControllerInfo controllerInfo = (ControllerInfo)item.Tag;
                if (controllerInfo.Availability == Availability.Available)
                {
                    if (this.controller != null)
                    {
                        this.controller.Logoff();
                        this.controller.Dispose();
                        this.controller = null;
                    }
                    try
                    {
                        this.controller = ControllerFactory.CreateFrom(controllerInfo);
                        this.controller.Logon(UserInfo.DefaultUser);
                        if (controllerInfo.IsVirtual) VirtualReal = "Virtual";
                        else VirtualReal = "Real";
                        controller.EventLog.MessageWritten += new EventHandler<MessageWrittenEventArgs>(HandleFoundEventLogMsg);

                    }
                    catch (Exception) { MessageBox.Show("No conncetion with the controller", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning); }
                }
                else MessageBox.Show("Selected controller not available.");
            }
        }
        #endregion
        #region BACKUP single controller
        private void button1_Click(object sender, EventArgs e)
        {
            // after selected a controller, if clear all scanned result and scanned again.
            if (controller == null)
            {
                MessageBox.Show("No conncetion with any the controller", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            try
            {
                // after selected a controller, if clear all scanned result and scanned again.
                if (controller == null) return;

                if ((controller.OperatingMode == ControllerOperatingMode.ManualReducedSpeed) || (controller.OperatingMode == ControllerOperatingMode.Auto))
                {
                    try
                    {
                        // for real controller
                        if (VirtualReal != "Virtual")
                        {
                            try
                            {
                                // 1. step is to create backup file onto the robot controller
                                //controller.FileSystem.RemoteDirectory = "/hd0a";
                                string remote = controller.FileSystem.RemoteDirectory;
                                FolderString = item.Tag.ToString() + "-Real" + DateTime.Now.ToString("-yyyy_MM_dd-HH_mm");
                                //backup of the current system of real controller
                                controller.Backup(FolderString);
                                // backup progress start for real controller
                                // 2. step is to wait until backup progress is finished
                                do
                                {
                                    this.Cursor = Cursors.WaitCursor;
                                    System.Threading.Thread.Sleep(2000);
                                    this.Cursor = Cursors.Default;
                                } while (controller.BackupInProgress);
                                System.Console.Write(".");
                                // 3. step is to take backup file to the computer from controller
                                controller.FileSystem.GetDirectory(System.IO.Path.Combine(remote, FolderString), BackupComputerPath + FolderString, true);
                                // 4. step is to remove backup file from controller
                                controller.FileSystem.RemoveDirectory(FolderString, true);
                                textBox2.Text = localDir + FolderString; // FOR THE opening backup file where it is created;
                                FolderString = "";
                                MessageBox.Show("Backup file is created", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show("Backup file is not created\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }

                        }
                        // for virtual controller
                        else if (VirtualReal != "Real")
                        {
                            try
                            {
                                //System.IO.Directory.CreateDirectory("C:" + BackupComputerPath);
                                FolderString = System.IO.Path.Combine(FolderString, textBox1.Text + "-Virtual" + DateTime.Now.ToString("-yyyy_MM_dd-HH_mm"));
                                //backup of the current system of virtual controller
                                BackupManager x = new BackupManager(controller);
                                x.RemoveBackupDirectory = true;
                                controller.Backup(BackupComputerPath + FolderString);
                                bool t = x.RemoveBackupDirectory ;
                                textBox2.Text = BackupComputerPath + FolderString;
                                PrezipFolder = BackupComputerPath + FolderString;
                                zipFolder = BackupComputerPath + FolderString + ".zip";
                                FolderString = "";
                                // backup progress start for real controller
                                do
                                {
                                    this.Cursor = Cursors.WaitCursor;
                                    System.Threading.Thread.Sleep(2000);
                                    this.Cursor = Cursors.Default;
                                } while (controller.BackupInProgress);
                                System.Console.Write(".");
                                // to call the selection of zip file option
                                archive_CheckedChanged(sender, e);
                                
                                MessageBox.Show("Backup file is created", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                            catch (Exception)
                            {
                                MessageBox.Show("Backup file is not created", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }
                    }
                    catch (System.Exception x)
                    {
                        MessageBox.Show("An error occured when backup file is creted" + x, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                else
                    MessageBox.Show("Manuel or Automatic mode is required to start execution from a remote client.");

            }
            catch (System.InvalidOperationException ex)
            {
                MessageBox.Show("Mastership is held by another client." + ex.Message);
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("Unexpected error occurred: " + ex.Message);
            }
        }
        #endregion

        #region SCAN the controllers that on the network
        private void button2_Click(object sender, EventArgs e)
        {
            // init
            initConditions();
            this.scanner = new NetworkScanner();
            this.scanner.Scan();
            ControllerInfoCollection controllers = scanner.Controllers;

            foreach (ControllerInfo controllerInfo in controllers)
            {
                if (controllerInfo.IsVirtual) VirtualReal = "Virtual";
                else VirtualReal = "Real";

                item = new ListViewItem(controllerInfo.IPAddress.ToString());
                item.SubItems.Add(controllerInfo.SystemId.ToString());// guid
                item.SubItems.Add(controllerInfo.Availability.ToString());
                item.SubItems.Add(VirtualReal);
                item.SubItems.Add(controllerInfo.SystemName);
                item.SubItems.Add(controllerInfo.Version.ToString());
                item.SubItems.Add(controllerInfo.ControllerName);
                this.listView1.Items.Add(item);
                item.Tag = controllerInfo;
            }
            this.networkwatcher = new NetworkWatcher(scanner.Controllers);
            this.networkwatcher.Found += new EventHandler<NetworkWatcherEventArgs>(HandleFoundEvent);
            this.networkwatcher.Lost += new EventHandler<NetworkWatcherEventArgs>(HandleLostEvent);
            this.networkwatcher.EnableRaisingEvents = true;
        }
        #endregion

        #region CLEAR the controllers that on the network
        private void button3_Click(object sender, EventArgs e)
        {
            initConditions();
        }
        #endregion

        #region finding new controller
        // add controllers to the list
        void HandleFoundEvent(object sender, NetworkWatcherEventArgs e)
        {
            this.Invoke(new EventHandler<NetworkWatcherEventArgs>(AddControllerToListView), new Object[] { this, e });
        }
        private void AddControllerToListView(object sender, NetworkWatcherEventArgs e)
        {
            ControllerToListView(sender, e);
        }
        #endregion

        #region remove lost controller
        // remove controllers from listview
        void HandleLostEvent(object sender, NetworkWatcherEventArgs e)
        {
            this.Invoke(new EventHandler<NetworkWatcherEventArgs>(RemoveControllerToListView), new Object[] { this, e });
        }
        private void RemoveControllerToListView(object sender, NetworkWatcherEventArgs e)
        {
            ControllerToListView(sender, e);
        }
        #endregion

        #region notify when a new msg occured
        //notified when a new messages is written to the controller event log
        private void log_MessageWritten(object sender, MessageWrittenEventArgs e)
        {
            EventLogMessage msg = e.Message;
            string categoryType = msg.Type.ToString();
            string body = msg.Body.ToString();

            string Title_substring;
            string Consequences_substring;
            string Description_substring;

            string[] searchString = new string[3];
            searchString[0] = "<Title>";
            searchString[1] = "<Description>";
            searchString[2] = "<Consequences>";
            int[] startIndex = new int[3];
            int[] endIndex = new int[3];
            string showMSG;
            try
            {
                // Title part of the body
                startIndex[0] = body.IndexOf(searchString[0]);
                searchString[0] = "</" + searchString[0].Substring(1);
                endIndex[0] = body.IndexOf(searchString[0]);
                Title_substring = body.Substring(startIndex[0]+7 , endIndex[0]-15 + searchString[0].Length - startIndex[0]);

                // Description part of the body
                startIndex[1] = body.IndexOf(searchString[1]);
                searchString[1] = "</" + searchString[1].Substring(1);
                endIndex[1] = body.IndexOf(searchString[1]);
                Description_substring = body.Substring(startIndex[1] + 13, endIndex[1] - 27 + searchString[1].Length - startIndex[1]);

                // Consequences part of the body
                startIndex[2] = body.IndexOf(searchString[2]);
                searchString[2] = "</" + searchString[2].Substring(1);
                endIndex[2] = body.IndexOf(searchString[2]);
                if ((startIndex[2] + 14) < 0 || (endIndex[2] - 29 + searchString[2].Length - startIndex[2]) < 0)
                {
                    showMSG = "Title : \n" + Title_substring + "\n\nDescription : \n" + Description_substring;
                    goto next;
                }
                Consequences_substring = body.Substring(startIndex[1] + 14, endIndex[1] - 29 + searchString[1].Length - startIndex[1]);
                showMSG = "Title: \n" + Title_substring + "\n\nDescription: \n" + Description_substring
                   + "\n\nConsequences: \n" + Consequences_substring;
                next:;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        void HandleFoundEventLogMsg(object sender, MessageWrittenEventArgs e)
        {
             this.Invoke(new EventHandler<MessageWrittenEventArgs>(log_MessageWritten), new Object[] { this, e });
        }
        #endregion

        #region information about the robot and current system 
        private void button8_Click(object sender, EventArgs e)
        {
            int SpeedRatio = 0;
            string OperatingMode = "", state = "";
            // to read analog signal (TCP Speed)
            float tcpSpeed = 0;
            Signal sig_tcpSpeed = null;
            // when any connection is established with any controller
            if (controller != null)
            {
                try
                {
                    sig_tcpSpeed = controller.IOSystem.GetSignal("AO_tcpspeed");   
                }
                catch
                {
                    MessageBox.Show("No connection with controller");
                }

                AnalogSignal analog_tcpSpeed = (AnalogSignal)sig_tcpSpeed;        
                ControllerState controllerState = controller.State;
                SpeedRatio = controller.MotionSystem.SpeedRatio;
                OperatingMode = controller.OperatingMode.ToString();
                switch (controllerState)
                {
                    case ControllerState.EmergencyStop:
                        state = "Emergency stop state";
                        break;
                    case ControllerState.EmergencyStopReset:
                        state = "Emergency stop reset state";
                        break;
                    case ControllerState.GuardStop:
                        state = "Guard stop state";
                        break;
                    case ControllerState.Init:
                        state = "Initial state";
                        break;
                    case ControllerState.MotorsOff:
                        state = "Motors off state";
                        break;
                    case ControllerState.MotorsOn:
                        state = "Motors on state";
                        break;
                    case ControllerState.SystemFailure:
                        state = "System failure state";
                        break;
                    case ControllerState.Unknown:
                        state = "Unknown state";
                        break;
                }
                // TCP speed is used from an Analog Signal that created in the robot(AO) 
                //(only if AO is created )
                try
                {
                    tcpSpeed = analog_tcpSpeed.Value;
                }
                catch (Exception)
                {
                    MessageBox.Show("No signal about TCP Speed","Warning",MessageBoxButtons.OK,MessageBoxIcon.Warning);
                    tcpSpeed = 0;
                }
                MessageBox.Show("Speed Ratio: " + SpeedRatio + "%" + "\nOperating Mode: " + OperatingMode
                    + "\nController State: " + state + "\nTCP Speed: " + tcpSpeed*1000 +" mm/s"
                    , "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else MessageBox.Show("No conncetion with any the controller", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
        #endregion

        #region GET IO SIGNALS
        // to select io signals
        private void comboBox5_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (controller != null)
            {
                ListViewItem item;
                IOsignals.Items.Clear();
                try
                {
                    switch (comboBox5.SelectedItem.ToString())
                    {
                        case "All Signals":
                            IOFilterTypes sigFilterALL = IOFilterTypes.All ;
                            SignalCollection signalsALL = controller.IOSystem.GetSignals(sigFilterALL);
                            foreach (Signal signal in signalsALL)
                            {
                                item = new ListViewItem(signal.Name);
                                item.SubItems.Add(signal.Type.ToString());
                                item.SubItems.Add(signal.Value.ToString());
                                IOsignals.Items.Add(item);
                            }
                            
                            break;

                        case "Digital Input Signals":
                            IOFilterTypes sigFilterIn = IOFilterTypes.Digital | IOFilterTypes.Input;
                            SignalCollection signalsIn = controller.IOSystem.GetSignals(sigFilterIn);
                            foreach (Signal signal in signalsIn)
                            {
                                item = new ListViewItem(signal.Name);
                                item.SubItems.Add(signal.Type.ToString());
                                item.SubItems.Add(signal.Value.ToString());
                                IOsignals.Items.Add(item);
                            }
                            break;
                        case "Digital Output Signals":
                            IOFilterTypes sigFilterOut = IOFilterTypes.Digital | IOFilterTypes.Output;
                            SignalCollection signalsOut = controller.IOSystem.GetSignals(sigFilterOut);
                            foreach (Signal signal in signalsOut)
                            {
                                item = new ListViewItem(signal.Name);
                                item.SubItems.Add(signal.Type.ToString());
                                item.SubItems.Add(signal.Value.ToString());
                                IOsignals.Items.Add(item);
                            }
                            //IOsignals.BackColor = Color.Red;
                            break;
                        case "Analog Input Signals":
                            sigFilterIn = IOFilterTypes.Analog | IOFilterTypes.Input;
                            signalsIn = controller.IOSystem.GetSignals(sigFilterIn);
                            foreach (Signal signal in signalsIn)
                            {
                                item = new ListViewItem(signal.Name);
                                item.SubItems.Add(signal.Type.ToString());
                                item.SubItems.Add(signal.Value.ToString());
                                IOsignals.Items.Add(item);
                            }
                            break;
                        case "Analog Ouput Signals":
                            sigFilterOut = IOFilterTypes.Analog | IOFilterTypes.Output;
                            signalsOut = controller.IOSystem.GetSignals(sigFilterOut);
                            foreach (Signal signal in signalsOut)
                            {
                                item = new ListViewItem(signal.Name);
                                item.SubItems.Add(signal.Type.ToString());
                                item.SubItems.Add(signal.Value.ToString());
                                IOsignals.Items.Add(item);
                            }
                            break;
                        case "Group Input Signals":
                            sigFilterIn = IOFilterTypes.Group | IOFilterTypes.Input;
                            signalsIn = controller.IOSystem.GetSignals(sigFilterIn);
                            foreach (Signal signal in signalsIn)
                            {
                                item = new ListViewItem(signal.Name);
                                item.SubItems.Add(signal.Type.ToString());
                                item.SubItems.Add(signal.Value.ToString());
                                IOsignals.Items.Add(item);
                            }
                            break;
                        case "Group Output Signals":
                            sigFilterOut = IOFilterTypes.Group | IOFilterTypes.Output;
                            signalsOut = controller.IOSystem.GetSignals(sigFilterOut);
                            foreach (Signal signal in signalsOut)
                            {
                                item = new ListViewItem(signal.Name);
                                item.SubItems.Add(signal.Type.ToString());
                                item.SubItems.Add(signal.Value.ToString());
                                IOsignals.Items.Add(item);
                            }
                            break;
                        case "System Input Signals":
                            sigFilterIn = IOFilterTypes.System;
                            signalsIn = controller.IOSystem.GetSignals(sigFilterIn);
                            foreach (Signal signal in signalsIn)
                            {
                                item = new ListViewItem(signal.Name);
                                item.SubItems.Add(signal.Type.ToString());
                                item.SubItems.Add(signal.Value.ToString());
                                IOsignals.Items.Add(item);
                            }
                            break;
                        case "System Output Signals":
                            sigFilterOut = IOFilterTypes.System | IOFilterTypes.Output;
                            signalsOut = controller.IOSystem.GetSignals(sigFilterOut);
                            foreach (Signal signal in signalsOut)
                            {
                                item = new ListViewItem(signal.Name);
                                item.SubItems.Add(signal.Type.ToString());
                                item.SubItems.Add(signal.Value.ToString());
                                IOsignals.Items.Add(item);
                            }
                            //IOsignals.BackColor = Color.Red;
                            break;

                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }                
            }
        }
        #endregion

        #region clear all event logs  
        private void button9_Click(object sender, EventArgs e)
        {
            if (controller != null)
            {
                // robot must be in Manuel mode
                // after selected a controller, if clear all scanned result and scanned again.
                
                if (controller.OperatingMode == ControllerOperatingMode.ManualReducedSpeed)
                {
                    //DialogResult output = new DialogResult();
                    DialogResult output = MessageBox.Show("Do you want to clear all 'Event Log'", "Information", MessageBoxButtons.OKCancel, MessageBoxIcon.Information);
                    if (output == DialogResult.OK)
                    {
                        //using (Mastership m = Mastership.Request(controller.Rapid))
                        controller.EventLog.ClearAll();
                        MessageBox.Show("All 'Event Log' are cleared!", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        listBox1.Items.Clear();
                        listBox1.Items.Clear();
                    }
                    else
                        MessageBox.Show("Cancelled!", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                    MessageBox.Show("Manuel mode is required to clear all event log from a remote client.");

            }
            else MessageBox.Show("No conncetion with any the controller", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
        #endregion

        #region OPENS THE FOLDER THAT SAVED IN THE BACKUP FILE
        private void button4_Click(object sender, EventArgs e)
        {
            openFileButton(textBox2.Text);
        }
        #endregion

        #region STARTS THE ROBOT
        private void startRobot_Click(object sender, EventArgs e)
        {
            if(controller != null)
            {
            try
            {
                if (controller.OperatingMode == ControllerOperatingMode.Auto)
                {
                    using (Mastership m = Mastership.Request(controller.Rapid))
                        this.controller.Rapid.Start();
                }
                else
                    MessageBox.Show("Automatic mode is required to start execution from a remote client.");
            }
            catch (System.InvalidOperationException ex)
            {
                MessageBox.Show("Mastership is held by another client. " + ex.Message);
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("Unexpected error occurred: " + ex.Message);
            }
            }
            else MessageBox.Show("No conncetion with any the controller", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
        #endregion

        #region using combobox to determine which Restart type are selected
        private void restartTypes_SelectedIndexChanged(object sender, EventArgs e)
        {
            DialogResult output;
            // it makes combobox readonly
            comboBox1.DropDownStyle = ComboBoxStyle.DropDown;
            try
            {
                switch (comboBox1.SelectedItem.ToString())
                {
                    case "Warm":
                        if (controller != null)
                        {
                            output = MessageBox.Show("Restart with current system and current settings. ", "Information", MessageBoxButtons.OKCancel, MessageBoxIcon.Information);
                            if (output == DialogResult.OK)
                            {
                                if (controller.OperatingMode == ControllerOperatingMode.Auto || controller.OperatingMode == ControllerOperatingMode.ManualReducedSpeed)
                                {
                                    using (Mastership m = Mastership.Request(controller.Rapid))
                                        this.controller.Restart(ControllerStartMode.Warm);
                                }
                                else MessageBox.Show("Manuel or Automatic mode is required.");
                            }

                        }
                        else
                            MessageBox.Show("No conncetion with any the controller. You have to select a controller", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        break;

                    case "SStart":
                        if (controller != null)
                        {
                            output = MessageBox.Show("Shut down ", "Information", MessageBoxButtons.OKCancel, MessageBoxIcon.Information);
                            if (output == DialogResult.OK)
                            {
                                if (controller.OperatingMode == ControllerOperatingMode.Auto || controller.OperatingMode == ControllerOperatingMode.ManualReducedSpeed)
                                {
                                    using (Mastership m = Mastership.Request(controller.Rapid))
                                        this.controller.Restart(ControllerStartMode.SStart);
                                }
                                else MessageBox.Show("Manuel or Automatic mode is required.");
                            }
                        }
                        else
                            MessageBox.Show("No conncetion with any the controller. You have to select a controller", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        break;

                    case "Cold":
                        if (controller != null)
                        {
                            output = MessageBox.Show("Delete current system and start boot server. ", "Information", MessageBoxButtons.OKCancel, MessageBoxIcon.Information);
                            if (output == DialogResult.OK)
                            {
                                if (controller.OperatingMode == ControllerOperatingMode.Auto || controller.OperatingMode == ControllerOperatingMode.ManualReducedSpeed)
                                {
                                    using (Mastership m = Mastership.Request(controller.Rapid))
                                        this.controller.Restart(ControllerStartMode.Cold);
                                }
                                else MessageBox.Show("Manuel or Automatic mode is required.");
                            }
                        }
                        else
                            MessageBox.Show("No conncetion with any the controller. You have to select a controller", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        break;
                    case "PStart":
                        if (controller != null)
                        {
                            output = MessageBox.Show("Restart and delete programs and modules. ", "Information", MessageBoxButtons.OKCancel, MessageBoxIcon.Information);
                            if (output == DialogResult.OK)
                            {
                                if (controller.OperatingMode == ControllerOperatingMode.Auto || controller.OperatingMode == ControllerOperatingMode.ManualReducedSpeed)
                                {
                                    using (Mastership m = Mastership.Request(controller.Rapid))
                                        this.controller.Restart(ControllerStartMode.PStart);
                                }
                                else MessageBox.Show("Manuel or Automatic mode is required.");
                            }
                        }
                        else
                            MessageBox.Show("No conncetion with any the controller. You have to select a controller", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        break;
                    case "IStart":
                        if (controller != null)
                        { 
                            output = MessageBox.Show("Restart with current system and default settings. ", "Information", MessageBoxButtons.OKCancel, MessageBoxIcon.Information);
                            if (output == DialogResult.OK)
                            {
                                if (controller.OperatingMode == ControllerOperatingMode.Auto || controller.OperatingMode == ControllerOperatingMode.ManualReducedSpeed)
                                {
                                    using (Mastership m = Mastership.Request(controller.Rapid))
                                        this.controller.Restart(ControllerStartMode.IStart);
                                }
                                else MessageBox.Show("Manuel or Automatic mode is required.");
                            }
                        }
                        else
                            MessageBox.Show("No conncetion with any the controller. You have to select a controller", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        break;
                    case "XStart":
                        if(controller != null)
                        { 
                            output = MessageBox.Show("Restart and select another system. ", "Information", MessageBoxButtons.OKCancel, MessageBoxIcon.Information);
                            if (output == DialogResult.OK)
                            {
                                if (controller.OperatingMode == ControllerOperatingMode.Auto || controller.OperatingMode == ControllerOperatingMode.ManualReducedSpeed)
                                {
                                    using (Mastership m = Mastership.Request(controller.Rapid))
                                        this.controller.Restart(ControllerStartMode.XStart);
                                }
                                else MessageBox.Show("Manuel or Automatic mode is required.");
                            }
                        }
                        else
                            MessageBox.Show("No conncetion with any the controller. You have to select a controller", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        break;

                    case "BStart":
                        if(controller != null)
                        {
                            output = MessageBox.Show("Restart from previously stored system. ", "Information", MessageBoxButtons.OKCancel, MessageBoxIcon.Information);
                            if (output == DialogResult.OK)
                            {
                                if (controller.OperatingMode == ControllerOperatingMode.Auto || controller.OperatingMode == ControllerOperatingMode.ManualReducedSpeed)
                                {
                                    using (Mastership m = Mastership.Request(controller.Rapid))
                                        this.controller.Restart(ControllerStartMode.BStart);
                                }
                                else MessageBox.Show("Manuel or Automatic mode is required.");
                            }
                        }
                        else
                            MessageBox.Show("No conncetion with any the controller. You have to select a controller", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        break;
                }
            }
            catch (System.InvalidOperationException ex){
                MessageBox.Show("Mastership is held by another client. " + ex.Message); }
            catch (System.Exception ex) {
                MessageBox.Show("Unexpected error occurred: " + ex.Message); }
        }
        #endregion

        #region using combobox to determine which stop modes are selected
        private void stopModes(object sender, EventArgs e)
        {
            DialogResult output;
            // it makes combobox readonly
            comboBox2.DropDownStyle = ComboBoxStyle.DropDown;
                try
                {
                    switch (comboBox2.SelectedItem.ToString())
                    {
                        case "Cycle":
                            if (controller != null)
                            {
                                output = MessageBox.Show("Stops RAPID execution when the current cycle is completed. ", "Information", MessageBoxButtons.OKCancel, MessageBoxIcon.Information);
                                if (output == DialogResult.OK && controller != null)
                                {
                                    if (controller.OperatingMode == ControllerOperatingMode.Auto)
                                    {
                                        using (Mastership m = Mastership.Request(controller.Rapid))
                                            this.controller.Rapid.Stop(StopMode.Cycle);
                                    }
                                    else MessageBox.Show("Controller is not running");
                                }
                            }
                            else
                                MessageBox.Show("No conncetion with any the controller. You have to select a controller", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        break;
                        case "Instruction":
                            if (controller != null)
                            {
                                output = MessageBox.Show("Stops RAPID execution when the current instruction is completed. ", "Information", MessageBoxButtons.OKCancel, MessageBoxIcon.Information);
                                if (output == DialogResult.OK)
                                {
                                    if (controller.OperatingMode == ControllerOperatingMode.Auto)
                                    {
                                        using (Mastership m = Mastership.Request(controller.Rapid))
                                            this.controller.Rapid.Stop(StopMode.Instruction);
                                    }
                                    else MessageBox.Show("Controller is not running");
                                }
                            }                           
                            else
                                MessageBox.Show("No conncetion with any the controller. You have to select a controller", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        break;
                        case "Immediate":
                            if (controller != null)
                                {
                                    output = MessageBox.Show("Stops RAPID execution immediately. ", "Information", MessageBoxButtons.OKCancel, MessageBoxIcon.Information);
                                    if (output == DialogResult.OK && controller != null)
                                {
                                    if (controller.OperatingMode == ControllerOperatingMode.Auto)
                                    {
                                        using (Mastership m = Mastership.Request(controller.Rapid))
                                            this.controller.Rapid.Stop(StopMode.Immediate);
                                    }
                                    else MessageBox.Show("Controller is not running");
                                }
                                }
                            else
                                MessageBox.Show("No conncetion with any the controller. You have to select a controller", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            break;
                    }
                }
                catch (System.InvalidOperationException ex)
                {
                    MessageBox.Show("Mastership is held by another client. " + ex.Message);
                }
                catch (System.Exception ex)
                {
                    MessageBox.Show("Unexpected error occurred: " + ex.Message);
                }
        }
#endregion

        #region event log types 
        private void eventLog_SelectedIndexChanged(object sender, EventArgs e)
        {
            listBox1.Items.Clear();
            for(int i=0;i<ctr;i++)
            {
                // if any controller(doubleclick) or all controller(chechkbox) is selected
                if (selectedAll == false && controller != null) 
                    log = controller.EventLog;// from double click take only one controller
                if(selectedAll == true && scannedController[i] != null )
                    scannedLog[i] = scannedController[i].EventLog; // selects all controller

                comboBox3.DropDownStyle = ComboBoxStyle.DropDown;
                try
                {
                    switch (comboBox3.SelectedItem.ToString())
                    {
                        case "Common":
                            if (controller != null && ctr == 1)
                            {
                                this.listBox1.Items.Add("EVENT LOG - COMMON LOG MESSAGES");
                                this.listBox1.Items.Add("");
                                cat = log.GetCategory(CategoryType.Common);
                                EventLogMSG(cat);
                            }
                            else if (scannedController[i] != null)
                            {
                                systemProp(i);
                                this.listBox1.Items.Add("EVENT LOG - COMMON LOG MESSAGES");
                                this.listBox1.Items.Add("");
                                scannedCat[i] = scannedLog[i].GetCategory(CategoryType.Common);
                                EventLogMSG(scannedCat[i]);
                            }
                            else
                                MessageBox.Show("No conncetion with any the controller. You have to select a controller", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            break;

                        case "Operational":
                            if (controller != null && ctr == 1)
                            {
                                this.listBox1.Items.Add("EVENT LOG - OPERATIONAL LOG MESSAGES");
                                this.listBox1.Items.Add("");
                                cat = log.GetCategory(CategoryType.Operational);
                                EventLogMSG(cat);
                            }
                            else if (scannedController[i] != null)
                            {
                                systemProp(i);
                                this.listBox1.Items.Add("EVENT LOG - OPERATIONAL LOG MESSAGES");
                                this.listBox1.Items.Add("");
                                scannedCat[i] = scannedLog[i].GetCategory(CategoryType.Operational);
                                EventLogMSG(scannedCat[i]);
                            }
                            else
                                MessageBox.Show("No conncetion with any the controller. You have to select a controller", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            break;

                        case "System":
                            if (controller != null && ctr == 1)
                            {
                                this.listBox1.Items.Add("EVENT LOG - SYSTEM LOG MESSAGES");
                                this.listBox1.Items.Add("");
                                cat = log.GetCategory(CategoryType.System);
                                EventLogMSG(cat);
                            }
                            else if (scannedController[i] != null)
                            {
                                systemProp(i);
                                this.listBox1.Items.Add("EVENT LOG - SYSTEM LOG MESSAGES");
                                this.listBox1.Items.Add("");
                                scannedCat[i] = scannedLog[i].GetCategory(CategoryType.System);
                                EventLogMSG(scannedCat[i]);
                            }
                            else
                                MessageBox.Show("No conncetion with any the controller. You have to select a controller", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            break;
                        case "Hardware":
                            if (controller != null && ctr == 1)
                            {
                                this.listBox1.Items.Add("EVENT LOG - HARDWARE LOG MESSAGES");
                                this.listBox1.Items.Add("");
                                cat = log.GetCategory(CategoryType.Hardware);
                                EventLogMSG(cat);
                            }
                            else if (scannedController[i] != null)
                            {
                                systemProp(i);
                                this.listBox1.Items.Add("EVENT LOG - HARDWARE LOG MESSAGES");
                                this.listBox1.Items.Add("");
                                scannedCat[i] = scannedLog[i].GetCategory(CategoryType.Hardware);
                                EventLogMSG(scannedCat[i]);
                            }
                            else
                                MessageBox.Show("No conncetion with any the controller. You have to select a controller", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            break;
                        case "Program":
                            if (controller != null && ctr == 1)
                            {
                                this.listBox1.Items.Add("EVENT LOG - PROGRAM LOG MESSAGES");
                                this.listBox1.Items.Add("");
                                cat = log.GetCategory(CategoryType.Program);
                                EventLogMSG(cat);
                            }
                            else if (scannedController[i] != null)
                            {
                                systemProp(i);
                                this.listBox1.Items.Add("EVENT LOG - PROGRAM LOG MESSAGES");
                                this.listBox1.Items.Add("");
                                scannedCat[i] = scannedLog[i].GetCategory(CategoryType.Program);
                                EventLogMSG(scannedCat[i]);
                            }
                            else
                                MessageBox.Show("No conncetion with any the controller. You have to select a controller", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            break;
                        case "Motion":
                            if (controller != null && ctr == 1)
                            {
                                this.listBox1.Items.Add("EVENT LOG - MOTION LOG MESSAGES");
                                this.listBox1.Items.Add("");
                                cat = log.GetCategory(CategoryType.Motion);
                                EventLogMSG(cat);
                            }
                            else if (scannedController[i] != null)
                            {
                                systemProp(i);
                                this.listBox1.Items.Add("EVENT LOG - MOTION LOG MESSAGES");
                                this.listBox1.Items.Add("");
                                scannedCat[i] = scannedLog[i].GetCategory(CategoryType.Motion);
                                EventLogMSG(scannedCat[i]);
                            }
                            else
                                MessageBox.Show("No conncetion with any the controller. You have to select a controller", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            break;

                        case "IOandCommunication":
                            if (controller != null && ctr == 1)
                            {
                                this.listBox1.Items.Add("EVENT LOG - IO and COMMUNICATION LOG MESSAGES");
                                this.listBox1.Items.Add("");
                                cat = log.GetCategory(CategoryType.IOCommunication);
                                EventLogMSG(cat);
                            }
                            else if (scannedController[i] != null)
                            {
                                systemProp(i);
                                this.listBox1.Items.Add("EVENT LOG - IO and COMMUNICATION LOG MESSAGES");
                                this.listBox1.Items.Add("");
                                scannedCat[i] = scannedLog[i].GetCategory(CategoryType.IOCommunication);
                                EventLogMSG(scannedCat[i]);
                            }
                            else
                                MessageBox.Show("No conncetion with any the controller. You have to select a controller", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            break;
                        case "User":
                            if (controller != null && ctr == 1)
                            {
                                this.listBox1.Items.Add("EVENT LOG - USER LOG MESSAGES");
                                this.listBox1.Items.Add("");
                                cat = log.GetCategory(CategoryType.User);
                                EventLogMSG(cat);
                            }
                            else if (scannedController[i] != null)
                            {
                                systemProp(i);
                                this.listBox1.Items.Add("EVENT LOG - USER LOG MESSAGES");
                                this.listBox1.Items.Add("");
                                scannedCat[i] = scannedLog[i].GetCategory(CategoryType.User);
                                EventLogMSG(scannedCat[i]);
                            }
                            else
                                MessageBox.Show("No conncetion with any the controller. You have to select a controller", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            break;

                        case "Internal":
                            if (controller != null && ctr == 1)
                            {
                                this.listBox1.Items.Add("EVENT LOG - INTERNAL LOG MESSAGES");
                                this.listBox1.Items.Add("");
                                cat = log.GetCategory(CategoryType.Internal);
                                EventLogMSG(cat);
                            }
                            else if (scannedController[i] != null)
                            {
                                systemProp(i);
                                this.listBox1.Items.Add("EVENT LOG - INTERNAL LOG MESSAGES");
                                this.listBox1.Items.Add("");
                                scannedCat[i] = scannedLog[i].GetCategory(CategoryType.Internal);
                                EventLogMSG(scannedCat[i]);
                            }
                            else
                                MessageBox.Show("No conncetion with any the controller. You have to select a controller", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            break;
                        case "Process":
                            if (controller != null && ctr == 1)
                            {
                                this.listBox1.Items.Add("EVENT LOG - PROCESS LOG MESSAGES");
                                this.listBox1.Items.Add("");
                                cat = log.GetCategory(CategoryType.Process);
                                EventLogMSG(cat);
                            }
                            else if (scannedController[i] != null)
                            {
                                systemProp(i);
                                this.listBox1.Items.Add("EVENT LOG - PROCESS LOG MESSAGES");
                                this.listBox1.Items.Add("");
                                scannedCat[i] = scannedLog[i].GetCategory(CategoryType.Process);
                                EventLogMSG(scannedCat[i]);
                            }
                            else
                                MessageBox.Show("No conncetion with any the controller. You have to select a controller", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            break;
                        case "Configuration":
                            if (controller != null && ctr == 1)
                            {
                                this.listBox1.Items.Add("EVENT LOG - CONFIGURATION LOG MESSAGES");
                                this.listBox1.Items.Add("");
                                cat = log.GetCategory(CategoryType.Configuration);
                                EventLogMSG(cat);
                            }
                            else if (scannedController[i] != null)
                            {
                                systemProp(i);
                                this.listBox1.Items.Add("EVENT LOG - CONFIGURATION LOG MESSAGES");
                                this.listBox1.Items.Add("");
                                scannedCat[i] = scannedLog[i].GetCategory(CategoryType.Configuration);
                                EventLogMSG(scannedCat[i]);
                            }
                            else
                                MessageBox.Show("No conncetion with any the controller. You have to select a controller", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            break;
                    }
                }
                catch (System.InvalidOperationException ex)
                {
                    MessageBox.Show("Mastership is held by another client. " + ex.Message);
                }
                catch (System.Exception ex)
                {
                    MessageBox.Show("Unexpected error occurred: " + ex.Message);
                }
            }
        }
        #endregion

        #region description of the event log messages listBox1 -> DoubleClick
        private void listBox1_DoubleClick(object sender, EventArgs e)
        {
            // this property works only using single controller

            // DATA decleration and initializition
            int x = 0;
            x = listBox1.SelectedIndex - 4;
            string Consequences_substring;
            string Description_substring;
            string[] searchString = new string[2];
            searchString[0] = "<Description>";
            searchString[1] = "<Consequences>";
            int[] startIndex = new int[2];
            int[] endIndex = new int[2];

            if (selectedAll == false)
            {
                try
                {
                    // Description part of the body
                    startIndex[0] = eventLogBody[x].IndexOf(searchString[0]);
                    searchString[0] = "</" + searchString[0].Substring(1);
                    endIndex[0] = eventLogBody[x].IndexOf(searchString[0]);
                    Description_substring = eventLogBody[x].Substring(startIndex[0] + 13, endIndex[0] - 27 + searchString[0].Length - startIndex[0]);

                    // Consequences part of the body
                    startIndex[1] = eventLogBody[x].IndexOf(searchString[1]);
                    searchString[1] = "</" + searchString[1].Substring(1);
                    endIndex[1] = eventLogBody[x].IndexOf(searchString[1]);
                    if ((startIndex[1] + 14) < 0 || (endIndex[1] - 29 + searchString[1].Length - startIndex[1]) < 0)
                    {
                        MessageBox.Show("Description : \n" + Description_substring, "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        goto next;
                    }
                    Consequences_substring = eventLogBody[x].Substring(startIndex[1] + 14, endIndex[1] - 29 + searchString[1].Length - startIndex[1]);

                    MessageBox.Show("Description : \n" + Description_substring
                        + "\n\nConsequences: \n" + Consequences_substring, "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    next:;
                }
                catch (Exception)
                {
                    return;
                }
            }
        }
        #endregion

        #region opens the event log file
        private void openEventLogPath_Click(object sender, EventArgs e)
        {
            openFileButton(textBox4.Text);
        }
        #endregion

        #region save as button
        private void saveAs_Click(object sender, EventArgs e)
        {
            if(listBox1.Items.Count <= 1)
            {
                return;
            }
            try
            {
                bool process = false;
                while (!process)
                {
                    this.Cursor = Cursors.WaitCursor;
                    saveFile();
                    process = true;
                }
                this.Cursor = Cursors.Default;
                listBox1.Items.Clear();
            }
            catch (Exception)
            {
                this.Cursor = Cursors.Default;
                MessageBox.Show("Please select one type of event log","Warning",MessageBoxButtons.OK,MessageBoxIcon.Warning);
            }
        }
        #endregion

        #region first create text file and then send mail this file as an attchement
        private void sendMail_Click(object sender, EventArgs e)
        {
            bool process = false;
            // to create a text file that contains event log in the current directory
            Directory.SetCurrentDirectory(EventLogComputerPath);
            //save.InitialDirectory
            fileName = comboBox3.SelectedItem.ToString() + "-" + textBox1.Text.ToString() + "-" + DateTime.Now.ToString("-yyyy_MM_dd-HH_mm_ss") + ".txt";
            save.FileName = fileName;
            save.Filter = "Text File (.txt)|*.txt";
            
            StreamWriter writer = new StreamWriter(save.OpenFile());
            writer.WriteLine("System Name : " + controller.SystemName);
            writer.WriteLine("IP Address  : " + controller.IPAddress);
            writer.WriteLine("MAC Address : " + controller.MacAddress);
            for (int i = 0; i < listBox1.Items.Count; i++)
            {
                writer.WriteLine(listBox1.Items[i].ToString());
            }
            writer.Dispose();
            writer.Close();
            // this makes created file is read only
            File.SetAttributes(save.FileName, FileAttributes.ReadOnly);
            // this file is used as attchement file. after the sending process it will be deleted
            MailMessage msg = new MailMessage();
            SmtpClient client = new SmtpClient("smtp.gmail.com");
            client.Credentials = new System.Net.NetworkCredential("okanokumuss@gmail.com", "OK425452AN");
            client.Port = 587;
            client.EnableSsl = true;
            try
            {
                msg.To.Add(email.Text.ToString());
                msg.From = new MailAddress("okanokumuss@gmail.com", "Robot Surveillance System");
                try
                {
                    msg.Subject = "ABB ROBOT - " + textBox1.Text.ToString() + " - EVENT LOG " + comboBox3.SelectedItem.ToString();
                    msg.Body = "As your request, the event log of the robot are atteched to this mail";
                    // attchement the .txt file 
                    System.Net.Mail.Attachment attachment;
                    attachment = new System.Net.Mail.Attachment(fileName);
                    EventLogPath = EventLogComputerPath;
                    textBox4.Text = EventLogPath;
                    msg.Attachments.Add(attachment);
                    while (!process)
                    {
                        this.Cursor = Cursors.WaitCursor;
                        client.Send(msg);
                        process = true;
                    }
                    this.Cursor = Cursors.Default;
                    Directory.SetCurrentDirectory(localDir);
                    MessageBox.Show("The file is sent to your email!", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception)
                {
                    this.Cursor = Cursors.Default;
                    MessageBox.Show("1. Select controller\n2.Select type of event log", "Warning", MessageBoxButtons.OK,MessageBoxIcon.Warning);
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Invalid email address!!!", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        #endregion

        #region select path restore the system from the backup file
        private void selectPath_Click(object sender, EventArgs e)
        {
            using (var fbd = new FolderBrowserDialog())
            {
                DialogResult result = fbd.ShowDialog();
                restorePath = fbd.SelectedPath;
                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                {
                    textBox3.Text = restorePath;
                    string[] files = Directory.GetFiles(fbd.SelectedPath);
                    try
                    {
                        if (controller.OperatingMode == ControllerOperatingMode.Auto)
                        {
                            DialogResult output = MessageBox.Show("Do you really want to restore your system!", "Warning", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
                            if (output == DialogResult.OK)
                            {
                                using (Mastership m = Mastership.Request(controller.Rapid))
                                using (Mastership m2 = Mastership.Request(controller.Configuration))
                                    controller.Restore(restorePath,RestoreIncludes.All,RestoreIgnores.All);
                            }
                            else
                                MessageBox.Show("Cancelled!", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        else MessageBox.Show("Auto mode is required to clear all event log from a remote client.");
                    }
                    catch (System.InvalidOperationException ex)
                    {
                        MessageBox.Show("Mastership is held by another client." + ex.Message);
                    }
                    catch (System.Exception ex)
                    {
                        MessageBox.Show("Unexpected error occurred: " + ex.Message);
                    }
                }
            }
        }
        #endregion

        #region to take backup all controllers in the folder
        private void backupAll_Click(object sender, EventArgs e)
        {
            
            for (int i = 0; i < ctr; i++)
            {
                // after selected a controller, if clear all scanned result and scanned again.
                if (scannedController[i] == null)
                {
                    MessageBox.Show("No conncetion with any the controller", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                try
                {
                    if ((scannedController[i].OperatingMode == ControllerOperatingMode.ManualReducedSpeed) || (scannedController[i].OperatingMode == ControllerOperatingMode.Auto))
                    {
                        try
                        {
                            // for real controller
                            if (scannedVirtualReal[i] != "Virtual")
                            {
                                try
                                {
                                    // 1. step is to create backup file onto the robot controller
                                    scannedController[i].FileSystem.RemoteDirectory = @"/hd0a";
                                    System.IO.Directory.CreateDirectory(BackupComputerPath);
                                    FolderString = System.IO.Path.Combine(BackupComputerPath, scanneditem[i].Tag.ToString() + "-Real" + DateTime.Now.ToString("-yyyy_MM_dd-HH_mm_ss"));
                                    //backup of the current system of real controller
                                    scannedController[i].Backup(FolderString);
                                    // backup progress start for real controller
                                    // 2. step is to wait until backup progress is finished
                                    do
                                    {
                                        this.Cursor = Cursors.WaitCursor;
                                        System.Threading.Thread.Sleep(2000);
                                        this.Cursor = Cursors.Default;
                                    } while (scannedController[i].BackupInProgress);
                                    System.Console.Write(".");
                                    // 3. step is to take backup file to the computer from controller
                                    scannedController[i].FileSystem.GetDirectory(FolderString, true);
                                    // 4. step is to remove backup file from controller
                                    scannedController[i].FileSystem.RemoveDirectory(FolderString, true);
                                    textBox2.Text = BackupComputerPath;
                                    FolderString = "";

                                    MessageBox.Show("Backup file is created", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                }
                                catch (Exception)
                                {
                                    MessageBox.Show("Backup file is not created", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                }

                            }
                            // for virtual controller
                            else if (scannedVirtualReal[i] != "Real")
                            {
                                try
                                {
                                    FolderString = System.IO.Path.Combine(FolderString, scanneditem[i].Tag.ToString() + "-Virtual" + DateTime.Now.ToString("-yyyy_MM_dd-HH_mm_ss"));
                                    //backup of the current system of virtual controller
                                    scannedController[i].Backup(BackupComputerPath + FolderString);
                                    textBox2.Text = BackupComputerPath;
                                    FolderString = "";
                                    // backup progress start for real controller
                                    do
                                    {
                                        this.Cursor = Cursors.WaitCursor;
                                        System.Threading.Thread.Sleep(2000);
                                        this.Cursor = Cursors.Default;
                                    } while (scannedController[i].BackupInProgress);
                                    System.Console.Write(".");
                                    MessageBox.Show("Backup file of the " + scanneditem[i].Tag.ToString() + " is created", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                }
                                catch (Exception)
                                {
                                    MessageBox.Show("Backup file is not created", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                }
                            }
                        }
                        catch (System.Exception x)
                        {
                            MessageBox.Show("An error occured when backup file is creted" + x, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                    else
                        MessageBox.Show("Manuel or Automatic mode is required to start execution from a remote client.");

                }
                catch (System.InvalidOperationException ex)
                {
                    MessageBox.Show("Mastership is held by another client." + ex.Message);
                }
                catch (System.Exception ex)
                {
                    MessageBox.Show("Unexpected error occurred: " + ex.Message);
                }
            }
        }
        #endregion

        #region checkboxes (select all controller and backup to archive file )
        // to select all controller 
        private void selectAllController_CheckedChanged(object sender, EventArgs e)
        {
            if (selectAllController.Checked == true) // only when all controllers are selected
            {
                selectedAll = true;
                controller = null;
                textBox1.Text = "";
                listBox1.Items.Clear();
                comboBox1.SelectedItem = "ROBOT RESTART TYPES";
                comboBox2.SelectedItem = "STOP MODES";
                comboBox3.SelectedItem = "EVENT LOG";
                comboBox5.SelectedItem = "SIGNALS";
                // ctr -> number of scanned controller
                ctr = this.listView1.Items.Count;
                // to access all controller
                for (int i = 0; i < ctr; i++)
                {
                    ListViewItem item = this.listView1.Items[i];
                    scanneditem[i] = item;
                    comboBox4.Items.Add(item.Tag.ToString());
                    if (item.Tag != null)
                    {
                        scannedControllerInfo[i] = (ControllerInfo)item.Tag;
                        if (scannedControllerInfo[i].Availability == Availability.Available)
                        {
                            if (this.scannedController[i] != null)
                            {
                                this.scannedController[i].Logoff();
                                this.scannedController[i].Dispose();
                                this.scannedController[i] = null;
                            }
                            try
                            {
                                this.scannedController[i] = ControllerFactory.CreateFrom(scannedControllerInfo[i]);
                                this.scannedController[i].Logon(UserInfo.DefaultUser);
                                if (scannedControllerInfo[i].IsVirtual) scannedVirtualReal[i] = "Virtual";
                                else scannedVirtualReal[i] = "Real";
                            }
                            catch (Exception) { MessageBox.Show("No conncetion with the controller", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning); }
                        }
                        else MessageBox.Show("Selected controller not available.");
                    }
                }
            }
            else
            {
                // no controller is selected
                selectedAll = false;
                comboBox4.Items.Clear();
                comboBox1.SelectedItem = "ROBOT RESTART TYPES";
                comboBox2.SelectedItem = "STOP MODES";
                comboBox3.SelectedItem = "EVENT LOG";
                comboBox5.SelectedItem = "SIGNALS";
                for (int i= 0; i < ctr; i++)
                {
                    scannedController[i] = null;
                }
                ctr = 1;
            }
        }
        private void archive_CheckedChanged(object sender, EventArgs e)
        {
            if (archive.Checked == true) // only when to select to save as archive file
            {
                try
                {
                    //https://www.codeguru.com/csharp/.net/zip-and-unzip-files-programmatically-in-c.htm
                    //https://coderwall.com/p/hgotua/simple-way-to-zip-files-with-c-net-framework-4-5-4-6

                    ZipFile.CreateFromDirectory(PrezipFolder, zipFolder);
                    TryToDelete(PrezipFolder);
                }
                catch (Exception)
                {
                    return;
                }
            }
        }
        #endregion

        #region APP BUTTONS (EXIT,RESTART)
        // exit the application 
        private void button5_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
        // RESTART the application 
        private void button7_Click(object sender, EventArgs e)
        {
            Application.Restart();
        }
        #endregion

        #region SUBFUNCTIONS
        //add or remove controller to the listview
        void ControllerToListView(object sender, NetworkWatcherEventArgs e)
        {
            ControllerInfo controllerInfo = e.Controller;
            if (controllerInfo.IsVirtual) VirtualReal = "Virtual";
            else VirtualReal = "Real";
            ListViewItem item = new ListViewItem(controllerInfo.IPAddress.ToString());
            item.SubItems.Add(controllerInfo.SystemId.ToString());// guid
            item.SubItems.Add(controllerInfo.Availability.ToString());
            item.SubItems.Add(controllerInfo.IsVirtual.ToString());
            item.SubItems.Add(controllerInfo.SystemName);
            item.SubItems.Add(controllerInfo.Version.ToString());
            item.SubItems.Add(controllerInfo.ControllerName);
            this.listView1.Items.Add(item);
            item.Tag = controllerInfo;
        }
        // open the file of the given path
        private void openFileButton(string path)
        {
            if (path != "")
            {
                try
                {
                    System.Diagnostics.Process.Start(path);
                }
                catch (Exception)
                {
                    MessageBox.Show("File path could not found by system", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            else MessageBox.Show("No file is created", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
        // creates txt file from listbox  for only double click
        private void saveFile()
        {
            save.InitialDirectory = EventLogComputerPath;
            save.Filter = "Text File (.txt)|*.txt"; //  "Log File(.log)|*.log"; 
            // opening the save file dialog 
            save.RestoreDirectory = true;
            if (selectedAll == false) // one controller is selected
            {
                
                save.FileName = textBox1.Text.ToString() + "-" + "EVENT LOG - " + comboBox3.SelectedItem.ToString() + DateTime.Now.ToString("-yyyy_MM_dd-HH_mm") + ".txt";
                if (save.ShowDialog() == DialogResult.OK)
                {
                    StreamWriter writer = new StreamWriter(save.OpenFile());
                    textBox4.Text = EventLogComputerPath;
                    writer.WriteLine("System Name : " + controller.SystemName);
                    writer.WriteLine("IP Address  : " + controller.IPAddress);
                    writer.WriteLine("MAC Address : " + controller.MacAddress);
                    writer.WriteLine(" ");
                    for (int i = 0; i < listBox1.Items.Count; i++)
                    {
                        writer.WriteLine(listBox1.Items[i].ToString());
                    }
                    writer.Dispose();
                    writer.Close();
                    // this makes created file is read only
                    File.SetAttributes(save.FileName, FileAttributes.ReadOnly);
                }
            }
            if(selectedAll == true)
            {
                save.FileName = "ABB ROBOTS" + "-" + comboBox3.SelectedItem.ToString() + "EventLog" + DateTime.Now.ToString("-yyyy_MM_dd-HH_mm") + ".txt";
                if (save.ShowDialog() == DialogResult.OK)
                {
                    StreamWriter writer = new StreamWriter(save.OpenFile());
                    textBox4.Text = EventLogComputerPath;
                    for (int i = 0; i < listBox1.Items.Count; i++)
                    {
                        writer.WriteLine(listBox1.Items[i].ToString());
                    }
                    writer.Dispose();
                    writer.Close();
                    // this makes created file is read only
                    File.SetAttributes(save.FileName, FileAttributes.ReadOnly);
                }
            }
        }
        private void EventLogMSG(EventLogCategory cat)
        {
            int i = 0;   
            listBox1.Items.Add(("Seq. Number").PadRight(15) + ("Category ID").PadRight(15) + ("Title").PadRight(50) + ("Category Type").PadRight(20) + ("Type").PadRight(20) + ("Date and Time").PadRight(30) );
            this.listBox1.Items.Add("");
            if (cat.Messages.Count() != 0)
            {
                int x = cat.Messages.Count();
                foreach (EventLogMessage emsg in cat.Messages)
                {
                    eventLogBody[i] = emsg.Body.ToString();
                    i++;
                    this.listBox1.Items.Add(emsg.SequenceNumber.ToString().PadRight(15) + emsg.CategoryId.ToString().PadRight(15) + emsg.Title.ToString().PadRight(50) + emsg.CategoryType.ToString().PadRight(20) + emsg.Type.ToString().PadRight(20) + emsg.Timestamp.ToString().PadRight(30));
                }
            }
            else return;//MessageBox.Show("There is nothing to show inside!", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        void initConditions()
        {
            selectAllController.Checked = false;
            selectedAll = false;
            listView1.Items.Clear();
            listBox1.Items.Clear();
            comboBox4.Items.Clear();
            textBox1.Text = "";
            textBox2.Text = "";
            textBox3.Text = "";
            EventLogPath = "";
            comboBox1.SelectedItem = "ROBOT RESTART TYPES";
            comboBox2.SelectedItem = "STOP MODES";
            comboBox3.SelectedItem = "EVENT LOG";
            comboBox4.SelectedItem = "ROBOTS";
            comboBox5.SelectedItem = "SIGNALS";
        }    
        void systemProp(int i)
        {
            this.listBox1.Items.Add("   #" + (i + 1).ToString());
            this.listBox1.Items.Add("System Name : " + scannedController[i].SystemName);
            this.listBox1.Items.Add("IP Address  : " + scannedController[i].IPAddress);
            this.listBox1.Items.Add("MAC Address : " + scannedController[i].MacAddress);
        }
        // try to delete given file
        void TryToDelete(string file)
        {
            try
            {
                // Try to delete the file.
                File.Delete(file);
            }
            catch (Exception ex)
            {
                // We could not delete the file.
                MessageBox.Show(ex.Message);
            }
        }
        #endregion
    }
}