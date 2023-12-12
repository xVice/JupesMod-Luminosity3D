using BulletSharp;
using BulletSharp.Math;
using Luminosity3D.EntityComponentSystem;
using Luminosity3D.Utils;
using MyGame;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luminosity3D.Rendering
{
    public class Physics
    {
        private static CollisionConfiguration collisionConfig = new DefaultCollisionConfiguration();
        private static CollisionDispatcher collisiondispatcher = new CollisionDispatcher(collisionConfig);
        private static DbvtBroadphase broadphase = new DbvtBroadphase();

        // External uses
        public static DiscreteDynamicsWorld World { get; } = new DiscreteDynamicsWorld(collisiondispatcher, broadphase, null, collisionConfig);
        public static AlignedCollisionObjectArray ObjectsArray { get => World.CollisionObjectArray; }


        private static float _gravity = -9.807f;
        public static float Gravity
        {
            get => _gravity;
            set
            {
                _gravity = -value;
                World.Gravity = new BulletSharp.Math.Vector3(0.0f, _gravity, 0.0f);
            }
        }

        public static void Step()
        {
            World.StepSimulation(Time.deltaTime);
        }

        public static void MakePlane()
        {
            CollisionShape groundShape = new StaticPlaneShape(new BulletSharp.Math.Vector3(0, 1, 0), -50);
            RigidBodyConstructionInfo rbInfo = new RigidBodyConstructionInfo(0, new DefaultMotionState(), groundShape);
            RigidBody groundBody = new RigidBody(rbInfo);
            World.AddRigidBody(groundBody);
        }

        public static RigidBody CreateStaticRigidBody(Model model, BulletSharp.Math.Matrix transform, string name)
        {
            // Create the ConvexHullShape from the model's points
            ConvexHullShape shape = new ConvexHullShape(model.assimpModel.PointsForCollision.ToArray());

            // Apply the scale transformation to the shape
            shape.LocalScaling = Matrix.Transpose(transform).ScaleVector;

            DefaultMotionState myMotionState = new DefaultMotionState(Matrix.Transpose(transform));

            // Set the mass to zero to create a static rigid body
            RigidBodyConstructionInfo rbInfo = new RigidBodyConstructionInfo(0.0f, myMotionState, shape);

            RigidBody body = new RigidBody(rbInfo);

            body.UserObject = name;

            World.AddRigidBody(body);

            return body;
        }

        public static CollisionObject CreateStaticCollider(Model model, BulletSharp.Math.Matrix transform, GameObject name)
        {
            // Create a collision shape from the model (adjust as needed)
            CollisionShape shape = new ConvexHullShape(model.assimpModel.PointsForCollision.ToArray());
            shape.LocalScaling = Matrix.Transpose(transform).ScaleVector;

            // Create a DefaultMotionState with the provided transform
            DefaultMotionState motionState = new DefaultMotionState(Matrix.Transpose(transform));

            // Create a RigidBodyConstructionInfo with zero mass (for a static object)
            RigidBodyConstructionInfo rbInfo = new RigidBodyConstructionInfo(0f, motionState, shape);

            // Create a RigidBody
            RigidBody rigidBody = new RigidBody(rbInfo);

            // Set the collision flags to indicate it's a static object
            rigidBody.CollisionFlags |= CollisionFlags.StaticObject;

            // Set the user object (if needed)
            rigidBody.UserObject = name;

            // Create a CollisionObject using the RigidBody
            CollisionObject collider = rigidBody;
            World.AddCollisionObject(collider);
            return collider;
        }





        public static RigidBody CreateRigidBody(Model model, float mass, BulletSharp.Math.Matrix transform, GameObject name)
        {
            BulletSharp.Math.Vector3 localInertia = BulletSharp.Math.Vector3.Zero;
            var shape = new ConvexHullShape(model.assimpModel.PointsForCollision.ToArray());
            shape.LocalScaling = Matrix.Transpose(transform).ScaleVector;

            if (mass > 0.0)
            {
                localInertia = shape.CalculateLocalInertia(mass);
            }

            DefaultMotionState myMotionState = new DefaultMotionState(Matrix.Transpose(transform));

            RigidBodyConstructionInfo rbInfo = new RigidBodyConstructionInfo(mass, myMotionState, shape, localInertia);

            RigidBody body = new RigidBody(rbInfo);

            body.UserObject = name;

            World.AddRigidBody(body);
            rbInfo.Dispose();
            return body;
        }

        public static bool Raycast(System.Numerics.Vector3 start, System.Numerics.Vector3 direction, out System.Numerics.Vector3 hitPoint, out System.Numerics.Vector3 hitNormal, out CollisionObject hitObject)
        {
            var bsStart = LMath.ToVecBs(start);
            var bsDirection = LMath.ToVecBs(direction);

            var bspd = bsStart + bsDirection;

            ClosestRayResultCallback rayCallback = new ClosestRayResultCallback(ref bsStart, ref bspd);

            World.RayTest(bsStart, bspd, rayCallback);

            if (rayCallback.HasHit)
            {
                hitPoint = LMath.ToVec(rayCallback.HitPointWorld);
                hitNormal = LMath.ToVec(rayCallback.HitNormalWorld);
                hitObject = rayCallback.CollisionObject;
                return true;
            }
            else
            {
                hitPoint = LMath.ToVec(Vector3.Zero);
                hitNormal = LMath.ToVec(Vector3.Zero);
                hitObject = null;
                return false;
            }
        }

        public static RigidBody GetRigidBodyPositionByUserObjectName(GameObject userObjectName)
        {
            foreach (CollisionObject obj in World.CollisionObjectArray)
            {
                // Check if the CollisionObject has a RigidBody and a matching user object name
                if (obj is RigidBody rigidBody && obj.UserObject != null && obj.UserObject == userObjectName)
                {
                    // Get the position of the RigidBody
                    return rigidBody;
                }
            }

            // If the user object name is not found, return a default position or handle the case accordingly
            return null;
        }



    }
}
