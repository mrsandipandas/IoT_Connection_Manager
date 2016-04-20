using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Windows.Forms;
namespace TCPIPListener {

    public class CommandListener : BaseListener {
        public CommandListener(System.Net.IPAddress address, int port)
            : base(address, port, ConnectionType.CMD) {
        }

        public override void processIncomingMessage(Message incoming) {
            switch (incoming.incomingConnection.connectionType) {
                case ConnectionType.CMD:
                    if (incoming.msgVal.Equals(BPAPIData.GWListenerResponse.GW_RESPONSE_TIME_MEASURING.Description())) {
                        Message dummy = new Message();
                        dummy.msgVal = BPAPIData.GWListenerResponse.OK_RESPONSE.Description();
                        incoming.incomingConnection.sendOutgoingMessages(dummy);
                    }
                    else{
                        gwListener.processIncomingMessage(incoming.Clone());
                        // This part does the logging for command messages in dbservicelistener
                        try {
                            Message dummy = new Message();
                            dummy.incomingConnection = incoming.incomingConnection;
                            var internalConnId = incoming.internalPartnerConnectionId;
                            dummy.msgVal = internalConnId + 
                                           Constants.DELIMITER_CONN_ID_AND_MSG +
                                           "F" +
                                           Constants.DELIMITER_BETWEEN_MSG +
                                           "127.0.0.1" +
                                           Constants.DELIMITER_BETWEEN_MSG +
                                           BPAPIData.GWListenerResponse.CMD_LISTENER_INITIAL_MESSAGE.Description() +
                                           Constants.DELIMITER_BETWEEN_MSG +
                                           incoming.msgVal;
                            dbListener.processIncomingMessage(dummy);
                        } catch (Exception ex) {
                            throw new Exception("Illegal message from command listener (" + incoming.msgVal + ")", ex); // Added ex as innerException by Bjørn 07.11.2015
                        }               
                    }                    
                    break;
                case ConnectionType.DB:
                    throw new BPAPIData.InvalidEnumException(incoming.incomingConnection.connectionType,"Not allowed now");
                case ConnectionType.GW:
                    // Efficient if there are many conections, since we are checking through its corresponding partner connections only if a message has arrived from there
                    // If we loop through the all command listener connections it will be inefficient
                    foreach (var partnerConnection in incoming.incomingConnection.partnerConnectionId) {
                        Connection connection;
                        if (connections.TryGetValue(partnerConnection.Key, out connection)) {
                            ConcurrentDictionary<char, DateTime> serialNoTimeStamp = partnerConnection.Value;
                            char serverSerialNo = incoming.incomingConnection.lastServerSerialNoReceivedFromGateway; // This works because in this current thread the lastServerSerialNo = lastGatewaySerialNo                           
                            DateTime endTime;
                            if (serialNoTimeStamp.TryRemove(serverSerialNo, out endTime)) {
                                if (endTime != null && (DateTime.Now < endTime)) { // This means that there has NOT been timeout
                                    incoming.msgVal = BPAPIData.GWListenerResponse.OK_PREFIX_TO_CMD_LISTENER_RESPONSE.Description() + Constants.DELIMITER_BETWEEN_MSG + incoming.getMessageComponentWithoutSerialNo();
                                    //incoming.msgVal = incoming.getInternalIdPrependedWithMessage();
                                    connection.sendOutgoingMessages(incoming);
                                }
                            }
                        }
                    }
                    break;
                default:
                    throw new BPAPIData.InvalidEnumException(incoming.incomingConnection.connectionType);
            }
        }
    }
}


