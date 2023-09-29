using Luminosity3D.EntityComponentSystem;
using OpenTK.Mathematics;
using System;

namespace Luminosity3D.Builtin
{
    public class Camera : Component
    {
        public Matrix4 ViewMatrix { get; private set; }
        public Matrix4 ProjectionMatrix { get; private set; }
        public Vector3 Position { get; set; }
        public Vector3 Front { get; private set; }
        public Vector3 Up { get; private set; }
        public float FieldOfView { get; set; }
        public float AspectRatio { get; set; }
        public float NearClip { get; set; }
        public float FarClip { get; set; }
        public float Yaw { get; private set; }
        public float Pitch { get; private set; }
        private float MoveSpeed = 2.5f;
        private float MouseSensitivity = 0.1f;

        public Camera(Entity entity, float fieldOfView, float aspectRatio, float nearClip, float farClip)
            : base(entity)
        {
            Position = Vector3.Zero;
            Up = Vector3.UnitY;
            FieldOfView = fieldOfView;
            AspectRatio = aspectRatio;
            NearClip = nearClip;
            FarClip = farClip;
            Yaw = -90.0f; // Initialize with a facing direction
            Pitch = 0.0f;

            UpdateProjectionMatrix();
            UpdateViewMatrix();
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

        public void UpdateViewMatrix()
        {
            // Calculate the new Front vector based on the current yaw and pitch
            Front = new Vector3(
                MathF.Cos(MathHelper.DegreesToRadians(Yaw)) * MathF.Cos(MathHelper.DegreesToRadians(Pitch)),
                MathF.Sin(MathHelper.DegreesToRadians(Pitch)),
                MathF.Sin(MathHelper.DegreesToRadians(Yaw)) * MathF.Cos(MathHelper.DegreesToRadians(Pitch))
            );
            Front = Vector3.Normalize(Front);

            // Update the view matrix with the new position, target, and up vectors
            ViewMatrix = Matrix4.LookAt(Position, Position + Front, Up);
        }

        public void Move(Vector3 direction, float deltaTime)
        {
            // Move the camera based on the direction and speed
            Position += direction * MoveSpeed * deltaTime;
        }

        public void ProcessMouseMovement(float xOffset, float yOffset, bool constrainPitch = true)
        {
            xOffset *= MouseSensitivity;
            yOffset *= MouseSensitivity;

            Yaw += xOffset;
            Pitch += yOffset;

            // Constrain the pitch to prevent camera flipping
            if (constrainPitch)
            {
                if (Pitch > 89.0f)
                    Pitch = 89.0f;
                if (Pitch < -89.0f)
                    Pitch = -89.0f;
            }

            UpdateViewMatrix();
        }

        public override void Start()
        {
            // Initialize the projection matrix when the camera component starts
            UpdateProjectionMatrix();
            UpdateViewMatrix();
        }

        public override void Awake()
        {
        
        }

        public override void EarlyUpdate()
        {
       
        }

        public override void Update()
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

        // Other ECS component lifecycle methods and properties...
    }
}
