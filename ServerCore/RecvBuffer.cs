﻿namespace ServerCore;

public class RecvBuffer {
    
    //[ ][ ][ ][ ][ ][ ][ ][ ][ ][ ]
    private ArraySegment<byte> _buffer;
    private int _readPos;
    private int _writePos;
    

    public RecvBuffer(int bufferSize) {
        _buffer = new ArraySegment<byte>(new byte[bufferSize], 0, bufferSize);
    }
    
    public int DataSize {
        get { return _writePos - _readPos; }
    }
    public int FreeSize {
        get { return _buffer.Count - _writePos; }
    }

    public ArraySegment<byte> ReadSegment {
        get { return new ArraySegment<byte>(_buffer.Array, _buffer.Offset + _readPos, DataSize);}
    }
    public ArraySegment<byte> WriteSegment {
        get { return new ArraySegment<byte>(_buffer.Array, _buffer.Offset + _writePos, FreeSize);}
    }

    public void Clean() {
        int dataSize = DataSize;
        if (dataSize == 0) {
            // 남은 데이터가 없으면 커서 위치만 0으로 초기화
            _readPos = _writePos = 0;
        }else{
            // 남은 데이터가 있으면 시작 위치로 복사
            Array.Copy(_buffer.Array, _buffer.Offset+_readPos, _buffer.Array, _buffer.Offset, dataSize);
            _readPos = 0;
            _writePos = dataSize;
        }
    }

    public bool OnRead(int numOfBytes) {
        if (numOfBytes > DataSize) {
            return false;
        }
        _readPos += numOfBytes;
        return true;
    }

    public bool OnWrite(int numOfBytes) {
        if (numOfBytes > FreeSize) {
            return false;
        }
        _writePos += numOfBytes;
        return true;
    }
    
}