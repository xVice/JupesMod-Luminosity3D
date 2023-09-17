using Luminosity3D.EntityComponentSystem;
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
        private float sensitivity = 0.2f;

        public CameraController(Entity entity) : base(entity)
        {
            camera = entity.GetComponent<Camera>();
        }

        public override void Update()
        {
            // Get the current keyboard state
            KeyboardState keyboardState = Engine.Instance.KeyboardState;

            // Move the camera based on input
            Vector3 moveDirection = Vector3.Zero;

            if (keyboardState.IsKeyDown(Keys.W))
                moveDirection += camera.Forward;
            if (keyboardState.IsKeyDown(Keys.S))
                moveDirection -= camera.Forward;
            if (keyboardState.IsKeyDown(Keys.Space))
                moveDirection += Vector3.UnitY;
            if (keyboardState.IsKeyDown(Keys.LeftShift))
                moveDirection -= Vector3.UnitY;

            // Normalize the movement vector to prevent faster diagonal movement
            if (moveDirection.LengthSquared > 0)
                moveDirection.Normalize();

            // Move the camera
            camera.Position += moveDirection * moveSpeed * (float)Engine.Instance.DeltaTime;

            // Rotate the camera based on mouse input
            var mouseDelta = Engine.Instance.MouseState.Delta;
            camera.Yaw += mouseDelta.X * sensitivity;
            camera.Pitch -= mouseDelta.Y * sensitivity;

            // Clamp the camera's pitch to avoid flipping
            camera.Pitch = MathHelper.Clamp(camera.Pitch, -MathHelper.PiOver2 + 0.01f, MathHelper.PiOver2 - 0.01f);
        }

        public override void Start()
        {
            // Set the initial camera position and orientation if needed
            camera.Position = new Vector3(0, 1, 3);
            camera.Yaw = 0;
            camera.Pitch = 0;
        }

        public override void OnEnable()
        {
            // Lock and hide the mouse cursor when the camera controller is enabled
            Engine.Instance.Renderer.CursorState = OpenTK.Windowing.Common.CursorState.Grabbed;

        }

        public override void OnDisable()
        {
            // Unlock and show the mouse cursor when the camera controller is disabled
            Engine.Instance.Renderer.CursorState = OpenTK.Windowing.Common.CursorState.Normal;

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

        // Implement other abstract methods as needed
        // ...
    }
}
