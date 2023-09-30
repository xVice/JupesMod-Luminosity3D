using Luminosity3D.EntityComponentSystem;
using Luminosity3D.Utils;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;



namespace Luminosity3D.Builtin
{
    public class SineMovement : Component
    {
        private TransformComponent transform = null;
        public float amplitude = 1f; // Height of the sine wave
        public float frequency = 1f; // Speed of the sine wave

        public Vector3 initialPosition;
        public float timeElapsed;
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
          
        }

        public override void Update()
        {

            if (transform == null)
            {
                transform = Entity.GetComponent<TransformComponent>();
                if (transform == null)
                {
                    return;
                }

            }
            // Update the timeElapsed based on the game time
            timeElapsed += (float)Time.deltaTime;

            // Calculate the new Y position using the sine wave formula
            float newY = initialPosition.Y + amplitude * (float)Math.Sin(frequency * timeElapsed);

            // Update the object's position
            transform.Position = new Vector3(transform.Position.X, newY, transform.Position.Z);
            transform.Update();
        }
    }
}
