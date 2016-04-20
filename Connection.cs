using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace TCPIPListener {
    public class Connection {
        public string id { get; set; }
        public Task task { get; set; }
        public System.Net.Sockets.TcpClient client { get; set; }
        public System.IO.StreamWriter writer;
        public ConnectionType connectionType;
        public ConcurrentQueue<Message> outgoingMessages = new ConcurrentQueue<Message>();
        public int taskIsRunning = 0;
        
        /// <summary>
        /// This property helps in keeping track if the connection is dead
        /// </summary>
        public bool isDead = false;

        /// <summary>
        /// This property helps to track connections which are created internally i.e. if a db or cmd listener sends an illegal connection id a connection is created, in those cases we will update this property as true 
        /// Those connections which are created internally and are dead will be removed by monitor connections
        /// Since established connections can be dead anytime, this property helps in identifying between established connections which are dead and non-established connections which are dead 
        /// </summary>
        public bool isCreatedInternally = false;
       
        /// <summary>
        /// Keeps track of when the connection was created
        /// </summary>
        public DateTime createdTime = DateTime.Now;

        /// <summary>
        /// Properties for the serial no implementation
        /// Serial no is implemented per connection from the gateway listener to the gateway  
        /// </summary>
        public char lastServerSerialNo = 'A'; // ANSI Number of '@' = 64
        public char lastServerSerialNoReceivedFromGateway = ' ';
        public char lastGatewaySerialNo = ' '; // Better than \0 by default because the latter allows "printing" of string with which char inside itself terminates       

        public DateTime timeOfLastMessageSentToPartner;
        public DateTime timeOfLastUpdateFromPartner;

        /// <summary>
        /// Contains all partner connection ids for a connection, this can help extending the system for many to many connections as well
        /// Helps in identifying a reply message to the correct partner
        /// 
        /// CMDL1 --> GWL1: M1, M2, M4, M5 (So the CMDL1 will have the serial nos. A, B and E)
        /// CMDL2 --> GWL1: M3, M6 (So the CMDL2 will have the serial nos. C and F)
        /// GWL1  --> GW1 (This connection will have the serial nos. A, B, C, D, E, F which are disjointly shared between the different command listener connections)
        /// Hence, there is never a overlap of messages coming from the gateway listener to the command listener since the serial nos. are disjointly shared
        /// </summary>
        public ConcurrentDictionary<String, ConcurrentDictionary<char, DateTime>> partnerConnectionId = new ConcurrentDictionary<String, ConcurrentDictionary<char, DateTime>>();

        /// <summary>
        /// This parameter keeps track of if the connection is for tracking 4146/14147 port response times
        /// It might be helpful at a later point to manage connections more efficiently because of connection counter
        /// </summary>
        public bool isResponseTrackerConnection = false;

        /// <summary>
        /// Duplicated code in ConnectionContext inner class in BPAPIListener application 
        /// </summary>
        /// <param name="message"></param>
        public void sendOutgoingMessages(Message message) {
            if (null != message) outgoingMessages.Enqueue(message);
            if (isDead) return;
            try {
                var wasRunning = System.Threading.Interlocked.Exchange(ref taskIsRunning, 1);
                if (wasRunning != 0) {
                    return;
                }
                Task.Factory.StartNew(() => {
                    do {
                        try {
                            if (isDead) break;
                            //empty queue synchronously, process all outgoingMessages and write on the stream writer
                            Message outGoingMessage;
                            while (outgoingMessages.TryPeek(out outGoingMessage)) {
                                try {
                                    if (null != writer && writer.BaseStream != null) { //writer.BaseStream != null --Checked to avoid null pointer as of 28.07.2015
                                        writer.AutoFlush = true;
                                        //This write line is really meant to be synchronous, so do not put async write
                                        if (!String.IsNullOrEmpty(outGoingMessage.msgVal)) {
                                            writer.WriteLine(outGoingMessage.msgVal); // As of 06 Jan 2016 we still get System.NullReferenceException here (Stacktrace: at System.IO.StreamWriter.Flush, at System.IO.StreamWriter.Write, at System.IO.TextWriter.WriteLine)
                                            timeOfLastMessageSentToPartner = DateTime.Now;
                                        }   

                                        // Hack. ACK sendes separat fordi Sikom GSM Fixi Plus / GSM ECO-Comfort / GSM ECO-Starter 
                                        // vil ignorere responsen vår etter ACK hvis alt sammen havner i samme TCP/IP-pakke
                                        // I tillegg venter vi med resten. Antar at dette er tilstrekkelig for at pakken nå blir sendt 
                                        if (connectionType == ConnectionType.GW && outGoingMessage.incomingConnection.connectionType == ConnectionType.DB && BPAPIData.DBServiceResponse.ACK.ToString().Equals(outGoingMessage.msgVal)) {
                                            System.Threading.Thread.Sleep(50);
                                        }
                                        outgoingMessages.TryDequeue(out outGoingMessage);
                                    } else {
                                        isDead = true;
                                        break;
                                    }
                                } catch (Exception ex) {
                                    BPAPIData.Utilities.HandleException(ex, System.Reflection.MethodBase.GetCurrentMethod().Name, BaseListener.logPath, "");
                                    Log("Exception " + ex.GetType().ToString() + " when sending message '" + outGoingMessage.msgVal + "'. Marking connection as dead and returning message to queue...");
                                    isDead = true; // To be extra sure -:)
                                    break;
                                }
                            }
                        } finally {
                            taskIsRunning = 0;
                        }
                        // Don't remove this: This will really help in emptying the queue if two threads are working on the outgoingMessages queue
                        // For example if one thread empties the queue and sets the taskIsRunning = 0, while the other thread didn't start emptying the queue since the previous thread was running at that time
                        // This remaining code is used to avoid those situations, so that the queue are really empty always.
                        // This solution works as of June, 2015 in the initial phase development
                        if (outgoingMessages.Count == 0) {
                            break;
                        }
                        wasRunning = System.Threading.Interlocked.Exchange(ref taskIsRunning, 1);
                        if (wasRunning != 0) {
                            break;
                        }
                    } while (true);
                });
            } catch (Exception ex) {
                Log("Exception " + ex.GetType().ToString() + " in creating creating task from factory.");
            }
        }

        public char incrementLastServerSerialNo() {
            lock (this) {
                lastServerSerialNo++;
                if (lastServerSerialNo > 'Z') lastServerSerialNo = 'A';
            }
            return lastServerSerialNo;
        }

        public bool updateGatewaySerialNumber(string dataToServer) {
            lock (this) {
                bool retval = false;
                var d = dataToServer;
                if (d.Length >= 3) {
                    char clientSerialNo = d.Substring(0, 1).ToCharArray()[0];
                    if (clientSerialNo >= 65 && clientSerialNo <= 96) {
                        if (clientSerialNo != lastGatewaySerialNo) {
                            retval = true;
                            lastGatewaySerialNo = clientSerialNo;
                        }
                    }
                    char serverSerialNo = d.Substring(1, 1).ToCharArray()[0];
                    if (serverSerialNo >= 65 && serverSerialNo <= 96) {
                        if (serverSerialNo != lastServerSerialNoReceivedFromGateway) {
                            lastServerSerialNoReceivedFromGateway = serverSerialNo;
                        }
                    }
                } else {
                    Log("Illegal message, less than 3 characters in length");
                }
                return retval;
            }
        }

        public bool gatewayHasRespondedAfterThisServerSerialNo(int serverSerialNo) {
            lock (this) {
                if (null != timeOfLastUpdateFromPartner && timeOfLastUpdateFromPartner.AddSeconds(7) < DateTime.Now) return false;
                if (lastServerSerialNoReceivedFromGateway >= serverSerialNo) {
                    return ((lastServerSerialNoReceivedFromGateway - serverSerialNo) <= 7);
                } else {
                    return ((serverSerialNo - lastServerSerialNoReceivedFromGateway) >= 19);
                }
            }
        }

        public string status {
            get {
                try {
                    if (null != client) {
                        return client.Client.RemoteEndPoint.ToString() + " " + task.Status.ToString();
                    } else {
                        return "No remote end point: Dead Connection";
                    }
                } catch (System.ObjectDisposedException ex) { // Typisk er Message: Cannot access a disposed object. Object name: 'System.Net.Sockets.Socket'.
                    return ex.GetType().ToString() + " " + task.Status.ToString();
                }
            }
        }

        protected void Log(string text, [System.Runtime.CompilerServices.CallerMemberName] string caller = "") {
            BPAPIData.Utilities.Log(this, caller, BaseListener.logPath, id + ": " + text);
        }

        public void closeConnection() {
            isDead = true;
            try { writer.Close(); } catch (Exception) { }
            try { client.Close(); } catch (Exception) { }
        }
    }
}
