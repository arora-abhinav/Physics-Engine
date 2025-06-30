//This is where the rigidibodies come to life 
namespace AbhinavPhysicsEngine
{
    public sealed class World
    {
        public static readonly float minimumObjectSize = 10 * 10;
        public static readonly float maximumObjectSize = 800 * 600;

        public static readonly float minDensity = 500; //Unit is kg/m^3
        public static readonly float maxDensity = 210000; 

    }
}