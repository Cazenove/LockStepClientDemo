using Script.GamePlay.Attribute;
using TrueSync;

namespace Script.GamePlay.Component
{
    public class MoveComponent : Component
    {
        [Snapshot]
        public TSVector2 m_LogicPosition;

        [Snapshot]
        public TSVector2 m_InputDir;

        [Snapshot]
        public FP m_MoveSpeed;
    }
}