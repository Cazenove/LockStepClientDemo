using System;
using TrueSync;
using UnityEngine;

namespace Script.GamePlay
{
    public class Actor : MonoBehaviour
    {
        private TSVector2 m_LogicPosition;//当前逻辑位置
        private TSVector2 m_PreLogicPosition;//预测逻辑位置
        private TSVector2 m_CurPosition;//用于插值计算

        private FP m_MoveSpeed;
        private TSVector2 m_InputDir;

        private TSVector2 m_PreInputDir;

        public GameObject m_LogicGo;//显示逻辑位置用
        private void Start()
        {
            m_MoveSpeed = FP.FromFloat(1 / 30f) * 5;
            m_LogicPosition = new TSVector2();
            m_PreLogicPosition = new TSVector2();
            m_CurPosition = new TSVector2();
        }
        
        //预测
        public void PreUpdate(bool hasInput, TSVector2 input)
        {
            if (hasInput)
            {
                m_PreInputDir = input;
            }
            m_PreLogicPosition += m_PreInputDir * m_MoveSpeed;
        }

        public void LogicUpdate(bool hasInput, TSVector2 input)
        {
            if (hasInput)
            {
                m_InputDir = input;
            }
            m_LogicPosition += m_InputDir * m_MoveSpeed;
            m_LogicGo.transform.position = new Vector3(m_LogicPosition.x.AsFloat(), m_LogicPosition.y.AsFloat(), 0);
        }

        public void Update()
        {
            if (m_CurPosition != m_LogicPosition)
            {
                var dir = (m_LogicPosition - m_CurPosition).normalized;
                var dis = (m_LogicPosition - m_CurPosition).magnitude;
                var frameRate = 1 / Time.deltaTime;
                var speed = FP.FromFloat(30 / frameRate) * m_MoveSpeed;
                if (dis <= speed)
                {
                    m_CurPosition = m_LogicPosition;
                }
                else
                {
                    m_CurPosition += speed * dir;
                }
                
                transform.position = new Vector3(m_CurPosition.x.AsFloat(), m_CurPosition.y.AsFloat(), 0);
            }
        }
    }
}