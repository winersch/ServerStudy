namespace Server;

public class SessionManager {
    static SessionManager _session = new SessionManager();
    public static SessionManager Instance { get { return _session; } }
    
    int _sessionId = 0;
    Dictionary<int, ClientSession> _sessions = new Dictionary<int, ClientSession>();
    object _lock = new object();
    
    public ClientSession Generate() {
        lock (_lock) {
            ClientSession session = new ClientSession();
            session.SessionId = ++_sessionId;
            _sessions.Add(session.SessionId, session);
            Console.WriteLine($"Connected : {session.SessionId}");
            return session;
        }
    }
    
    public ClientSession Find(int id) {
        lock (_lock) {
            ClientSession session = null;
            _sessions.TryGetValue(id, out session);
            return session;
        }
    }
    
    public void Remove(ClientSession session) {
        lock (_lock) {
            _sessions.Remove(session.SessionId);
            Console.WriteLine($"Disconnected : {session.SessionId}");
        }
    }
}