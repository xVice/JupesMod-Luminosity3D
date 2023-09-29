using Luminosity3D.EntityComponentSystem;
using Luminosity3D.Utils;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luminosity3D.Builtin
{
    public class CameraController : Component
    {
        private Camera camera;
        private float moveSpeed = 5.0f;
        private float strafeSpeed = 3.0f; // Adjust the strafe speed as needed
        private float sensitivity = 0.2f;
        private KeyboardState keyboardState;

        public CameraController(Entity entity) : base(entity)
        {
            camera = entity.GetComponent<Camera>();
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

        public override void OnDestroy()
        {
          
        }

        public override void OnDisable()
        {
           
        }

        public override void OnEnable()
        {
          
        }

        public override void Start()
        {
           
            keyboardState = Engine.KeyboardState;
        }

        public override void Update()
        {
            // Ensure that deltaTime is non-zero to prevent division by zero
            float deltaTime = (float)Math.Max(Engine.DeltaTime, float.Epsilon);

            if (keyboardState != null)
            {
                // Handle mouse input
                var mouseState = Engine.MouseState;
                
                // Calculate the change in mouse position
                float mouseXDelta = mouseState.Delta.X;
                float mouseYDelta = mouseState.Delta.Y;

                // Adjust the camera's yaw and pitch based on mouse movement
                float sensitivity = 0.1f; // Adjust the sensitivity to your preference
                camera.Yaw += mouseXDelta * sensitivity * deltaTime;
                camera.Pitch += mouseYDelta * sensitivity * deltaTime;

                // Limit the pitch angle to prevent camera flipping
                float maxPitch = MathHelper.DegreesToRadians(89.0f);
                camera.Pitch = MathHelper.Clamp(camera.Pitch, -maxPitch, maxPitch);

                // Calculate the new direction the camera is looking
                Quaternion rotation = Quaternion.FromAxisAngle(Vector3.UnitY, camera.Yaw) *
                                     Quaternion.FromAxisAngle(Vector3.UnitX, camera.Pitch);
                camera.Target = Vector3.Transform(-Vector3.UnitZ, rotation);
                camera.Target.Normalize();

                // Update the view matrix with the new position, target, and up vectors
                camera.UpdateViewMatrix(camera.Position, camera.Position + camera.Target, camera.Up);

                // Handle movement based on keyboard input
                Vector3 moveDirection = Vector3.Zero;

                if (keyboardState.IsKeyDown(Keys.W))
                {
                    camera.Position += camera.Target * moveSpeed * deltaTime;
                }

                if (keyboardState.IsKeyDown(Keys.S))
                {
                    camera.Position -= camera.Target * moveSpeed * deltaTime;
                }

                if (keyboardState.IsKeyDown(Keys.A))
                {
                    // Strafe left
                    camera.Strafe(-strafeSpeed * deltaTime); // Adjust for deltaTime
                }

                if (keyboardState.IsKeyDown(Keys.D))
                {
                    // Strafe right
                    camera.Strafe(strafeSpeed * deltaTime); // Adjust for deltaTime
                }

                // Normalize the movement vector to prevent faster diagonal movement
                if (moveDirection.LengthSquared > 0)
                    moveDirection.Normalize();

                // Update the projection matrix (it doesn't change based on input)
                camera.UpdateProjectionMatrix();
            }
            else
            {
                keyboardState = Engine.KeyboardState;
            }
        }



        // Other methods and properties as needed...
    }
}
