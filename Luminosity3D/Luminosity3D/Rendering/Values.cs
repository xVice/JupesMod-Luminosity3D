using Vector4 = System.Numerics.Vector4;
using System.Numerics;
using OpenTK.Graphics.OpenGL4;

namespace MyGame
{
    // the values 
    public struct Values
    {
        public static Vector4 lightColor = Vector4.One,
        fpsColor = Vector4.One,
        StencilColor = Vector4.One,
        crosshairColor = Vector4.One,
        ParticlesColor = Vector4.One;
        public static float gammaBackground = 1.0f;
        public static float interpolatedBack = 0.9f;


        public static float ParticlesLum = 2.5f;
        public static float ForceLightScene = 15.0f;
        public static float ParticlesScale = 0f;

        public static float stencilSize = 1.001f;

        // Objects value
        public static bool rotate = true;
        public static float gammaObject = 0.858f;
        public static float luminousStrength = 1.2f;
        public static float specularStrength = 0.011f;

        public static float RotateX = 0f;
        public static float RotateY = 0f;
        public static float RotateZ = 0f;

        // bloom
        public struct Bloom
        {
            public static bool Enable = false;
            public static float Exposure = 0.593f;
            public static float Strength = 0.045f;
            public static float Gamma = 0.896f;
            public static float FilterRadius = 0.010f;
            public static float FilmGrain = -0.00f;
            public static float Nitidez = 0.0f;
            public static int Vibrance = 31;
            public static bool Negative = false;
        }

        // ghost
        public static int uGhost = 0;
        public static float uGhostDispersal = 0;


    }
}