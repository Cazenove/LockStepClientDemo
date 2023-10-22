using System;
using System.Buffers;
using System.Net;
using System.Net.Sockets;
using System.Net.Sockets.Kcp;
using System.Threading.Tasks;
using Kcp = System.Net.Sockets.Kcp.SimpleSegManager.Kcp;

namespace Script
{
    public class KcpClient : IKcpCallback
    {
        private UdpClient client;
        public Kcp kcp;
        public IPEndPoint EndPoint;

        public KcpClient(int port, IPEndPoint endPoint)
        {
            client = new UdpClient(port);
            kcp = new Kcp(20001, this);
            // kcp.NoDelay(1, 20, 2, 1);
            // kcp.WndSize(64, 64);
            EndPoint = endPoint;
            BeginRecv();
        }

        private async void BeginRecv()
        {
            var res = await client.ReceiveAsync();
            EndPoint = res.RemoteEndPoint;
            kcp.Input(res.Buffer);
            BeginRecv();
        }
        
        public void Output(IMemoryOwner<byte> buffer, int avalidLength)
        {
            var s = buffer.Memory.Span.Slice(0, avalidLength).ToArray();
            client.SendAsync(s, s.Length, EndPoint);
            buffer.Dispose();
        }

        public async void SendAsync(byte[] datagram, int bytes)
        {
            kcp.Send(datagram.AsSpan().Slice(0, bytes));
        }
        
        public async ValueTask<byte[]> ReceiveAsync()
        {
            var (buffer, avalidLength) = kcp.TryRecv();
            while (buffer == null)
            {
                await Task.Delay(10);
                (buffer, avalidLength) = kcp.TryRecv();
            }

            var s = buffer.Memory.Span.Slice(0, avalidLength).ToArray();
            return s;
        }

        public IMemoryOwner<byte> RentBuffer(int length)
        {
            return null;
        }
    }
}