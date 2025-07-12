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
using System.IO;

namespace PhysicsEngine;

public class Game1 : Game
{
    private GraphicsDeviceManager graphics;
    KeyboardState currentKeyboardState;
    KeyboardState previousKeyboardState;
    private SpriteBatch spriteBatch;
    Texture2D pixel;
    AbhinavVector testVectorOne = new AbhinavVector(200, 300);
    AbhinavVector testVectorTwo = new AbhinavVector(100, 200);
    private List<Rigidbody> rigidbodiesList;
    typeOfShape shapeType;
    World world;
    int type;
    MouseState currentMouseState;
    MouseState previousMouseState;

    public int playerInt = 0;
    private int loopCount;
    bool generateRandomBodies;
    Rigidbody groundBody;
    Rigidbody slantedStaticBody;
    Rigidbody droppingBody; //I will be drawing a box that will drop on the ground body


    public Game1()
    {
        graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }
    protected override void Initialize()
    {
        base.Initialize();
        generateRandomBodies = false; //Right now, I am setting generate random bodies to be false because I will implement gravity now
        world = new World();
        world.setGravityFloat = 100f;//Setting the gravity float to 9.81f
        rigidbodiesList = new List<Rigidbody>();
        int maxBodies = 4;
        int attempts = 0;
        Random randomNumber = new Random();

        while (rigidbodiesList.Count < maxBodies && attempts < 10000 && generateRandomBodies == true)
        {
            int staticNum = randomNumber.Next(0, 2);
            bool isStatic = false;
            if (staticNum == 1 && loopCount != 0) //This basically ensures that the player character (rigidbodiesList[0]) is NOT set to static at all
            {
                isStatic = true;
            }
            loopCount++;
            int type = randomNumber.Next(0, 2);
            Rigidbody newBody = null;
            int xPosition = randomNumber.Next(100, 700);
            int yPosition = randomNumber.Next(100, 300);

            if (type == 0)
            {
                Rigidbody.createBox(new AbhinavVector(xPosition, yPosition), 40, 40, 1, 1f, isStatic, out newBody);
            }
            else
            {
                Rigidbody.createCircle(new AbhinavVector(xPosition, yPosition), 20, 1, 1, isStatic, out newBody);
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
        Rigidbody.createBox(new AbhinavVector(400, 400), 500, 50, 0.5f, 0.7f, true, out groundBody); //This is the body on the ground that is flat
        Rigidbody.createBox(new AbhinavVector(600, 200), 200, 20, 0.5f, 0.7f, true, out slantedStaticBody); //This is the body on the ground that is flat
        slantedStaticBody.rotateBody(-MathF.PI / 4);
        this.world.addBodies(groundBody);
        this.world.addBodies(slantedStaticBody);
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
        float forceMagnitude = 400000f; //This is equivalent to speed and is a parameter that will be multiplied by the NORMALIZED force vector
        AbhinavVector clickPosition = new AbhinavVector(); //This will record the position of mouse input based on screen coordinates
        world.timeStep((float)gameTime.ElapsedGameTime.TotalSeconds);
        previousKeyboardState = currentKeyboardState;
        currentKeyboardState = Keyboard.GetState();
        previousMouseState = currentMouseState;
        currentMouseState = Mouse.GetState();
        if (currentMouseState.LeftButton == ButtonState.Pressed && previousMouseState.LeftButton == ButtonState.Released)
        {
            clickPosition = new AbhinavVector(currentMouseState.X, currentMouseState.Y);
            Rigidbody.createBox(clickPosition, 40, 40, 0.5f, 1f, false, out droppingBody);
            rigidbodiesList.Add(droppingBody);
            this.world.addBodies(droppingBody);
        }
        if (currentMouseState.RightButton == ButtonState.Pressed && previousMouseState.RightButton == ButtonState.Released)
        {
            clickPosition = new AbhinavVector(currentMouseState.X, currentMouseState.Y);
            Rigidbody.createCircle(clickPosition, 20, 0.5f, 1f, false, out droppingBody);
            rigidbodiesList.Add(droppingBody);
            this.world.addBodies(droppingBody);
        }
        if (currentKeyboardState.IsKeyDown(Keys.A) && previousKeyboardState.IsKeyUp(Keys.A))
        {
            Console.WriteLine(this.world.getBodyCount());
        }
        if (currentKeyboardState.IsKeyDown(Keys.Up))
        {
            yDirection--;//This is because the coordinates of the screen start from (0,0) on the top-left corner and the positive direction is downards
        }
        if (currentKeyboardState.IsKeyDown(Keys.Down))
        {
            yDirection++;//This is because the coordinates of the screen start from (0,0) on the top-left corner and the negative direction is upwards
        }
        if (currentKeyboardState.IsKeyDown(Keys.Left))
        {
            xDirection--;
        }
        if (currentKeyboardState.IsKeyDown(Keys.Right))
        {
            xDirection++;
        }

        if ((xDirection != 0 || yDirection != 0 )&& generateRandomBodies == true)
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
        if (currentKeyboardState.IsKeyDown(Keys.R) && generateRandomBodies == true)
        {
            rigidbodiesList[playerInt].rotateBody(((float)Math.PI / 2f) * (float)gameTime.ElapsedGameTime.TotalSeconds);
        }
        base.Update(gameTime);
    }
    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);
        int count = 0;
        // TODO: Add your drawing code here
        spriteBatch.Begin();
        CustomFunctions.DrawBox(GraphicsDevice, spriteBatch, pixel, groundBody.getTransformedVertices(), Color.Red, Color.Gray, true); //Draws the ground
        CustomFunctions.DrawBox(GraphicsDevice, spriteBatch, pixel, slantedStaticBody.getTransformedVertices(), Color.Red, Color.Gray, true); //Draws the ground
        foreach (AbhinavVector contactPoint in world.contactList)
        {
            spriteBatch.Draw(pixel, typeConverter.ToVector(contactPoint), null, Color.Blue, 0f, Vector2.Zero, new Vector2(4, 4), SpriteEffects.None, 0f);
        }
        foreach (Rigidbody testBody in rigidbodiesList)
        {
            if (testBody.shapeType == typeOfShape.Box)
            {
                if (count != 0) //Every box other than the player character is yellow
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
                if (count != 0)
                {
                    CustomFunctions.DrawCircle(GraphicsDevice, spriteBatch, pixel, (int)testBody.position.X, (int)testBody.position.Y, (int)testBody.radius, 12, Color.Black, true, Color.Red);
                }
                else
                {
                    CustomFunctions.DrawCircle(GraphicsDevice, spriteBatch, pixel, (int)testBody.position.X, (int)testBody.position.Y, (int)testBody.radius, 12, Color.Black, true, Color.Orange);
                }
            }
            count++;
        }
        //CustomFunctions.DrawLine(spriteBatch, pixel, typeConverter.ToVector(AbhinavVector.zeroVector), new Vector2(5,0), Color.Green, 10);
        //The (0,0) coordinate is the top-left corner of the screen
        spriteBatch.End();
        Console.WriteLine(customMath.dotProduct(testVectorOne, testVectorTwo));
        base.Draw(gameTime);
    }
}
 