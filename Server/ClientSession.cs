using System.Net;
using System.Text;
using ServerCore;

namespace Server;

abstract class Packet {
    public ushort size;
    public ushort packetId;

    public abstract ArraySegment<byte> Write();
    public abstract void Read(ArraySegment<byte> segment);
}

class PlayerInfoReq {
    public long playerId;
    public string name;

    public struct Skill {
        public int id;
        public short level;
        public float duration;

        public void Read(ReadOnlySpan<byte> s, ref ushort count) {
            id = BitConverter.ToInt32(s.Slice(count, s.Length - count));
            count += sizeof(int);
            level = BitConverter.ToInt16(s.Slice(count, s.Length - count));
            count += sizeof(short);
            duration = BitConverter.ToSingle(s.Slice(count, s.Length - count));
            count += sizeof(float);
        }

        public bool Write(Span<byte> s, ref ushort count) {
            bool success = true;
            success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.id);
            count += sizeof(int);
            success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.level);
            count += sizeof(short);
            success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.duration);
            count += sizeof(float);
            return success;
        }
    }

    public List<Skill> skills = new List<Skill>();


    public void Read(ArraySegment<byte> segment) {
        ushort count = 0;
        ReadOnlySpan<byte> s = new ReadOnlySpan<byte>(segment.Array, segment.Offset, segment.Count);

        count += sizeof(ushort);
        count += sizeof(ushort);

        playerId = BitConverter.ToInt64(s.Slice(count, s.Length - count));
        count += sizeof(long);
        ushort nameLen = (ushort)BitConverter.ToUInt16(s.Slice(count, s.Length - count));
        count += sizeof(ushort);
        this.name = Encoding.Unicode.GetString(s.Slice(count, nameLen));
        count += nameLen;

        this.skills.Clear();
        ushort skillLen = BitConverter.ToUInt16(s.Slice(count, s.Length - count));
        count += sizeof(ushort);
        for (int i = 0; i < skillLen; i++) {
            Skill skill = new Skill();
            skill.Read(s, ref count);
            skills.Add(skill);
        }
    }

    public ArraySegment<byte> Write() {
        ArraySegment<byte> segment = SendBufferHelper.Open(4096);
        bool success = true;
        ushort count = 0;

        Span<byte> s = new Span<byte>(segment.Array, segment.Offset, segment.Count);

        count += sizeof(ushort);
        success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), (ushort)PacketId.PlayerInfoReq);
        count += sizeof(ushort);
        success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.playerId);
        count += sizeof(long);
        ushort nameLen = (ushort)Encoding.Unicode.GetBytes(this.name, 0, this.name.Length, segment.Array,
            segment.Offset + count + sizeof(ushort));
        success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), nameLen);
        count += sizeof(ushort);
        count += nameLen;
        success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), (ushort)skills.Count);
        count += sizeof(ushort);
        foreach (Skill skill in this.skills) {
            success &= skill.Write(s, ref count);
        }

        success &= BitConverter.TryWriteBytes(s, count);
        if (success == false) {
            return null;
        }

        ArraySegment<byte> sendBuffer = SendBufferHelper.Close(count);
        return sendBuffer;
    }
}

public enum PacketId {
    PlayerInfoReq = 1,
    PlayerInfoOk = 2,
}

class ClientSession : PacketSession {
    public class Knight {
        public int hp;
        public int attack;
        public string name;
        public List<int> skills = new List<int>();
    }

    public override void OnConnected(EndPoint endPoint) {
        Console.WriteLine($"OnConnected : {endPoint}");

        /*Packet packet = new Packet() { size = 100, packetId = 10 };


        ArraySegment<byte> openSegment = SendBufferHelper.Open(4096);
        byte[] buffer = BitConverter.GetBytes(packet.size);
        byte[] buffer1 = BitConverter.GetBytes(packet.packetId);
        Array.Copy(buffer, 0, openSegment.Array, openSegment.Offset, buffer.Length);
        Array.Copy(buffer1, 0, openSegment.Array, openSegment.Offset + buffer.Length, buffer1.Length);
        ArraySegment<byte> sendBuffer = SendBufferHelper.Close(buffer.Length + buffer1.Length);

        Send(sendBuffer);*/
        Thread.Sleep(1000);
        Disconnect();
    }


    public override void OnRecvPacket(ArraySegment<byte> buffer) {
        ushort count = 0;

        ushort size = BitConverter.ToUInt16(buffer.Array, buffer.Offset);
        count += 2;
        ushort id = BitConverter.ToUInt16(buffer.Array, buffer.Offset + count);
        count += 2;

        switch ((PacketId)id) {
            case PacketId.PlayerInfoReq:
                PlayerInfoReq p = new PlayerInfoReq();
                p.Read(buffer);
                Console.WriteLine($"Player InfoReq : {p.playerId} {p.name}");
                foreach (PlayerInfoReq.Skill skill in p.skills) {
                    Console.WriteLine($"skill({skill.id})({skill.level})({skill.duration})");
                }

                break;
        }

        Console.WriteLine($"RecvPacketId : {id} RecvPacketSize : {size}");
    }

    public override void OnSend(int numOfBytes) {
        Console.WriteLine($"Transferred bytes:{numOfBytes}");
    }

    public override void OnDisconnected(EndPoint endPoint) {
        Console.WriteLine($"OnDisconnected :{endPoint}");
    }
}