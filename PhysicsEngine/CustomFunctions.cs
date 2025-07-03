using Microsoft.Xna.Framework;
using System;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;

namespace AbhinavPhysicsEngine
{
    public static class CustomFunctions
    {
        private static BasicEffect _basicEffect;
        public static void DrawLine(SpriteBatch spriteBatch, Texture2D pixel, Vector2 start, Vector2 end, Color color, int thickness = 1)
        {
            //This Custom Function Draws a line
            Vector2 edge = end - start; //Finds the resultant vector
            float angle = (float)Math.Atan2(edge.Y, edge.X); //Derives the angle using the arctan function

            spriteBatch.Draw(pixel,
                new Rectangle((int)start.X, (int)start.Y, (int)edge.Length(), thickness),
                null,
                color,
                angle,
                Vector2.Zero,
                SpriteEffects.None,
                0);
        }

        public static void Initialize(GraphicsDevice graphicsDevice)
        {
            _basicEffect = new BasicEffect(graphicsDevice)
            {
                VertexColorEnabled = true,
                Projection = Matrix.CreateOrthographicOffCenter(
                    0, graphicsDevice.Viewport.Width,
                    graphicsDevice.Viewport.Height, 0,
                    0, 1)
            };
        }

        public static void DrawFilledTriangle(GraphicsDevice graphicsDevice, Vector2 p1, Vector2 p2, Vector2 p3, Color c1)
        {
            VertexPositionColor[] verts = new VertexPositionColor[3]
            {
                new VertexPositionColor(new Vector3(p1, 0), c1),
                new VertexPositionColor(new Vector3(p2, 0), c1),
                new VertexPositionColor(new Vector3(p3, 0), c1)
            };

            foreach (EffectPass pass in _basicEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                graphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, verts, 0, 1);
            }
        }
        public static void DrawBox(GraphicsDevice graphicsDevice, SpriteBatch spriteBatch, Texture2D pixel, AbhinavVector[] vertices, Color outlineColor, Color fillColor, bool isFilled)
        {
            DrawLine(spriteBatch, pixel, typeConverter.ToVector(vertices[0]), typeConverter.ToVector(vertices[1]), outlineColor);
            DrawLine(spriteBatch, pixel, typeConverter.ToVector(vertices[1]), typeConverter.ToVector(vertices[2]), outlineColor);
            DrawLine(spriteBatch, pixel, typeConverter.ToVector(vertices[2]), typeConverter.ToVector(vertices[3]), outlineColor);
            DrawLine(spriteBatch, pixel, typeConverter.ToVector(vertices[3]), typeConverter.ToVector(vertices[0]), outlineColor);

            if (isFilled)
            {
                //Draws 2 triangles
                //Triangle One = Top left, top right, bottom right corners of the box
                DrawFilledTriangle(graphicsDevice, typeConverter.ToVector(vertices[2]), typeConverter.ToVector(vertices[1]), typeConverter.ToVector(vertices[0]), fillColor);
                //Triangle Two = Top left, bottom left, bottom right corners of the box
                //Apparently, there is something weird going on with the way the vertices are inputted in the DrawFilledTriangle function and so all 3 lines are needed
                //If it ain't broke, don't fix it
                DrawFilledTriangle(graphicsDevice, typeConverter.ToVector(vertices[2]), typeConverter.ToVector(vertices[3]), typeConverter.ToVector(vertices[1]), fillColor);
                DrawFilledTriangle(graphicsDevice, typeConverter.ToVector(vertices[0]), typeConverter.ToVector(vertices[1]), typeConverter.ToVector(vertices[2]), fillColor);
                DrawFilledTriangle(graphicsDevice, typeConverter.ToVector(vertices[0]), typeConverter.ToVector(vertices[3]), typeConverter.ToVector(vertices[2]), fillColor);
                //For some weird reason, ALL 4 of these are needed to create a triangle THAT CAN BE FILLED????? I CANNOT Explain this code 

            }
        }

        public static void DrawCircle(GraphicsDevice graphicsDevice, SpriteBatch spriteBatch, Texture2D pixel, int xPos, int yPos, int radius, int points, Color outlineColor, bool isFilled, Color fillColor)
        {
            //1)Initializes a point to the right of the initial xPosition
            //2)Creates a vector from center to the initial xPosition
            //3)Creates a new vector rotated by the specific angle based on the number of points 
            //4)Obtains a new end point for the new rotated vctor
            //5)Joins a line between the initial point to the right of the center and the new point 
            //6)Substitutes the new point as the next base point for the next set of rotation for a for loop

            AbhinavVector basePosition = new AbhinavVector(xPos + radius, yPos);
            AbhinavVector permanentBasePosition = basePosition;
            float angle = MathHelper.ToRadians(360 / points);
            float cosComponent = (float)Math.Cos(angle);
            float sinComponent = (float)Math.Sin(angle);
            //Obtains the last vertex joining the basePosition vector to find the angle required for rotation
            float angleOfLastVertex = MathHelper.ToRadians((360 / points) * (points - 1));
            float lastVertexSinComponent = (float)Math.Sin(angleOfLastVertex);
            float lastVertexCosComponent = (float)Math.Cos(angleOfLastVertex);
            AbhinavVector lastVertex = new AbhinavVector(permanentBasePosition.X * lastVertexCosComponent - permanentBasePosition.Y * lastVertexSinComponent, permanentBasePosition.X * lastVertexSinComponent + permanentBasePosition.Y * lastVertexCosComponent);
            customMath.Clamp(points, 12, 100);

            for (int i = 0; i < points; i++)
            {
                // Step 1: Translate to origin (circle center is xPos, yPos)
                float dx = basePosition.X - xPos;
                float dy = basePosition.Y - yPos;

                // Step 2: Rotate
                float rotatedX = dx * cosComponent - dy * sinComponent;
                float rotatedY = dx * sinComponent + dy * cosComponent;

                AbhinavVector newPosition = new AbhinavVector(rotatedX + xPos, rotatedY + yPos);
                //Draw a line
                DrawLine(spriteBatch, pixel, typeConverter.ToVector(basePosition), typeConverter.ToVector(newPosition), outlineColor);
                if (isFilled == true)
                {
                    DrawFilledTriangle(graphicsDevice, typeConverter.ToVector(new AbhinavVector(xPos, yPos)), typeConverter.ToVector(basePosition), typeConverter.ToVector(newPosition), fillColor);
                }
                basePosition = newPosition;
            }
        }
    }   
}