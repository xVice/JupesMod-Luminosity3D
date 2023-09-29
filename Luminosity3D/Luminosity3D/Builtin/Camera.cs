using Luminosity3D.EntityComponentSystem;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luminosity3D.Builtin
{
    public class Camera : Component
    {
        public Matrix4 ViewMatrix { get; private set; }
        public Matrix4 ProjectionMatrix { get; private set; }
        public Vector3 Position { get; set; }
        public Vector3 Target { get; set; }
        public Vector3 Up { get; set; }
        public float FieldOfView { get; set; }
        public float AspectRatio { get; set; }
        public float NearClip { get; set; }
        public float FarClip { get; set; }
        public float Yaw { get; set; }
        public float Pitch { get; set; }

        public Vector3 Forward => -Vector3.UnitZ;

        public Camera(Entity entity, float fieldOfView, float aspectRatio, float nearClip, float farClip)
            : base(entity)
        {
            Position = Vector3.Zero;
            Target = Vector3.UnitZ;
            Up = Vector3.UnitY;
            FieldOfView = fieldOfView;
            AspectRatio = aspectRatio;
            NearClip = nearClip;
            FarClip = farClip;
            Yaw = 0.0f;
            Pitch = 0.0f;

            UpdateProjectionMatrix();
            UpdateViewMatrix(Position, Target, Up);
        }

        public void UpdateProjectionMatrix()
        {
            ProjectionMatrix = Matrix4.CreatePerspectiveFieldOfView(
                MathHelper.DegreesToRadians(FieldOfView),
                AspectRatio,
                NearClip,
                FarClip
            );
        }

        public void UpdateViewMatrix(Vector3 position, Vector3 target, Vector3 up)
        {
            ViewMatrix = Matrix4.LookAt(position, position + target, up);
        }

        public override void Update()
        {
            // Update the view matrix based on the camera's position and orientation
            Quaternion rotation = Quaternion.FromAxisAngle(Vector3.UnitY, Yaw) *
                                Quaternion.FromAxisAngle(Vector3.UnitX, Pitch);

            Target = Vector3.Transform(-Vector3.UnitZ, rotation);
            Target.Normalize();
            Up = Vector3.Transform(Vector3.UnitY, rotation);

            // Update the view matrix with the new position, target, and up vectors
            UpdateViewMatrix(Position, Target, Up);
            UpdateProjectionMatrix();
        }



        public void Strafe(float distance)
        {
            // Calculate the strafe direction perpendicular to the camera's forward vector
            Vector3 strafeDirection = Vector3.Cross(Target, Up);
            strafeDirection.Normalize();

            // Move the camera position along the strafe direction
            Position += strafeDirection * distance;
        }

        public override void Start()
        {
            // Initialize the projection matrix when the camera component starts
            UpdateProjectionMatrix();
            UpdateViewMatrix(Position, Target, Up);
        }

        public override void Awake()
        {
           
        }

        public override void EarlyUpdate()
        {
          
        }

        public override void LateUpdate()
        {
          
        }

        public override void OnEnable()
        {
         
        }

        public override void OnDisable()
        {
          
        }

        public override void OnDestroy()
        { 

        }

        // Other methods and properties as needed...
        // You may want to add methods to handle camera movement and input.
    }
}
