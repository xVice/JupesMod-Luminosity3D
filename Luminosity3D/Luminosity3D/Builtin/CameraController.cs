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
        private float sensitivity = 200f;
        public bool lockMovement = false;
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
            if (lockMovement == false)
            {
                // Ensure that deltaTime is non-zero to prevent division by zero
                float deltaTime = (float)Math.Max(Engine.DeltaTime, float.Epsilon);

                // Handle mouse input
                var mouseState = Engine.MouseState;

                // Calculate the change in mouse position
                float mouseXDelta = mouseState.Delta.X;
                float mouseYDelta = mouseState.Delta.Y;

                // Adjust the camera's yaw and pitch based on mouse movement

                camera.ProcessMouseMovement(mouseXDelta * sensitivity * deltaTime, mouseYDelta * sensitivity * deltaTime);

                // Handle movement based on keyboard input
                Vector3 moveDirection = Vector3.Zero;

                if (keyboardState == null)
                {
                    keyboardState = Engine.KeyboardState;
                    return;
                }

                if (keyboardState.IsKeyDown(Keys.W))
                {
                    moveDirection += camera.Front;
                }

                if (keyboardState.IsKeyDown(Keys.S))
                {
                    moveDirection -= camera.Front;
                }

                if (keyboardState.IsKeyDown(Keys.A))
                {
                    // Strafe left
                    moveDirection -= Vector3.Cross(camera.Front, camera.Up);
                }

                if (keyboardState.IsKeyDown(Keys.D))
                {
                    // Strafe right
                    moveDirection += Vector3.Cross(camera.Front, camera.Up);
                }

                // Normalize the movement vector to prevent faster diagonal movement
                if (moveDirection.LengthSquared > 0)
                    moveDirection.Normalize();

                // Update the camera's position based on movement input


                camera.Move(moveDirection, moveSpeed * deltaTime);



                // Update the projection matrix (it doesn't change based on input)
                camera.UpdateProjectionMatrix();
            }
            
        }
    }


}
