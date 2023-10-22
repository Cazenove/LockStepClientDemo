using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets.Kcp.Simple;
using System.Threading;
using System.Threading.Tasks;
using ProtoBuf;
using TrueSync;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Random = System.Random;

namespace Script
{
    public class Battle
    {
        public Action<S2CFrameMessage> OnReceive;
        
        private SimpleKcpClient kcpClient;
        
        private IPEndPoint serverEndPoint = new IPEndPoint(IPAddress.Loopback, 8848);

        public void LinkStart(Action<S2CFrameMessage> callback)
        {
            OnReceive = callback;
            Random r = new Random(DateTime.Now.Millisecond);
            kcpClient = new SimpleKcpClient(r.Next(10000, 20000), serverEndPoint);
            kcpClient.kcp.TraceListener = new DefaultTraceListener();
            Task.Run(async () =>
            {
                while (true)
                {
                    kcpClient.kcp.Update(DateTimeOffset.UtcNow);
                    await Task.Delay(10);
                }
            });
        }

        public async void ReceiveUpdate()
        {
            if (kcpClient.kcp.PeekSize() > 0)
            {
                var mes = await kcpClient.ReceiveAsync();
                S2CFrameMessage frameMessage;
                using (MemoryStream memoryStream = new MemoryStream(mes))
                {
                    frameMessage = Serializer.Deserialize<S2CFrameMessage>(memoryStream);
                }

                // Debug.Log($"逻辑帧： {frameMessage.frameId}");
                OnReceive?.Invoke(frameMessage);
                LogicUpdate(frameMessage);
            }
        }

        public async void SendMessage(TSVector2 dir)
        {
            C2SInput message = new C2SInput();
            message.playerInput = new PBInput();
            if (dir == TSVector2.zero)
            {
                message.playerInput.Type = InputType.MoveEnd;
            }
            else
            {
                message.playerInput.Type = InputType.MoveBegin;
            }
            message.playerInput.inputDir = new PBVector2D
            {
                X = dir.x.RawValue,
                Y = dir.y.RawValue,
            };
            Debug.Log($"输入: {dir.x.AsFloat()}  {dir.y.AsFloat()}");

            byte[] buffer = null;
            using (MemoryStream ms = new MemoryStream())
            {
                Serializer.Serialize<C2SInput>(ms, message);
                ms.Position = 0;
                int length = (int)ms.Length;
                buffer = new byte[length];
                ms.Read(buffer, 0, length);
            }
            
            kcpClient.SendAsync(buffer, buffer.Length);
            
            if (!m_Begin)
            {
                m_Begin = true;
                m_StartTime = Time.time;
                // m_LocalFrame = m_CurFrame;
            }
        }

        public ulong m_CurFrame = 0;
        public void LogicUpdate(S2CFrameMessage data)
        {
            if (data.frameId != m_CurFrame + 1)
            {
                Debug.Log("帧数不匹配！");
            }

            m_CurFrame = data.frameId;
        }

        public bool m_Begin = false;
        public ulong m_LocalFrame = 0;
        public float m_LocalTime = 0;
        public float m_FrameLen = 1 / 30f;
        public float m_StartTime;
        public void LocalUpdate()
        {
            if (!m_Begin) return;
            
            while (Math.Floor((Time.time - m_StartTime) * 30) > m_LocalFrame)
            {
                ++m_LocalFrame;
                // m_LocalTime -= m_FrameLen;
            }
        }
    }
}