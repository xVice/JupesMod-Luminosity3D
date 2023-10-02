using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Luminosity3D.Utils
{
    //This is purely so its all really open and easy to understand.
    public static class Temp
    {
        public const string TempPath = "./temp";

        public static void ClearTemp()
        {
            if (Directory.Exists(TempPath))
            {
                Directory.Delete(TempPath, true);
                Directory.CreateDirectory(TempPath);
            }
            else
            {
                Directory.CreateDirectory(TempPath);
            }
        }

        public static void CreateFolder(string folderName)
        {
            string folderPath = Path.Combine(TempPath, folderName);
            Directory.CreateDirectory(folderPath);
        }

        public static void DeleteFolder(string folderName)
        {
            string folderPath = Path.Combine(TempPath, folderName);
            if (Directory.Exists(folderPath))
            {
                Directory.Delete(folderPath, true);
            }
        }

        public static string Get(string name)
        {
            return File.ReadAllText(Path.Combine(TempPath, name));
        }

        public static string[] GetLines(string name)
        {
            return File.ReadAllLines(Path.Combine(TempPath, name));
        }

        public static byte[] GetFileBytes(string name)
        {
            return File.ReadAllBytes(Path.Combine(TempPath, name));
        }

        public static void WriteLines(string name, string[] lines)
        {
            File.WriteAllLines(Path.Combine(TempPath, name), lines);
        }

        public static void Write(string name, string content)
        {
            File.WriteAllText(Path.Combine(TempPath, name), content);
        }

        public static void WriteBytes(string name, byte[] bytes)
        {
            File.WriteAllBytes(Path.Combine(TempPath, name), bytes);
        }
    }

    public static class InputManager
    {
        private static KeyboardState kb = Engine.Renderer.KeyboardState;
        private static MouseState mice = Engine.Renderer.MouseState;
        private static JoystickState cont = Engine.Renderer.JoystickStates[0];

        public static JoystickState GetController()
        {
            return cont;
        }

        public static KeyboardState GetKeyboard()
        {
            return kb;
        }

        public static MouseState GetMouse()
        {
            return mice;
        }

        public static float GetMouseDeltaX()
        {
            return mice.Delta.X;
        }

        public static float GetMouseDeltaY()
        {
            return mice.Delta.Y;
        }

        public static Vector2 GetMouseDelta()
        {
            return LMath.ToVec(mice.Delta);
        }

        public static bool GetKeyDown(Keys key)
        {
            return kb.IsKeyDown(key);
        }

        public static bool GetKeyPressed(Keys key)
        {
            return kb.IsKeyPressed(key);
        }

        public static bool IsKeyReleased(Keys key)
        {
            return kb.IsKeyReleased(key);
        }

        public static bool IsAnyKeyDown()
        {
            return kb.IsAnyKeyDown;
        }
    }

    public static class Resources
    {
        public const string ResourcesPath = "./resources";

        public static void CreateResourcesFolder()
        {
            if (!Directory.Exists(ResourcesPath))
            {
                Directory.CreateDirectory(ResourcesPath);
            }
        }

        public static string Get(string name)
        {
            return File.ReadAllText(Path.Combine(ResourcesPath, name));
        }

        public static string[] GetLines(string name)
        {
            return File.ReadAllLines(Path.Combine(ResourcesPath, name));
        }

        public static byte[] GetFileBytes(string name)
        {
            return File.ReadAllBytes(Path.Combine(ResourcesPath, name));
        }

        public static void WriteLines(string name, string[] lines)
        {
            File.WriteAllLines(Path.Combine(ResourcesPath, name), lines);
        }

        public static void Write(string name, string content)
        {
            File.WriteAllText(Path.Combine(ResourcesPath, name), content);
        }

        public static void WriteBytes(string name, byte[] bytes)
        {
            File.WriteAllBytes(Path.Combine(ResourcesPath, name), bytes);
        }
    }
}
