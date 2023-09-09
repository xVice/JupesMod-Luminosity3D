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

    public class PAK
    {
        public string FilePath = string.Empty;
        public string UnpackedPath = string.Empty;
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

            Directory.CreateDirectory(dest);
            using (ZipArchive archive = ZipFile.Open(FilePath, ZipArchiveMode.Read))
            {
                foreach (var entry in archive.Entries)
                {
                    string entryPath = Path.Combine(dest, entry.FullName);

                    if (entryPath.EndsWith("/"))
                    {
                        // If it's a directory, create it
                        Directory.CreateDirectory(entryPath);
                    }
                    else
                    {
                        // If it's a file, extract it
                        using (Stream entryStream = storageProvider.OpenRead(entry.FullName))
                        using (FileStream fileStream = File.Create(entryPath))
                        {
                            entryStream.CopyTo(fileStream);
                        }
                    }
                }
            }
            UnpackedPath = dest;
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

            // Create a new PAK file or overwrite an existing one
            using (ZipArchive archive = ZipFile.Open(destinationFileName, ZipArchiveMode.Create))
            {
                foreach (string file in Directory.GetFiles(FilePath))
                {
                    string entryName = Path.GetFileName(file);

                    // Add each file from the source directory to the PAK archive
                    using (Stream fileStream = File.OpenRead(file))
                    using (Stream entryStream = archive.CreateEntry(entryName).Open())
                    {
                        fileStream.CopyTo(entryStream);
                    }
                }
            }
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
