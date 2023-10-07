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
    [RequireComponent(typeof(CameraController))]

    public class FPSController : LuminosityBehaviour
    {
        RigidBodyComponent rb;
        ColliderComponent collider;
        Camera camera;
        CameraController controller;

        public override void Awake()
        {
            rb = GetComponent<RigidBodyComponent>();
            collider = rb.Collider;
            camera = GetComponent<Camera>();
            controller = GetComponent<CameraController>();
            controller.LockMovement();
          
        }

        public override void Update()
        {
            var move = new Vector3();
            if (InputManager.GetKeyDown(Keys.W))
            {
                move += camera.Forward;
            }

            if (InputManager.GetKeyDown(Keys.S))
            {
                move -= camera.Forward;
            }

           

            rb.ApplyForce(move * 15 * Time.deltaTime);
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
