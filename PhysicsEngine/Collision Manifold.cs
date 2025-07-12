using System;
namespace AbhinavPhysicsEngine
{
    public readonly struct CollisionManifold
    {
        public readonly Rigidbody bodyOne;
        public readonly Rigidbody bodyTwo;
        public readonly AbhinavVector collisionNormal;
        public readonly float depth;
        //These refer to the first and second points of contact during a collision. This is necessary to determine the rotation of the objects to create reallistic collisions
        public readonly AbhinavVector PContactOne;
        public readonly AbhinavVector pContactTwo;
        public readonly int PContactNum;//Number of contact points. There don't always have to be two (ex: a circle colliding with anything will have only 1 point of contact)
        public CollisionManifold(Rigidbody bodyOne, Rigidbody bodyTwo, AbhinavVector collisionNormal, float depth, AbhinavVector PContactOne, AbhinavVector PContactTwo, int PContactNum)
        {
            this.bodyOne = bodyOne;
            this.bodyTwo = bodyTwo;
            this.collisionNormal = collisionNormal;
            this.depth = depth;
            this.PContactOne = PContactOne;
            this.pContactTwo = PContactTwo;
            this.PContactNum = PContactNum;
        }
    }
}