using FMOD;
using FMOD.Studio;
using Luminosity3D.Builtin;
using Luminosity3D.EntityComponentSystem;
using Luminosity3D.Utils;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Noesis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Xml.Linq;
using static BulletSharp.DiscreteCollisionDetectorInterface;

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
            studio.initialize(32, FMOD.Studio.INITFLAGS.LIVEUPDATE, FMOD.INITFLAGS.NORMAL, (IntPtr)0);

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

        public static Bank LoadBankFromFile(string path)
        {
            return new Bank(path);
        }

        public static Sound LoadSoundFromFile(string path)
        {
            return new Sound(path);
        }

        public static void CheckResult(FMOD.RESULT result)
        {
            if (result != FMOD.RESULT.OK)
            {
                var error = String.Format("FMOD error! ({0}) {1}\n", result, FMOD.Error.String(result));
                Logger.Log(error, true, LogType.Error);
                //throw new Exception(error);
            }
        }
    }

    public class Bank
    {
        private FMOD.Studio.Bank bank;
        private FMOD.Studio.Bank samplebank;
        private FMOD.Studio.System studio;

        private FMOD.Studio.LOADING_STATE loadingState;

        public Bank(string path)
        {
            studio = RuntimeManager.GetStudio();

            studio.getBank(path, out bank);

            bank.getLoadingState(out loadingState);
            if (loadingState != FMOD.Studio.LOADING_STATE.LOADED)
            {
                RuntimeManager.CheckResult(studio.loadBankFile(path, FMOD.Studio.LOAD_BANK_FLAGS.NORMAL, out bank));

                RuntimeManager.GetFMOD().update();
                RuntimeManager.GetStudio().update();
            }



        }

        public Bank(string path, string sampleFile)
        {
            studio = RuntimeManager.GetStudio();

            studio.getBank(path, out bank);
            studio.getBank(sampleFile, out samplebank);

            bank.getLoadingState(out loadingState);
            if (loadingState != FMOD.Studio.LOADING_STATE.LOADED)
            {
                RuntimeManager.CheckResult(studio.loadBankFile(path, FMOD.Studio.LOAD_BANK_FLAGS.NORMAL, out bank));
                RuntimeManager.CheckResult(studio.loadBankFile(sampleFile, FMOD.Studio.LOAD_BANK_FLAGS.NORMAL, out samplebank));
                RuntimeManager.CheckResult(samplebank.loadSampleData());
                RuntimeManager.GetFMOD().update();
                RuntimeManager.GetStudio().update();
            }



        }

        public FMODEvent LoadEvent(string path)
        {
            RuntimeManager.CheckResult(studio.getEvent(path, out EventDescription eventdisc));
            RuntimeManager.CheckResult(eventdisc.createInstance(out EventInstance inst));
            RuntimeManager.GetFMOD().update();
            RuntimeManager.GetStudio().update();
            return new FMODEvent(inst);

        }

        public List<string> ListEvents()
        {

            List<string> paths = new List<string>();
            FMOD.Studio.EventDescription[] evList;
            bank.getEventList(out evList);

            foreach (var ev in evList)
            {
                string path;
                ev.getPath(out path);
                paths.Add(path);
            }

            return paths;
        }




    }

    public class FMODEvent : IDisposable
    {
        private EventInstance Instance;
        private EventDescription desc;

        public FMODEvent(FMOD.Studio.EventInstance eventInstance)
        {
            Instance = eventInstance;
            eventInstance.getDescription(out desc);

            RuntimeManager.GetFMOD().update();
            RuntimeManager.GetStudio().update();
        }

        public FMODEvent Attach(GameObject go)
        {
            var soundcomp = go.GetComponent<SoundComponent>();
            if (soundcomp != null)
            {
                soundcomp.AttachEvent(this);
            }
            return this;
        }

        public void Play()
        {
            RuntimeManager.CheckResult(Instance.start());
        }

        public void SetParameter(string id, float value)
        {
            RuntimeManager.CheckResult(Instance.setParameterByName(id, value));
        }

        public IEnumerable<string> ListParameters()
        { 
            int count;
            RuntimeManager.CheckResult(desc.getParameterDescriptionCount(out count));

            var descList = new LinkedList<FMOD.Studio.PARAMETER_DESCRIPTION>();
            var nameList = new LinkedList<string>();

            for (int i = 0; i < count; i++)
            {
                FMOD.Studio.PARAMETER_DESCRIPTION pDesc;
                RuntimeManager.CheckResult(desc.getParameterDescriptionByIndex(i, out pDesc));

                if (pDesc.type == FMOD.Studio.PARAMETER_TYPE.GAME_CONTROLLED)
                    nameList.AddLast(pDesc.name);
            }

            return nameList;
        }


        public void Set3DAttributes(Vector3 pos)
        {
            var attribs = new ATTRIBUTES_3D();
            
            attribs.position = pos.ToFMODVec();
                
            
            Instance.set3DAttributes(attribs);
        }

        public void Dispose()
        {
            Instance.release();
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
            RuntimeManager.CheckResult(fmod.createStream(path, mode, out sound));
            
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
