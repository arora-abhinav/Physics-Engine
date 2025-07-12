//This is where the rigidibodies come to life 
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using PhysicsEngine;

namespace AbhinavPhysicsEngine
{
    public sealed class World
    {
        public static readonly float minimumObjectSize = 10 * 10;
        public static readonly float maximumObjectSize = 800 * 600;

        public static readonly float minDensity = 0.5f; //Unit is kg/m^3
        public static readonly float maxDensity = 210000;
        private float playerInt = 0;
        AbhinavVector gravity;
        float gravityFloat; //I will allow the player to get and set the gravity based on this float
        private List<Rigidbody> allBodies;
        public List<AbhinavVector> contactList; //This stores all the points of contact in collisions

        public World()
        {
            //The reason that this class is not static is because it allows the players to create multiple worlds in one scene if necessary
            gravity = new AbhinavVector(0, gravityFloat); //Its a positive value because 
            this.allBodies = new List<Rigidbody>();
            this.contactList = new List<AbhinavVector>();
        }
        public float setGravityFloat
        {
            set { this.gravityFloat = value; gravity = new AbhinavVector(0, gravityFloat); }
        }
        public void addBodies(Rigidbody body)
        {
            //This function adds all rigidbodies to the list
            allBodies.Add(body);
        }
        public bool removeBody(Rigidbody body)
        {
            return this.allBodies.Remove(body);
            //This returns true if the body can be removed, false if it cannot since it cannot be found
        }
        public bool obtainBody(int index, out Rigidbody body)
        {
            if (index < 0 || index >= allBodies.Count)
            {
                body = null;
                return false;
            }
            body = allBodies[index];
            return true;
        }
        public void timeStep(float time)
        {
            //Time step basiclly handles any function that has to be executed per frame or per time passed, like the update function in game1.cs
            //Handles movement and rotation
            for (int i = 0; i < allBodies.Count; i++)
            {
                allBodies[i].Step(time, gravity);
            }
            contactList.Clear();
            //Collision Handling
            collisionHandling();

        }
        public int getBodyCount()
        {
            return allBodies.Count;
        }
        public void ResolveCollison(Rigidbody bodyOne, Rigidbody bodyTwo, AbhinavVector normalVector, float depth)
        {
            //Realistic Collision Handling Using impulses:

            /*
            Velocity Vector for the first object += (impulse magnitude / mass of first object * normal vector of collison)
            Velocity Vector for the second object -= (impluse magnitude / mass of second object * normal vector of collison)
            Impulse magnitude = (-(1 + restitution constant) * relative velocity between two objects dot product witt normal vector of collision) 
            / (normal of collision dot product with normal of collision * ((1/mass of first object) + (1/mass of the second object)))

            relative velocity is calculated by subtracting the linear velocities of the two objects
            Since both objects may have different restitution constants, the always lesser restitution constant is considered to be the ultimate restitution constant
            */
            float resitutionConstant = Math.Min(bodyOne.resitution, bodyTwo.resitution);
            AbhinavVector relativeVelocity = bodyOne.LinearVelocity - bodyTwo.LinearVelocity;
            float impulseMagnitude = (-1 * (1 + resitutionConstant) * customMath.dotProduct(relativeVelocity, normalVector)) / ((bodyOne.invMass) + (bodyTwo.invMass));
            float velAlongNormal = customMath.dotProduct(relativeVelocity, normalVector);
            if (velAlongNormal > 0) return; // Bodies are separating, skip

            bodyOne.LinearVelocity += normalVector * (impulseMagnitude * bodyOne.invMass);
            bodyTwo.LinearVelocity -= normalVector * (impulseMagnitude * bodyTwo.invMass);
            //The code below is NECESSARY to prevent any object from sinking into a surface for some reason
            if (bodyOne.isStatic == false)
            {
                bodyOne.moveBody(normalVector * depth / 2);
            }
            if (bodyTwo.isStatic == false)
            {
                bodyTwo.moveBody(normalVector * -depth / 2);
            }

            /* This code prevents penetration into the ground but is extremely jittery
            const float percent = 0.2f; // Penetration percentage to correct (20% is common)
            const float slop = 0.001f;   // Small penetration allowed before correction

            float correctionMagnitude = Math.Max(depth - slop, 0f) / (bodyOne.invMass + bodyTwo.invMass) * percent;
            AbhinavVector correction = normalVector * correctionMagnitude;

            bodyOne.moveBody(correction * bodyOne.invMass);
            */


            //The inverse mass is the reciprocal of the mass
        }

        public void collisionHandling()
        {
            //This function applies all the collision resolution logic so that game1.cs is more organised and clean
            for (int i = 0; i < allBodies.Count; i++)
            {
                //#if false
                for (int j = i + 1; j < allBodies.Count; j++)
                {
                    if ((int)allBodies[i].shapeType == 1 && (int)allBodies[j].shapeType == 1) //This means both shapes in collision are circles
                    {
                        AbhinavVector collisionPoint = Collisions.circleCircleCollisionPoint(allBodies[i], allBodies[j], out int contactPointNumber);
                        if (Collisions.circleIsIntersecting(allBodies[i], allBodies[j], out AbhinavVector normalVectorTwo, out float intersectionDistTwo))
                        {
                            ResolveCollison(allBodies[j], allBodies[i], normalVectorTwo, intersectionDistTwo);
                            //allBodies[i].moveBody(normalVectorTwo * -1 * intersectionDistTwo / 2);
                            //allBodies[j].moveBody(normalVectorTwo * intersectionDistTwo / 2);

                            if (contactList.Contains(collisionPoint) == false)
                            {
                                contactList.Add(collisionPoint);
                            }
                        }
                    }
                    if ((int)allBodies[i].shapeType == 0 && (int)allBodies[j].shapeType == 0) //This means both shapes in collisions are boxes
                    {
                        Collisions.polygonPolygonContactPoints(allBodies[i], allBodies[j], out AbhinavVector contactOne, out AbhinavVector contactTwo, out float contactPointCount);
                        if (Collisions.intersectingPolygons(allBodies[i].getTransformedVertices(), allBodies[j].getTransformedVertices(), out float intersectionDistTwo, out AbhinavVector normalVectorTwo))
                        {
                            ResolveCollison(allBodies[j], allBodies[i], normalVectorTwo, intersectionDistTwo);
                            if (customMath.nearlyEqualVector(AbhinavVector.zeroVector, contactOne) == false)
                            {
                                contactList.Add(contactOne);
                            }
                            if (customMath.nearlyEqualVector(AbhinavVector.zeroVector, contactTwo))
                            {
                                contactList.Add(contactTwo);
                            }
                        }
                    }
                    if ((int)allBodies[i].shapeType == 0 && (int)allBodies[j].shapeType == 1) //This checks if the first object is a box and the second is a circle
                    {
                        AbhinavVector collisionPoint = Collisions.circleCircleCollisionPoint(allBodies[j], allBodies[i], out int contactPointNumber);
                        if (Collisions.intersectingCirclesAndPolygons(allBodies[j].position, allBodies[j].radius, allBodies[i].getTransformedVertices(), out AbhinavVector normalVectorTwo, out float intersectionDistTwo))
                        {
                            ResolveCollison(allBodies[j], allBodies[i], normalVectorTwo, intersectionDistTwo);
                            if (allBodies[i].isStatic == false && allBodies[j].isStatic == false && contactList.Contains(collisionPoint) == false)
                            {
                                contactList.Add(Collisions.circlePolygonCollisionPoint(allBodies[j], allBodies[i], out int contactPointNum));
                            }
                            //allBodies[i].moveBody(normalVectorTwo * -1 * intersectionDistTwo / 2);
                            //allBodies[j].moveBody(normalVectorTwo * intersectionDistTwo / 2);
                        }
                    }
                    if ((int)allBodies[i].shapeType == 1 && (int)allBodies[j].shapeType == 0) //This checks if the first object is a circle and the second is a box
                    {
                        AbhinavVector collisionPoint = Collisions.circleCircleCollisionPoint(allBodies[i], allBodies[j], out int contactPointNumber);
                        if (Collisions.intersectingCirclesAndPolygons(allBodies[i].position, allBodies[i].radius, allBodies[j].getTransformedVertices(), out AbhinavVector normalVectorTwo, out float intersectionDistTwo))
                        {
                            ResolveCollison(allBodies[i], allBodies[j], normalVectorTwo, intersectionDistTwo);
                            if (allBodies[i].isStatic == false && allBodies[j].isStatic == false && contactList.Contains(collisionPoint) == false)
                            {
                                contactList.Add(collisionPoint);
                            }
                            //allBodies[i].moveBody(normalVectorTwo * intersectionDistTwo / 2);
                            //allBodies[j].moveBody(normalVectorTwo * -1 * intersectionDistTwo / 2);//I HAVE ABSOLUTELY NO IDEA WHY I HAD TO ADD THE * -1 FOR allBodies[j]
                        }
                    }
                }
            }
        }
    }
}