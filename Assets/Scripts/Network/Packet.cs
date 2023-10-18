using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;

public class Packet
{
    private Header header;

    private string message;

    private int moveFrom;
    private int moveTo;

    private DateTime utcTimeStamp;

    Packet(string message)
    {
        this.message = message;

        utcTimeStamp = DateTime.UtcNow;

        int messageSize = message.Length * sizeof(char);
        int moveInfoSize = sizeof(int);
        int timeStampSize = Marshal.SizeOf(utcTimeStamp);

        header = new Header
        {
            padding = new int[] { messageSize, moveInfoSize, moveInfoSize, timeStampSize }
        };

        utcTimeStamp = DateTime.UtcNow;
        header.packetSize = Marshal.SizeOf(this);
    }

    Packet(int moveFrom, int moveTo)
    {
        this.moveFrom = moveFrom;
        this.moveTo = moveTo;

        utcTimeStamp = DateTime.UtcNow;

        int messageSize = IntPtr.Size; // Used to determinate size of null string
        int moveInfoSize = sizeof(int);
        int timeStampSize = Marshal.SizeOf(utcTimeStamp);

        header = new Header
        {
            padding = new int[] { messageSize, moveInfoSize, moveInfoSize, timeStampSize }
        };

        utcTimeStamp = DateTime.UtcNow;
        header.packetSize = Marshal.SizeOf(this);
    }

    byte[] Serialize()
    {
        byte[] packetSize = BitConverter.GetBytes(header.packetSize);
        byte[] padding = new byte[header.padding.Length * sizeof(int)];
        Buffer.BlockCopy(header.padding, 0, padding, 0, padding.Length);

        byte[] message = Encoding.ASCII.GetBytes(this.message);
        byte[] moveFrom = BitConverter.GetBytes(this.moveFrom);
        byte[] moveTo = BitConverter.GetBytes(this.moveTo);

        if (BitConverter.IsLittleEndian)
        {
            Array.Reverse(packetSize);
            Array.Reverse(message);
            Array.Reverse(moveFrom);
            Array.Reverse(moveTo);
        }

    }

    void Deserialize(byte[] data)
    {

    }
}

public struct Header
{
    public int packetSize;
    public int[] padding;
}

