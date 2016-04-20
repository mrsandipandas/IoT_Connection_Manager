using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Windows.Forms;

namespace TCPIPListener {
    public class DatabaseListener : BaseListener {

        public DatabaseListener(System.Net.IPAddress address, int port)
            : base(address, port, ConnectionType.DB) {
        }

        public ConcurrentQueue<Message> outgoingQueue = new ConcurrentQueue<Message>();

        public override void processIncomingMessage(Message incoming) {
            switch (incoming.incomingConnection.connectionType) {
                case ConnectionType.DB:
                    gwListener.processIncomingMessage(incoming.Clone());
                    break;
                case ConnectionType.CMD:
                case ConnectionType.GW:
                    // Send all cmd messages also for logging
                    if (incoming.incomingConnection.connectionType == ConnectionType.CMD) {
                        // Futuristic: To do something if necessary, currently only used for logging from command to db listener
                    }
                        // Send all messages to all DB-connections (all DB-queues)
                        //Remove the serial no. from the incoming message
                    else if (incoming.incomingConnection.connectionType == ConnectionType.GW) {
                        if (!incoming.isInvalidFormat) incoming.msgVal = incoming.getMessageComponentWithoutSerialNo();
                        incoming.msgVal = incoming.updateMessageComponentWithIsNewSerialNumberReceivedFromGatewayAndIP();
                        incoming.msgVal = incoming.getInternalIdPrependedWithMessage();
                    } else {
                        throw new BPAPIData.InvalidEnumException(incoming.incomingConnection.connectionType);
                    }

                    // Remove dead connections. 
                    // TODO: This code is not ideal (we may risk loosing messages or maybe duplicating them somewhere if
                    // the connections go up and down frequently). As of June, 2015 it works good enough because we have only one concurrent connection.
                    connections.Where(c => c.Value.isDead).ToList().ForEach(c => {
                        if (connections.Count == 1) { // This was the last one, enqueue the messages in the general queue
                            c.Value.outgoingMessages.ForEach(m => outgoingQueue.Enqueue(m));
                        }
                        Connection dummy; connections.TryRemove(c.Key, out dummy);
                    });

                    if (connections.Count == 0) {
                        outgoingQueue.Enqueue(incoming);
                        // TODO: Start a delay task and clear up the queue whenever there is an available database connection
                        // This need to be done since otherwise the messages will not be send immediately from the queue to the DB Service listener.
                        // Because this part of the code only get called when an incoming message arrives
                        // Current solution assumes that there will always be messages from gateway at frequent intervals, so messages will hardly be queued up
                        // But a bug will be there until monitor connections is implemented, since dead connections will remain in the queue and
                        // we try to write the messages in the queue to that connection as a result of which messages are lost
                        // Scenario: Start a db connection. Close it and again start it. Since no monitor connections implemented subsequent messages from gateway will be lost,
                        // which ideally should have been stored in the dblistener outgoing message queue
                    } else {
                        foreach (var connection in connections) {
                            if (!connection.Value.isDead) { // This line should not be necessary, but done as a precautionary measure
                                // Note that queued messages will only go to one connection
                                // i.e. the first connection will receive all messages in the queue
                                Message outGoingMessage;
                                while (outgoingQueue.TryDequeue(out outGoingMessage)) {
                                    connection.Value.sendOutgoingMessages(outGoingMessage);
                                }
                                connection.Value.sendOutgoingMessages(incoming);
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