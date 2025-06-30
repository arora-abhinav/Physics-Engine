using System;
using System.Numerics;
using AbhinavPhysicsEngine;

namespace AbhinavPhysicsEngine
{
    public static class customMath
    {
        public static float Clamp(float value, float min, float max)
        {
            if (min == max)
            {
                return min;
            }
            if (min > max)
            {
                throw new ArgumentOutOfRangeException("Minimum value cannot be greater than the maximum value");
                //This command is specifically made for checking if a value adheres to a specific range, notifying the user of an error
            }
            if (value < min)
            {
                return min;
            }
            if (value > max)
            {
                return max;
            }
            return value;
        }
        public static double vectorLength(AbhinavVector vector)
        {
            return Math.Sqrt(vector.X * vector.X + vector.Y * vector.Y);
        }
        public static double vectorDistance(AbhinavVector vectorOne, AbhinavVector vectorTwo)
        {
            AbhinavVector resultantVector = new AbhinavVector(vectorTwo.X - vectorOne.X, vectorTwo.Y - vectorOne.Y);
            return vectorLength(resultantVector);
        }
        public static AbhinavVector normalizedVector(AbhinavVector vector)
        {
            return new AbhinavVector(vector.X / (float)vectorLength(vector), vector.Y / (float)vectorLength(vector));
        }
        public static float dotProduct(AbhinavVector vectorOne, AbhinavVector vectorTwo)
        {
            return vectorOne.X * vectorTwo.X + vectorOne.Y * vectorTwo.Y;
        }
        public static float crossProduct(AbhinavVector vectorOne, AbhinavVector vectorTwo)
        {
            return vectorOne.X * vectorTwo.X - vectorOne.Y * vectorTwo.Y;
            //This formula assumes that the z component of the 3 dimensional vector is 0
        }
        public static float vectorAngle(AbhinavVector vectorOne, AbhinavVector vectorTwo)
        {
            return (float)((dotProduct(vectorOne, vectorTwo)) / ((vectorLength(vectorOne) * vectorLength(vectorTwo))));
        }
        public static bool SolveParametricIntersection(AbhinavVector vectorOne, AbhinavVector directionalVectorOne, AbhinavVector vectorTwo, AbhinavVector directionalVectorTwo, out float t, out float g)
        {
            // Equation: A + B * t = C + D * g
            // Rearranged: B * t - D * g = C - A

            AbhinavVector rhs = vectorTwo - vectorOne;

            float det = directionalVectorOne.X * (-directionalVectorTwo.Y) - (-directionalVectorTwo.X) * directionalVectorOne.Y; // 2x2 determinant
            if (Math.Abs(det) < 1e-6f)
            {
                // Lines are parallel or coincident
                t = g = 0;
                return false;
            }

            // Cramer's Rule
            t = (rhs.X * (-directionalVectorTwo.Y) - (-directionalVectorTwo.X) * rhs.Y) / det; //t is a parameter for the first parametric equation
            g = (directionalVectorOne.X * rhs.Y - rhs.X * directionalVectorOne.Y) / det; //g is a parameter for the second parametric equation

            return true;
        }

    }
}