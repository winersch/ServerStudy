using System.Net;
using System.Text;
using Server;
using ServerCore;


class Program {
    private static Listener _listener = new Listener();
    public static GameRoom Room = new GameRoom();

    static void FlushRoom() {
        Room.Push(() => Room.Flush());
        JobTimer.Instance.Push(FlushRoom, 250);
    }

    static void Main(String[] args) {
        string host = Dns.GetHostName();
        IPHostEntry ipHost = Dns.GetHostEntry(host);
        IPAddress ipAddress = ipHost.AddressList[0];
        IPEndPoint endPoint = new IPEndPoint(ipAddress, 7777);


        _listener.Init(endPoint, () => { return SessionManager.Instance.Generate(); });
        Console.WriteLine("Listening...");

        FlushRoom();

        while (true) {
            JobTimer.Instance.Flush();
        }
    }
}

