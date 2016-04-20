using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Windows.Forms;

namespace TCPIPListener {
    public class GatewayListener : BaseListener {
        public GatewayListener(System.Net.IPAddress address, int port)
            : base(address, port, ConnectionType.GW) {
        }

        public override void processIncomingMessage(Message incoming) {
            switch (incoming.incomingConnection.connectionType) {
                case ConnectionType.GW: {
                        try {
                            incoming.remotePartnerIP = incoming.incomingConnection.client.Client.RemoteEndPoint.ToString().Split(":")[0];
                        } catch (Exception) {
                            incoming.remotePartnerIP = "UnknownGateway";
                        }
                        var newSerialNumberReceivedFromGateway = incoming.incomingConnection.updateGatewaySerialNumber(incoming.msgVal);
                        Message incomingMsg = incoming.Clone();
                        if (newSerialNumberReceivedFromGateway) {
                            Increase("NewClientSerialNo_count");
                            incomingMsg.isNewSerialNumberReceivedFromGateway = "T";
                        } else {
                            incomingMsg.isNewSerialNumberReceivedFromGateway = "F";
                        }
                        if (incomingMsg.msgVal != null && incomingMsg.msgVal.Length < 3) {
                            incomingMsg.msgVal = BPAPIData.GWListenerResponse.ILLEGAL_GW_RESPONSE.Description() + incomingMsg.msgVal;
                            incomingMsg.isInvalidFormat = true;
                        }
                        // Response time measuring
                        if (incomingMsg.msgVal != null && incomingMsg.msgVal.Equals(BPAPIData.GWListenerResponse.GW_RESPONSE_TIME_MEASURING.Description())) {
                            incomingMsg.isInvalidFormat = true;
                            incoming.incomingConnection.isResponseTrackerConnection = true;
                        }

                        // Cloning is important because the same message has to go to different end points, otherwise if same reference then things can screw up
                        dbListener.processIncomingMessage(incomingMsg);
                        if (!incomingMsg.isInvalidFormat && !incomingMsg.msgVal.Equals(BPAPIData.GWListenerResponse.GW_RESPONSE_TIME_MEASURING.Description())) {
                            cmdListener.processIncomingMessage(incoming.Clone());
                        }
                        break;
                    }
                case ConnectionType.DB: {
                        var connectionId = incoming.internalPartnerConnectionId;
                        var connection = connections.GetOrAdd(connectionId, (id) => {
                            var newConnection = new Connection();
                            newConnection.id = id;
                            newConnection.isDead = true;
                            newConnection.isCreatedInternally = true;
                            newConnection.connectionType = ConnectionType.GW;
                            return newConnection;
                        });
                        // Uncomment the lines below if you want serial number implementation for the database listener
                        // Currently, as of June, 2015 it is not required because all messages from gateway must go to db listener
                        // char thisSerialNo = connection.incrementLastServerSerialNo();
                        // Currently, I am not incrementing the server serial no. for DB messages....I am just sending the old server serial no.
                        char thisSerialNo = connection.lastServerSerialNo;
                        // var serialNoLastTimeStamp = connection.partnerConnectionId.GetOrAdd(incoming.incomingConnection.id, (id) => new ConcurrentDictionary<char, DateTime>());
                        // incoming.updateTimeoutAndMessageComponent();
                        // var endTime = DateTime.Now.AddSeconds(incoming.maxWaitingTimeInSeconds);
                        // serialNoLastTimeStamp[thisSerialNo] = endTime;
                        if (connection.isDead) {
                            sendResponseMessageForError(connection, incoming, BPAPIData.GWListenerResponse.NO_SUCH_GATEWAY_RESPONSE.Description(), "UnknownGateway");
                        } else {
                            BPAPIData.DBServiceResponse response;
                            BPAPIData.Utilities.EnumTryParse(incoming.msgVal.Split(' ')[0], out response);
                            switch (response) {
                                case BPAPIData.DBServiceResponse.RESPONSE_TIME_MEASURING_ACK:
                                    incoming.msgVal = BPAPIData.GWListenerResponse.OK_RESPONSE.Description();
                                    connection.sendOutgoingMessages(incoming);
                                    break;
                                case BPAPIData.DBServiceResponse.IDENTIFY_YOURSELF:
                                    incoming.msgVal = BPAPIData.GWListenerResponse.OK_RESPONSE.Description();
                                    connection.sendOutgoingMessages(incoming);
                                    break;
                                case BPAPIData.DBServiceResponse.EMPTY_RESPONSE:
                                    //incoming.msgVal = Constants.MSG_RECEIVED_RESPONSE;
                                    //connection.sendOutgoingMessages(incoming);
                                    break;
                                case BPAPIData.DBServiceResponse.CHANGE_CONNECTION:
                                    if (incoming.msgVal.Split(Constants.DELIMITER_BETWEEN_MSG).Count > 0) {
                                        var oldConectionId = incoming.msgVal.Substring(response.ToString().Length + 1);
                                        Connection oldConnection;
                                        if (connections.TryRemove(oldConectionId, out oldConnection)) {
                                            if (oldConnection != null) {
                                                if (oldConnection.outgoingMessages.Count > 0) {
                                                    Message outGoingMessage;
                                                    while (oldConnection.outgoingMessages.TryDequeue(out outGoingMessage)) {
                                                        connection.outgoingMessages.Enqueue(outGoingMessage);
                                                    }
                                                    connection.sendOutgoingMessages(null);
                                                }
                                                oldConnection.closeConnection();
                                            }
                                        }
                                    }
                                    break;
                                case BPAPIData.DBServiceResponse.CLOSE_CONNECTION:
                                    connection.closeConnection();
                                    break;
                                case BPAPIData.DBServiceResponse.SEND_MESSAGE:
                                    if (incoming.msgVal.Equals(response.ToString())) break; // Added 19 Nov 2015 by Bjørn to avoid System.ArgumentOutOfRangeException below
                                    incoming.msgVal = incoming.msgVal.Substring(response.ToString().Length + 1);
                                    if (string.IsNullOrEmpty(incoming.msgVal)) break; // Added 19 Nov 2015 by Bjørn 

                                    if (BPAPIData.DBServiceResponse.ACK.ToString().Equals(incoming.msgVal)) {
                                        // Do not add any serial no. in case of "ACK"
                                    } else {
                                        incoming.msgVal = incoming.getSikomPrefixAndSerialNoPrependedWithMessage(thisSerialNo);
                                    }
                                    connection.sendOutgoingMessages(incoming);
                                    break;
                                default:
                                    string partnerIP;
                                    try { partnerIP = connection.client.Client.RemoteEndPoint.ToString().Split(":")[0]; } catch (Exception) { partnerIP = "UnknownGateway"; }
                                    Log("Unknown message format '" + incoming.msgVal + "' received from from database service listener with the following response: " + response.ToString());
                                    if (!connection.isDead) sendResponseMessageForError(connection, incoming, BPAPIData.GWListenerResponse.UNKNOWN_DB_SERVICE_RESPONSE.Description(), partnerIP);
                                    break;
                            }
                        }
                        break;
                    }
                case ConnectionType.CMD: {
                        var connectionId = incoming.internalPartnerConnectionId;
                        var connection = connections.GetOrAdd(connectionId, (id) => {
                            var newConnection = new Connection();
                            newConnection.id = id;
                            newConnection.isDead = true;
                            newConnection.isCreatedInternally = true;
                            newConnection.connectionType = ConnectionType.GW;
                            return newConnection;
                        });

                        char thisSerialNo = connection.incrementLastServerSerialNo();
                        // Another way of writing single line lambda expression, thread safe operation 
                        var serialNoLastTimeStamp = connection.partnerConnectionId.GetOrAdd(incoming.incomingConnection.id, (id) => new ConcurrentDictionary<char, DateTime>());
                        //var serialNoLastTimeStamp = connection.partnerConnectionId.GetOrAdd(incoming.incomingConnection.id, (id) => {
                        //    return new ConcurrentDictionary<char, DateTime>();
                        //});
                        incoming.updateTimeoutAndMessageComponent();
                        var endTime = DateTime.Now.AddSeconds(incoming.maxWaitingTimeInSeconds);
                        //This is thread safe since 'thisSerialNo' is calculated in a thread safe manner using 'lock', which might not be ideal
                        serialNoLastTimeStamp[thisSerialNo] = endTime;
                        if (connection.isDead) {
                            sendResponseMessageForError(connection, incoming, BPAPIData.GWListenerResponse.NO_SUCH_GATEWAY_RESPONSE.Description(), "UnknownGateway");
                        } else {
                            // Send back an immediate OK? response on receiving the message and if the gateway is not dead
                            // if (!String.IsNullOrEmpty(incoming.msgVal)) sendImmediateReceivedResponseMessage(connection, incoming);

                            // Delay task here to reset the timer value...and reply with error to web api (command listener)
                            var responseCheckerTask = sendResponseMessageAfterTimeout(thisSerialNo, connection, incoming);
                        }
                        //Uncomment this line for testing the timeout thing or put a break point here
                        //System.Threading.Thread.Sleep(15000);
                        incoming.msgVal = incoming.getSikomPrefixAndSerialNoPrependedWithMessage(thisSerialNo);
                        connection.sendOutgoingMessages(incoming);
                        break;
                    }
                default:
                    throw new BPAPIData.InvalidEnumException(incoming.incomingConnection.connectionType);
            }
        }

        private async Task sendResponseMessageAfterTimeout(char serialNo, Connection gwConnection, Message incoming) {
            await Task.Run(async () => //Task.Run automatically unwraps nested Task types!
            {
                await Task.Delay(TimeSpan.FromSeconds(incoming.maxWaitingTimeInSeconds));
                ConcurrentDictionary<char, DateTime> serialNoLastTimeStamp;
                if (gwConnection.partnerConnectionId.TryGetValue(incoming.incomingConnection.id, out serialNoLastTimeStamp)) {
                    // Timeout: Hence resetting the timestamp to minimum time to avoid null value, DO NOT remove the serial no. here, it is removed in the command listener
                    DateTime prevTime; if (!serialNoLastTimeStamp.TryGetValue(serialNo, out prevTime)) return;
                    if (!serialNoLastTimeStamp.TryUpdate(serialNo, DateTime.MinValue, prevTime)) return;
                    Message dummy = new Message();
                    dummy.msgVal = BPAPIData.GWListenerResponse.TIMEOUT_RESPONSE.Description();
                    //dummy.msgVal = dummy.getInternalIdPrependedWithMessage(gwConnection.id);
                    dummy.incomingConnection = gwConnection;
                    incoming.incomingConnection.sendOutgoingMessages(dummy);
                }
            });
        }

        private void sendResponseMessageForError(Connection gwConnection, Message incoming, string message, string remoteIP) {
            Message dummy = new Message();
            dummy.msgVal = message;
            //dummy.msgVal = dummy.getInternalIdPrependedWithMessage(gwConnection.id);
                        
            dummy.incomingConnection = gwConnection;
            if (incoming.incomingConnection.connectionType == ConnectionType.DB) {
                dummy.remotePartnerIP = remoteIP;
                dummy.isNewSerialNumberReceivedFromGateway = "F";
                dummy.msgVal = dummy.updateMessageComponentWithIsNewSerialNumberReceivedFromGatewayAndIP();
                dummy.internalPartnerConnectionId = gwConnection.id;
                dummy.msgVal = dummy.getInternalIdPrependedWithMessage();
                incoming.incomingConnection.sendOutgoingMessages(dummy);

                // The error response only goes to the originating connection, to mitigate hacking
                // An attacker may try to open a db connection and try to hack the system by sending illegal commands
                // But the responses will only come back to his terminal only. It would not affect the other db connections and hence will avoid DOS attacks
                // As of 05.08.2015 this works!!
                // If this is not intended, send all the error messages to dbservice listener by simply dbListener.sendMessages() method           
                // It is already implemented, just uncomment the lines below and comment the lines above

                /****************Uncomment these lines if you want the error to be send to all db connections*******************/
                //dummy.isInvalidFormat = true;
                //dummy.isNewSerialNumberReceivedFromGateway = "F";
                //dummy.remotePartnerIP = remoteIP;
                //dummy.internalPartnerConnectionId = gwConnection.id;
                //dbListener.processIncomingMessage(dummy);
                /***************************************************************************************************************/

            } else if (incoming.incomingConnection.connectionType == ConnectionType.CMD) {
                incoming.incomingConnection.sendOutgoingMessages(dummy);
            }
        }

        private void sendImmediateReceivedResponseMessage(Connection gwConnection, Message incoming) {
            Message dummy = new Message();
            dummy.msgVal = BPAPIData.GWListenerResponse.OK_RESPONSE.Description();
            //TODO: Commented since Virtual command listener only expects back OK? as a response maybe..ask Bjørn
            //dummy.msgVal = dummy.getInternalIdPrependedWithMessage(gwConnection.id);
            dummy.incomingConnection = gwConnection;
            incoming.incomingConnection.sendOutgoingMessages(dummy);
        }
    }
}