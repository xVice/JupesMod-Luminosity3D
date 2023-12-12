using Luminosity3D.EntityComponentSystem;
using Luminosity3D.jfmod;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Luminosity3D.Builtin
{
    [RequireComponent(typeof(TransformComponent))]
    public class SoundComponent : LuminosityBehaviour
    {
        private Sound sound;
        private Bank bank;

        public SoundComponent(string Path, FMOD.MODE mode = FMOD.MODE.DEFAULT)
        {
            sound = new Sound(Path, mode);
            sound.Set3DAttributes(new Vector3(0, 0, 0), new Vector3(0, 0, 0));
            sound.SetMinMaxDistance(0f, 1500f);
        }

        public static SoundComponent LoadSoundFromFile(string path, FMOD.MODE mode = FMOD.MODE.DEFAULT)
        {
            return new SoundComponent(path, mode);
        }

        public SoundComponent Play()
        {
            sound.PlayOnce();
            return this;
        }

        public override void Update()
        {
            sound.Set3DAttributes(Transform.Position, new Vector3(0,0,0));
        }

        public Sound GetSound()
        {
            return sound;
        }

    }
}
