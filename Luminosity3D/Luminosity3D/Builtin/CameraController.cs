using Luminosity3D.EntityComponentSystem;
using Luminosity3D.Utils;
using System.Numerics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using ImGuiNET;

namespace Luminosity3D.Builtin
{
    [RequireComponent(typeof(Camera))]
    public class CameraController : LuminosityBehaviour, IImguiSerialize
    {
        private Camera camera;
        private float moveSpeed = 15.0f;
        private float sensitivity = 0.005f;
        public bool lockMovement = true;

        public static Component OnEditorCreation()
        {
            return new CameraController();
        }

        public void EditorUI()
        {
            ImGui.InputFloat("Movement speed: ", ref moveSpeed);
            ImGui.InputFloat("Mouse Sensitivity: ", ref sensitivity);

        }

        public override void Awake()
        {
            camera = GetComponent<Camera>();
        }

        public override void Update()
        {
            if (camera == null)
            {
                camera = GetComponent<Camera>();
                Logger.Log("Cam null in cont");
            }
            if (!lockMovement && camera == Engine.SceneManager.ActiveScene.activeCam)
            {
                // Handle mouse input
                float mouseXDelta = InputManager.GetMouseDeltaX();
                float mouseYDelta = InputManager.GetMouseDeltaY();
                camera.RotateCamera(-camera.Up, mouseXDelta * sensitivity);
                camera.RotateCamera(camera.Right, mouseYDelta * sensitivity);

                // Handle movement based on keyboard input
                Vector3 moveDirection = Vector3.Zero;
                if (InputManager.GetKeyDown(Keys.W))
                {
                    moveDirection += Vector3.Transform(-Vector3.UnitZ, Matrix4x4.CreateFromQuaternion(camera.Rotation)); // Move forward
                }

                if (InputManager.GetKeyDown(Keys.S))
                {
                    moveDirection -= Vector3.Transform(-Vector3.UnitZ, Matrix4x4.CreateFromQuaternion(camera.Rotation)); // Move backward
                }

                if (InputManager.GetKeyDown(Keys.A))
                {
                    // Strafe left
                    moveDirection -= Vector3.Cross(Vector3.Transform(-Vector3.UnitZ, Matrix4x4.CreateFromQuaternion(camera.Rotation)), Vector3.UnitY);
                }

                if (InputManager.GetKeyDown(Keys.D))
                {
                    // Strafe right
                    moveDirection += Vector3.Cross(Vector3.Transform(-Vector3.UnitZ, Matrix4x4.CreateFromQuaternion(camera.Rotation)), Vector3.UnitY);
                }


                // Normalize moveDirection if you want constant speed when moving diagonally.
                //moveDirection.Normalize();

                // Update the camera's position based on movement input
                camera.Move(moveDirection, moveSpeed);
            }
        }
   
   
    }


}
