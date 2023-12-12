using Assimp;
using BulletSharp;
using BulletSharp.Math;
using BulletSharp.SoftBody;
using ImGuiNET;
using Luminosity3D.Builtin;
using Luminosity3D.Rendering;
using Luminosity3D.Utils;
using Luminosity3DRendering;
using static BulletSharp.Dbvt;

namespace Luminosity3D.EntityComponentSystem
{
    [AddComponentMenu("Physics/3D/Rigid Body")]
    [RequireComponent(typeof(TransformComponent))]
    public class ColliderComponent : LuminosityBehaviour
    {
        public CollisionShape CollisionShape = null;
        public CollisionObject collider = null;

        private MeshBatch batch = null;

        public override void Awake()
        {
            
            
            if (HasComponent<MeshBatch>())
            {
                batch = GetComponent<MeshBatch>();
                
                collider = Physics.CreateStaticCollider(batch.GetModel(), LMath.ToMatBs(Transform.GetTransformMatrix()), GameObject);
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
            /*
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
            */
        }

        private List<Vector3> ExtractVerticesFromMeshBatch(MeshBatch batch)
        {
            // Implement code to extract vertices from your MeshBatch object.
            // You'll need to access the 'Vertices' property from your MeshModel object.
            // Fill 'verticesList' with Vector3 instances representing the mesh vertices.
            // Typically, you'd iterate through the vertices in your mesh data structure.
            List<Vector3> verticesList = new List<Vector3>();

            /*
            // Example: Extract vertices from your MeshModel
            for (int i = 0; i < batch.model.Vertices.Length; i += 3)
            {
                float x = batch.model.Vertices[i];
                float y = batch.model.Vertices[i + 1];
                float z = batch.model.Vertices[i + 2];
                verticesList.Add(new Vector3(x, y, z));
            }

            return verticesList;
            */
            return new List<Vector3>();
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

    public class RigidBodyComponent : LuminosityBehaviour, IImguiSerialize
    {
        public RigidBody RigidBody { get; private set; }
        public ColliderComponent Collider { get; private set; }

        public bool Static = false;

        private System.Numerics.Vector3 directionInEditor = System.Numerics.Vector3.Zero;

        public static RigidBodyComponent BuildStatic()
        {
            var rb = new RigidBodyComponent();
            rb.Static = true;
            return rb;
        }

        public static LuminosityBehaviour OnEditorCreation()
        {
            return new RigidBodyComponent();
        }

        public void EditorUI()
        {
            ImGui.Text("Simulation data");


            ImGui.Separator();
            ImGui.Text("Functions");

            ImGui.InputFloat3("Direction", ref directionInEditor);

            if (ImGui.Button("Apply Force"))
            {
                ApplyForce(directionInEditor);
            }
            if (ImGui.Button("Apply Impulse"))
            {
                ApplyImpulse(directionInEditor);
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

        public override void Awake() 
        {
            //Collider = GetComponent<ColliderComponent>();
            CreateRigidBody();
        }

        public override void LateUpdate()
        {
            if (RigidBody != null)
            {
                // Get the motion state of the RigidBody
                var motionState = RigidBody.MotionState;

                if (motionState != null)
                {
                    // Get the transformation matrix from the motion state
                    var worldTransform = new Matrix();
                    motionState.GetWorldTransform(out worldTransform);

                    // Extract the position and rotation from the transformation matrix
                    var newPosition = new Vector3(worldTransform.M41, worldTransform.M42, worldTransform.M43);

                    // Extract the rotation quaternion from the transformation matrix
                    var newRotation = BulletSharp.Math.Quaternion.RotationMatrix(worldTransform);

                    // Update the Collider's position and rotation
                    Transform.Position = LMath.ToVec(newPosition);
                    Transform.Rotation = LMath.ToQuat(newRotation);
                }
            }
        }

        #region RB helper functions.
        [SerializeField] void CreateRigidBody()
        {
            float mass = 25.0f;

            if (HasComponent<MeshBatch>())
            {
                var batch = GetComponent<MeshBatch>();
                RigidBody = Physics.CreateRigidBody(batch.GetModel(), mass, LMath.ToMatBs(Transform.GetTransformMatrix()), GameObject);

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

        public void ApplyForce(System.Numerics.Vector3 force)
        {
            if (RigidBody != null)
            {
                // Create a BulletSharp Vector3 from the System.Numerics Vector3
                Vector3 relativeForce = LMath.ToVecBs(force);

                // Get the current world transform of the RigidBody
                Matrix boxTrans = RigidBody.MotionState.WorldTransform;

                // Extract the rotation quaternion from the 4x4 matrix
                BulletSharp.Math.Quaternion rotation = BulletSharp.Math.Quaternion.Identity;
                rotation.X = boxTrans.M11;
                rotation.Y = boxTrans.M12;
                rotation.Z = boxTrans.M13;
                rotation.W = boxTrans.M44; // Assuming the quaternion's W component is in M44

                // Rotate the relativeForce by the extracted quaternion
                Vector3 correctedForce = Vector3.Transform(relativeForce, rotation);

                // Apply the corrected force as central force
                RigidBody.ApplyCentralForce(correctedForce);
            }
        }



        public void ApplyImpulse(System.Numerics.Vector3 impulse)
        {
            if (RigidBody != null)
            {
                // Create a BulletSharp Vector3 from the System.Numerics Vector3
                Vector3 relativeForce = LMath.ToVecBs(impulse);

                // Get the current world transform of the RigidBody
                Matrix boxTrans = RigidBody.MotionState.WorldTransform;

                // Extract the rotation quaternion from the 4x4 matrix
                BulletSharp.Math.Quaternion rotation = BulletSharp.Math.Quaternion.Identity;
                rotation.X = boxTrans.M11;
                rotation.Y = boxTrans.M12;
                rotation.Z = boxTrans.M13;
                rotation.W = boxTrans.M44; // Assuming the quaternion's W component is in M44

                // Rotate the relativeForce by the extracted quaternion
                Vector3 correctedForce = Vector3.Transform(relativeForce, rotation);

                RigidBody.ApplyCentralImpulse(relativeForce);
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
        #endregion
    }
}