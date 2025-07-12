using System;
namespace AbhinavPhysicsEngine
{
    public readonly struct AABB
    {
        //AABB stands for Aligned Axis Bounding Boxes
        //AABB basically returns the vectors which can form a box around a polygon
        public readonly AbhinavVector minVector; //This returns the minimum value of the edge on the X and Y axis, which are the edges towards the left and up
        public readonly AbhinavVector maxVector; //This returns the maximum value fo the edge on the X and Y axis, which are the edges towards the right and doward

        //Note: downard direction is the positive Y-axis direction

        public AABB(AbhinavVector min, AbhinavVector max)
        {
            minVector = min;
            maxVector = max;
        }
    }
}