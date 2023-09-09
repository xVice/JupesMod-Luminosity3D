using Luminosity3D.EntityComponentSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Luminosity3D.Utils
{
    //Serialize Entitys to json, future use might be: scene saving, even more easy modding with a simple tool
    public static class EntitySerializer
    {
        public static string SerializeEntity(SerializedEntity serializedEntity)
        {
            JsonSerializerOptions options = new JsonSerializerOptions
            {
                // Set formatting options here
                WriteIndented = true, // To format with indentation
                                      // Add other options as needed
            };

            return JsonSerializer.Serialize(serializedEntity, options);
        }

        public static void ExportEntity(string path ,SerializedEntity serializedEntity)
        {
            File.WriteAllText(path, SerializeEntity(serializedEntity));
        }

        public static SerializedEntity DeserializeEntity(string jsonString)
        {
            return JsonSerializer.Deserialize<SerializedEntity>(jsonString);
        }
    }
}
