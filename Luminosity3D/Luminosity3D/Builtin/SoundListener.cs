using Luminosity3D.EntityComponentSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using Luminosity3D.Utils;
using Luminosity3D.jfmod;
using Luminosity3DScening;

namespace Luminosity3D.Builtin
{
    [RequireComponent(typeof(TransformComponent))]
    public class SoundListener : LuminosityBehaviour
    {
        public override void Update()
        {
            var cam = SceneManager.ActiveScene.activeCam;
            if(cam != null )
            {
                var fmodPos = LMath.ToFMODVec(cam.Position);
                var fmodVel = new FMOD.VECTOR();
                var fmodForward = LMath.ToFMODVec(-cam.Forward);
                var fmodUp = LMath.ToFMODVec(cam.Up);

                RuntimeManager.GetFMOD().set3DListenerAttributes(0, ref fmodPos, ref fmodVel, ref fmodForward, ref fmodUp);
            }
        }
    }
}
