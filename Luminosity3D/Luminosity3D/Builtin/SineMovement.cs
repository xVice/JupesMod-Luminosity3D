using Luminosity3D.EntityComponentSystem;
using Luminosity3D.Utils;
using System.Numerics;

namespace Luminosity3D.Builtin
{
    [RequireComponent(typeof(TransformComponent))]
    public class SineMovement : Component
    {
        private TransformComponent transform = null;
        public float amplitude = 1f; // Height of the sine wave
        public float frequency = 1f; // Speed of the sine wave
        private Vector3 initialPosition;

        public override void Awake()
        {
            transform = GetComponent<TransformComponent>();
            initialPosition = transform.Position; // Store the initial position once
        }

        public override void Update()
        {
            if (transform == null)
            {
                return;
            }

            // Calculate the new Y position using the sine wave formula
            float newY = initialPosition.Y + amplitude * (float)Math.Sin(frequency * Time.time);

            // Update the object's position
            transform.Position = new Vector3(transform.Position.X, newY, transform.Position.Z);
        }
    }
}