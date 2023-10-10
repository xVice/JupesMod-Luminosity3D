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

        public static Matrix ToMatBs(System.Numerics.Matrix4x4 m)
        {
            Matrix r = new Matrix();
            r.M11 = m.M11; r.M12 = m.M12; r.M13 = m.M13; r.M14 = m.M14;
            r.M21 = m.M21; r.M22 = m.M22; r.M23 = m.M23; r.M24 = m.M24;
            r.M31 = m.M31; r.M32 = m.M32; r.M33 = m.M33; r.M34 = m.M34;
            r.M41 = m.M41; r.M42 = m.M42; r.M43 = m.M43; r.M44 = m.M44;
            return r;
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

        private static float[] Matrix4x4ToArray(System.Numerics.Matrix4x4 matrix)
        {
            float[] result = new float[16];

            result[0] = matrix.M11;
            result[1] = matrix.M12;
            result[2] = matrix.M13;
            result[3] = matrix.M14;

            result[4] = matrix.M21;
            result[5] = matrix.M22;
            result[6] = matrix.M23;
            result[7] = matrix.M24;

            result[8] = matrix.M31;
            result[9] = matrix.M32;
            result[10] = matrix.M33;
            result[11] = matrix.M34;

            result[12] = matrix.M41;
            result[13] = matrix.M42;
            result[14] = matrix.M43;
            result[15] = matrix.M44;

            return result;
        }


        public static float[] MatriciesToFloats(System.Numerics.Matrix4x4 view, System.Numerics.Matrix4x4 proj, System.Numerics.Matrix4x4 trans)
        {

            // Extract view, projection, and translation components
            System.Numerics.Vector3 translation = trans.Translation;
            System.Numerics.Matrix4x4 viewProjection = System.Numerics.Matrix4x4.Transpose(trans);
            viewProjection.M41 = 0.0f;
            viewProjection.M42 = 0.0f;
            viewProjection.M43 = 0.0f;

            // Convert the components to floats
            float[] viewProjectionFloats = new float[16];
            for (int row = 0; row < 4; row++)
            {
                for (int col = 0; col < 4; col++)
                {
                    viewProjectionFloats[row * 4 + col] = viewProjection[row, col];
                }
            }

            // Extract individual floats for view, projection, and translation
            float viewFloat = viewProjectionFloats[0];
            float projectionFloat = viewProjectionFloats[5];
            float translationFloat = translation.Length();

            float[] projectionFloats = new float[3];
            projectionFloats[0] = viewFloat;
            projectionFloats[1] = projectionFloat;
            projectionFloats[2] = translationFloat;

            return projectionFloats;
        }

        public static unsafe float[] Matrix4x4ToFloatPointer(System.Numerics.Matrix4x4 matrix)
        {
            float[] matrixPtr = new float[15];

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
