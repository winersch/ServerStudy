using System;
using System.Collections.Generic;
using System.Text;

namespace DummyClient;

public class SessionManager {
    static SessionManager _session = new SessionManager();
    public static SessionManager Instance { get { return _session; } }

    private List<ServerSession> _sessions = new List<ServerSession>();
    object _lock = new object();

    public ServerSession Generate() {
        lock (_lock) {
            ServerSession session = new ServerSession();
            _sessions.Add(session);
            return session;
        }
    }
    
    public void Remove(ServerSession session) {
        lock (_lock) {
            _sessions.Remove(session);
        }
    }
    
    public void SendForEach() {
        lock (_lock) {
            foreach (ServerSession s in _sessions) {
                C_Chat chatPacket = new C_Chat();
                chatPacket.chat = $"Hello Server~";
                ArraySegment<byte> segment = chatPacket.Write();
                s.Send(segment);
            }
        }
    }
    
    
}