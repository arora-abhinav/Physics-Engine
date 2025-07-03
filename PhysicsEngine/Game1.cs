using System;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using AbhinavPhysicsEngine;
using MonoGame;
using System.Collections.Generic;
using System.Text;
using System.Reflection.Metadata.Ecma335;

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
    World world;
    int type;
    AbhinavVector screenDimensions = new AbhinavVector();
    public int playerInt = 0;

    public Game1()
    {
        graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }
    protected override void Initialize()
    {
        base.Initialize();
        world = new World();
        rigidbodiesList = new List<Rigidbody>();
        int maxBodies = 30;
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
                Rigidbody.createBox(new AbhinavVector(xPosition, yPosition), 40, 40, 1, 1f, false, out newBody);
            }
            else
            {
                Rigidbody.createCircle(new AbhinavVector(xPosition, yPosition), 20, 1, 1, false, out newBody);
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
                world.addBodies(newBody);
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
        world.timeStep((float)gameTime.ElapsedGameTime.TotalSeconds);
        float xDirection = 0f;
        float yDirection = 0f;
        float speed = 80f;
        float forceMagnitude = 400000f; //This is equivalent to speed and is a parameter that will be multiplied by the NORMALIZED force vector
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
            /*
            This implementation of the player's motion checks the direction of motion by sheer change in position

            AbhinavVector directionVector = customMath.normalizedVector(new AbhinavVector(xDirection, yDirection));
            //gameTime.ElapsedGameTime.TotalSeconds measures the time between each frame. This allows the progression of the circle to be in a specific unit such as m/s
            AbhinavVector velocityVector = directionVector * speed * (float)gameTime.ElapsedGameTime.TotalSeconds;
            rigidbodiesList[playerInt].moveBody(velocityVector);
            */

            //This implementation of the player's motion adds force for the motion
            AbhinavVector forceDirection = customMath.normalizedVector(new AbhinavVector(xDirection, yDirection));
            //gameTime.ElapsedGameTime.TotalSeconds measures the time between each frame. This allows the progression of the circle to be in a specific unit such as m/s
            AbhinavVector forceVector = forceDirection * forceMagnitude;
            rigidbodiesList[0].addForce(forceVector);
            
        }
        for (int i = 0; i < rigidbodiesList.Count; i++)
        {
            Console.WriteLine(rigidbodiesList[i].mass);
        }
        if (keyboardState.IsKeyDown(Keys.R))
        {
            rigidbodiesList[playerInt].rotateBody(((float)Math.PI / 2f) * (float)gameTime.ElapsedGameTime.TotalSeconds);
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
 