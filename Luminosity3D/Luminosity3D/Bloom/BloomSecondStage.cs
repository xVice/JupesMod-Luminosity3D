using Luminosity3D.Rendering;
using Luminosity3DRendering;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;


namespace Luminosity3D.BloomStages
{
    class BloomSecondStage : IDisposable
    {
        private Vector2i sizeWindow { get => Engine.Renderer.Size; }
        private ShaderProgram shaderBloomFinal;

        private int renderBuffer;
        private int frameBuffer;
        public int[] TexturesBuffer = new int[2];
        public int UseTexture { get => TexturesBuffer[1]; }

        // new bloom
        private BloomFirstStage FirstStage;

        public BloomSecondStage(int NumBlomMips)
        {

            shaderBloomFinal = new ShaderProgram("./shaders/builtin/shadersBloom/bloom.vert", "./shaders/builtin/shadersBloom/BloomFinal.frag");

            FirstStage = new BloomFirstStage(NumBlomMips);

            frameBuffer  = GL.GenFramebuffer();
            renderBuffer = GL.GenRenderbuffer();
            GL.GenTextures(2, TexturesBuffer);

            ResizedFrame();
        }
        private void ResizedFrame()
        {
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, frameBuffer);

            for(int i = 0; i < 2; i++)
            {
                GL.BindTexture(TextureTarget.Texture2D, TexturesBuffer[i]);
                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba16f,
                    sizeWindow.X, sizeWindow.Y, 0, PixelFormat.Rgba, PixelType.Float, IntPtr.Zero);

                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);

                GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0 + i, 
                    TextureTarget.Texture2D, TexturesBuffer[i], 0);
            }

            GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer , renderBuffer);
            GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, RenderbufferStorage.DepthComponent, sizeWindow.X, sizeWindow.Y);
            GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, RenderbufferTarget.Renderbuffer, renderBuffer);   

            DrawBuffersEnum[] attachments = { DrawBuffersEnum.ColorAttachment0, DrawBuffersEnum.ColorAttachment1 }; 
            GL.DrawBuffers(2, attachments);

            if(GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer) != FramebufferErrorCode.FramebufferComplete)
                    Console.WriteLine("Framebuffer Not complete!");



            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        }
        public void Active()
        {
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, frameBuffer);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);
        }
        public void ResizedFrameBuffer()
        {
            FirstStage.ResizedFrameBuffer();
            ResizedFrame();
        }
        public void RenderFrame()
        {
            
            FirstStage.RenderBloomTexture(UseTexture, 0.005f);

            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);

            shaderBloomFinal.Use();
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, TexturesBuffer[0]);
            shaderBloomFinal.SetUniform("scene", 0);

            GL.ActiveTexture(TextureUnit.Texture1);
            GL.BindTexture(TextureTarget.Texture2D, FirstStage.UseTexture);
            shaderBloomFinal.SetUniform("bloomBlur", 1);

            shaderBloomFinal.SetUniform("exposure", 0.8f);
            shaderBloomFinal.SetUniform("bloomStrength", 0.3f);
            shaderBloomFinal.SetUniform("gamma", 0.6f);
            shaderBloomFinal.SetUniform("film_grain", -0.03f);
            shaderBloomFinal.SetUniform("elapsedTime", TimerGL.ElapsedTime);

            shaderBloomFinal.SetUniform("nitidezStrength", 0.0f);
            shaderBloomFinal.SetUniform("vibrance", 0 / -255f);
            shaderBloomFinal.SetUniform("activeNegative", false);


            Quad.RenderQuad();

        }
        public void Dispose()
        {
            FirstStage.Dispose();
            shaderBloomFinal.Dispose();

            GL.DeleteTextures(2, TexturesBuffer);
            GL.DeleteRenderbuffer(renderBuffer);
            GL.DeleteFramebuffer(frameBuffer);

        }
    }
}