// See https://aka.ms/new-console-template for more information

using System.Net;
using System.Net.Sockets;
using System.Text;
using ServerCore;

namespace DummyClient {
    
   
    class Program {
        static void Main(string[] args) {
            // DNS
            string host = Dns.GetHostName();
            IPHostEntry ipHost = Dns.GetHostEntry(host);
            IPAddress ipAddress = ipHost.AddressList[0];
            IPEndPoint endPoint = new IPEndPoint(ipAddress, 7777);

            Connector connector = new Connector();
            connector.Connect(endPoint, () => {
                return SessionManager.Instance.Generate();
            }, 50);
            
            while (true) {
                try {
                    SessionManager.Instance.SendForEach();
                }
                catch (Exception e) {
                    Console.WriteLine(e.ToString());
                    throw;
                }
                Thread.Sleep(250);
            }
        }
    }
}