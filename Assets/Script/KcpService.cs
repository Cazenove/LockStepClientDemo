// using System;
// using System.Buffers;
// using System.Collections.Generic;
// using System.Net.Sockets;
// using System.Net;
// using System.Net.Sockets.Kcp;
// using System.Threading.Tasks;
//
// namespace MR.Net {
//     public class KcpService {
//         private UdpClient m_Udp;
//         private Dictionary<IPEndPoint, Client> m_Clients = new Dictionary<IPEndPoint, Client>();
//         public event Action<Client> onNewClient;
//         public KcpService(int port) {
//             m_Udp = new UdpClient(port);
//             m_Udp.Client.SendBufferSize = 1;
//             m_Udp.Client.DontFragment = true;
//             m_Udp.DontFragment = true;
//             BeginRecv();
//         }
//
//         public Client GetClient(IPEndPoint endPoint) {
//             if (m_Clients.TryGetValue(endPoint, out var client))
//                 return client;
//             m_Clients[endPoint] = client = new Client(m_Udp, endPoint);
//             onNewClient?.Invoke(client);
//             return client;
//         }
//
//         private async void BeginRecv() {
//             while (m_Udp != null)
//                 try {
//                     UdpReceiveResult udpReceiveResult = await m_Udp.ReceiveAsync();
//                     var client = GetClient(udpReceiveResult.RemoteEndPoint);
//                     client.Input(udpReceiveResult.Buffer);
//                 } catch {
//                 }
//         }
//
//         public void Dispose() {
//             m_Udp.Close();
//             m_Udp.Dispose();
//             m_Udp = null;
//             m_Clients.Clear();
//         }
//
//         public class Client : IKcpCallback {
//
//             private UdpClient m_Udp;
//             private IPEndPoint m_EndPoint;
//             private SimpleSegManager.Kcp m_Kcp;
//
//             public Client(UdpClient udp, IPEndPoint endPoint) {
//                 m_Kcp = new SimpleSegManager.Kcp(2001u, this);
//                 m_Kcp.Interval(20);
//                 //m_Kcp.NoDelay(1, 20, 2, 1);
//                 //m_Kcp.TraceListener = new System.Diagnostics.ConsoleTraceListener();
//                 m_Udp = udp;
//                 m_EndPoint = endPoint;
//                 Task.Run(async () => {
//                     while (true) {
//                         m_Kcp.Update(DateTimeOffset.UtcNow);
//                         await Task.Delay(10);
//                     }
//                 });
//             }
//
//             public void Input(ReadOnlySpan<byte> span) {
//                 m_Kcp.Input(span);
//             }
//
//             public void Output(IMemoryOwner<byte> buffer, int avalidLength) {
//                 Span<byte> span = buffer.Memory.Span;
//                 span = span.Slice(0, avalidLength);
//                 byte[] array = span.ToArray();
//                 m_Udp.SendAsync(array, array.Length, m_EndPoint);
//                 buffer.Dispose();
//             }
//
//             public void Send(byte[] datagram, int bytes) {
//                 m_Kcp.Send(datagram.AsSpan().Slice(0, bytes));
//             }
//
//             public async ValueTask<byte[]> ReceiveAsync() {
//                 var (memoryOwner, length) = m_Kcp.TryRecv();
//                 while (memoryOwner == null) {
//                     await Task.Delay(10);
//                     (memoryOwner, length) = m_Kcp.TryRecv();
//                 }
//                 return GetArray(memoryOwner, length);
//             }
//
//             private byte[] GetArray(IMemoryOwner<byte> memoryOwner, int length) {
//                 Span<byte> span = memoryOwner.Memory.Span;
//                 span = span.Slice(0, length);
//                 return span.ToArray();
//             }
//         }
//     }
// }
