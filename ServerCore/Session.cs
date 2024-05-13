using System.Net;
using System.Net.Sockets;
using System.Text;

namespace ServerCore;

public abstract class PacketSession : Session {
    public static readonly int HeaderSize = 2;
    public sealed override int OnRecv(ArraySegment<byte> buffer) {
        int processLen = 0;

        while (true) {
            // 최소한 헤더는 파싱할 수 있는지 확인
            if (buffer.Count < HeaderSize) {
                break;
            }
            
            // 패킷이 완전체로 도착했는지 확인
            ushort dataSize = BitConverter.ToUInt16(buffer.Array, buffer.Offset);
            if (buffer.Count < dataSize) {
                break;
            }
            
            // 여기까지 왔으면 패킷 조립가능
            OnRecvPacket(new ArraySegment<byte>(buffer.Array, buffer.Offset,dataSize));

            processLen += dataSize;
            buffer = new ArraySegment<byte>(buffer.Array, buffer.Offset + dataSize, buffer.Count - dataSize);
        }
        return processLen;
    }

    public abstract void OnRecvPacket(ArraySegment<byte> buffer);

}


public abstract class Session {
    private Socket _socket;
    private int _disconnected = 0;

    private RecvBuffer _recvBuffer = new RecvBuffer(1024);
    
    private object _lock = new object();
    private Queue<ArraySegment<byte>> _sendQueue = new Queue<ArraySegment<byte>>();
    List<ArraySegment<byte>> _pendingList = new List<ArraySegment<byte>>();
    SocketAsyncEventArgs _sendArgs = new SocketAsyncEventArgs();
    SocketAsyncEventArgs _recvArgs = new SocketAsyncEventArgs();

    public abstract void OnConnected(EndPoint endPoint);
    public abstract int OnRecv(ArraySegment<byte> buffer);
    public abstract void OnSend(int numOfBytes);
    public abstract void OnDisconnected(EndPoint endPoint); 
    
    public void Start(Socket socket) {
        
        _socket = socket;
        
        _recvArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnRecvCompleted);
        _sendArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnSendCompleted);

        RegisterRecv();
    }

    public void Send(ArraySegment<byte> sendBuffer) {
        // _socket.Send(sendBuffer);
        lock (_lock) {
            _sendQueue.Enqueue(sendBuffer);
            if (_pendingList.Count == 0) {
                RegisterSend();
            }
        }
    }

    public void Disconnect() {
        if (Interlocked.Exchange(ref _disconnected, 1) == 1) {
            return;
        }

        OnDisconnected(_socket.RemoteEndPoint);
        _socket.Shutdown(SocketShutdown.Both);
        _socket.Close();
    }

    #region network communication

    void RegisterSend() {
        
        while (_sendQueue.Count > 0) {
            ArraySegment<byte> buffer = _sendQueue.Dequeue();
            _pendingList.Add(buffer);
        }

        _sendArgs.BufferList = _pendingList;
        
        bool pending = _socket.SendAsync(_sendArgs);
        if (pending == false) {
            OnSendCompleted(null, _sendArgs);
        }
    }

    void OnSendCompleted(object sender, SocketAsyncEventArgs args) {
        lock (_lock) {
            if (args.BytesTransferred > 0 && args.SocketError == SocketError.Success) {
                try {
                    _sendArgs.BufferList = null;
                    _pendingList.Clear();
                    
                    OnSend(_sendArgs.BytesTransferred);
                    
                    if (_sendQueue.Count > 0) {
                        RegisterSend();
                    }
                    
                }
                catch (Exception e) {
                    Console.WriteLine($"OnSendFailed!!!!!!! {e}");
                }
            }
            else {
                Disconnect();
            }
        }
    }

    void RegisterRecv() {
        _recvBuffer.Clean();
        ArraySegment<byte> segment = _recvBuffer.WriteSegment;
        _recvArgs.SetBuffer(segment.Array, segment.Offset, segment.Count);
        
        bool pending = _socket.ReceiveAsync(_recvArgs);
        if (pending == false) {
            OnRecvCompleted(null, _recvArgs);
        }
    }

    void OnRecvCompleted(object sender, SocketAsyncEventArgs args) {
        if (args.BytesTransferred > 0 && args.SocketError == SocketError.Success) {
            try {
    
                // write 커서 이동
                if (_recvBuffer.OnWrite(args.BytesTransferred) == false) {
                    Disconnect();
                    return;
                }
                
                // 컨텐츠쪽으로 데이터를 넘겨주고 얼마나 처리했는지 받는다.
                int processLen = OnRecv(_recvBuffer.ReadSegment);
                if (processLen < 0 || _recvBuffer.DataSize < processLen) {
                    Disconnect();
                    return;
                }
                
                // move read cursor
                if (_recvBuffer.OnRead(processLen) == false) {
                    Disconnect();
                    return;
                }
                
                RegisterRecv();
            }
            catch (Exception e) {
                Console.WriteLine($"OnRecvFailed!!!!!!! {e}");
            }
        }
        else {
            Disconnect();
        }
    }

    #endregion
}