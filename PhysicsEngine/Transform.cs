using System;
namespace AbhinavPhysicsEngine
{
    public readonly struct Transform
    {
        //This struct handles transformations and translations of objects (including rotaion)
        public readonly AbhinavVector position;
        public readonly float sinComponent;
        public readonly float cosComponent;
         

        public Transform(AbhinavVector position, float angle)
        {
            this.position = position;
            this.sinComponent = (float)Math.Sin(angle);
            this.cosComponent = (float)Math.Cos(angle);
        }
    }
}