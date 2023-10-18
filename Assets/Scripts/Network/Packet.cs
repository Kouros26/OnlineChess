using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using System.Text;
using UnityEditor.VersionControl;
using UnityEngine;

[Serializable]
public class Packet
{
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
    }

    public Packet(int moveFrom, int moveTo)
    {
        this.moveFrom = moveFrom;
        this.moveTo = moveTo;

        utcTimeStamp = DateTime.UtcNow; // Used to evaluate size
    }

    public byte[] Serialize()
    {
        utcTimeStamp = DateTime.UtcNow;

        using (MemoryStream stream = new MemoryStream())
        {
            IFormatter formatter = new BinaryFormatter();
            formatter.Serialize(stream, this);
            return stream.ToArray();
        }
    }

    public void Deserialize(byte[] data)
    {
        using (MemoryStream stream = new MemoryStream(data))
        {
            IFormatter formatter = new BinaryFormatter();
            Packet tempPacket = (Packet)formatter.Deserialize(stream);

            message = tempPacket.message;
            moveFrom = tempPacket.moveFrom;
            moveTo = tempPacket.moveTo;
            utcTimeStamp = tempPacket.utcTimeStamp;
        }

        DateTime now = DateTime.UtcNow;
        latency = (float)(now - utcTimeStamp).TotalMilliseconds;
    }

    public char[] GetMessage()
    {
        return message;
    }

    public int GetFrom()
    {
        return moveFrom;
                packetSize.Length, padding.Length);
        Buffer.BlockCopy(message, 0, final,
    public int GetTo()
        Buffer.BlockCopy(moveFrom, 0, final,
        return moveTo;
    }

    public float GetLatency()
    {
        return latency;
    }
        Buffer.BlockCopy(dateTimeBytes, 0, final,
                packetSize.Length + padding.Length + message.Length + moveFrom.Length + moveTo.Length, dateTimeBytes.Length);

        return final;
    }

    void Deserialize(byte[] data)
    {

    }
}

    public DateTime GetTimeStamp()
    {
        return utcTimeStamp;
    }
}