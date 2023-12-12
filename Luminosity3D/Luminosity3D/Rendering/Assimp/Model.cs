using Luminosity3D.Builtin;
using Luminosity3D.EntityComponentSystem;
using Luminosity3DScening;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace MyGame
{
    public class Model : IDisposable
    {
        public AssimpModel assimpModel;
        public List<Meshe> meshes;
        private ShaderProgram ShaderPBR;
        private Dictionary<string, TextureProgram> TexturesMap = new Dictionary<string, TextureProgram>();

        private GameObject go = null;
        private TransformComponent trans = null;
        

        public Model(string modelPath)
        {
            assimpModel = new AssimpModel(modelPath);
            meshes = new List<Meshe>(assimpModel.meshes);
            
            ShaderPBR = new ShaderProgram("Rendering/Assimp/PBR/PBR_Shader.vert", "Rendering/Assimp/PBR/PBR_Shader.frag");
            
            foreach(var index in meshes)
            {
                LoadTextures(index.DiffusePath          , PixelInternalFormat.SrgbAlpha, TextureUnit.Texture4);
                LoadTextures(index.NormalPath           , PixelInternalFormat.Rgba,      TextureUnit.Texture5);
                LoadTextures(index.LightMap             , PixelInternalFormat.Rgba,      TextureUnit.Texture6);
                LoadTextures(index.EmissivePath         , PixelInternalFormat.SrgbAlpha, TextureUnit.Texture7);
                LoadTextures(index.SpecularPath         , PixelInternalFormat.Rgba,      TextureUnit.Texture8);
                LoadTextures(index.HeightMap            , PixelInternalFormat.Rgba,      TextureUnit.Texture9);
                LoadTextures(index.MetallicPath         , PixelInternalFormat.Rgba,      TextureUnit.Texture10);
                LoadTextures(index.RoughnnesPath        , PixelInternalFormat.Rgba,      TextureUnit.Texture11);
                LoadTextures(index.AmbientOcclusionPath , PixelInternalFormat.Rgba,      TextureUnit.Texture12);
            }
            


        }

        public void SetGameObject(GameObject go)
        {
            this.go = go;
            if (go.HasComponent<TransformComponent>())
            {
                trans = go.GetComponent<TransformComponent>();
            }
        }

        public TexturesCBMaps UseTexCubemap;
        public void RenderFrame()
        {

            ShaderPBR.Use();
            ShaderPBR.SetUniform("model", trans.ModelMatrix);
            ShaderPBR.SetUniform("view", SceneManager.ActiveScene.activeCam.ViewMatrix);
            ShaderPBR.SetUniform("projection", SceneManager.ActiveScene.activeCam.ProjectionMatrix);

            ShaderPBR.SetUniform("viewPos", SceneManager.ActiveScene.activeCam.Position);
            ShaderPBR.SetUniform("lightPositions", Vector3.UnitY * 5.0f);
            ShaderPBR.SetUniform("lightColors", Values.lightColor);

            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.TextureCubeMap, UseTexCubemap.Irradiance);
            ShaderPBR.SetUniform("irradianceMap", 0);

            GL.ActiveTexture(TextureUnit.Texture1);
            GL.BindTexture(TextureTarget.TextureCubeMap, UseTexCubemap.Background);
            ShaderPBR.SetUniform("backgroundMap", 1);

            ShaderPBR.SetUniform("gammaCubemap", Values.gammaBackground);
            ShaderPBR.SetUniform("interpolation", Values.interpolatedBack);

            ShaderPBR.SetUniform("emissiveStrength", Values.ForceLightScene);
            
            ShaderPBR.SetUniform("gamma", Values.gammaObject);
            ShaderPBR.SetUniform("luminousStrength", Values.luminousStrength);
            ShaderPBR.SetUniform("specularStrength", Values.specularStrength);


            GL.Enable(EnableCap.CullFace);
            
            foreach(var item in meshes)
            {

                TexturesMap.TryGetValue(item.DiffusePath, out var albedo);
                if (albedo != null)
                {
                    ShaderPBR.SetUniform("AlbedoMap", TexturesMap[item.DiffusePath].Use);
                }

                TexturesMap.TryGetValue(item.NormalPath, out var normal);
                if (normal != null)
                {
                    ShaderPBR.SetUniform("NormalMap", TexturesMap[item.NormalPath].Use);
                }

                TexturesMap.TryGetValue(item.LightMap, out var lightMap);
                if (lightMap != null)
                {
                    ShaderPBR.SetUniform("AmbienteRoughnessMetallic", TexturesMap[item.LightMap].Use);
                }

                TexturesMap.TryGetValue(item.EmissivePath, out var emmisiveMap);
                if (emmisiveMap != null)
                {
                    ShaderPBR.SetUniform("EmissiveMap", TexturesMap[item.EmissivePath].Use);
                }


                item.RenderFrame();
            }
            GL.Disable(EnableCap.CullFace);



        }
        public void RenderForStencil()
        {
            if(Stencil.RenderStencil)
            {
                foreach(var item in meshes)
                {
                    item.RenderFrame();
                }

            }
        }

        public void Dispose()
        {
            for(int i = 0; i < meshes.Count; i++)
                meshes[i].Dispose();
                
            foreach(var index in TexturesMap.Keys) 
                TexturesMap[index].Dispose();

            ShaderPBR.Dispose();
        }
        private void LoadTextures(string tex_path, PixelInternalFormat pixelFormat, TextureUnit unit)
        {
            if(!TexturesMap.ContainsKey(tex_path))
            {
                if(tex_path != string.Empty)
                {
                    TextureProgram _texture_map = new TextureProgram(tex_path, pixelFormat, unit);
                    TexturesMap.Add(tex_path, _texture_map);
                }
            }

        }
    }
}