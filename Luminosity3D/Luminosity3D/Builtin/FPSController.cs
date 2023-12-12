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
    [AddComponentMenu("Player/FPS Controller")]
    [RequireComponent(typeof(TransformComponent))]
    [RequireComponent(typeof(RigidBodyComponent))]
    [RequireComponent(typeof(Camera))]
    [RequireComponent(typeof(CameraController))]

    public class FPSController : LuminosityBehaviour
    {
        RigidBodyComponent rb;
        Camera camera;
        CameraController controller;

        public override void Awake()
        {
            rb = GetComponent<RigidBodyComponent>();
            camera = GetComponent<Camera>();
            camera.SetPosition(Transform.Position);
            controller = GetComponent<CameraController>();
            controller.LockMovement();
        }

        public override void Update()
        {
            var move = Vector3.Zero;  // Initialize the move vector to zero.

            if (InputManager.GetKeyDown(Keys.W))
            {
                move += camera.Forward;
            }

            if (InputManager.GetKeyDown(Keys.S))
            {
                move -= camera.Forward;
            }

            if (InputManager.GetKeyDown(Keys.A))
            {
                move += camera.Right;
            }

            if (InputManager.GetKeyDown(Keys.D))
            {
                move -= camera.Right;
            }

            if (InputManager.GetKeyDown(Keys.Space))
            {
                move += Vector3.UnitY;
            }

            // Ensure that the move vector is normalized.
            if (move.LengthSquared() > 1.0f)
            {
                move = Vector3.Normalize(move);
            }

            // Cap the speed to a maximum value (e.g., 5 units per second).
            float maxSpeed = 5.0f;
            move *= maxSpeed;

            // Apply the impulse to the Rigidbody.
            rb.ApplyImpulse(move * 15);

            camera.SetPosition(Transform.Position);
        }

        public static LuminosityBehaviour OnEditorCreation(GameObject ent)
        {
            return new FPSController();
        }

        public void EditorUI()
        {
            
        }
    }
}
