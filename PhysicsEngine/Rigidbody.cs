using System;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AbhinavPhysicsEngine
{
    public enum typeOfShape //An enum represents a group of readonly variables
    {
        Circle = 1,
        Box = 0 //Only 2 possible shapes that can be considered for Rigidbodies
    }
    public sealed class Rigidbody //Prevents other classes from inheriting from it (Sealed Keyword)
    {
        public AbhinavVector position;
        private AbhinavVector linearVelocity;
        private float rotation;
        private float angularVelocity;

        public readonly float density;
        public readonly float mass;
        public readonly float resitution; //This is a factor that determines the bounciness and fluctuates between 0 and 1
        public readonly float surfaceArea;
        public readonly bool isStatic; //Determines if the object is unaffected by physics or not

        //Applicable only to circles
        public readonly float radius;
        //Applicable to rectangles
        public readonly float width;
        public readonly float Length;

        public readonly typeOfShape shapeType;
        public readonly AbhinavVector[] allVertices; //This array will store the 'untransformed' vertices of a rigidbody (Only for boxes and not for circles)
        public readonly AbhinavVector[] allTransformedVertices;
        private bool transformUpdateRequired; //This bool essentially checks if a transformation to vertices must be done due to the change in position or rotation. 
        // So, each vertex will go under the same transformation
        public readonly int[] triangulatedVertices; //This is an array that stores the indices of the triangulatted vertices created from the box body
        public AbhinavVector forceVector;
        private Rigidbody(AbhinavVector position,
        float density, float mass, float resitution, float surfaceArea, bool isStatic, float radius, float width, float Length, typeOfShape shape)
        {
            this.shapeType = shape;
            this.position = position;
            this.forceVector = AbhinavVector.zeroVector;
            this.linearVelocity = AbhinavVector.zeroVector;
            this.rotation = 0f;
            this.angularVelocity = 0f;
            this.density = density;
            this.mass = mass;
            this.resitution = resitution;
            this.surfaceArea = surfaceArea;
            this.isStatic = isStatic;
            this.radius = radius;
            this.width = width;
            this.Length = Length;
            if (this.shapeType is typeOfShape.Box)
            {
                allVertices = createBoxVertices(this.width, this.Length);
                allTransformedVertices = new AbhinavVector[allVertices.Length];
                triangulatedVertices = triangulateBox();
            }
            else
            {
                this.allVertices = null;
                allTransformedVertices = null;
                triangulatedVertices = null;
            }
            this.transformUpdateRequired = true;

        }
        public AbhinavVector LinearVelocity
        {
            get { return this.linearVelocity; } //The keyword get only works with properties, not with methods
            set { this.linearVelocity = value; } // Allows to set the value of the internal velocity to an instance of the rigidbody object
        }
        public void Step(float time)
        {
            AbhinavVector acceleration = this.forceVector / this.mass;
            this.linearVelocity += acceleration * time; //derived from the SUVAT equations

            this.position += linearVelocity * time;
            this.rotation += angularVelocity * time;

            this.forceVector = AbhinavVector.zeroVector;
            maintainObjectinScreen(800, 480);
            this.transformUpdateRequired = true;
        }
        public void maintainObjectinScreen(float screenDimensionX, float screenDimensionY)
        {
            if (this.position.X < 0)
            {
                this.position = new AbhinavVector(screenDimensionX, this.position.Y);
            }
            if (this.position.X > screenDimensionX)
            {
                this.position = new AbhinavVector(0, this.position.Y);
            }
            if (this.position.Y < 0)
            {
                this.position = new AbhinavVector(this.position.X, screenDimensionY);
            }
            if (this.position.Y > screenDimensionY)
            {
                this.position = new AbhinavVector(this.position.X, 0);
            }
            //This is a temporary function that prevents objects from going out of the screen
        }
        public void addForce(AbhinavVector force)
        {
            this.forceVector = force;
        }
        public static AbhinavVector[] createBoxVertices(float width, float height)
        {
            AbhinavVector[] allVertices = new AbhinavVector[4];

            // Relative to (0, 0) origin
            allVertices[0] = new AbhinavVector(-width / 2, height / 2);   // Top Left
            allVertices[1] = new AbhinavVector(width / 2, height / 2);    // Top Right
            allVertices[2] = new AbhinavVector(width / 2, -height / 2);   // Bottom Right
            allVertices[3] = new AbhinavVector(-width / 2, -height / 2);  // Bottom Left

            return allVertices;
        }

        public AbhinavVector[] getTransformedVertices()
        {
            if (transformUpdateRequired == true)
            {
                Transform transform = new Transform(this.position, this.rotation);//Takes a position and rotation angle as parameters
                for (int i = 0; i < allVertices.Length; i++)
                {
                    AbhinavVector vector = allVertices[i];
                    allTransformedVertices[i] = AbhinavVector.vectorTransformation(vector, transform);
                }
            }
            this.transformUpdateRequired = false;//Once the for loop is over, the boolean value doesn't need to be set to true
            return allTransformedVertices;
        }
        public static int[] triangulateBox() //Returns a list of indices to triangulate a box
        {
            //This is done so that the box can be filled
            int[] triangles = new int[6];
            triangles[0] = 0;//Top Left corner of the Box
            triangles[1] = 1;//Top Right corner
            triangles[2] = 2;//Bottom Right corner
            triangles[3] = 0;//Top Left corner
            triangles[4] = 3;//Bottom Left corner
            triangles[5] = 2;//Bottom Right corner
            return triangles;
        }
        public void moveBody(AbhinavVector movementVector)
        {
            position += movementVector;
            this.transformUpdateRequired = true;
        }
        public void moveTo(AbhinavVector newPosition)
        {
            position = newPosition;
            this.transformUpdateRequired = true;
        }
        public void rotateBody(float rotateAmnt)
        {
            this.rotation += rotateAmnt;
            this.transformUpdateRequired = true;
        }
        public static bool createCircle(AbhinavVector position, float radius, float density, float resitution, bool isStatic, out Rigidbody body)
        {
            body = null;
            float surfaceArea = (float)Math.PI * radius * radius;
            float mass;

            if (surfaceArea < World.minimumObjectSize || surfaceArea > World.maximumObjectSize)
            {
                Console.WriteLine("Surface area is not within the required minimum and maximum range");
                return false;
            }
            if (density < World.minDensity || density > World.maxDensity)
            {
                Console.WriteLine("Density is not within the required minimum and maximum range");
                return false;
            }
            resitution = customMath.Clamp(resitution, 0, 1); //Restitution fluctuates between 0 and 1
                                                             //Restitution formula = (final velocity of object b - final velocity of object a)/(Initial velocity of object b - initial velocity of object a)
            mass = density * surfaceArea;//Assumes that the circle is a flipped over cylinder with a height of 1 meter. Therefore, the mass = density * volume
            body = new Rigidbody(position, density, mass, resitution, surfaceArea, isStatic, radius, 0f, 0f, typeOfShape.Circle);
            return true;
        }
        public static bool createBox(AbhinavVector position, float width, float height, float density, float resitution, bool isStatic, out Rigidbody body)
        {
            body = null;
            float surfaceArea = width * height;
            float mass;

            if (surfaceArea < World.minimumObjectSize || surfaceArea > World.maximumObjectSize)
            {
                Console.WriteLine("Surface area is not within the required minimum and maximum range to form a box of the specified properties");
                return false;
            }
            if (density < World.minDensity || density > World.maxDensity)
            {
                Console.WriteLine("Density is not within the required minimum and maximum range to form a box of the specified properties");
                return false;
            }
            resitution = customMath.Clamp(resitution, 0, 1); //Restitution fluctuates between 0 and 1
                                                             //Restitution formula = (final velocity of object b - final velocity of object a)/(Initial velocity of object b - initial velocity of object a)
            mass = density * surfaceArea;//Assumes that the box is a flipped over rectangle with a height of 1 meter. Therefore, the mass = density * volume
            body = new Rigidbody(position, density, mass, resitution, surfaceArea, isStatic, 0f, width, height, typeOfShape.Box);
            return true;
        }
    }
}