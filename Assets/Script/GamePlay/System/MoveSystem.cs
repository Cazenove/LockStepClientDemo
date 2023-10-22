using Script.GamePlay.Component;

namespace Script.GamePlay.System
{
    public class MoveSystem : EcsSystem<MoveComponent>
    {
        public override void Process(ref MoveComponent component)
        {
            component.m_LogicPosition += component.m_InputDir * component.m_MoveSpeed;
        }
    }
}