using System.Net;
using System.Text;
using Server;
using ServerCore;


class Program {
    private static Listener _listener = new Listener();

    static void Main(String[] args) {
        string host = Dns.GetHostName();
        IPHostEntry ipHost = Dns.GetHostEntry(host);
        IPAddress ipAddress = ipHost.AddressList[0];
        IPEndPoint endPoint = new IPEndPoint(ipAddress, 7777);


        _listener.Init(endPoint, () => new ClientSession());
        Console.WriteLine("Listening...");


        while (true) {
            ;
        }
    }
}