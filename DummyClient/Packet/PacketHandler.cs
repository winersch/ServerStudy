using DummyClient;
using ServerCore;

public class PacketHandler {
    
    
    public static void S_ChatHandler(PacketSession session, IPacket packet) {
        S_Chat chatPacket = packet as S_Chat;
        ServerSession serverSession = session as ServerSession;

        // Console.WriteLine($"Player : {chatPacket.playerId} => {chatPacket.chat}");
    }
}