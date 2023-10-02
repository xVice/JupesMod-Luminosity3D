using BulletSharp;
using BulletSharp.Math;
using ImGuiNET;
using Luminosity3D.Builtin;
using Luminosity3D.Utils;
using static BulletSharp.Dbvt;

namespace Luminosity3D.EntityComponentSystem
{
    
    [RequireComponent(typeof(TransformComponent))]
    public class ColliderComponent : LuminosityBehaviour
    {
        public CollisionShape CollisionShape = null;
        public TransformComponent Transform = null;

        private MeshBatch batch = null;

        public override void Awake()
        {
            Transform = GetComponent<TransformComponent>(); // Assign the TransformComponent reference
            
            if (HasComponent<MeshBatch>())
            {
                batch = GetComponent<MeshBatch>();
                CollisionShape = BuildConvexHull(batch).CollisionShape;
            }
            else
            {
                CollisionShape = BuildSphere(1f).CollisionShape;
            }
            

        }

        public static ColliderComponent BuildFromMesh(MeshBatch batch)
        {
            var collider = new ColliderComponent();
            collider.CreateCollisionShapeFromMesh(batch);
            return collider;
        }

        public static ColliderComponent BuildConvexHull(MeshBatch batch)
        {
            var collider = new ColliderComponent();
            collider.CreateConvexHullCollisionShape(batch);
            return collider;
        }

        public static ColliderComponent BuildSphere(float radius)
        {
            var collider = new ColliderComponent();
            collider.CreateSphereCollisionShape(radius);
            return collider;
        }

        public void CreateCollisionShapeFromMesh(MeshBatch batch)
        {
            // Calculate the bounding box of your mesh (simplified example)
            Vector3 min = new Vector3(float.MaxValue);
            Vector3 max = new Vector3(float.MinValue);

            var vertices = batch.model.Vertices;
            for (int i = 0; i < vertices.Length; i += 3)
            {
                Vector3 vertex = new Vector3(vertices[i], vertices[i + 1], vertices[i + 2]);
                vertex = LMath.ToVecBs(Transform.TransformPoint(LMath.ToVec(vertex))); // Transform vertices to world space if needed
                min = Vector3.Min(min, vertex);
                max = Vector3.Max(max, vertex);
            }

            var halfExtents = (max - min) * 0.5f;
            var center = min + halfExtents;

            // Create a collision shape (using a box shape in this example)
            CollisionShape = new BoxShape(halfExtents);
        }

        private List<Vector3> ExtractVerticesFromMeshBatch(MeshBatch batch)
        {
            // Implement code to extract vertices from your MeshBatch object.
            // You'll need to access the 'Vertices' property from your MeshModel object.
            // Fill 'verticesList' with Vector3 instances representing the mesh vertices.
            // Typically, you'd iterate through the vertices in your mesh data structure.
            List<Vector3> verticesList = new List<Vector3>();

            // Example: Extract vertices from your MeshModel
            for (int i = 0; i < batch.model.Vertices.Length; i += 3)
            {
                float x = batch.model.Vertices[i];
                float y = batch.model.Vertices[i + 1];
                float z = batch.model.Vertices[i + 2];
                verticesList.Add(new Vector3(x, y, z));
            }

            return verticesList;
        }

        public void CreateConvexHullCollisionShape(MeshBatch batch)
        {
            // 1. Extract mesh data (vertices) from your MeshBatch object.
            List<Vector3> verticesList = ExtractVerticesFromMeshBatch(batch);

            // 2. Create a ConvexHullShape from the computed vertices.
            ConvexHullShape convexHullShape = new ConvexHullShape(verticesList);

            CollisionShape = convexHullShape;
        }

        public void CreateSphereCollisionShape(float radius)
        {
            // Create a collision shape with a sphere
            CollisionShape = new SphereShape(radius);
        }
    }

    [RequireComponent(typeof(TransformComponent))]
    [RequireComponent(typeof(ColliderComponent))]
    public class RigidBodyComponent : LuminosityBehaviour
    {
        public RigidBody RigidBody { get; private set; }
        public ColliderComponent Collider { get; private set; }


        public override void Awake() 
        {
            Collider = GetComponent<ColliderComponent>();
            CreateRigidBody();
        }

        public override void LateUpdate()
        {
            if (RigidBody != null)
            {
                // Get the motion state of the RigidBody
                var motionState = RigidBody.MotionState as DefaultMotionState;

                if (motionState != null)
                {
                    // Get the transformation matrix from the motion state
                    var worldTransform = new Matrix();
                    motionState.GetWorldTransform(out worldTransform);

                    // Extract the position and rotation from the transformation matrix
                    var newPosition = new Vector3(worldTransform.M41, worldTransform.M42, worldTransform.M43);

                    // Extract the rotation quaternion from the transformation matrix
                    var newRotation = Quaternion.RotationMatrix(worldTransform);

                    // Update the Collider's position and rotation
                    Collider.Transform.Position = LMath.ToVec(newPosition);
                    Collider.Transform.Rotation = LMath.ToQuat(newRotation);
                }
            }
        }

        private System.Numerics.Vector3 directionInEditor = System.Numerics.Vector3.Zero; 

        public void EditorUI()
        {
            ImGui.Text("Simulation data");


            ImGui.Separator();
            ImGui.Text("Functions");

            ImGui.InputFloat3("Direction", ref directionInEditor);

            if(ImGui.Button("Apply Force"))
            {
                ApplyForce(LMath.ToVecBs(directionInEditor));
            }
            if (ImGui.Button("Apply Impulse"))
            {
                ApplyImpulse(LMath.ToVecBs(directionInEditor));
            }
            if (ImGui.Button("Apply Torque"))
            {
                ApplyTorque(LMath.ToVecBs(directionInEditor));
            }
            if (ImGui.Button("Apply Torque Impulse"))
            {
                ApplyTorqueImpulse(LMath.ToVecBs(directionInEditor));
            }
        }

        public bool CollidesWith(RigidBody rb)
        {
            return RigidBody.CheckCollideWith(rb);
        }

        public float GetFriction()
        {
            return RigidBody.Friction;
        }

        public void SetFriction(float fric)
        {
            RigidBody.Friction = fric;
        }

        public Vector3 GetGravity()
        {
            return RigidBody.Gravity;
        }

        public void SetGravity(Vector3 grav)
        {
            RigidBody.Gravity = grav;
        }

        public float GetHitFriction()
        {
            return RigidBody.HitFraction;
        }

        public void SetHitFriction(float hitfric)
        {
            RigidBody.HitFraction = hitfric;
        }

        public void SetMass(float mass)
        {
            RigidBody.SetMassProps(mass, Collider.CollisionShape.CalculateLocalInertia(mass));
        }

        public float GetMass()
        {
            return RigidBody.InvMass;
        }

        public void SetDamping(float angularDampen, float linearDampen)
        {
            RigidBody.SetDamping(angularDampen, linearDampen);
        }

        public float GetAngularDamping()
        {
            return RigidBody.AngularDamping;
        }

        public float GetLinearDamping()
        {
            return RigidBody.LinearDamping;
        }

        public void ApplyForce(Vector3 force)
        {
            if (RigidBody != null)
            {
                RigidBody.ApplyCentralForce(force);
            }
        }

        public void ApplyImpulse(Vector3 impulse)
        {
            if (RigidBody != null)
            {
                RigidBody.ApplyCentralImpulse(impulse);
            }
        }

        public void ApplyTorque(Vector3 torque)
        {
            if (RigidBody != null)
            {
                RigidBody.ApplyTorque(torque);
            }
        }

        public void ApplyTorqueImpulse(Vector3 torqueImpulse)
        {
            if (RigidBody != null)
            {
                RigidBody.ApplyTorqueImpulse(torqueImpulse);
            }
        }

        private void CreateRigidBody()
        {
            float mass = 25.0f;

            var shape = Collider.CollisionShape;
            var position = Collider.Transform.Position;

            var startTransform = Matrix.Translation(LMath.ToVecBs(position));
            var motionState = new DefaultMotionState(startTransform);
            var localInertia = shape.CalculateLocalInertia(mass);

            var rigidBodyInfo = new RigidBodyConstructionInfo(mass, motionState, shape, localInertia);
            var rigidBody = new RigidBody(rigidBodyInfo);

            Engine.Renderer.dynamicsWorld.AddRigidBody(rigidBody);

            RigidBody = rigidBody;
        }
    }
}