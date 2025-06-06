﻿using Server;
using ServerCore;

public class PacketHandler {
    public static void C_ChatHandler(PacketSession session, IPacket packet) {
        C_Chat chatPacket = packet as C_Chat;
        ClientSession clientSession = session as ClientSession;

        if (clientSession.Room == null) {
            return;
        }

        GameRoom room = clientSession.Room;
        room.Push(
            () => room.Broadcast(clientSession, chatPacket.chat)
            );
    }
    
    public static void testHandler(PacketSession session, IPacket packet) {
    }
}