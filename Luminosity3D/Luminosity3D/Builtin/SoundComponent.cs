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

        private List<FMODEvent> fmodEvents = new List<FMODEvent>();

        public SoundComponent(string Path, FMOD.MODE mode = FMOD.MODE.DEFAULT, bool isBankFile = false, string sampleFile = "")
        {
            if (isBankFile == false)
            {
                sound = new Sound(Path, mode);
                sound.Set3DAttributes(new Vector3(0, 0, 0), new Vector3(0, 0, 0));
                sound.SetMinMaxDistance(0f, 1500f);
            }
            else
            {
                
                if (File.Exists(sampleFile))
                {
                    bank = new Bank(Path, sampleFile);
                }
                else
                {
                    bank = new Bank(Path);
                }

            }
        }

        public void AttachEvent(FMODEvent fevent)
        {
            fmodEvents.Add(fevent);
        }

        public Bank GetBank()
        {
            return bank;
        }

        public Sound GetSound()
        {
            return sound;
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
            if(sound != null)
            {

                sound.Set3DAttributes(Transform.Position, new Vector3(0,0,0));
            }

            foreach (var eve in fmodEvents)
            {
                eve.Set3DAttributes(Transform.Position);
            }


   
        }

   

    }
}
