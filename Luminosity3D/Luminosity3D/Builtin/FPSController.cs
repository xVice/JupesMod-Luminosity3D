using Luminosity3D.EntityComponentSystem;
using Luminosity3D.Utils;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using static Assimp.Metadata;
using Quaternion = System.Numerics.Quaternion;
using Vector3 = System.Numerics.Vector3;

namespace Luminosity3D.Builtin
{
    [RequireComponent(typeof(TransformComponent))]
    [RequireComponent(typeof(ColliderComponent))]
    [RequireComponent(typeof(RigidBodyComponent))]
    [RequireComponent(typeof(Camera))]

    public class FPSController : LuminosityBehaviour, IImguiSerialize
    {
        TransformComponent transform;
        RigidBodyComponent rb;
        ColliderComponent collider;
        Camera camera;

        private float moveSpeed = 5.0f;
        private float sensitivity = 0.1f;
        private float pitch = 0.0f;
        private float yaw = 0.0f;
        private bool isgrabbed = false;

        public override void Awake()
        {
            transform = GetComponent<TransformComponent>();
            rb = GetComponent<RigidBodyComponent>();
            collider = rb.Collider;
            camera = GetComponent<Camera>();
        }

        public override void Update()
        {
            if (InputManager.GetKeyPressed(Keys.F6))
            {
                var cam = Engine.SceneManager.ActiveScene.activeCam.Parent.GetComponent<CameraController>();

                if (cam != null)
                {
                    isgrabbed = !isgrabbed;
                    if (isgrabbed)
                    {
                        cam.lockMovement = false;
                        Engine.Renderer.CursorState = CursorState.Grabbed;
                    }
                    else
                    {
                        cam.lockMovement = true;
                        Engine.Renderer.CursorState = CursorState.Normal;
                    }
                }

            }
        }

        public override void LateUpdate()
        {
            // Handle player input for movement
            Vector3 moveDirection = Vector3.Zero;

            if (InputManager.GetKeyDown(Keys.W))
                moveDirection += camera.Forward;
            if (InputManager.GetKeyDown(Keys.S))
                moveDirection -= camera.Forward;
            if (InputManager.GetKeyDown(Keys.A))
                moveDirection -= camera.Right;
            if (InputManager.GetKeyDown(Keys.D))
                moveDirection += camera.Right;

            // Normalize the movement vector and apply the move speed
            if (moveDirection.LengthSquared() > 0.0f)
            {
                moveDirection *= moveSpeed;

                // Apply the force to the rigid body
                RigidBodyComponent rigidBodyComponent = GetComponent<RigidBodyComponent>();
                rigidBodyComponent.ApplyForce(LMath.ToVecBs(moveDirection));
            }

            // Handle mouse input for camera rotation
            float deltaX = InputManager.GetMouseDeltaX() * sensitivity * (float)Time.deltaTime;
            float deltaY = InputManager.GetMouseDeltaY() * sensitivity * (float)Time.deltaTime;

            yaw += deltaX;
            pitch = MathHelper.Clamp(pitch + deltaY, -MathHelper.PiOver2, MathHelper.PiOver2);

            // Update the camera orientation based on pitch and yaw
            camera.Orientation = Quaternion.CreateFromYawPitchRoll(pitch, yaw, 0.0f);

            // Update the camera's view matrix
            camera.UpdateViewMatrix();
        }

        public static LuminosityBehaviour OnEditorCreation(Entity ent)
        {
            return new FPSController();
        }

        public void EditorUI()
        {
            
        }
    }
}
