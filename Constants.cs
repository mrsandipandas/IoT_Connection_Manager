using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TCPIPListener {
    public static class Constants {

        // The current listening ports for the different connection types 
        public const int CMD_PORT = 14147;
        public const int DB_PORT = 14148;
        public const int GW_PORT = 4146; // It should be 14146 here as well as Virtual Listener and 4146 in MasterListener to switch to old system

        // Default delimeter between the internal connection id and the messages
        public const string DELIMITER_CONN_ID_AND_MSG = " ";
        public const string DELIMITER_BETWEEN_MSG = " ";
        // public const string DELIMETER_MSG_AND_TIMEOUT = "-T-";
        public const string SIKOM_CMD_PREFIX = "Skm";
        
        // Current length of server serial number, currently server serial number follows the pattern A, B, C, D....Z and then recycles
        public const int SERIAL_NUMBER_SERVER_LENGTH = 1;
        // Gateway serial number also follows the pattern A, B, C, D....Z
        public const int SERIAL_NUMBER_GATEWAY_LENGTH = 1;
        
        // Default waiting time for messages 
        public const int DEFAULT_WAITING_TIME_IN_SECONDS = 10;            
    }

    // Different connection types
    public enum ConnectionType {
        None = 0,
        CMD = 1,
        DB = 2,
        GW = 3
    }
}
