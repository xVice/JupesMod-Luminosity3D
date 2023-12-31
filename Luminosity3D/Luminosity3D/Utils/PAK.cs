﻿using Luminosity3D.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;

namespace Luminosity3DPAK
{
    public interface IStorageProvider
    {
        bool Exists(string path);
        Stream OpenRead(string path);
        Stream CreateFile(string path);
        void Delete(string path);
    }

    public class FolderStorageProvider : IStorageProvider
    {
        private readonly string basePath;

        public FolderStorageProvider(string basePath)
        {
            this.basePath = basePath;
        }

        public bool Exists(string path)
        {
            return File.Exists(Path.Combine(basePath, path));
        }

        public Stream OpenRead(string path)
        {
            return File.OpenRead(Path.Combine(basePath, path));
        }

        public Stream CreateFile(string path)
        {
            return File.Create(Path.Combine(basePath, path));
        }

        public void Delete(string path)
        {
            File.Delete(Path.Combine(basePath, path));
        }
    }

    public class ZipStorageProvider : IStorageProvider
    {
        private readonly ZipArchive archive;

        public ZipStorageProvider(ZipArchive archive)
        {
            this.archive = archive;
        }

        public bool Exists(string path)
        {
            return archive.GetEntry(path) != null;
        }

        public Stream OpenRead(string path)
        {
            var entry = archive.GetEntry(path);
            if (entry != null)
            {
                return entry.Open();
            }
            return null;
        }

        public Stream CreateFile(string path)
        {
            throw new NotSupportedException("Creating files in a ZIP archive is not supported.");
        }

        public void Delete(string path)
        {
            var entry = archive.GetEntry(path);
            if (entry != null)
            {
                entry.Delete();
            }
        }
    }

    public class PAKMetaData
    {
        public string Name { get; set; }
        public string Description { get; set; }

        public string[] Dependencies { get; set; }

        public string Namespace { get; set; }
        public string Class { get; set; }
        


    }

    public class PAK
    {
        public string FilePath = string.Empty;
        public string UnpackedPath = string.Empty;
        public PAKMetaData metadata = null;
        public string CheckSum { get => ComputePackedCheckSum(); }

        private readonly IStorageProvider storageProvider;

        public string PakName
        {
            get
            {
                // Get the file name without extension or the folder name
                if (!string.IsNullOrEmpty(FilePath))
                {
                    if (File.Exists(FilePath))
                    {
                        return Path.GetFileNameWithoutExtension(FilePath);
                    }
                    else if (Directory.Exists(FilePath))
                    {
                        return new DirectoryInfo(FilePath).Name;
                    }
                }
                return string.Empty;
            }
        }
        private string ComputePackedCheckSum()
        {
            if (storageProvider == null)
            {
                throw new InvalidOperationException("Storage provider not initialized.");
            }

            using (MemoryStream memoryStream = new MemoryStream())
            {
                using (var hashAlgorithm = SHA256.Create())
                using (var cryptoStream = new CryptoStream(memoryStream, hashAlgorithm, CryptoStreamMode.Write))
                {
                    using (ZipArchive archive = ZipFile.Open(FilePath, ZipArchiveMode.Read))
                    {
                        foreach (var entry in archive.Entries)
                        {
                            using (Stream entryStream = storageProvider.OpenRead(entry.FullName))
                            {
                                entryStream.CopyTo(cryptoStream);
                            }
                        }
                    }
                }

                byte[] hashBytes = memoryStream.ToArray();
                return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
            }
        }

        public byte[] GetIcon()
        {
            return GetEntryBytes("./icon.png");
        }

        public PAK(string path)
        {
            this.FilePath = path;

            if (Directory.Exists(path))
            {
                // Use folder-based storage if the path is a directory
                storageProvider = new FolderStorageProvider(path);
            }
            else if (File.Exists(path) && Path.GetExtension(path).Equals(".pak", StringComparison.OrdinalIgnoreCase))
            {
                // Use ZIP-based storage if the path is a .pak file
                storageProvider = new ZipStorageProvider(ZipFile.OpenRead(path));
            }

            else if (File.Exists(path) && Path.GetExtension(path).Equals(".lupk", StringComparison.OrdinalIgnoreCase))
            {
                // Use ZIP-based storage if the path is a .pak file
                storageProvider = new ZipStorageProvider(ZipFile.OpenRead(path));
            }

            else
            {
                throw new InvalidOperationException("Unsupported storage type.");
            }
        }

        public void Unpack(string dest)
        {
            if (storageProvider == null)
            {
                throw new InvalidOperationException("Storage provider not initialized.");
            }
            
            ZipFile.ExtractToDirectory(FilePath, dest);

            UnpackedPath = dest;

            if (File.Exists(UnpackedPath + "/metadata.json"))
            {
                metadata = JsonConvert.DeserializeObject<PAKMetaData>(File.ReadAllText(UnpackedPath + "/metadata.json"));
            }

            if (Directory.Exists(Path.Combine(UnpackedPath, $"{metadata.Name}")))
            {
                if (!Directory.Exists("./resources/mods"))
                {
                    Directory.CreateDirectory("./resources/mods");
                }
                Directory.Move(Path.Combine(UnpackedPath, $"{metadata.Name}"), "./resources/mods");
            }
        }

        public PAKMetaData ExtractMetadata()
        {
            if (storageProvider == null)
            {
                throw new InvalidOperationException("Storage provider not initialized.");
            }

            return JsonConvert.DeserializeObject<PAKMetaData>(GetEntry("metadata.json"));
        }

        public void Pack(IEnumerable<string> sourceFiles)
        {
            if (storageProvider == null)
            {
                throw new InvalidOperationException("Storage provider not initialized.");
            }

            foreach (var sourceFile in sourceFiles)
            {
                if (File.Exists(sourceFile))
                {
                    string entryName = Path.GetFileName(sourceFile);

                    using (Stream fileStream = File.OpenRead(sourceFile))
                    using (Stream entryStream = storageProvider.CreateFile(entryName))
                    {
                        fileStream.CopyTo(entryStream);
                    }
                }
                else
                {
                    throw new FileNotFoundException($"The source file '{sourceFile}' does not exist.");
                }
            }
        }

        public void PackageToPAK(string destinationFileName)
        {
            if (storageProvider == null)
            {
                throw new InvalidOperationException("Storage provider not initialized.");
            }

            if (!Directory.Exists(FilePath))
            {
                throw new DirectoryNotFoundException($"The source directory '{FilePath}' does not exist.");
            }

            ZipFile.CreateFromDirectory(FilePath, destinationFileName);

        }

        public void PackDirectory(string sourceDirectory)
        {
            if (storageProvider == null)
            {
                throw new InvalidOperationException("Storage provider not initialized.");
            }

            if (!Directory.Exists(sourceDirectory))
            {
                throw new DirectoryNotFoundException($"The source directory '{sourceDirectory}' does not exist.");
            }



            foreach (string file in Directory.GetFiles(sourceDirectory))
            {
                string entryName = Path.GetFileName(file);
                using (Stream fileStream = File.OpenRead(file))
                using (Stream entryStream = storageProvider.CreateFile(entryName))
                {
                    fileStream.CopyTo(entryStream);
                }
            }
        }



        public string GetEntry(string path)
        {
            if (storageProvider == null)
            {
                throw new InvalidOperationException("Storage provider not initialized.");
            }

            using (Stream entryStream = storageProvider.OpenRead(path))
            using (StreamReader reader = new StreamReader(entryStream))
            {
                return reader.ReadToEnd();
            }
        }

        public void WriteEntryBytes(string entryPath, byte[] data)
        {
            if (string.IsNullOrEmpty(entryPath))
            {
                throw new ArgumentException("Entry path cannot be empty.");
            }

            if (data == null || data.Length == 0)
            {
                throw new ArgumentException("Data cannot be empty.");
            }

            if (storageProvider == null)
            {
                throw new InvalidOperationException("Storage provider not initialized.");
            }

            using (Stream entryStream = storageProvider.CreateFile(entryPath))
            {
                entryStream.Write(data, 0, data.Length);
            }
        }


        public void WriteEntry(string entryPath, string content)
        {
            if (string.IsNullOrEmpty(entryPath))
            {
                throw new ArgumentException("Entry path cannot be empty.");
            }

            if (string.IsNullOrEmpty(content))
            {
                throw new ArgumentException("Content cannot be empty.");
            }

            if (storageProvider == null)
            {
                throw new InvalidOperationException("Storage provider not initialized.");
            }

            using (Stream entryStream = storageProvider.CreateFile(entryPath))
            using (StreamWriter writer = new StreamWriter(entryStream))
            {
                writer.Write(content);
            }
        }

        public byte[] GetEntryBytes(string path)
        {
            if (storageProvider == null)
            {
                throw new InvalidOperationException("Storage provider not initialized.");
            }

            using (Stream entryStream = storageProvider.OpenRead(path))
            using (MemoryStream memoryStream = new MemoryStream())
            {
                entryStream.CopyTo(memoryStream);
                return memoryStream.ToArray();
            }
        }

        // Rest of the PAK class methods can be updated to use the storageProvider.
        // ...
    }
}
