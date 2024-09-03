using System;
using System.Net.Sockets;

namespace BSLedShow.Utils
{
    public static class TCPPacketSender
    {
        public static void SendBytes(byte[] bytes) => SendBytes(bytes, bytes.Length);

        public static void SendBytes(byte[] bytes, int size)
        {
            try
            {
                TcpClient _client = new TcpClient(Configuration.PluginConfig.Instance.LEDControllerIpAddress, Configuration.PluginConfig.Instance.LEDControllerPort);

                NetworkStream stream = _client.GetStream();

                stream.Write(bytes, 0, size);

                stream.Close();
                stream.Dispose();
                _client.Close();
                _client.Dispose();
            }
            catch (Exception ex)
            {
                Plugin.Instance.LEDControllerConnectionStatus = Plugin.ConnectionStatus.Failed;
                throw ex;
            }
        }

        public static void AppendByteArray(int firstLedIndex, int secondLedIndex, byte[] RGBColor, ref byte[] byteArray, ref int bitPosition)
        {
            PackBits(ref byteArray, ref bitPosition, (uint)firstLedIndex, 11);
            PackBits(ref byteArray, ref bitPosition, (uint)secondLedIndex, 11);
            PackBits(ref byteArray, ref bitPosition, RGBColor[0], 8);
            PackBits(ref byteArray, ref bitPosition, RGBColor[1], 8);
            PackBits(ref byteArray, ref bitPosition, RGBColor[2], 8);
            if (bitPosition >= 4096)
            {
                SendBytes(byteArray);
                bitPosition = 2;
                byteArray[0] = 0;
            }
        }

        public static void AppendByteArray(int effectIndex, uint[] effectParams, ref byte[] byteArray, ref int bitPosition)
        {
            PackBits(ref byteArray, ref bitPosition, (uint)effectIndex, 32);
            foreach (var param in effectParams)
            {
                PackBits(ref byteArray, ref bitPosition, (uint)param, 32);
            }
            if (bitPosition >= 4096)
            {
                SendBytes(byteArray);
                bitPosition = 2;
                byteArray[0] = 0;
            }
        }

        public static void PackBits(ref byte[] byteArray, ref int bitPosition, uint value, int bitLength)
        {
            for (int i = 0; i < bitLength; i++)
            {

                int byteIndex = bitPosition / 8;
                int bitOffset = 7 - (bitPosition % 8);
                if (bitPosition % 8 == 0)
                {
                    byteArray[byteIndex] = 0;
                }

                byteArray[byteIndex] = (byte)(byteArray[byteIndex] | (((value >> (bitLength - 1 - i)) & 1) << bitOffset));

                bitPosition++;
            }
        }
    }
}
