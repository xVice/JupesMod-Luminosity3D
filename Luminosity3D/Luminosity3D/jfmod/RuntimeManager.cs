using FMOD.Studio;
using Luminosity3D.EntityComponentSystem;
using Luminosity3D.Utils;
using Noesis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Luminosity3D.jfmod
{
    public static class RuntimeManager
    {
        private static FMOD.System fmod;
        private static FMOD.Studio.System studio;
        private static FMOD.ChannelGroup channelGroup;

        public static void Init()
        {
            FMOD.Factory.System_Create(out fmod);
            
            fmod.setDSPBufferSize(1024, 10);
            fmod.init(32, FMOD.INITFLAGS.NORMAL, (IntPtr)0);
            fmod.createChannelGroup("main", out channelGroup);
            FMOD.Studio.System.create(out studio);
            //fmod.set3DSettings(15, 1, 5);        
        }

        public static FMOD.ChannelGroup GetChannelGroup()
        {
            return channelGroup;
        }

        public static FMOD.Studio.System GetStudio()
        {
            return studio;
        }

        public static FMOD.System GetFMOD()
        {
            return fmod;
        }

        public static void Update()
        {
            fmod.update();
        }
    }

    public class Bank
    {
        private FMOD.Studio.Bank bank;
        private FMOD.Studio.System studio;

        private FMOD.Studio.EventDescription[] events;

        private Bank(string path)
        {
            studio = RuntimeManager.GetStudio();
            FMOD.RESULT r = studio.loadBankFile(path, FMOD.Studio.LOAD_BANK_FLAGS.NORMAL, out bank);
            bank.getEventCount(out int numEvents);
            bank.getEventList(out events);

        }

        public FMODEvent LoadEvent(string path)
        {
            foreach (var e in events)
            {
                e.getPath(out string currentPath);
                
                if (currentPath == path)
                {
                    e.createInstance(out FMOD.Studio.EventInstance eve);
                    return new FMODEvent(eve);
                }
            }
            return null;
        }



    }

    public class FMODEvent
    {
        private EventInstance Instance;

        public FMODEvent(FMOD.Studio.EventInstance eventInstance)
        {
            Instance = eventInstance;
        }

        public void Play()
        {
            Instance.start();
        }

        public void Set3DAttributes(Vector3 pos)
        {
            //Instance.set3DAttributes()
        }
    }


    public class Sound
    {
        private FMOD.Sound sound;
        private FMOD.Channel channel;
        private FMOD.System fmod;

        public Sound(string path, FMOD.MODE mode = FMOD.MODE.DEFAULT)
        {
            fmod = RuntimeManager.GetFMOD();
            FMOD.RESULT r = fmod.createStream(path, mode, out sound);
            
        }

        public FMOD.Sound GetSound()
        {
            return sound;
        }

        public FMOD.Channel GetChannel()
        {
            return channel;
        }

        public void SetMode(FMOD.MODE mode)
        {
            sound.setMode(mode);
        }

        public void Set3DAttributes(Vector3 pos, Vector3 vel)
        {
            var fmodpos = LMath.ToFMODVec(pos);
            var fmodvel = LMath.ToFMODVec(vel);
            channel.set3DAttributes(ref fmodpos, ref fmodvel);
        }

        public void SetMinMaxDistance(float min, float max)
        {
            channel.set3DMinMaxDistance(min, max);
        }


        public void PlayOnce()
        {
            var channelGroup = RuntimeManager.GetChannelGroup();
            fmod.playSound(sound, channelGroup, false, out channel);
            channel.setMode(FMOD.MODE.LOOP_NORMAL);
            channel.setLoopCount(-1);
        }

 
    }
}
