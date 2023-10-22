using System;
using System.Globalization;
using Script.GamePlay;
using TrueSync;
using UnityEngine;
using UnityEngine.UI;

namespace Script
{
    public class Init : MonoBehaviour
    {
        [InspectorName("逻辑帧")]
        public Text logicFrameText;
        [InspectorName("本地帧")]
        public Text localFrameText;
        
        public InputField inputField;

        private Battle battle;

        public Actor go;

        private PlayerControls m_PlayerControls;
        
        private void Start()
        {
            DontDestroyOnLoad(this);
            battle = new Battle();
            battle.LinkStart(input =>
            {
                logicFrameText.text = $"{input.frameId.ToString()}  {DateTime.UtcNow}";
                localFrameText.text = $"帧率差{(long)battle.m_LocalFrame - (long)battle.m_CurFrame}";
                // Debug.Log($"收到帧号: {input.frameId}");
                if (input.Messages != null && input.Messages.Count > 0)
                {
                    if (input.Messages[0].inputDir.X == 0 && input.Messages[0].inputDir.Y == 0)
                    {
                        Debug.Log($"收到停止移动");
                    }
                    else
                    {
                        Debug.Log($"收到: {FP.FromRaw(input.Messages[0].inputDir.X).AsFloat()}  {FP.FromRaw(input.Messages[0].inputDir.Y).AsFloat()}");
                    }

                    TSVector2 dir = new TSVector2();

                    foreach (var i in input.Messages)
                    {
                        dir = new TSVector2(FP.FromRaw(i.inputDir.X), FP.FromRaw(i.inputDir.Y));
                    }
                    
                    go.LogicUpdate(true, dir);
                }
                else
                {
                    go.LogicUpdate(false, TSVector2.zero);
                }
            });
            
            SetupInput();
        }

        private void SetupInput()
        {
            m_PlayerControls = new PlayerControls();
            m_PlayerControls.Enable();
            m_PlayerControls.Move.Move.performed += context =>
            {
                var input = context.ReadValue<Vector2>();
                battle.SendMessage(new TSVector2(FP.FromFloat(input.x), FP.FromFloat(input.y)));
            };
        }

        private void Update()
        {
            battle.ReceiveUpdate();
            battle.LocalUpdate();
        }

        public void Send()
        {
            battle.SendMessage(TSVector2.zero);
        }
    }
}