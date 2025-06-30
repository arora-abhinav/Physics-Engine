using System;
using Microsoft.Xna.Framework;

namespace AbhinavPhysicsEngine
{
    //A struct is a collection of related members or a bunch of variables
    //The variables can be of different data types
    //A struct is listed under one name in a block of memory
    //Similar to classes but there are no methods
    //Basically, a struct is a lightweight data container
    //A struct is PUBLIC by default
    public readonly struct AbhinavVector //readonly so it cannot be edited by mistake
    {
        public readonly float X;
        public readonly float Y;
        public AbhinavVector(float x, float y)
        {
            this.X = x;
            this.Y = y;
        }
        public static readonly AbhinavVector zeroVector = new AbhinavVector(0, 0);
        //A readonly zero vector in case required
        public static AbhinavVector operator +(AbhinavVector vectorOne, AbhinavVector vectorTwo)
        {
            return new AbhinavVector(vectorOne.X + vectorTwo.X, vectorOne.Y + vectorTwo.Y);
            //Addition of two Vectors
            //'operator +' overrides the function of the operator '+' in the vector struct, making it so that it only allows the addition of two vectors
            /*The static method here allows to directly add two vectors
            instead of having a method like addVectors (vectorOne, vectorTwo);*/

            //The static method makes the '+' operator belong specifically to the vector struct
        }
        public static AbhinavVector operator -(AbhinavVector vectorOne, AbhinavVector vectorTwo)
        {
            return new AbhinavVector(vectorOne.X - vectorTwo.X, vectorOne.Y - vectorTwo.Y);
        }
        public static AbhinavVector operator *(AbhinavVector vectorOne, float scalarMultiple)
        {
            return new AbhinavVector(vectorOne.X * scalarMultiple, vectorOne.Y * scalarMultiple);
        }
        public static AbhinavVector operator /(AbhinavVector vectorOne, float scalarMultiple)
        {
            return new AbhinavVector(vectorOne.X / scalarMultiple, vectorOne.Y / scalarMultiple);
        }
        public static AbhinavVector negateVector(AbhinavVector vector)
        {
            return new AbhinavVector(-vector.X, -vector.Y);
        }
        public bool isEqual(AbhinavVector vectorOne, AbhinavVector vectorTwo)
        {
            return vectorOne.X == vectorTwo.X && vectorOne.Y == vectorTwo.Y;
            //Checks if two vectors are equal
        }
        public static AbhinavVector vectorTransformation(AbhinavVector vector, Transform transform)
        {
            //The order of transformation is rotation, then translation
            //This construct transforms a specific vertex
            //Calculates the angular rotation
            float rotationOfX = transform.cosComponent * vector.X - transform.sinComponent * vector.Y;
            float rotationOfY = transform.sinComponent * vector.X + transform.cosComponent * vector.Y;
            //Calculates the translation or the new vector based on the angle of rotation
            float translationOfX = rotationOfX + transform.position.X;
            float translationOfY = rotationOfY + transform.position.Y;

            return new AbhinavVector(translationOfX, translationOfY);
        }
    }
}