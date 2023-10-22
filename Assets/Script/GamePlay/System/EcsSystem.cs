using System.Diagnostics;

namespace Script.GamePlay.System
{
    public abstract class EcsSystem<T> where T : Component.Component
    {
        public abstract void Process(ref T component);
    }
}