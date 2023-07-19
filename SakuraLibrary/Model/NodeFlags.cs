using SakuraLibrary.Proto;

namespace SakuraLibrary.Model
{
    public static class NodeFlags
    {
        public const int AcceptNewTunnelFlag = 1 << 2,
            IsChineseFlag = 1 << 3,
            IsWeakFlag = 1 << 4,
            AcceptUdpFlag = 1 << 5,
            IsPrivateFlag = 1 << 6;

        public static bool AcceptNewTunnel(Node node) => (node.Flag & AcceptNewTunnelFlag) != 0;

        public static bool IsChinese(Node node) => (node.Flag & IsChineseFlag) != 0;

        public static bool IsWeak(Node node) => (node.Flag & IsWeakFlag) != 0;

        public static bool AcceptUdp(Node node) => (node.Flag & AcceptUdpFlag) != 0;

        public static bool IsPrivate(Node node) => (node.Flag & IsPrivateFlag) != 0;
    }
}
