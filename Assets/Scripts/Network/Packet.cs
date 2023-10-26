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
    public enum Type
    {
        Command,
        Message,
        Move,
        Castle,
        Color,
    }
    
    public Type       type         { get; private set; }
    public byte[]     data         { get; private set; }
    public float      latency      { get; private set; }
    public DateTime   utcTimeStamp { get; private set; }

    public Packet() { }

    public Packet(Type _type, byte[] _data)
    {
        type = _type;
        data = _data;
    }

    public Packet(Type _type, string message)
    {
        type = _type;
        data = ByteConverter.FromString(message);
    }

    public Packet(Type _type, float _data)
    {
        type = _type;
        data = ByteConverter.FromFloat(_data);
    }

    public Packet(Type _type, Vector2 _data)
    {
        type = _type;
        data = ByteConverter.FromVector2(_data);
    }

    public Packet(Type _type, Vector3 _data)
    {
        type = _type;
        data = ByteConverter.FromVector3(_data);
    }

    public Packet(Type _type, Vector4 _data)
    {
        type = _type;
        data = ByteConverter.FromVector4(_data);
    }

    public Packet(Type _type, Color _data)
    {
        type = _type;
        data = ByteConverter.FromColor(_data);
    }

    public string  DataAsString()  { return ByteConverter.ToString (data); }
    public float   DataAsFloat()   { return ByteConverter.ToFloat  (data); }
    public Vector2 DataAsVector2() { return ByteConverter.ToVector2(data); }
    public Vector3 DataAsVector3() { return ByteConverter.ToVector3(data); }
    public Vector4 DataAsVector4() { return ByteConverter.ToVector4(data); }
    public Color   DataAsColor()   { return ByteConverter.ToColor  (data); }

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

    public void Deserialize(byte[] bytes)
    {
        using (MemoryStream stream = new MemoryStream(bytes))
        {
            IFormatter formatter = new BinaryFormatter();
            Packet tempPacket = (Packet)formatter.Deserialize(stream);

            data = tempPacket.data;
            utcTimeStamp = tempPacket.utcTimeStamp;
        }

        DateTime now = DateTime.UtcNow;
        latency = (float)(now - utcTimeStamp).TotalMilliseconds;
    }
}

public static class ByteConverter
{
    public static byte[] FromString(string s)
    {
        return Encoding.UTF8.GetBytes(s);
    }

    public static byte[] FromFloat(float n)
    {
        int size = sizeof(float);
        float[] f = { n };
        byte[]  b = new byte[size];
        Buffer.BlockCopy(f, 0, b, 0, size);
        return b;
    }

    public static byte[] FromVector2(Vector2 v)
    {
        int size = sizeof(float) * 2;
        float[] f = { v.x, v.y };
        byte[]  b = new byte[size];
        Buffer.BlockCopy(f, 0, b, 0, size);
        return b;
    }

    public static byte[] FromVector3(Vector3 v)
    {
        int size = sizeof(float) * 3;
        float[] f = { v.x, v.y, v.z };
        byte[]  b = new byte[size];
        Buffer.BlockCopy(f, 0, b, 0, size);
        return b;
    }

    public static byte[] FromVector4(Vector4 v)
    {
        int size = sizeof(float) * 4;
        float[] f = { v.x, v.y, v.z, v.w };
        byte[]  b = new byte[size];
        Buffer.BlockCopy(f, 0, b, 0, size);
        return b;
    }

    public static byte[] FromColor(Color c)
    {
        int size = sizeof(float) * 4;
        float[] f = { c.r, c.g, c.b, c.a };
        byte[]  b = new byte[size];
        Buffer.BlockCopy(f, 0, b, 0, size);
        return b;
    }

    public static string ToString(byte[] b)
    {
        return Encoding.UTF8.GetString(b);
    }

    public static float ToFloat(byte[] b)
    {
        int size = sizeof(float);
        float[] f = new float[1];
        Buffer.BlockCopy(b, 0, f, 0, size);
        return f[0];
    }

    public static Vector2 ToVector2(byte[] b)
    {
        int size = sizeof(float) * 2;
        float[] f = new float[2];
        Buffer.BlockCopy(b, 0, f, 0, size);
        return new Vector2(f[0], f[1]);
    }

    public static Vector3 ToVector3(byte[] b)
    {
        int size = sizeof(float) * 3;
        float[] f = new float[3];
        Buffer.BlockCopy(b, 0, f, 0, size);
        return new Vector3(f[0], f[1], f[2]);
    }

    public static Vector4 ToVector4(byte[] b)
    {
        int size = sizeof(float) * 4;
        float[] f = new float[4];
        Buffer.BlockCopy(b, 0, f, 0, size);
        return new Vector4(f[0], f[1], f[2], f[3]);
    }

    public static Color ToColor(byte[] b)
    {
        int size = sizeof(float) * 4;
        float[] f = new float[4];
        Buffer.BlockCopy(b, 0, f, 0, size);
        return new Color(f[0], f[1], f[2], f[3]);
    }
}
