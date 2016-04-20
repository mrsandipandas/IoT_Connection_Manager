using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TCPIPListener {
    public class Message {
        /// <summary>
        /// Connection on which this message originated
        /// </summary>
        public Connection incomingConnection;
        public string msgVal;
        public string isNewSerialNumberReceivedFromGateway;
        public string internalPartnerConnectionId;
        //Checks if the message has the desired format or not
        public bool isInvalidFormat = false;
        public int maxWaitingTimeInSeconds = Constants.DEFAULT_WAITING_TIME_IN_SECONDS;
        public string remotePartnerIP = "";
        public Message Clone() {
            return new Message { 
                incomingConnection = incomingConnection, 
                msgVal = msgVal, 
                isNewSerialNumberReceivedFromGateway = isNewSerialNumberReceivedFromGateway,
                internalPartnerConnectionId = internalPartnerConnectionId,
                isInvalidFormat = isInvalidFormat,
                maxWaitingTimeInSeconds = maxWaitingTimeInSeconds,
                remotePartnerIP = remotePartnerIP
            };
        }

        /// <summary>
        /// Makes it possible to debug manually without writing the full connection-id's
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public void updateInternalPartnerConnectionId(string connectionIdPrefix) {
            try {
                msgVal = msgVal.Trim();
                switch (incomingConnection.connectionType) {
                    case ConnectionType.CMD:
                    case ConnectionType.DB:
                        internalPartnerConnectionId = getIdComponent();
                        msgVal = msgVal.Substring(internalPartnerConnectionId.Length + 1);
                        // Looks like manual testing, only the int-part of the id was given, that is connection-id was given without _connectionIdPrefix    
                        int connectionCounter; if (int.TryParse(internalPartnerConnectionId, out connectionCounter)) {
                            internalPartnerConnectionId = connectionIdPrefix + ConnectionType.GW.ToString() + "_" + internalPartnerConnectionId;
                        }
                        break;
                    case ConnectionType.GW:
                        internalPartnerConnectionId = incomingConnection.id;
                        break;
                }
            } catch (Exception e) {
                Log("Error in processing '" + msgVal + "' for internal handling: " + e.GetType().ToString());
            }
        }


        private string getIdComponent() {
            string connectionId = "";
            try {
                if (!String.IsNullOrEmpty(msgVal) && (msgVal.Split(Constants.DELIMITER_CONN_ID_AND_MSG).Count > 0)) {
                    connectionId = msgVal.Split(Constants.DELIMITER_CONN_ID_AND_MSG)[0];
                }
            } catch (Exception e) {
                Log("Error in extracting Id part in a  '" + msgVal + "' : " + e.GetType().ToString());
            }
            return connectionId;
        }

        public string getMessageComponentWithoutSerialNo() {
            string message = msgVal;
            try {
                if (!String.IsNullOrEmpty(msgVal) && (msgVal.Length > (Constants.SERIAL_NUMBER_SERVER_LENGTH + Constants.SERIAL_NUMBER_GATEWAY_LENGTH))) {
                    message = msgVal.Substring(Constants.SERIAL_NUMBER_SERVER_LENGTH + Constants.SERIAL_NUMBER_GATEWAY_LENGTH);
                }
            } catch (Exception e) {
                Log("Error in removing the serial no. from a message in '" + msgVal + "' : " + e.GetType().ToString());
            }
            return message;
        }

        public string updateMessageComponentWithIsNewSerialNumberReceivedFromGatewayAndIP() {
            string message = msgVal;
            message = isNewSerialNumberReceivedFromGateway + Constants.DELIMITER_BETWEEN_MSG + remotePartnerIP + Constants.DELIMITER_BETWEEN_MSG + message;
            return message;
        }

        public void updateTimeoutAndMessageComponent() {
            int timeout = Constants.DEFAULT_WAITING_TIME_IN_SECONDS;
            try {
                if (!String.IsNullOrEmpty(msgVal)) {
                    var msgArr = msgVal.Split(Constants.DELIMITER_BETWEEN_MSG);
                    if (msgArr.Count > 0) {
                        if (int.TryParse(msgArr[0], out timeout)) {
                            maxWaitingTimeInSeconds = timeout;
                            msgVal = msgVal.Substring(maxWaitingTimeInSeconds.ToString().Length + 1);
                        }
                    }
                }
            } catch (Exception e) {
                Log("Error in extracting the timeout from a message in '" + msgVal + "' : " + e.GetType().ToString());
            }
        }

        public string getInternalIdPrependedWithMessage() {
            return internalPartnerConnectionId + Constants.DELIMITER_CONN_ID_AND_MSG + msgVal;
        }

        public string getInternalIdPrependedWithMessage(string internalPartnerConnectionId) {
            return internalPartnerConnectionId + Constants.DELIMITER_CONN_ID_AND_MSG + msgVal;
        }

        public string getSikomPrefixAndSerialNoPrependedWithMessage(char serialNo) {
            return Constants.SIKOM_CMD_PREFIX + serialNo + msgVal;
        }

        protected void Log(string text, [System.Runtime.CompilerServices.CallerMemberName] string caller = "") {
            BPAPIData.Utilities.Log(this, caller, BaseListener.logPath, incomingConnection.id + ": " + text);
        }
    }
}
