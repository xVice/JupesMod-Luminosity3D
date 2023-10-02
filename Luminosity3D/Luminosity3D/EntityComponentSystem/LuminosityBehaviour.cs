using Luminosity3D.Utils;
using Luminosity3DScening;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Luminosity3D.EntityComponentSystem
{
    public class SomeBehav : LuminosityBehaviour
    {
        public override void OnLoad()
        {
            var meshent = SummonEntity(EntitySummoner.CreatePBREntityWithRbConvexHull("Testmesh", "./teapot.obj", new System.Numerics.Vector3(0,0,0)));
            
        }
    }

    public class LuminosityBehaviour 
    {
        public SceneManager SceneManager { get => Engine.SceneManager; }

        public string Name = string.Empty;

        private List<Entity> Entitys = new List<Entity>();

        public LuminosityBehaviour() 
        { 
            
        }

        public LuminosityBehaviour(string name)
        {
            Name = name;
        }

        

        public Entity SummonEntity(Entity entity)
        {
            Entitys.Add(entity);
            return entity;
        }

        public void Remove()
        {
            foreach (var ent in Entitys)
            {
                ent.Kill();//graceful c:
            }
        }

        public virtual void Awake()
        {
            foreach(var ent in Entitys)
            {
                ent.Awake();
            }
        }


        public virtual void Start()
        {
            foreach (var ent in Entitys)
            {
                ent.Start();
            }
        }


        public virtual void EarlyUpdate()
        {
            foreach (var ent in Entitys)
            {
                ent.EarlyUpdate();
            }
        }


        public virtual void Update()
        {
            foreach (var ent in Entitys)
            {
                ent.Update();
            }
        }


        public virtual void LateUpdate()
        {
            foreach (var ent in Entitys)
            {
                ent.LateUpdate();
            }
        }
        
        /// <summary>
        /// Commonly used for autoloaded lupk, allows some more flexible stuff
        /// </summary>
        public virtual void OnStart()
        {

        }

        /// <summary>
        /// Commonly used for normal non autoloaded lupks, as the dev probably has some kind of idea what to hook/do when it gets loaded, with that said, OnStart isnt any diffrent other then allowing more engine tweakbility
        /// </summary>
        public virtual void OnLoad()
        {

        }

        public virtual void OnEnable()
        {
  
        }
   
        public virtual void OnDisable()
        {

        }

        public virtual void OnDestroy()
        {

        }


    }
}
