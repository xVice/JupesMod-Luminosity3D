using glTFLoader;
using Luminosity3D.EntityComponentSystem;
using Luminosity3D.Utils;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luminosity3D.Builtin
{
    public class TransformComponent : Component
    {
        public Vector3 Position { get; set; } = Vector3.Zero;
        public Vector3 Rotation { get; set; } = Vector3.Zero;
        public Vector3 Scale { get; set; } = Vector3.One;
        

        public void LookAt()
        {
            var cam = Engine.FindComponents<Camera>().FirstOrDefault();

            if(cam != null)
            {
                cam.LookAt(Position);
            }
        }

        public TransformComponent(Vector3 Position, Vector3 Rotation, Vector3 Scale)
        {
            this.Rotation = Rotation;
            this.Position = Position;
            
            this.Scale = Scale;
            base.ExecutionOrder = 1;
        }

        public Matrix4 GetTransformMatrix()
        {
            var translationMatrix = Matrix4.CreateTranslation(Position);
            var rotationMatrix = Matrix4.CreateRotationX(Rotation.X) * Matrix4.CreateRotationY(Rotation.Y) * Matrix4.CreateRotationZ(Rotation.Z);
            var scaleMatrix = Matrix4.CreateScale(Scale);
            return scaleMatrix * rotationMatrix * translationMatrix;
        }
        public override void Awake()
        {
            
        }

        public override void OnEnable()
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

        public override void Start()
        {
          
        }

        public override void Update()
        {
          
        }
    }
}
