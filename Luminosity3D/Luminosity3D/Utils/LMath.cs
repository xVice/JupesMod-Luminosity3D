using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BulletSharp.Math;
using OpenTK.Mathematics;
using System.Numerics;
using DevExpress.Utils;

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

        public static Matrix4x4 ToMat(OpenTK.Mathematics.Matrix4 openTkMatrix)
        {
            return new Matrix4x4(
                openTkMatrix.M11, openTkMatrix.M12, openTkMatrix.M13, openTkMatrix.M14,
                openTkMatrix.M21, openTkMatrix.M22, openTkMatrix.M23, openTkMatrix.M24,
                openTkMatrix.M31, openTkMatrix.M32, openTkMatrix.M33, openTkMatrix.M34,
                openTkMatrix.M41, openTkMatrix.M42, openTkMatrix.M43, openTkMatrix.M44
            );
        }

        public static Matrix4 ToMatTk(Matrix4x4 systemNumericsMatrix)
        {
            return new Matrix4(
                systemNumericsMatrix.M11, systemNumericsMatrix.M12, systemNumericsMatrix.M13, systemNumericsMatrix.M14,
                systemNumericsMatrix.M21, systemNumericsMatrix.M22, systemNumericsMatrix.M23, systemNumericsMatrix.M24,
                systemNumericsMatrix.M31, systemNumericsMatrix.M32, systemNumericsMatrix.M33, systemNumericsMatrix.M34,
                systemNumericsMatrix.M41, systemNumericsMatrix.M42, systemNumericsMatrix.M43, systemNumericsMatrix.M44
            );
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
