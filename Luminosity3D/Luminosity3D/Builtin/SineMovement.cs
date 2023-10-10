using Luminosity3D.EntityComponentSystem;
using Luminosity3D.Utils;
using System.Numerics;

namespace Luminosity3D.Builtin
{
    [RequireComponent(typeof(TransformComponent))]
    public class SineMovement : LuminosityBehaviour
    {
        [SerializeField] float amplitude = 1f; // Height of the sine wave
        [SerializeField] float frequency = 1f; // Speed of the sine wave
        [SerializeField] Vector3 initialPosition;

        public override void Awake()
        {
            initialPosition = Transform.Position; // Store the initial position once
        }

        public override void Update()
        {
            // Calculate the new Y position using the sine wave formula
            float newY = initialPosition.Y + amplitude * (float)Math.Sin(frequency * Time.time);

            // Update the object's position
            Transform.Position = new Vector3(Transform.Position.X, newY, Transform.Position.Z);
        }
    }
}