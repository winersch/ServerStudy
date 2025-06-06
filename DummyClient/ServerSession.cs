﻿using System.Net;
using System.Text;
using ServerCore;

namespace DummyClient;

abstract class Packet {
    public ushort size;
    public ushort packetId;

    public abstract ArraySegment<byte> Write();
    public abstract void Read(ArraySegment<byte> segment);
}

public class ServerSession : PacketSession {
    public override void OnConnected(EndPoint endPoint) {
        Console.WriteLine($"OnConnected : {endPoint}");
       
    }

    public override void OnRecvPacket(ArraySegment<byte> buffer) {
        PacketManager.Instance.OnRecvPacket(this, buffer);
    }

    public override void OnSend(int numOfBytes) {
        // Console.WriteLine($"Transferred bytes:{numOfBytes}");
    }

    public override void OnDisconnected(EndPoint endPoint) {
        Console.WriteLine($"OnDisconnected :{endPoint}");
    }
}