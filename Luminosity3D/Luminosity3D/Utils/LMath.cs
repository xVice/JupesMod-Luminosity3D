using OpenTK.Mathematics;
using Assimp;
using System.Runtime.InteropServices;
using BulletSharp.Math;

namespace Luminosity3D.Utils
{
    //the funnel for fix haha
    public static class LMath
    {
        public static System.Numerics.Vector2 ToVec(OpenTK.Mathematics.Vector2 vec)
        {
            return new System.Numerics.Vector2(vec.X, vec.Y);
        }




        public static System.Numerics.Vector3 ToVec(OpenTK.Mathematics.Vector3 vec)
        {
            return new System.Numerics.Vector3(vec.X, vec.Y, vec.Z);
        }

        public static System.Numerics.Vector3 ToVec(BulletSharp.Math.Vector3 vec)
        {
            return new System.Numerics.Vector3(vec.X, vec.Y, vec.Z);
        }

        public static OpenTK.Mathematics.Vector3 ToVecTk(System.Numerics.Vector3 vec)
        {
            return new OpenTK.Mathematics.Vector3(vec.X, vec.Y, vec.Z);
        }

        public static System.Numerics.Matrix4x4 ToMat(OpenTK.Mathematics.Matrix4 openTkMatrix)
        {
            return new System.Numerics.Matrix4x4(
                openTkMatrix.M11, openTkMatrix.M12, openTkMatrix.M13, openTkMatrix.M14,
                openTkMatrix.M21, openTkMatrix.M22, openTkMatrix.M23, openTkMatrix.M24,
                openTkMatrix.M31, openTkMatrix.M32, openTkMatrix.M33, openTkMatrix.M34,
                openTkMatrix.M41, openTkMatrix.M42, openTkMatrix.M43, openTkMatrix.M44
            );
        }

        public static Matrix ToMatBs(System.Numerics.Matrix4x4 systemMatrix)
        {
            return new Matrix
            {
                M11 = systemMatrix.M11,
                M12 = systemMatrix.M12,
                M13 = systemMatrix.M13,
                M14 = systemMatrix.M14,

                M21 = systemMatrix.M21,
                M22 = systemMatrix.M22,
                M23 = systemMatrix.M23,
                M24 = systemMatrix.M24,
                
                M31 = systemMatrix.M31,
                M32 = systemMatrix.M32,
                M33 = systemMatrix.M33,
                M34 = systemMatrix.M34,

                M41 = systemMatrix.M41,
                M42 = systemMatrix.M42,
                M43 = systemMatrix.M43,
                M44 = systemMatrix.M44
            };
        }

        public static Matrix4 ToMatTk(System.Numerics.Matrix4x4 systemNumericsMatrix)
        {
            return new Matrix4(
                systemNumericsMatrix.M11, systemNumericsMatrix.M12, systemNumericsMatrix.M13, systemNumericsMatrix.M14,
                systemNumericsMatrix.M21, systemNumericsMatrix.M22, systemNumericsMatrix.M23, systemNumericsMatrix.M24,
                systemNumericsMatrix.M31, systemNumericsMatrix.M32, systemNumericsMatrix.M33, systemNumericsMatrix.M34,
                systemNumericsMatrix.M41, systemNumericsMatrix.M42, systemNumericsMatrix.M43, systemNumericsMatrix.M44
            );
        }

        public static unsafe float* Matrix4x4ToFloatPointer(System.Numerics.Matrix4x4 matrix)
        {
            float* matrixPtr = (float*)Marshal.AllocHGlobal(16 * sizeof(float));

            matrixPtr[0] = matrix.M11;
            matrixPtr[1] = matrix.M12;
            matrixPtr[2] = matrix.M13;
            matrixPtr[3] = matrix.M14;

            matrixPtr[4] = matrix.M21;
            matrixPtr[5] = matrix.M22;
            matrixPtr[6] = matrix.M23;
            matrixPtr[7] = matrix.M24;

            matrixPtr[8] = matrix.M31;
            matrixPtr[9] = matrix.M32;
            matrixPtr[10] = matrix.M33;
            matrixPtr[11] = matrix.M34;

            matrixPtr[12] = matrix.M41;
            matrixPtr[13] = matrix.M42;
            matrixPtr[14] = matrix.M43;
            matrixPtr[15] = matrix.M44;

            return matrixPtr;
        }

        public static float[] Vector3DListToFloatArray(List<Vector3D> list)
        {
            if (list == null)
            {
                // Handle null list gracefully (return an empty float array or throw an exception).
                return new float[0]; // Return an empty float array in this example.
            }

            int vectorSize = 3; // Assuming Vector3D has 3 components (X, Y, Z).
            float[] floats = new float[list.Count * vectorSize];

            for (int i = 0; i < list.Count; i++)
            {
                if (list[i] != null) // Check for null elements in the list.
                {
                    floats[i * vectorSize] = list[i].X;
                    floats[i * vectorSize + 1] = list[i].Y;
                    floats[i * vectorSize + 2] = list[i].Z;
                }
                else
                {
                    // Handle null elements in the list gracefully (set to default values or log an error).
                    floats[i * vectorSize] = 0.0f; // Default X component.
                    floats[i * vectorSize + 1] = 0.0f; // Default Y component.
                    floats[i * vectorSize + 2] = 0.0f; // Default Z component.
                                                       // Alternatively, you can throw an exception or log an error message here.
                }
            }

            return floats;
        }


        public static float ToRadians(float degrees)
        {
            return degrees * (float)Math.PI / 180.0f;
        }

        public static T Clamp<T>(T value, T min, T max) where T : IComparable<T>
        {
            if (value.CompareTo(min) < 0)
            {
                return min;
            }
            else if (value.CompareTo(max) > 0)
            {
                return max;
            }
            else
            {
                return value;
            }
        }


        public static OpenTK.Mathematics.Vector3 ToVecTk(OpenTK.Mathematics.Vector3 vec)
        {
            return new OpenTK.Mathematics.Vector3(vec.X, vec.Y, vec.Z);
        }

        public static OpenTK.Mathematics.Vector3 ToVecTk(BulletSharp.Math.Vector3 vec)
        {
            return new OpenTK.Mathematics.Vector3(vec.X, vec.Y, vec.Z);
        }


        public static BulletSharp.Math.Vector3 ToVecBs(System.Numerics.Vector3 vec)
        {
            return new BulletSharp.Math.Vector3(vec.X, vec.Y, vec.Z);
        }

        public static System.Numerics.Quaternion ToQuat(System.Numerics.Quaternion quat)
        {
            return new System.Numerics.Quaternion(quat.X, quat.Y, quat.Z, quat.W);
        }

        public static System.Numerics.Quaternion ToQuat(OpenTK.Mathematics.Quaternion quat)
        {
            return new System.Numerics.Quaternion(quat.X, quat.Y, quat.Z, quat.W);
        }

        public static System.Numerics.Quaternion ToQuat(BulletSharp.Math.Quaternion quat)
        {
            return new System.Numerics.Quaternion(quat.X, quat.Y, quat.Z, quat.W);
        }

        public static OpenTK.Mathematics.Quaternion ToQuatTk(System.Numerics.Quaternion quat)
        {
            return new OpenTK.Mathematics.Quaternion(quat.X, quat.Y, quat.Z, quat.W);
        }

        public static OpenTK.Mathematics.Quaternion ToQuatTk(OpenTK.Mathematics.Quaternion quat)
        {
            return new OpenTK.Mathematics.Quaternion(quat.X, quat.Y, quat.Z, quat.W);
        }

        public static OpenTK.Mathematics.Quaternion ToQuatTk(BulletSharp.Math.Quaternion quat)
        {
            return new OpenTK.Mathematics.Quaternion(quat.X, quat.Y, quat.Z, quat.W);
        }

        public static BulletSharp.Math.Quaternion ToQuatBs(BulletSharp.Math.Quaternion quat)
        {
            return new BulletSharp.Math.Quaternion(quat.X, quat.Y, quat.Z, quat.W);
        }

        public static BulletSharp.Math.Quaternion ToQuatBs(System.Numerics.Quaternion quat)
        {
            return new BulletSharp.Math.Quaternion(quat.X, quat.Y, quat.Z, quat.W);
        }
    }
}
