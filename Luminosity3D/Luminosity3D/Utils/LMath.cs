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
