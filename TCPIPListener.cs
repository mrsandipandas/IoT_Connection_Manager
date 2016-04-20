using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Collections.Concurrent;

namespace TCPIPListener {

    public partial class TCPIPListener : Form {
        //ConcurrentDictionary<String, BaseListener.Connection> connections = new ConcurrentDictionary<String, BaseListener.Connection>();

        Timer t;
        bool startListener = false;
        public DateTime lastUpdateOfVitalStatus = new DateTime(2001, 1, 1);

        public TCPIPListener() {
            InitializeComponent();
            displayConnections();
        }

        private void TCPIPListener_Load(object sender, EventArgs e) {
            controlComponents(true);
        }

        //private async Task sendResponseMessageAfterTimeout() {
        //    await Task.Run(async () => //Task.Run automatically unwraps nested Task types!
        //    {
        //        await Task.Delay(TimeSpan.FromSeconds(5));
        //        Console.WriteLine(DateTime.MinValue);
        //    });
        //}

        private void start_Click(object sender, EventArgs e) {
            //while (true) {
            //    var t = sendResponseMessageAfterTimeout();
            //}

            BaseListener.gwListener = new GatewayListener(System.Net.IPAddress.Any, Constants.GW_PORT); // gateway listener for the gateway devices
            BaseListener.cmdListener = new CommandListener(System.Net.IPAddress.Loopback, Constants.CMD_PORT); // command listener for web api's
            BaseListener.dbListener = new DatabaseListener(System.Net.IPAddress.Loopback, Constants.DB_PORT); // database listener, can be multiple

            gwConnLabel.Text += Constants.GW_PORT;
            cmdConnLabel.Text += Constants.CMD_PORT;
            dbConnLabel.Text += Constants.DB_PORT;
            controlComponents(false);
            //var cycle = -1;
            //while (startListener) {
            //    cycle++;
            //    if (lastUpdateOfVitalStatus.AddSeconds(3) < DateTime.Now) {
            //        lastUpdateOfVitalStatus = DateTime.Now;
            //        UpdateDisplay();
            //    }
            //}
        }

        private void UpdateDisplay() {
            dbListBox.Items.Clear();
            cmdListBox.Items.Clear();
            gwListBox.Items.Clear();
            try {
                try {
                    if (null != BaseListener.dbListener) {
                        dbListBox.Items.Add("Current message queue: ");

                        //TODO: Display the message queue, although it should be displayed on click of the message queue item. To be done later
                        BaseListener.dbListener.outgoingQueue.ForEach(m => {
                            dbListBox.Items.Add(m.msgVal);
                        });
                        dbListBox.Items.Add("----------------------------------------------------------------------------------------------------");
                        if (BaseListener.dbListener.connections.Count > 0) {
                            printMessages(BaseListener.dbListener, dbListBox, dBStats);
                        } else {
                            dbListBox.Items.Add("No database connections present now");
                        }
                    } else {
                        dbListBox.Items.Add("No database connections present now");
                    }
                } catch (Exception ex) {
                    Log("Error in displaying db listener connections in UI due to '" + ex.GetType().ToString() + "'");
                }

                try {
                    if (null != BaseListener.cmdListener && BaseListener.cmdListener.connections.Count > 0) {
                        printMessages(BaseListener.cmdListener, cmdListBox, cmdStats);
                    } else {
                        cmdListBox.Items.Add("No web api connections present now");
                    }
                } catch (Exception ex) {
                    Log("Error in displaying cmd listener connections in UI due to '" + ex.GetType().ToString() + "'");
                }

                try {
                    if (null != BaseListener.gwListener && BaseListener.gwListener.connections.Count > 0) {
                        printMessages(BaseListener.gwListener, gwListBox, gwStats);
                    } else {
                        gwListBox.Items.Add("No gateway connections present now");
                    }
                } catch (Exception ex) {
                    Log("Error in displaying cmd listener connections in UI due to '" + ex.GetType().ToString() + "'");
                }
            } catch (Exception e) {
                Log("An exception occured: " + e.GetType().ToString());
            }
        }

        public void printMessages(BaseListener baseListener, System.Windows.Forms.ListBox connectionStats, System.Windows.Forms.ListBox stats) {
            systemTimeLabel.Text = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            ConcurrentDictionary<string, Connection> allConnections = baseListener.connections;
            allConnections.OrderByDescending(c => c.Value.createdTime);
            allConnections.ForEach(c => {
                var key = c.Key;
                if (null != c.Value.client && c.Value.connectionType == ConnectionType.GW) {
                    try {
                        key += " (" + c.Value.client.Client.RemoteEndPoint.ToString().Split(":")[0] + ")";
                    } catch (Exception) {
                        Log("Connection Id: " + c.Key + " is already closed or dead!");
                    }
                }
                //Add a new connection and its current message queue count
                connectionStats.Items.Add(key + " => " + "Message queue count: " + c.Value.outgoingMessages.Count);

                //TODO: Display the message queue, although it should be displayed on click of the connection item. To be done later
                c.Value.outgoingMessages.ForEach(m => {
                    connectionStats.Items.Add(m.msgVal);
                });
                //TODO: Adding a deleimeter to identify things, remove later
                connectionStats.Items.Add("----------------------------------------------------------------------------------------------------");
            });

            stats.Items.Clear();
            if (baseListener is DatabaseListener) {
                stats.Items.Add("OutgoingMessageQueue_Count: " + ((DatabaseListener)baseListener).outgoingQueue.Count);
            }
            baseListener.Statistics.ForEach(i => {
                stats.Items.Add(i.Key + ": " + i.Value.ToString());
            });
        }

        protected void Log(string text, [System.Runtime.CompilerServices.CallerMemberName] string caller = "") {
            BPAPIData.Utilities.Log(this, caller, BaseListener.logPath, text);
            //Status(text);
            //AddToStatusList(text);
        }

        private void stop_Click(object sender, EventArgs e) {
            Log("Stopping TCP IP Listener...");
            try {
                //BaseListener.abort = true;
                //for (var i = 0; i < 20; i++) {
                //    Application.DoEvents();
                //    System.Threading.Thread.Sleep(50);
                //}
                //BaseListener.dbListener.closeconnections();
                //BaseListener.gwListener.closeconnections();
                //BaseListener.cmdListener.closeconnections();
                controlComponents(true);
                t.Stop();
                gwListBox.Items.Clear();
                cmdListBox.Items.Clear();
                gwListBox.Items.Clear();
            } catch (Exception) {
                //HandleException(ex);
            }
        }

        private void TCPIPListener_FormClosing(object sender, FormClosingEventArgs e) {
            Debug.WriteLine("Calling Application.Exit() form close");
            try {
                //BaseListener.abort = true;
                for (var i = 0; i < 20; i++) {
                    Application.DoEvents();
                    System.Threading.Thread.Sleep(50);
                }
                Application.Exit();
            } catch (Exception) {
                //HandleException(ex);
            }
        }

        private void controlComponents(bool start) {
            Start.Enabled = start;
            Stop.Enabled = !start;
            garbageCollect.Enabled = !start;
            //View.Enabled = !start;
            startListener = !start;
        }

        private void displayConnections() {
            try {
                t = new Timer { Enabled = true, Interval = 3 * 1000 };
                t.Tick += delegate { UpdateDisplay(); };
            } catch (Exception ex) {
                Console.WriteLine("Error updating connection view..." + ex.GetBaseException());
            }

            //var thread = new System.Threading.Thread(new System.Threading.ThreadStart(() => {
            //    try {
            //        var t = new Timer { Enabled = true, Interval = 3 * 1000 };
            //        t.Tick += delegate { UpdateDisplay(); };
            //    } catch (Exception ex) {
            //        Console.WriteLine("Error updating connection view..." + ex.GetBaseException());
            //    }
            //}));
            //thread.Name = "T_" + this.GetType().ToString().Replace("TCPIPListener.", "UpdateConnectionView") + "_Start";
            //thread.IsBackground = true;
            //thread.Start();
        }

        private void garbageCollect_Click(object sender, EventArgs e) {
            //Task.Factory.StartNew(() => {
            garbageCollect.Enabled = false;
            try {
                var memoryBefore = System.GC.GetTotalMemory(forceFullCollection: false);
                Log("GetTotalMemory: " + memoryBefore.ToString("N0") + " bytes, starting garbage collection...");
                System.GC.Collect();
                var memoryAfter = System.GC.GetTotalMemory(forceFullCollection: false);
                Log("Finished garbage collection. Difference: " + (memoryAfter - memoryBefore).ToString("N0") + " bytes. (PrivateMemorySize: " + System.Diagnostics.Process.GetCurrentProcess().PrivateMemorySize64.ToString("N0") + ")");
            } catch (Exception ex) {
                Log("Error type '" + ex.GetType().ToString() + "' while garbage collecting!!");
            } finally {
                garbageCollect.Enabled = true;
            }
            //});
        }

        private void tableLayoutPanel1_Paint(object sender, PaintEventArgs e) {

        }
    }
}