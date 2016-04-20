using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Collections.Concurrent;

namespace TCPIPListener {
    public abstract class BaseListener {

        public event Action<string> Exception;
        public static string logPath {
            get {
                if (BPAPIData.Utilities.Environment.IsProductionEnvironment) return @"D:\p\logfiles\TCPIPListener\TCPIPListenerLog_[DATE].txt"; // Flyttet til D:-disk siden C:-disk er tom for diskplass
                return @"C:\p\logfiles\TCPIPListener\TCPIPListenerLog_[DATE].txt";
            }
        }

        public static CommandListener cmdListener;
        public static DatabaseListener dbListener;
        public static GatewayListener gwListener;

        private ConcurrentDictionary<string, Connection> _connections = new ConcurrentDictionary<string, Connection>();
        public ConcurrentDictionary<string, Connection> connections {
            get { return _connections; }
            //TODO: Implement observer pattern for smart update of the UI
            //set { if(value.Count >  greater than some old_value) then notify the subscribers of the connection object}
        }
        ConnectionType connectionType;

        public static string _connectionIdPrefix = DateTime.Now.ToString("yyyy-MM-dd::HH:mm") + "_";
        private int _connectionCounter = 0;

        public BaseListener(System.Net.IPAddress address, int port, ConnectionType connectionType
            ) {
            this._connections = connections;
            this.connectionType = connectionType;
            acceptConnections(address, port);
            manageConnections();
        }

        private void manageConnections() {
            var thread = new System.Threading.Thread(monitorConnections);
            thread.Name = "T_" + this.GetType().ToString().Replace("TCPIPListener.", "") + "_Connection_Monitor_Start";
            thread.IsBackground = true;
            thread.Start();
        }

        private void monitorConnections() {
            do {
                try {
                    var forRemovalConnectionIds = new List<string>();
                    int currentRunning = 0;
                    int currentWaitingForActivation = 0;
                    _connections.ForEach(c => {
                        if (c.Value.task == null) {
                            Log("Found Connection without task (Connection.status: " + c.Value.status.ToString() + "). Ignoring");
                            // forRemovalConnectionIds.Add(connectionId); // Dukket opp ved bytte til 64 bit kanskje?
                        } else {
                            switch (c.Value.task.Status) {
                                case TaskStatus.Running:
                                    currentRunning++;
                                    break; // Do nothing with this
                                case TaskStatus.WaitingForActivation: // Får jeg bare denne på Windows Server, ikke Windows Professional?
                                    currentWaitingForActivation++;
                                    break; // Gjør ingenting med denne
                                case TaskStatus.Canceled:
                                case TaskStatus.Faulted:
                                    // Kan for eksempel skyldes at klienten har sendt tom melding og at vi har terminert forbindelsen? Men kommer den virkelig da i "Faulted"?
                                    Log("Found task with status " + c.Value.task.Status.ToString() + " (" + c.Value.status + ")");
                                    forRemovalConnectionIds.Add(c.Key); // Det har skjedd noe fundamental galt nå men vi vet ikke nøyaktig hva. Sjekk koden i HandleConnectionAsync
                                    break;
                                case TaskStatus.RanToCompletion:
                                    // Log("Found task with status " + connectionId.task.status.ToString() + " (" + connectionId.status + ")");
                                    forRemovalConnectionIds.Add(c.Key); // Denne logger vi ikke
                                    break;
                                default:
                                    Log("Unknown TaskStatus (" + c.Value.task.Status.ToString() + ") (" + c.Value.task.Status + "), will remove from Collection (this message will not occur again for this task)");
                                    Increase("Connections_UnknownTaskStatus_Count_" + c.Value.task.Status.ToString()); // Bemerk at "gamle" data i databasen vil bli liggende når Listener restartes fordi vi nullstiller ikke tellingen
                                    forRemovalConnectionIds.Add(c.Key);
                                    break;
                            }
                        }
                        // Also remove the connections which are dead
                        //if (c.Value.isDead) forRemovalConnectionIds.Add(c.Key);
                    });
                    _statistics[BPAPIData.DeviceProperty.Connection_Count.ToString()] = _connections.Count;
                    _statistics[BPAPIData.DeviceProperty.Connection_CurrentWaitingForActivation.ToString()] = currentWaitingForActivation;
                    _statistics[BPAPIData.DeviceProperty.Connection_CurrentRunning.ToString()] = currentRunning;
                    if (forRemovalConnectionIds.Count > 0) Log("_connections.Count: " + _connections.Count + ", forRemovalConnectionIds.Count: " + forRemovalConnectionIds.Count);
                    forRemovalConnectionIds.ForEach(connectionId => {
                        Connection removableConnection;
                        if (_connections.TryRemove(connectionId, out removableConnection)) {
                            Increase("Connections_Count_" + removableConnection.task.Status.ToString());
                            if (removableConnection.task.Exception != null) { // Viktig at sjekker nå, hvis ikke blir disse helt usynlige og aldri logget.
                                Increase("Connections_Count_Exceptions_" + removableConnection.task.Status.ToString());
                                Log("Found Exception " + removableConnection.task.Exception.GetType().ToString() + " for task with status " + removableConnection.task.Status.ToString());
                                if (removableConnection.task.Exception is System.AggregateException) {
                                    var e = ((System.AggregateException)removableConnection.task.Exception);
                                    if (e.InnerExceptions.Count > 0) Log(removableConnection.task.Exception.GetType().ToString() + ": First inner exception is of type " + e.InnerExceptions[0].GetType().ToString());
                                }
                                HandleException(removableConnection.task.Exception);
                            }
                        }
                    });
                    if (forRemovalConnectionIds.Count > 0) Log("_connections.Count: " + _connections.Count);
                } catch (Exception ex) {
                    Log(ex.GetType().ToString() + " in MonitorConnections");
                    if (ex.Message.Contains("Collection was modified; enumeration operation may not execute")) {
                        Log("Ignoring because may happen now and then, but without any real consequence: '" + ex.Message + "'");
                    } else {
                        HandleException(ex);
                    }
                } finally {
                    System.Threading.Thread.Sleep(5000);
                }
            } while (true);
        }

        private void acceptConnections(System.Net.IPAddress address, int port) {
            var thread = new System.Threading.Thread(new System.Threading.ThreadStart(async () => {
                try {
                    var listener = new System.Net.Sockets.TcpListener(address, port);
                    listener.Start();
                    Log(this.GetType().ToString() + " is running");
                    Log("Listening on " + address.ToString() + ", port " + port);

                    while (true) {
                        Log("Waiting for _connections...");
                        try {
                            var tcpClient = await listener.AcceptTcpClientAsync();
                            //var connection = new Connection() { connectionId = _connectionId, client = tcpClient, messages = new List<String>(), task = HandleConnectionAsync(tcpClient, ref connection) };                                                     

                            System.Threading.Interlocked.Increment(ref _connectionCounter);
                            var connectionId = _connectionIdPrefix + connectionType + "_" + _connectionCounter.ToString();

                            // Now ideally if a connection is created internally(which is dead usally) and then the connection actually becomes active, then also the code works! 
                            // This code makes sure the following scenario works as well:
                            // Send a message to gw connection id 1 from cmd listener 
                            // If gw connection id 1 is not present it is created automatically
                            // Create a gw connection, which will be assigned an auto increament value. It can get id 1 if it is the first one
                            // And send messages to gw connection id 1 from cmd or db listener. Previously with the code above this scenario would not have worked!
                            var connection = connections.AddOrUpdate(connectionId,
                                (id) => {
                                    var newConnection = new Connection();
                                    newConnection.client = tcpClient;
                                    newConnection.id = id;
                                    newConnection.connectionType = connectionType;
                                    newConnection.task = HandleConnectionAsync(tcpClient, newConnection);
                                    return newConnection;
                                },
                                (id, existingConnection) => {
                                    existingConnection.client = tcpClient;
                                    existingConnection.id = id;
                                    existingConnection.connectionType = connectionType;
                                    existingConnection.task = HandleConnectionAsync(tcpClient, existingConnection);
                                    // This lines below make sure that even if connection was created dead internally, it is overwritten if same id comes 
                                    existingConnection.isDead = false;
                                    existingConnection.isCreatedInternally = false;
                                    return existingConnection;
                                });
                            // To make sure the connection message queue is empty immediately  
                            connection.sendOutgoingMessages(null);
                            //HandlePropertyEventIgnoreExceptions(ListenerAsGatewayWithStatus, listenersAsDevices[_applicationId].NodeId, BPAPIData.DeviceProperty.tcp_ip_connection, "1");
                        } catch (Exception ex) {
                            Log("Exception when waiting for (or accepting) a connection. Will now continue to wait for _connections");
                            HandleException(ex);
                        }
                    }
                } catch (Exception ex) {
                    HandleException(ex);
                    Log(ex.GetType().ToString() + " starting listening at " + address.ToString() + ", port " + port.ToString());
                }
            })); // Must have own thread till this
            thread.Name = "T_" + this.GetType().ToString().Replace("TCPIPListener.", "") + "_Start";
            thread.IsBackground = true;
            thread.Start();
        }

        /// <summary>
        /// Vi lagrer egentlig bare 1-2 Tasks i denne samlingen. Kan muligens bare bruke en 
        /// </summary>
        private static ConcurrentDictionary<DateTime, Task> delayTasks = new ConcurrentDictionary<DateTime, Task>();
        /// <summary>
        /// Returnerer en task som vil fullføre mellom en og to timer inn i fremtiden
        /// (Vi trenger å gjenbruke Tasks mellom hver enkelt connection, hvis ikke får vi rett og slett for mange av dem)
        /// </summary>
        /// <returns></returns>
        private Task GetDelayTask() {
            var now = DateTime.Now;
            // var key = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, 0).AddMinutes(1); // Timeout in 0-60 seconds
            var key = new DateTime(now.Year, now.Month, now.Day, now.Hour, 0, 0).AddHours(2);

            Task retval; if (!delayTasks.TryGetValue(key, out retval)) {
                Log("No task exists with delay until " + key.ToString("yyyy-MM-dd::HH:mm:ss") + ", adding to collection");
                delayTasks[key] = Task.Delay((int)key.Subtract(DateTime.Now).TotalMilliseconds);
                Log("Count of dealyTasks: " + delayTasks.Count);
                Task dummy; if (delayTasks.TryRemove(key.AddHours(-1), out dummy)) Log("Successful remove, count of delayTasks: " + delayTasks.Count);
                _statistics["GetDelayTask_delayTasksCount"] = delayTasks.Count;
                retval = delayTasks[key];  // delayTasks.TryAdd er vanskelig å bruke men det spiller ingen rolle hvis det en gang i blant blir laget en for mye delay-task.
            }
            return retval;
        }

        protected string HandleException(Exception ex, [System.Runtime.CompilerServices.CallerMemberName] string caller = "") {
            var text = BPAPIData.Utilities.HandleException(ex, caller, logPath, "");
            Log("An exception of type " + ex.GetType().ToString() + " occurred. See " + System.IO.Path.GetDirectoryName(logPath) + " for details");
            if (Exception != null) Exception(text);
            return text;
        }

        private async Task HandleConnectionAsync(System.Net.Sockets.TcpClient tcpPartner, Connection connection) {
            const string methodName = "HandleConnectionAsync";
            Increase(methodName + "_countNew");
            var closingReason = "UNKNOWN";
            var partnerIP = "UNKNOWN_IP";

            System.Text.Encoding encoding = this is GatewayListener ? 
                System.Text.Encoding.GetEncoding(1252) : // Codepage 1252 made specificially for Sikom Gateways 2 Nov 2015
                System.Text.Encoding.UTF8;               // UFT8 is default-encoding for StreamReader / StreamWriter (note that default IS NOT Encoding.Default(!)). 
            try {
                using (var networkStream = tcpPartner.GetStream())
                using (var r = new System.IO.StreamReader(networkStream, encoding))  
                using (var w = new System.IO.StreamWriter(networkStream, encoding)) {
                    w.AutoFlush = true;                    
                    connection.writer = w;

                    while (true) {
                        try { partnerIP = tcpPartner.Client.RemoteEndPoint.ToString().Split(":")[0]; } catch (Exception) { }
                        string dataReceived;
                        var readTask = r.ReadLineAsync();
                        var readOrTimeout = await Task.WhenAny(readTask, GetDelayTask()); // Timeout om 1-2 timer (kan da miste to ping uten at vi tar ned forbindelsen). Bemerk at samme timeout for alle Listeners, men det er mest for GatewayListener at vi trenger denne.
                        if (readOrTimeout != readTask) {
                            Log("Timeout for " + tcpPartner);
                            Increase(methodName + "_countCloseByTimeout");
                            closingReason = "TIMEOUT";
                            connection.isDead = true;
                            break;
                        }
                        // TODO: System.ObjectDisposedException handling as awaiting task may be disposed!
                        dataReceived = await (Task<string>)readOrTimeout;
                        //}

                        //HandlePropertyEventIgnoreExceptions(ListenerAsGatewayWithStatus, listenersAsDevices[_applicationId].NodeId, BPAPIData.DeviceProperty.tcp_ip_message, "1");

                        //if (cc.abort) {
                        //    Log("cc.abort, aborting connection " + context.partnerInfo);
                        //    closingReason = "Listener is aborting (normally due do shutdown)";
                        //    break; // Viktig, eneste stedet vi kan fange opp at ikke lenger skal kjøre. Kjekt å ha når Listeners går amok i å snakke med hverandre (Gateway og Virtual spesielt)
                        //}

                        _statistics["Heartbeat"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"); // Marker at normal aktivitet (Normalt er det DoMethod som legger inn en Heartbeat, men TCPListeners har ingen metoder som kjøres av Start / DoMethod og vil virke døde hvis vi ikke gjør noe slikt som dette).
                        //context.TimeOfLastUpdateFromPartner = DateTime.Now;

                        if (string.IsNullOrEmpty(dataReceived)) {
                            Increase(methodName + "_countEmptyMessages"); // Tom melding later til å være det vi får tilsendt fra for eksempel en .NET client når denne lukker
                            closingReason = "Connection terminated by client";     // Sparer oss for å sende ekstra "quit" melding fra BPAPI, så vi kan likegjerne lukke på dette kriteriet
                            break;
                        }

                        if ("quit".Equals(dataReceived.ToLower())) {
                            Increase(methodName + "_countCloseByPartner");
                            closingReason = "'quit' received from partner)";
                            break;
                        }

                        Increase(methodName + "_countMessage"); // Bemerk også BPAPIData.DeviceProperty.tcp_ip_message ovenfor (som går inn som aggregation i databasen)

                        Message message = new Message();
                        message.msgVal = dataReceived;
                        connection.timeOfLastUpdateFromPartner = DateTime.Now;
                        message.incomingConnection = connection;                    
                        if (connection.connectionType == ConnectionType.CMD && message.msgVal.Equals(BPAPIData.GWListenerResponse.GW_RESPONSE_TIME_MEASURING.Description())) {
                            // If it is simply "ResponseTimeMeasuring" from cmd listener port 14147, just send back and OK? response
                        } else {
                            message.updateInternalPartnerConnectionId(_connectionIdPrefix);
                        }                   

                        // Finally process message and send to the correct channel
                        Log("Message from " + this.GetType() + " : " + message.msgVal);
                        processIncomingMessage(message);
                    }
                }
            } catch (Exception ex) {
                Increase(methodName + "_countException");
                //Log("Exception " + ex.GetType().ToString() + ", message " + ex.Message);
                // FJERN! if (gateway != null) Log("GatewayId: " + gateway.Gateway.GatewayId + " (" + gateway.Gateway.GatewayType.ToString() + "), customerId " + gateway.Gateway.CustomerId);
                if (ex is System.IO.IOException) {
                    closingReason = ex.GetType().ToString() + " occurred";
                    Log(closingReason + ", not showing any details (assuming 'normal' exception)");
                    // Typically happens when the gateway goes offline.
                } else if (ex is System.ObjectDisposedException) { // Natural result of connection being closed on receiving a CLOSE_CONNECTION from DBServiceListener. See Connection class method closeConnection()
                    closingReason = ex.GetType().ToString() + " occurred";
                    Log(closingReason + ", not showing any details (assuming 'normal' exception)");
                } else {
                    closingReason = ex.GetType().ToString() + " occurred: " + ex.Message;
                    Log("Exception occured die to: " + closingReason);
                    HandleException(ex);
                }
            } finally { // Forsiktig at ikke generer Exceptions nå, det vil ta nede hele applikasjon!!  ????  Catcher derfor det meste nå (Eller gjaldt problemet kanskje FØR vi valgte å returnere TASK herfra?)
                Increase(methodName + "_countClosing");
                var msg = "Closing connection, reason: " + closingReason;
                Log(msg);

                try {
                    if (null != connection) connection.isDead = true;
                    if (null != connection && connection.connectionType == ConnectionType.GW) {
                        if (!connection.isResponseTrackerConnection) {
                            var closingMsg = new Message();
                            closingMsg.incomingConnection = connection;
                            closingMsg.isInvalidFormat = true;
                            closingMsg.isNewSerialNumberReceivedFromGateway = "F";
                            closingMsg.remotePartnerIP = partnerIP;
                            closingMsg.internalPartnerConnectionId = connection.id;
                            closingMsg.msgVal = BPAPIData.GWListenerResponse.GW_CLOSED_RESPONSE.Description();
                            // note: Illegal gatway message response is not possible to send to cmd listener in the current architecture
                            dbListener.processIncomingMessage(closingMsg);
                        }
                    }
                } catch (Exception) { }
                try { tcpPartner.Close(); } catch (Exception) { } // TODO: Enn hvis denne genererer Exception! Ikke bra!
            }
        }

        public abstract void processIncomingMessage(Message message);

        protected virtual string LogAndReturn(string text) {
            Increase("LogAndReturn_" + text); // Legger ikke ved gateway eller partnerInfo her(?). Kan eventuelt heller gjøre på det på device.
            return text;
        }

        protected void Log(string text, [System.Runtime.CompilerServices.CallerMemberName] string caller = "") {
            BPAPIData.Utilities.Log(this, caller, logPath, text);
        }

        protected void Increase(string key) {
            Increase(key, 1);
        }

        protected void Increase(string key, long add) {
            if (!_statistics.ContainsKey(key)) {
                _statistics[key] = add;
            } else {
                _statistics[key] = (long)_statistics[key] + add;
            }
        }

        protected ConcurrentDictionary<string, object> _statistics = new ConcurrentDictionary<string, object>();
        public string GetStatisticsAsString() {
            var retval = new StringBuilder();
            //retval.AppendLine(this.GetType().ToString());
            _statistics.ForEach(i => {
                retval.AppendLine(i.Key + ": " + i.Value.ToString());
            });

            return retval.ToString();
        }
        public ConcurrentDictionary<string, object> Statistics { get { return _statistics; } }
    }
}