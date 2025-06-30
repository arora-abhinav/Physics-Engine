using Microsoft.Xna.Framework;

namespace AbhinavPhysicsEngine
{
    public static class typeConverter
    {
        public static Vector2 ToVector(AbhinavVector vector)
        {
            return new Vector2(vector.X, vector.Y);
            //Converts Vector2 to custom Abhinav Vector
        }
    }
}