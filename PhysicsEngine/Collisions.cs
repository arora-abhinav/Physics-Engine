using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Numerics;
using System.Reflection.Metadata;
using System.Security.AccessControl;
using AbhinavPhysicsEngine;
using Microsoft.Xna.Framework.Graphics;

public static class Collisions
{
    //Will be using the separating axis theorem(SAT)
    //The Separating Axis Thereom States that a line of axis can be drawn between two objects that are not touching. 
    //This thereom does NOT work for more convex polygons

    /*
    How SAT works:
    1) The normal of each edge of both polygons will be considered
    2) Each vertex of both the objects will be projected PERPENDICULARKY on the normal
    3) The maximum and minimum values of the projected vertices will be found
    4) If the minimum value of the vertex is more than the maximum value of the vertex belonging to the other object, then the objects are not colliding
    5) Otherwise, a normal of another edge of the polygon will be taken for the separating axis theorem test
    */

    public static AbhinavVector circleCollisionPoint;
    public static bool intersectingCirclesAndPolygons(AbhinavVector circlePosition, float circleRadius, AbhinavVector[] polygonVerts, out AbhinavVector moveVector, out float moveDist)
    {
        //The intersection of circles and polygons work using the same separating axis theorem. The projection axis is simply the vector with the least smallest distance between the vertices of the polygon and the center of the circle
        //The parameters of circles are decided by projecting the center of the circle onto the projection axis and then adding and subtracting the radius onto the projected center to get the max and the min values for the parameters
        List<AbhinavVector> edges = new List<AbhinavVector>();
        float minOfPolygon = new float();
        float maxOfPolygon = new float();
        float minOfCircle = new float();
        float maxOfCircle = new float();
        float parameterForCircleCenter = new float();
        //Edges of the first polygon
        for (int i = 0; i < polygonVerts.Length; i++)
        {
            if (i < polygonVerts.Length - 1)
            {
                AbhinavVector edge = polygonVerts[i + 1] - polygonVerts[i];
                edges.Add(edge);
            }
            else if (i == polygonVerts.Length - 1)
            {
                AbhinavVector edge = polygonVerts[0] - polygonVerts[i];
                edges.Add(edge);
            }
        }
        AbhinavVector centerToClosestVertex = new AbhinavVector(float.MaxValue, float.MaxValue);//This vector is between the center of the circle and the closest point to the circle and will be the vector onto which all the points will be projected
        for (int i = 0; i < polygonVerts.Length; i++)
        {
            AbhinavVector toVertex = polygonVerts[i] - circlePosition;
            if (customMath.vectorLength(toVertex) < customMath.vectorLength(centerToClosestVertex))
            {
                centerToClosestVertex = customMath.normalizedVector(toVertex);
            }
        }
        AbhinavVector[] normals = new AbhinavVector[edges.Count + 1];
        for (int k = 0; k < edges.Count; k++)
        {
            //Full formula isn't written since sin(pi/2) = 1 and cosine(pi/2) = 0
            float xPos = -edges[k].Y;
            float yPos = edges[k].X;
            normals[k] = customMath.normalizedVector(new AbhinavVector(xPos, yPos));
        }
        normals[normals.Length - 1] = centerToClosestVertex;
        AbhinavVector movementVector = new AbhinavVector(0, 0);
        AbhinavVector startingPosition = new AbhinavVector(0, 0);
        float[] parametersForPolygon = new float[polygonVerts.Length];
        float[] parametersForCircle = new float[2];
        //The normal line will be represented as a number line wherein instead of vector values, parameters will be used to determine the position of the projected vertex
        float parameter;
        float minOverlap = float.MaxValue;//This calculates the minimum amount of overlap between the two polygons so that the overlap can be used as the distance to be moved
        for (int o = 0; o < normals.Length; o++)
        {
            for (int m = 0; m < polygonVerts.Length; m++)
            {
                parameter = normals[o].X * (polygonVerts[m].X - startingPosition.X) + normals[o].Y * (polygonVerts[m].Y - startingPosition.Y);
                //(normalizedNormalVector.X * normalizedNormalVector.X + normalizedNormalVector.Y * normalizedNormalVector.Y))); Since the length of normalize normal vector = 1, the dot product is also 1 which is this statement
                parametersForPolygon[m] = parameter;
            }
            //For circles, the parameters are calculated such that the center of the circle is initially projected onto the projection axis and then the minimum and maximum are 'plus minus' radius from the center of the circle in the same direction as projectionAxis.
            parameterForCircleCenter = normals[o].X * (circlePosition.X - startingPosition.X) + normals[o].Y * (circlePosition.Y - startingPosition.Y);
            parametersForCircle[0] = parameterForCircleCenter + circleRadius;
            parametersForCircle[1] = parameterForCircleCenter - circleRadius;
            maxOfPolygon = parametersForPolygon.Max();
            minOfPolygon = parametersForPolygon.Min();
            maxOfCircle = parametersForCircle.Max();
            minOfCircle = parametersForCircle.Min();

            if (minOfCircle >= maxOfPolygon || minOfPolygon >= maxOfCircle)
            {
                moveDist = 0;
                moveVector = AbhinavVector.zeroVector;
                return false;
            }
            float overlap = Math.Min(maxOfPolygon, maxOfCircle) - Math.Max(minOfPolygon, minOfCircle);//Calculates the lowest overlap out of all the overlaps
            if (overlap < minOverlap)
            {
                minOverlap = overlap;//If a NEW SMALLEST overlap is obtained then that will be set as the minimum overla 
                movementVector = normals[o];
                // Ensure correct direction
                //This code block is important because it checks the relative positions of both polygons. If the second polygon is towards the right of the first polygon
                //the direction of motion is reversed and the second polygon is pushed outwards from the first polygon instead of pulled inwards
                AbhinavVector centerOfPolygon = customMath.getCentroid(polygonVerts);
                float polygonProjectedCenter = normals[o].X * (centerOfPolygon.X - startingPosition.X) + normals[o].Y * (centerOfPolygon.Y - startingPosition.Y);
                if (parameterForCircleCenter < polygonProjectedCenter)
                {
                    movementVector = AbhinavVector.negateVector(movementVector);
                }
            }
        }
        moveDist = minOverlap;
        moveVector = movementVector;
        return true;
    }
    public static AbhinavVector circleCircleCollisionPoint(Rigidbody circleOne, Rigidbody circleTwo, out int contactPointNum)
    {
        //In a circle to circle collision, there only is 1 point of contact 
        AbhinavVector connectingCircleCenter = circleTwo.position - circleOne.position;//This is a vector pointing from circleOne to circleTwo
        AbhinavVector circleOneToRadius = customMath.normalizedVector(connectingCircleCenter) * circleOne.radius;
        contactPointNum = 1; //In any case with the circle there only is 1 contact point
        circleCollisionPoint = circleOne.position + circleOneToRadius;
        return circleCollisionPoint;
    }
   public static AbhinavVector circlePolygonCollisionPoint(Rigidbody circle, Rigidbody polygon, out int contactPointCount)
    {
        // We're assuming only one contact point
        contactPointCount = 1;

        AbhinavVector[] polygonVertices = polygon.getTransformedVertices();
        float shortestDistance = float.MaxValue;
        AbhinavVector closestContactPoint = new AbhinavVector();

        // Loop through each edge of the polygon
        for (int i = 0; i < polygonVertices.Length; i++)
        {
            AbhinavVector edgeStart = polygonVertices[i];
            AbhinavVector edgeEnd = polygonVertices[(i + 1) % polygonVertices.Length]; // Wraps around to the first vertex

            AbhinavVector edgeVector = edgeEnd - edgeStart;
            AbhinavVector circleToEdgeStart = circle.position - edgeStart;

            // Using parametric math for vectors, the center of the circle has been projected onto the edge
            float projectionScalar = customMath.dotProduct(circleToEdgeStart, edgeVector) / customMath.dotProduct(edgeVector, edgeVector);

            // Clamp to ensure the point lies within the segment
            projectionScalar = customMath.Clamp(projectionScalar, 0f, 1f);
            AbhinavVector projectedPoint = edgeStart + edgeVector * projectionScalar;
            float distance = (float)customMath.vectorDistance(circle.position, projectedPoint);

            // Keep track of the closest point found so far
            if (distance < shortestDistance)
            {
                shortestDistance = distance;
                closestContactPoint = projectedPoint;
            }
        }

        return closestContactPoint;
    }
    public static void polygonPolygonContactPoints(Rigidbody polygonOne, Rigidbody polygonTwo, out AbhinavVector contactOne, out AbhinavVector contactTwo, out float contactPointCount)
    {
        contactOne = AbhinavVector.zeroVector;
        contactTwo = AbhinavVector.zeroVector;
        contactPointCount = 0;
        float minDist = float.MaxValue;
        AbhinavVector[] polygonOneVerts = polygonOne.getTransformedVertices();
        AbhinavVector[] polygonTwoVerts = polygonTwo.getTransformedVertices();
        for (int i = 0; i < polygonOneVerts.Length; i++)
        {
            for (int j = 0; j < polygonTwoVerts.Length; j++)
            {
                customMath.pointToVectorDistance(polygonOneVerts[i], polygonTwoVerts[j], polygonTwoVerts[(j + 1) % polygonTwoVerts.Length], out float distance, out AbhinavVector closestPoint, out float parameter);
                if (customMath.nearlyEqual(minDist, distance) == true)
                {
                    //The reason this check is being performed is to account for the case in which the edges adjacent to the closest 2 edges,
                    //each belonging to each polygon, are collinear. When that happens, there become 2 contact points at exactly the same place
                    //So, one of the contact points are eliminated. That's why there is no assigned value to contactOne
                    if (customMath.nearlyEqualVector(closestPoint, contactOne) == false)
                    {
                        contactTwo = closestPoint;
                        contactPointCount = 2;
                    }
                }
                else if (distance < minDist)
                {
                    contactPointCount = 1;
                    minDist = distance;
                    contactOne = closestPoint;
                }
            }
        }
        for (int i = 0; i < polygonTwoVerts.Length; i++)
        {
            for (int j = 0; j < polygonOneVerts.Length; j++)
            {
                customMath.pointToVectorDistance(polygonTwoVerts[i], polygonOneVerts[j], polygonOneVerts[(j + 1) % polygonOneVerts.Length], out float distance, out AbhinavVector closestPoint, out float parameter);
                if (customMath.nearlyEqual(minDist, distance) == true)
                {
                    //The reason this check is being performed is to account for the case in which the edges adjacent to the closest 2 edges,
                    //each belonging to each polygon, are collinear. When that happens, there become 2 contact points at exactly the same place
                    //So, one of the contact points are eliminated. That's why there is no assigned value to contactOne
                    if (customMath.nearlyEqualVector(closestPoint, contactOne) == false)
                    {
                        contactTwo = closestPoint;
                        contactPointCount = 2;
                    }
                }
                else if (distance < minDist)
                {
                    contactPointCount = 1;
                    minDist = distance;
                    contactOne = closestPoint;
                }
            }
        }
    }


    public static bool intersectingPolygons(AbhinavVector[] polygonOneVerts, AbhinavVector[] polygonTwoVerts, out float depth, out AbhinavVector moveVector)
    {
        List<AbhinavVector> edges = new List<AbhinavVector>();
        //Edges of the first polygon
        for (int i = 0; i < polygonOneVerts.Length; i++)
        {
            if (i < polygonOneVerts.Length - 1)
            {
                AbhinavVector edge = polygonOneVerts[i + 1] - polygonOneVerts[i];
                edges.Add(edge);
            }
            else if (i == polygonOneVerts.Length - 1)
            {
                AbhinavVector edge = polygonOneVerts[0] - polygonOneVerts[i];
                edges.Add(edge);
            }
        }
        //Edges of the second polygon
        for (int j = 0; j < polygonTwoVerts.Length; j++)
        {
            if (j < polygonOneVerts.Length - 1)
            {
                AbhinavVector edge = polygonTwoVerts[j + 1] - polygonTwoVerts[j];
                edges.Add(edge);
            }
            else if (j == polygonTwoVerts.Length - 1)
            {
                AbhinavVector edge = polygonTwoVerts[0] - polygonTwoVerts[j];
                edges.Add(edge);
            }
        }
        AbhinavVector[] normals = new AbhinavVector[edges.Count];
        for (int k = 0; k < normals.Length; k++)
        {
            //Full formula isn't written since sin(pi/2) = 1 and cosine(pi/2) = 0
            float xPos = -edges[k].Y;
            float yPos = edges[k].X;
            normals[k] = new AbhinavVector(xPos, yPos);
        }
        AbhinavVector startingPosition = new AbhinavVector(0, 0);
        float[] parametersForPolygonOne = new float[polygonOneVerts.Length];
        float[] parametersForPolygonTwo = new float[polygonTwoVerts.Length];
        //The normal line will be represented as a number line wherein instead of vector values, parameters will be used to determine the position of the projected vertex
        float parameter;
        float minOverlap = float.MaxValue;//This calculates the minimum amount of overlap between the two polygons so that the overlap can be used as the distance to be moved
        AbhinavVector movementVector = AbhinavVector.zeroVector;//Best axis for moving the second object in the CORRECT direction
        for (int l = 0; l < normals.Length; l++)
        {
            AbhinavVector normalizedNormalVector = customMath.normalizedVector(normals[l]);
            /*
            The following shows the formula to obtain the value of the parameter at which vertex will be projected onto:
            1) A parametric equation in the form (a, b) + p*(c, d) is set where (a,b) is the starting position assumed to be the middle of the screen
            (c,d) represents the normalized normal vector as the direction vector
            2)The vertex is represented as (e,f)
            3) The resultant vector between a random point on the parametric line and the vertex is (e - (a + p*c), f- (b + p*d))
            4) The dot product of this resultant vector must be taken with the directional vector of the normalizedNormalVector
            5) So (c * (e - (a + p*c)), d * (f - (b + p*d))) = 0
            6) Solve for p = (c(e - a) + d(f - b))/(c^2 + d^2)
            7) Each vertex must be taken for each normal
            */
            for (int m = 0; m < polygonOneVerts.Length; m++)
            {
                parameter = normalizedNormalVector.X * (polygonOneVerts[m].X - startingPosition.X) + normalizedNormalVector.Y * (polygonOneVerts[m].Y - startingPosition.Y);
                //(normalizedNormalVector.X * normalizedNormalVector.X + normalizedNormalVector.Y * normalizedNormalVector.Y))); Since the length of normalize normal vector = 1, the dot product is also 1 which is this statement
                parametersForPolygonOne[m] = parameter;
            }
            for (int n = 0; n < polygonTwoVerts.Length; n++)
            {
                parameter = normalizedNormalVector.X * (polygonTwoVerts[n].X - startingPosition.X) + normalizedNormalVector.Y * (polygonTwoVerts[n].Y - startingPosition.Y);
                //(normalizedNormalVector.X * normalizedNormalVector.X + normalizedNormalVector.Y * normalizedNormalVector.Y))); Since the length of normalize normal vector = 1, the dot product is also 1 which is this statement
                parametersForPolygonTwo[n] = parameter;
            }

            float maxOfPolygonOne = parametersForPolygonOne.Max();
            float minOfPolygonOne = parametersForPolygonOne.Min();
            float maxOfPolygonTwo = parametersForPolygonTwo.Max();
            float minOfPolygoneTwo = parametersForPolygonTwo.Min();
            if (minOfPolygoneTwo >= maxOfPolygonOne || minOfPolygonOne >= maxOfPolygonTwo)
            {
                depth = 0;
                moveVector = AbhinavVector.zeroVector;
                return false;
            }
            else
            {
                float overlap = Math.Min(maxOfPolygonOne, maxOfPolygonTwo) - Math.Max(minOfPolygonOne, minOfPolygoneTwo);//Calculates the lowest overlap out of all the overlaps
                if (overlap < minOverlap)
                {
                    minOverlap = overlap;//If a NEW SMALLEST overlap is obtained then that will be set as the minimum overla 
                    movementVector = normalizedNormalVector;//The direction of movement is now the new normalizedNormalVector

                    // Ensure correct direction
                    //This code block is important because it checks the relative positions of both polygons. If the second polygon is towards the right of the first polygon
                    //the direction of motion is reversed and the second polygon is pushed outwards from the first polygon instead of pulled inwards
                    float centerA = (minOfPolygonOne + maxOfPolygonOne) / 2f;
                    float centerB = (minOfPolygoneTwo + maxOfPolygonTwo) / 2f;
                    if (centerB < centerA)
                    {
                        movementVector = AbhinavVector.negateVector(movementVector);
                    }
                }

            }
        }
        depth = minOverlap;
        moveVector = movementVector;
        return true;
    }
    /*The following are the steps for a circle collision:
    1)Compute a vector between the center points of circles
    2)Obtain the min and max values of the circle by subtracting and adding a vector with the same length of the radius respectively
    3)Repeat step 2 for other circles as well.
    4)If Max of Circle 1 > Min of Circle 2, the circles are colliding
    */
    public static bool circleIsIntersecting(Rigidbody circleOne, Rigidbody circleTwo, out AbhinavVector normalVector, out float distOfIntersection)
    {
        AbhinavVector centerToCenterVector = circleTwo.position - circleOne.position;
        float centerToCenterDist = (float)customMath.vectorLength(centerToCenterVector);
        normalVector = customMath.normalizedVector(centerToCenterVector);
        distOfIntersection = (circleOne.radius + circleTwo.radius) - centerToCenterDist;
        if (centerToCenterDist < (circleOne.radius + circleTwo.radius))
        {
            return true;
        }
        return false;
    }
}