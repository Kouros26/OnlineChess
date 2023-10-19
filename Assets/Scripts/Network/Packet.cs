using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using System.Text;
using UnityEngine;

[Serializable]
public class Packet
{
    private char[] message = new char[60];

    private float latency;
    private DateTime utcTimeStamp = new DateTime();

    public Packet() { }

    public Packet(string message)
    {
        this.message = message.ToCharArray();
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
            utcTimeStamp = tempPacket.utcTimeStamp;
        }

        DateTime now = DateTime.UtcNow;
        latency = (float)(now - utcTimeStamp).TotalMilliseconds;
    }

    public string GetMessage()
    {
        return new string(message);
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