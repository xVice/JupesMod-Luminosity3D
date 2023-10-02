using Luminosity3D.EntityComponentSystem;
using Luminosity3D.Utils;
using System.Numerics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace Luminosity3D.Builtin
{
    public class CameraController : Component
    {
        private Camera camera;
        private float moveSpeed = 15.0f;
        private float sensitivity = 0.005f;
        public bool lockMovement = true;

      
        public override void Awake()
        {
            camera = GetComponent<Camera>();
        }

        public override void Update()
        {
            if (lockMovement == false && camera == Engine.SceneManager.ActiveScene.activeCam) //long big query very slow big bad oh no
            {
                // Handle mouse input

                float deltaTime = (float)Time.deltaTime;

                float mouseXDelta = InputManager.GetMouseDeltaX();
                float mouseYDelta = InputManager.GetMouseDeltaY();
                camera.RotateCamera(-Vector3.UnitY, mouseXDelta * sensitivity);
                camera.RotateCamera(camera.Right, mouseYDelta * sensitivity);

                // Handle movement based on keyboard input
                Vector3 moveDirection = Vector3.Zero;


                if (InputManager.GetKeyDown(Keys.W))
                {
                    moveDirection += Vector3.Transform(-Vector3.UnitZ, Matrix4x4.CreateFromQuaternion(camera.Orientation)); // Move forward
                }

                if (InputManager.GetKeyDown(Keys.S))
                {
                    moveDirection -= Vector3.Transform(-Vector3.UnitZ, Matrix4x4.CreateFromQuaternion(camera.Orientation)); // Move backward
                }

                if (InputManager.GetKeyDown(Keys.A))
                {
                    // Strafe left
                    moveDirection -= Vector3.Cross(Vector3.Transform(-Vector3.UnitZ, Matrix4x4.CreateFromQuaternion(camera.Orientation)), Vector3.UnitY);
                }

                if (InputManager.GetKeyDown(Keys.D))
                {
                    // Strafe right
                    moveDirection += Vector3.Cross(Vector3.Transform(-Vector3.UnitZ, Matrix4x4.CreateFromQuaternion(camera.Orientation)), Vector3.UnitY);
                }


                // Update the camera's position based on movement input
                camera.Move(moveDirection, moveSpeed * deltaTime);
            }
        }


    }


}
