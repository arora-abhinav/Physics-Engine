using System;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using AbhinavPhysicsEngine;
using MonoGame;
using System.Collections.Generic;
using System.Text;

namespace PhysicsEngine;

public class Game1 : Game
{
    private GraphicsDeviceManager graphics;
    KeyboardState keyboardState;
    private SpriteBatch spriteBatch;
    Texture2D pixel;
    AbhinavVector testVectorOne = new AbhinavVector(200, 300);
    AbhinavVector testVectorTwo = new AbhinavVector(100, 200);
    private List<Rigidbody> rigidbodiesList;
    typeOfShape shapeType;
    int type;

    public Game1()
    {
        graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }

    protected override void Initialize()
    {
        base.Initialize();
        rigidbodiesList = new List<Rigidbody>();
        int maxBodies = 10;
        int attempts = 0;
        Random randomNumber = new Random();

        while (rigidbodiesList.Count < maxBodies && attempts < 10000)
        {
            int type = randomNumber.Next(0, 2);
            Rigidbody newBody = null;
            int xPosition = randomNumber.Next(100, 700);
            int yPosition = randomNumber.Next(100, 300);

            if (type == 0)
            {
                Rigidbody.createBox(new AbhinavVector(xPosition, yPosition), 100, 40, 1000, 1f, false, out newBody);
            }
            else
            {
                Rigidbody.createCircle(new AbhinavVector(xPosition, yPosition), 30, 1000, 1, false, out newBody);
            }

            bool isColliding = false;
            for (int i = 0; i < rigidbodiesList.Count; i++)
            {
                Rigidbody existing = rigidbodiesList[i];

                if (newBody.shapeType == typeOfShape.Circle && existing.shapeType == typeOfShape.Circle)
                {
                    if (Collisions.circleIsIntersecting(newBody, existing, out _, out _))
                    {
                        isColliding = true;
                        break;
                    }
                }
                else if (newBody.shapeType == typeOfShape.Box && existing.shapeType == typeOfShape.Box)
                {
                    if (Collisions.intersectingPolygons(newBody.getTransformedVertices(), existing.getTransformedVertices(), out _, out _))
                    {
                        isColliding = true;
                        break;
                    }
                }
                else if (newBody.shapeType == typeOfShape.Circle && existing.shapeType == typeOfShape.Box)
                {
                    if (Collisions.intersectingCirclesAndPolygons(newBody.position, newBody.radius, existing.getTransformedVertices(), out _, out _))
                    {
                        isColliding = true;
                        break;
                    }
                }
                else if (newBody.shapeType == typeOfShape.Box && existing.shapeType == typeOfShape.Circle)
                {
                    if (Collisions.intersectingCirclesAndPolygons(existing.position, existing.radius, newBody.getTransformedVertices(), out _, out _))
                    {
                        isColliding = true;
                        break;
                    }
                }
            }

            if (!isColliding)
            {
                rigidbodiesList.Add(newBody);
            }

            attempts++;
        }
    }
    protected override void LoadContent()
    {
        spriteBatch = new SpriteBatch(GraphicsDevice);
        pixel = new Texture2D(GraphicsDevice, 1, 1);
        pixel.SetData(new[] { Color.White });
        CustomFunctions.Initialize(GraphicsDevice);
        // TODO: use this.Content to load your game content here
    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        // TODO: Add your update logic here
        float xDirection = 0f;
        float yDirection = 0f;
        float speed = 80f;
        keyboardState = Keyboard.GetState();
        if (keyboardState.IsKeyDown(Keys.Up))
        {
            yDirection--;//This is because the coordinates of the screen start from (0,0) on the top-left corner and the positive direction is downards
        }
        if (keyboardState.IsKeyDown(Keys.Down))
        {
            yDirection++;//This is because the coordinates of the screen start from (0,0) on the top-left corner and the negative direction is upwards
        }
        if (keyboardState.IsKeyDown(Keys.Left))
        {
            xDirection--;
        }
        if (keyboardState.IsKeyDown(Keys.Right))
        {
            xDirection++;
        }

        if (xDirection != 0 || yDirection != 0)
        {
            AbhinavVector directionVector = customMath.normalizedVector(new AbhinavVector(xDirection, yDirection));
            //gameTime.ElapsedGameTime.TotalSeconds measures the time between each frame. This allows the progression of the circle to be in a specific unit such as m/s
            AbhinavVector velocityVector = directionVector * speed * (float)gameTime.ElapsedGameTime.TotalSeconds;
            rigidbodiesList[0].moveBody(velocityVector);
            
        }
        if (keyboardState.IsKeyDown(Keys.R))
        {
            rigidbodiesList[0].rotateBody(((float)Math.PI / 2f) * (float)gameTime.ElapsedGameTime.TotalSeconds);  
        }
        //Collision checking for ALL objects
        for (int i = 0; i < rigidbodiesList.Count; i++)
        {
            //#if false
            for (int j = i + 1; j < rigidbodiesList.Count; j++)
            {
                if ((int)rigidbodiesList[i].shapeType == 1 & (int)rigidbodiesList[j].shapeType == 1) //This means both shapes in collision are circles
                {
                    if (Collisions.circleIsIntersecting(rigidbodiesList[i], rigidbodiesList[j], out AbhinavVector normalVectorTwo, out float intersectionDistTwo))
                    {
                        rigidbodiesList[i].moveBody(normalVectorTwo * -1 * intersectionDistTwo / 2);
                        rigidbodiesList[j].moveBody(normalVectorTwo * intersectionDistTwo / 2);
                    }
                }
                if ((int)rigidbodiesList[i].shapeType == 0 & (int)rigidbodiesList[j].shapeType == 0) //This means both shapes in collisions are boxes
                {
                    if (Collisions.intersectingPolygons(rigidbodiesList[i].getTransformedVertices(), rigidbodiesList[j].getTransformedVertices(), out float intersectionDistTwo, out AbhinavVector normalVectorTwo))
                    {
                        rigidbodiesList[i].moveBody(normalVectorTwo * -1 * intersectionDistTwo / 2);
                        rigidbodiesList[j].moveBody(normalVectorTwo * intersectionDistTwo / 2);
                    }
                }
                if ((int)rigidbodiesList[i].shapeType == 0 & (int)rigidbodiesList[j].shapeType == 1) //This checks if the first object is a box and the second is a circle
                {
                    if (Collisions.intersectingCirclesAndPolygons(rigidbodiesList[j].position, rigidbodiesList[j].radius, rigidbodiesList[i].getTransformedVertices(), out AbhinavVector normalVectorTwo, out float intersectionDistTwo))
                    {
                        rigidbodiesList[i].moveBody(normalVectorTwo * -1 * intersectionDistTwo / 2);
                        rigidbodiesList[j].moveBody(normalVectorTwo * intersectionDistTwo / 2);
                    }
                }
                if ((int)rigidbodiesList[i].shapeType == 1 & (int)rigidbodiesList[j].shapeType == 0) //This checks if the first object is a circle and the second is a box
                {
                    if (Collisions.intersectingCirclesAndPolygons(rigidbodiesList[i].position, rigidbodiesList[i].radius, rigidbodiesList[j].getTransformedVertices(), out AbhinavVector normalVectorTwo, out float intersectionDistTwo))
                    {
                        rigidbodiesList[i].moveBody(normalVectorTwo * -1 * intersectionDistTwo / 2);
                        rigidbodiesList[j].moveBody(normalVectorTwo * intersectionDistTwo / 2);
                    }
                }
            }
            //#endif
            /*
            The following code only works for circle collisions
            AbhinavVector normalVector;
            float intersectionDist;
            if (Collisions.circleIsIntersecting(rigidbodiesList[0], rigidbodiesList[i], out normalVector, out intersectionDist))
            {
                rigidbodiesList[0].moveBody(normalVector * -1 * intersectionDist / 2);
                rigidbodiesList[i].moveBody(normalVector * intersectionDist / 2);
                //Will eventually change this because it doesnt make sense that I need a specific if statement for a SPECIFIED player object
            }
            for (int j = i + 1; j < rigidbodiesList.Count; j++)
            {
                AbhinavVector normalVectorTwo;
                float intersectionDistTwo;
                if (Collisions.circleIsIntersecting(rigidbodiesList[i], rigidbodiesList[j], out normalVectorTwo, out intersectionDistTwo))
                {
                    rigidbodiesList[i].moveBody(normalVectorTwo * -1 * intersectionDistTwo / 2);
                    rigidbodiesList[j].moveBody(normalVectorTwo * intersectionDistTwo/2);
                }
            }
            */
        }
            base.Update(gameTime);
    }


    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);
        int test = 0;
        // TODO: Add your drawing code here
        spriteBatch.Begin();
        foreach (Rigidbody testBody in rigidbodiesList)
        {
            if (testBody.shapeType == typeOfShape.Box)
            {
                if (test != 2)
                {
                    CustomFunctions.DrawBox(GraphicsDevice, spriteBatch, pixel, testBody.getTransformedVertices(), Color.Black, Color.Yellow, true);
                }
                else
                {
                    CustomFunctions.DrawBox(GraphicsDevice, spriteBatch, pixel, testBody.getTransformedVertices(), Color.Black, Color.Orange, true);
                    //Highlights a specific box with the orange colour to test if collisions work with box of an index with 2
                }
            }
            if (testBody.shapeType == typeOfShape.Circle)
            {
                CustomFunctions.DrawCircle(GraphicsDevice, spriteBatch, pixel, (int)testBody.position.X, (int)testBody.position.Y, (int)testBody.radius, 12, Color.Black, true, Color.Red);
                Debug.WriteLine("Circle Drawn");
            }
            test++;
        }
        //CustomFunctions.DrawLine(spriteBatch, pixel, typeConverter.ToVector(AbhinavVector.zeroVector), new Vector2(5,0), Color.Green, 10);
        //The (0,0) coordinate is the top-left corner of the screen
        spriteBatch.End();
        Console.WriteLine(customMath.dotProduct(testVectorOne, testVectorTwo));
        base.Draw(gameTime);
    }
}
 