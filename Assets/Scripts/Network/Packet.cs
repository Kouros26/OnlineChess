using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using UnityEditor.VersionControl;
using UnityEngine;

public class Packet
{
    private int packetSize;

    private char[] message = new char[60];

    private int moveFrom;
    private int moveTo;

    private float latency;
    private DateTime utcTimeStamp = new DateTime();

    public Packet()
    {

    }

    public Packet(string message)
    {
        this.message = message.ToCharArray();

        utcTimeStamp = DateTime.UtcNow; // Used to evaluate size
        packetSize = message.Length * sizeof(char) + sizeof(int) * 2 + sizeof(float) + sizeof;
    }

    public Packet(int moveFrom, int moveTo)
    {
        this.moveFrom = moveFrom;
        this.moveTo = moveTo;

        utcTimeStamp = DateTime.UtcNow; // Used to evaluate size
        packetSize = Marshal.SizeOf(this);
    }

    public byte[] Serialize()
    {
        utcTimeStamp = DateTime.UtcNow;
        byte[] data = new byte[packetSize];
        GCHandle handle = GCHandle.Alloc(data, GCHandleType.Pinned);
        Marshal.StructureToPtr(this, handle.AddrOfPinnedObject(), false);
        handle.Free();

        return data;
    }

    public void Deserialize(byte[] data)
    {
        GCHandle handle = GCHandle.Alloc(data, GCHandleType.Pinned);
        Packet tempPacket = (Packet)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(Packet));
        handle.Free();

        packetSize = tempPacket.packetSize;
        message = tempPacket.message;
        moveFrom = tempPacket.moveFrom;
        moveTo = tempPacket.moveTo;
        utcTimeStamp = tempPacket.utcTimeStamp;
        
        DateTime now = DateTime.Now;
        latency = (float)(now - utcTimeStamp).TotalMilliseconds;
    }

    public int GetSize()
    {
        return packetSize;
    }

    public char[] GetMessage()
    {
        return message;
    }

    public int GetFrom()
    {
        return moveFrom;
    }

    public int GetTo()
    {
        return moveTo;
    }

    public float GetLatency()
    {
        return latency;
    }

    public DateTime GetTimeStamp()
    {
        return utcTimeStamp;
    }
}