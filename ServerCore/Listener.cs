﻿using System.Net;
using System.Net.Sockets;

namespace ServerCore;

public class Listener {
    private Socket _listenSocket;
    private Func<Session> _sessionFactory;
    
    public void Init(IPEndPoint endPoint, Func<Session> sessionFactory, int register = 10, int backlog = 100) {
        _listenSocket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
        _sessionFactory += sessionFactory;
        // guard educate
        _listenSocket.Bind(endPoint);

        // start
        // backlog : maximum standby
        _listenSocket.Listen(backlog);

        for (int i = 0; i < register; i++) {
            SocketAsyncEventArgs args = new SocketAsyncEventArgs();
            args.Completed += new EventHandler<SocketAsyncEventArgs>(OnAcceptCompleted);
            RegisterAccept(args);
        }
    }

    void RegisterAccept(SocketAsyncEventArgs args) {
        args.AcceptSocket = null;
        bool pending = _listenSocket.AcceptAsync(args);
        if (pending == false) {
            OnAcceptCompleted(null, args);
            Console.WriteLine("Access immediately");
        }
    }

    void OnAcceptCompleted(object sender, SocketAsyncEventArgs args) {
        if (args.SocketError == SocketError.Success) {
            
            Session session = _sessionFactory.Invoke();
            session.Start(args.AcceptSocket);
            session.OnConnected(args.AcceptSocket.RemoteEndPoint);
            
        }
        else {
            Console.WriteLine(args.SocketError.ToString());
        }

        RegisterAccept(args);
    }
}