namespace SakuraFrpService.WebSocketShim
{
    public static class SR
    {
        public static readonly string net_WebSockets_InvalidCloseStatusDescription = Strings.net_WebSockets_InvalidCloseStatusDescription;
        public static readonly string net_WebSockets_InvalidState = Strings.net_WebSockets_InvalidState;
        public static readonly string net_WebSockets_InvalidEmptySubProtocol = Strings.net_WebSockets_InvalidEmptySubProtocol;
        public static readonly string net_WebSockets_InvalidCharInProtocolString = Strings.net_WebSockets_InvalidCharInProtocolString;
        public static readonly string net_WebSockets_ReasonNotNull = Strings.net_WebSockets_ReasonNotNull;
        public static readonly string net_WebSockets_InvalidCloseStatusCode = Strings.net_WebSockets_InvalidCloseStatusCode;
        public static readonly string net_WebSockets_UnsupportedPlatform = Strings.net_WebSockets_UnsupportedPlatform;
        public static readonly string net_uri_NotAbsolute = Strings.net_uri_NotAbsolute;
        public static readonly string net_WebSockets_Scheme = Strings.net_WebSockets_Scheme;
        public static readonly string net_WebSockets_AlreadyStarted = Strings.net_WebSockets_AlreadyStarted;
        public static readonly string net_WebSockets_NotConnected = Strings.net_WebSockets_NotConnected;
        public static readonly string net_webstatus_ConnectFailure = Strings.net_webstatus_ConnectFailure;
        public static readonly string net_WebSockets_AcceptUnsupportedProtocol = Strings.net_WebSockets_AcceptUnsupportedProtocol;
        public static readonly string net_securityprotocolnotsupported = Strings.net_securityprotocolnotsupported;
        public static readonly string net_Websockets_AlreadyOneOutstandingOperation = Strings.net_Websockets_AlreadyOneOutstandingOperation;
        public static readonly string net_WebSockets_ArgumentOutOfRange_TooSmall = Strings.net_WebSockets_ArgumentOutOfRange_TooSmall;
        public static readonly string net_WebSockets_Generic = Strings.net_WebSockets_Generic;
        public static readonly string net_WebSockets_Argument_InvalidMessageType = Strings.net_WebSockets_Argument_InvalidMessageType;
        public static readonly string net_WebSockets_InvalidResponseHeader = Strings.net_WebSockets_InvalidResponseHeader;
        public static readonly string net_WebSockets_InvalidState_ClosedOrAborted = Strings.net_WebSockets_InvalidState_ClosedOrAborted;
        public static readonly string net_WebSockets_NoDuplicateProtocol = Strings.net_WebSockets_NoDuplicateProtocol;
        public static readonly string NotReadableStream = Strings.NotReadableStream;
        public static readonly string NotWriteableStream = Strings.NotWriteableStream;

        public static string Format(string str, params object[] p)
        {
            return string.Format(str, p);
        }

    }
}