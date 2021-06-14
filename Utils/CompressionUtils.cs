using System.IO;
using System.IO.Compression;
using UnityEngine;

public static class CompressionUtils
{
    public static byte[] CompressString(string str)
    {
        return CompressBytes(System.Text.Encoding.UTF8.GetBytes(str));
    }

    public static byte[] CompressBytes(byte[] data)
    {
        try
        {
            MemoryStream outstream = new MemoryStream();

            // write size of data so that the size can be quickly determined when decompressing
            outstream.Write(System.BitConverter.GetBytes(data.Length), 0, sizeof(int));

            // write content
            using (GZipStream zipstream = new GZipStream(outstream, CompressionMode.Compress))
                zipstream.Write(data, 0, data.Length);

            return outstream.ToArray();
        }
        catch (System.Exception e)
        {
            Debug.LogError("Failed to Compress Bytes.");
            Debug.LogException(e);
            return null;
        }
    }

    public static string DecompressString(byte[] data)
    {
        return System.Text.Encoding.UTF8.GetString(DecompressBytes(data));
    }

    public static byte[] DecompressBytes(byte[] data)
    {
        try
        {
            MemoryStream instream = new MemoryStream(data);
            return DecompressStream(instream);
        }
        catch (System.Exception e)
        {
            Debug.LogError("Failed to Decompress Bytes.");
            Debug.LogException(e);
            return null;
        }
    }

    public static byte[] DecompressStream(Stream instream)
    {
        try
        {
            // read size of output data
            byte[] sizebytes = new byte[sizeof(int)];
            instream.Read(sizebytes, 0, sizebytes.Length);
            byte[] outdata = new byte[System.BitConverter.ToInt32(sizebytes, 0)];

            // read data
            using (GZipStream zipstream = new GZipStream(instream, CompressionMode.Decompress))
                zipstream.Read(outdata, 0, outdata.Length);

            return outdata;
        }
        catch (System.Exception e)
        {
            Debug.LogError("Failed to Decompress Stream.");
            Debug.LogException(e);
            return null;
        }
    }
}
