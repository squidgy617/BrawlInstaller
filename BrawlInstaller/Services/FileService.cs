using BrawlLib.SSBB.ResourceNodes;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Windows.Media.Imaging;
using BrawlInstaller.Common;
using System.IO.Compression;
using System.Net.Cache;
using BrawlLib.Internal;
using System.Security.Cryptography;
using Force.Crc32;
using System.Drawing.Imaging;
using BrawlInstaller.Classes;
using BrawlInstaller.StaticClasses;
using System.Diagnostics;
using BrawlInstaller.Exceptions;
using System.Text.RegularExpressions;
using System.Web.UI.Design.WebControls;
using Newtonsoft.Json;
using System.Xml.Linq;

namespace BrawlInstaller.Services
{
    public interface IFileService
    {
        /// <inheritdoc cref="FileService.OpenFile(string)"/>
        ResourceNode OpenFile(string path);

        /// <inheritdoc cref="FileService.SaveFile(ResourceNode)"/>
        void SaveFile(ResourceNode node);

        /// <inheritdoc cref="FileService.SaveFileAs(ResourceNode, string)"/>
        void SaveFileAs(ResourceNode node, string path);

        /// <inheritdoc cref="FileService.CloseFile(ResourceNode)"/>
        void CloseFile(ResourceNode node);

        /// <inheritdoc cref="FileService.CopyNode(ResourceNode)"/>
        ResourceNode CopyNode(ResourceNode node);

        /// <inheritdoc cref="FileService.CopyFile(string, string)"/>
        void CopyFile(string inFile, string outFile);

        /// <inheritdoc cref="FileService.DeleteFile(string)"/>
        void DeleteFile(string file);

        /// <inheritdoc cref="FileService.SaveImage(BitmapImage, string)"/>
        void SaveImage(BitmapImage image, string outPath);

        /// <inheritdoc cref="FileService.SaveTextFile(string, string)"/>
        void SaveTextFile(string filePath, string text);

        /// <inheritdoc cref="FileService.ReadTextFile(string)"/>
        string ReadTextFile(string filePath);

        /// <inheritdoc cref="FileService.ReadTextLines(string)"/>
        List<string> ReadTextLines(string filePath);

        /// <inheritdoc cref="FileService.LoadImage(string)"/>
        BitmapImage LoadImage(string filePath);

        /// <inheritdoc cref="FileService.ReadRawData(ResourceNode)"/>
        byte[] ReadRawData(ResourceNode node);

        /// <inheritdoc cref="FileService.ReplaceNodeRaw(ResourceNode, byte[])"/>
        void ReplaceNodeRaw(ResourceNode node, byte[] data);

        /// <inheritdoc cref="FileService.CreateDirectory(string)"/>
        void CreateDirectory(string path);

        /// <inheritdoc cref="FileService.DirectoryExists(string)"/>
        bool DirectoryExists(string path);

        /// <inheritdoc cref="FileService.DeleteDirectory(string)"/>
        void DeleteDirectory(string path);

        /// <inheritdoc cref="FileService.GetDirectories(string, string, SearchOption)"/>
        List<string> GetDirectories(string path, string searchPattern, SearchOption searchOption);

        /// <inheritdoc cref="FileService.GetFiles(string, string)"/>
        List<string> GetFiles(string path, string searchPattern);

        /// <inheritdoc cref="FileService.GetFiles(string, string, SearchOption)"/>
        List<string> GetFiles(string path, string searchPattern, SearchOption searchOption);

        /// <inheritdoc cref="FileService.FileExists(string)"/>
        bool FileExists(string path);

        /// <inheritdoc cref="FileService.FileOrDirectoryExists(string)"/>
        bool FileOrDirectoryExists(string path);

        /// <inheritdoc cref="FileService.GenerateZipFileFromDirectory(string, string)"/>
        string GenerateZipFileFromDirectory(string inDirectory, string outFile);

        /// <inheritdoc cref="FileService.ExtractZipFile(string, string)"/>
        string ExtractZipFile(string inFile, string outDirectory);

        /// <inheritdoc cref="FileService.GetNodes(ResourceNode)"/>
        List<ResourceNode> GetNodes(ResourceNode rootNode);

        /// <inheritdoc cref="FileService.DecryptBinFile(string)"/>
        byte[] DecryptBinFile(string filePath);

        /// <inheritdoc cref="FileService.DecryptBinData(byte[])"/>
        byte[] DecryptBinData(byte[] binData);

        /// <inheritdoc cref="FileService.EncryptBinData(byte[])"/>
        byte[] EncryptBinData(byte[] binData);

        /// <inheritdoc cref="FileService.ReadAllBytes(string)"/>
        byte[] ReadAllBytes(string filePath);

        /// <inheritdoc cref="FileService.ParseIniFile(string)"/>
        Dictionary<string, string> ParseIniFile(string iniFile);

        /// <inheritdoc cref="FileService.StartBackup()"/>
        Backup StartBackup();

        /// <inheritdoc cref="FileService.RestoreBackup(Backup)"/>
        void RestoreBackup(Backup backup);

        /// <inheritdoc cref="FileService.GetBackups()"/>
        List<Backup> GetBackups();

        /// <inheritdoc cref="FileService.EndBackup()"/>
        void EndBackup();

        /// <inheritdoc cref="FileService.OpenXmlFile(string)"/>
        XElement OpenXmlFile(string path);

        /// <inheritdoc cref="FileService.SaveXmlFile(XElement, string)"/>
        void SaveXmlFile(XElement xml, string path);

        /// <inheritdoc cref="FileService.BackupBuildFile(string, bool)"/>
        void BackupBuildFile(string path, bool deleteFile = false);

        Backup CurrentBackup { get; set; }
    }

    [Export(typeof(IFileService))]
    internal class FileService : IFileService
    {
        // Services
        ISettingsService _settingsService { get; }

        [ImportingConstructor]
        public FileService(ISettingsService settingsService)
        {
            _settingsService = settingsService;
        }

        // Properties
        public Backup CurrentBackup { get; set; } = null;

        // Methods

        /// <summary>
        /// Open a BrawlLib-compatible file
        /// </summary>
        /// <param name="path">Path to file</param>
        /// <returns>Root node of file</returns>
        public ResourceNode OpenFile(string path)
        {
            if (!File.Exists(path))
                return null;
            var rootNode = NodeFactory.FromFile(null, path);
            return rootNode;
        }

        /// <summary>
        /// Save a BrawlLib-compatible file
        /// </summary>
        /// <param name="node">Root node of file</param>
        public void SaveFile(ResourceNode node)
        {
            SaveFileAs(node, node.FilePath);
        }

        /// <summary>
        /// Save a BrawlLib-compatible file to a specified path
        /// </summary>
        /// <param name="node">Root node of file</param>
        /// <param name="path">Path to save file to</param>
        public void SaveFileAs(ResourceNode node, string path)
        {
            BackupBuildFile(path);
            if (!string.IsNullOrEmpty(Path.GetDirectoryName(path)) && !Directory.Exists(Path.GetDirectoryName(path)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(path));
            }
            node.Export(path);
            if (node?.IsDirty == true)
                node.IsDirty = false;
        }

        /// <summary>
        /// Close a BrawlLib-compatible file
        /// </summary>
        /// <param name="node">Root node of file to close</param>
        public void CloseFile(ResourceNode node)
        {
            if (node != null)
            {
                // TODO: Figure out what actually causes this error. For now, this should force modules to eventually close like they should
                if (node.GetType() == typeof(RELNode))
                {
                    var sw = new Stopwatch();
                    sw.Start();
                    while (true)
                    {
                        try
                        {
                            if (sw.ElapsedMilliseconds >= 10000)
                            {
                                sw.Stop();
                                var error = new Exception($"Module {node.FileName} could not be closed after 10 seconds. Please try again.");
                                throw error;
                            }
                            node.Dispose();
                            break;
                        }
                        catch (InvalidOperationException ex)
                        {
                            if (ex.Message == "Collection was modified; enumeration operation may not execute.")
                            {
                                continue;
                            }
                        }
                    }
                    sw.Stop();
                }
                else
                {
                    node.Dispose();
                }
            }
        }

        /// <summary>
        /// Create a duplicate of a ResourceNode
        /// </summary>
        /// <param name="node">ResourceNode to copy</param>
        /// <returns>Copy of node</returns>
        public ResourceNode CopyNode(ResourceNode node)
        {
            //var newNode = NodeFactory.FromAddress(node.Parent, node.WorkingSource.Address, node.WorkingSource.Length);
            var guid = Guid.NewGuid().ToString();
            var path = $"{_settingsService.AppSettings.TempPath}\\{guid}";
            CreateDirectory(Path.GetDirectoryName(path));
            node.Export(path);
            ResourceNode newNode = null;
            if (node.GetType() == typeof(TEX0Node) && ((TEX0Node)node).SharesData)
            {
                newNode = new TEX0Node();
                ((TEX0Node)newNode).SharesData = true;
            }
            else
            {
                newNode = NodeFactory.FromFile(null, path, node.GetType());
            }
            if (newNode.GetType().IsAssignableFrom(typeof(BRESEntryNode)))
            {
                ((BRESEntryNode)newNode).OriginalPath = "";
            }
            File.Delete(path);
            newNode.Name = node.Name;
            return newNode;
        }

        /// <summary>
        /// Copy a file
        /// </summary>
        /// <param name="inFile">Filepath to copy</param>
        /// <param name="outFile">New file path</param>
        public void CopyFile(string inFile, string outFile)
        {
            BackupBuildFile(outFile);
            if (File.Exists(inFile))
            {
                CreateDirectory(Path.GetDirectoryName(outFile));
                File.Copy(inFile, outFile, true);
            }
        }

        /// <summary>
        /// Delete file
        /// </summary>
        /// <param name="file">File to delete</param>
        public void DeleteFile(string file)
        {
            BackupBuildFile(file, true);
            if (File.Exists(file))
                File.Delete(file);
        }

        /// <summary>
        /// Save an image
        /// </summary>
        /// <param name="image">BitmapImage to save</param>
        /// <param name="outFile">Output path of image file</param>
        public void SaveImage(BitmapImage image, string outFile)
        {
            BackupBuildFile(outFile);
            if (image != null)
            {
                CreateDirectory(Path.GetDirectoryName(outFile));
                SaveImage(image.ToBitmap(), outFile);
            }
        }

        /// <summary>
        /// Load an image from a file
        /// </summary>
        /// <param name="filePath">Path to image</param>
        /// <returns>BitmapImage of image</returns>
        public BitmapImage LoadImage(string filePath)
        {
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.UriSource = new Uri(filePath);
            bitmap.EndInit();
            bitmap.Freeze();
            return bitmap;
        }

        /// <summary>
        /// Save an image
        /// </summary>
        /// <param name="image">Bitmap to save</param>
        /// <param name="outFile">Output path of image file</param>
        private void SaveImage(Bitmap image, string outFile)
        {
            BackupBuildFile(outFile);
            CreateDirectory(Path.GetDirectoryName(outFile));
            image.Save(outFile);
        }

        /// <summary>
        /// Save a text file
        /// </summary>
        /// <param name="filePath">Path to save to</param>
        /// <param name="text">Text to save</param>
        public void SaveTextFile(string filePath, string text)
        {
            if (!string.IsNullOrEmpty(text))
            {
                BackupBuildFile(filePath);
                CreateDirectory(Path.GetDirectoryName(filePath));
                File.WriteAllText(filePath, text);
            }
        }

        /// <summary>
        /// Read text file if it exists
        /// </summary>
        /// <param name="filePath">Text file to read</param>
        /// <returns>Text from file</returns>
        public string ReadTextFile(string filePath)
        {
            if (FileExists(filePath))
            {
                return File.ReadAllText(filePath);
            }
            return string.Empty;
        }

        /// <summary>
        /// Read all lines from text file if it exists
        /// </summary>
        /// <param name="filePath">Text file to read</param>
        /// <returns>List of lines from text file</returns>
        public List<string> ReadTextLines(string filePath)
        {
            if (FileExists(filePath))
            {
                return File.ReadAllLines(filePath).ToList();
            }
            return new List<string>();
        }

        /// <summary>
        /// Create a directory if it does not exist
        /// </summary>
        /// <param name="path">Directory to create</param>
        public void CreateDirectory(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }

        /// <summary>
        /// Check if a directory exists
        /// </summary>
        /// <param name="path">Path to check</param>
        /// <returns>Whether or not directory exists</returns>
        public bool DirectoryExists(string path)
        {
            return Directory.Exists(path);
        }

        /// <summary>
        /// Delete specified directory
        /// </summary>
        /// <param name="path">Path to delete</param>
        public void DeleteDirectory(string path)
        {
            if (DirectoryExists(path))
            {
                BackupBuildFolder(path);
                Directory.Delete(path, true);
            }
        }

        /// <summary>
        /// Get directories in directory if it exists
        /// </summary>
        /// <param name="path">Path to search for folders</param>
        /// <param name="searchPattern">Search pattern</param>
        /// <param name="searchOption">Search option</param>
        /// <returns>List of directories</returns>
        public List<string> GetDirectories(string path, string searchPattern, SearchOption searchOption)
        {
            if (DirectoryExists(path))
            {
                return Directory.GetDirectories(path, searchPattern, searchOption).ToList();
            }
            return new List<string>();
        }

        /// <summary>
        /// Get files in directory if it exists
        /// </summary>
        /// <param name="path">Path to search</param>
        /// <param name="searchPattern">Search pattern</param>
        /// <returns>List of files</returns>
        public List<string> GetFiles(string path, string searchPattern)
        {
            if (DirectoryExists(path))
            {
                return Directory.GetFiles(path, searchPattern, SearchOption.TopDirectoryOnly).ToList();
            }
            return new List<string>();
        }

        /// <summary>
        /// Get files in directory if it exists
        /// </summary>
        /// <param name="path">Path to search</param>
        /// <param name="searchPattern">Search pattern</param>
        /// <param name="searchOption">Search option</param>
        /// <returns>List of files</returns>
        public List<string> GetFiles(string path, string searchPattern, SearchOption searchOption)
        {
            if (DirectoryExists(path))
            {
                return Directory.GetFiles(path, searchPattern, searchOption).ToList();
            }
            return new List<string>();
        }

        /// <summary>
        /// Check if file exists
        /// </summary>
        /// <param name="path">Path to file</param>
        /// <returns>Whether file exists or not</returns>
        public bool FileExists(string path)
        {
            return File.Exists(path);
        }

        /// <summary>
        /// Check if file or directory exists for the path
        /// </summary>
        /// <param name="path">Path to check</param>
        /// <returns>Whether file/directory exists or not</returns>
        public bool FileOrDirectoryExists(string path)
        {
            return FileExists(path) || DirectoryExists(path);
        }

        /// <summary>
        /// Read raw data of a node
        /// </summary>
        /// <param name="node">Node to read data from</param>
        /// <returns>Raw data of node</returns>
        public byte[] ReadRawData(ResourceNode node)
        {
            var length = node.WorkingUncompressed.Length;
            byte[] data = new byte[length];
            Marshal.Copy(node.WorkingUncompressed.Address, data, 0, length);
            return data;
        }

        /// <summary>
        /// Replace raw data of a node
        /// </summary>
        /// <param name="node">Node to replace</param>
        /// <param name="data">Byte array to replace node data with</param>
        public void ReplaceNodeRaw(ResourceNode node, byte[] data)
        {
            unsafe
            {
                fixed (byte* ptr = data)
                {
                    node.ReplaceRaw(ptr, data.Length);
                }
            }
        }

        /// <summary>
        /// Generate a zip file from a directory
        /// </summary>
        /// <param name="inDirectory">Directory to zip</param>
        /// <param name="outFile">Output file</param>
        /// <returns>Output file</returns>
        public string GenerateZipFileFromDirectory(string inDirectory, string outFile)
        {
            if (DirectoryExists(inDirectory) && !string.IsNullOrEmpty(outFile))
            {
                if (FileExists(outFile))
                {
                    DeleteFile(outFile);
                }
                ZipFile.CreateFromDirectory(inDirectory, outFile);
                return outFile;
            }
            return string.Empty;
        }

        /// <summary>
        /// Extract a zip file to a directory
        /// </summary>
        /// <param name="inFile">File to extract</param>
        /// <param name="outDirectory">Directory to extract to</param>
        /// <returns>Output directory</returns>
        public string ExtractZipFile(string inFile, string outDirectory)
        {
            if (!string.IsNullOrEmpty(inFile))
            {
                if (DirectoryExists(outDirectory))
                {
                    DeleteDirectory(outDirectory);
                }
                ZipFile.ExtractToDirectory(inFile, outDirectory);
                return outDirectory;
            }
            return string.Empty;
        }

        /// <summary>
        /// Get all top-level nodes from a file
        /// </summary>
        /// <param name="rootNode">Root node of file</param>
        /// <returns>List of top-level nodes</returns>
        public List<ResourceNode> GetNodes(ResourceNode rootNode)
        {
            var allNodes = rootNode.Children;
            return allNodes;
        }

        /// <summary>
        /// Decrypt a bin file
        /// </summary>
        /// <param name="filePath">Path to bin file</param>
        /// <returns>Decrypted bin file data</returns>
        public byte[] DecryptBinFile(string filePath)
        {
            var sourceData = File.ReadAllBytes(filePath);
            return DecryptBinData(sourceData);
        }

        /// <summary>
        /// Decrypt bin data
        /// </summary>
        /// <param name="binData">Bin data to decrypt</param>
        /// <returns>Decrypted bin data</returns>
        public byte[] DecryptBinData(byte[] binData)
        {
            using (Aes aes = Aes.Create())
            {
                aes.Key = Encryption.SDKey;
                aes.IV = Encryption.SDIV;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.None;

                ICryptoTransform decryptor = aes.CreateDecryptor();
                byte[] decryptedData = decryptor.TransformFinalBlock(binData, 0, binData.Length);
                return decryptedData;
            }
        }

        /// <summary>
        /// Encrypt bin data
        /// </summary>
        /// <param name="binData">Decrypted bin data</param>
        /// <returns>Encrypted bin data</returns>
        public byte[] EncryptBinData(byte[] binData)
        {
            // Update size fields
            var size = BitConverter.GetBytes(binData.Length - 0x20).Reverse().ToArray(); // these fields are technically size - 0x20 for some reason
            size.CopyTo(binData, 0x18); // Size
            size.CopyTo(binData, 0x1C); // Compressed size - we just use the normal size

            using (Aes aes = Aes.Create())
            {
                UpdateCRC(binData);
                aes.Key = Encryption.SDKey;
                aes.IV = Encryption.SDIV;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.None;

                ICryptoTransform encryptor = aes.CreateEncryptor();
                byte[] encryptedData = encryptor.TransformFinalBlock(binData, 0, binData.Length);
                return encryptedData;
            }
        }

        private static void UpdateCRC(byte[] data)
        {
            // Set bytes at offset 0x10 to 0xDEADBEEF
            data[0x10] = 0xDE;
            data[0x11] = 0xAD;
            data[0x12] = 0xBE;
            data[0x13] = 0xEF;

            // Calculate CRC32
            uint crc = Crc32Algorithm.Compute(data);
            byte[] crcBytes = BitConverter.GetBytes(crc);

            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(crcBytes); // Convert to big-endian
            }

            // Update the data at offset 0x10
            Array.Copy(crcBytes, 0, data, 0x10, 4);
        }

        /// <summary>
        /// Read all bytes of a file
        /// </summary>
        /// <param name="filePath">Path to file</param>
        /// <returns>Bytes of file</returns>
        public byte[] ReadAllBytes(string filePath)
        {
            if (FileExists(filePath))
            {
                return File.ReadAllBytes(filePath);
            }
            return null;
        }

        /// <summary>
        /// Read data from INI file
        /// </summary>
        /// <param name="iniFile">INI file to read</param>
        /// <returns>Dictionary of INI data</returns>
        public Dictionary<string, string> ParseIniFile(string iniFile) 
        {
            var iniData = new Dictionary<string, string>();
            var text = ReadTextFile(iniFile);
            var cleanText = Regex.Replace(text, "(;|#|//).*(\n|\r)", "");
            var lines = cleanText.Replace("\r\n", "\n").Split('\n');
            foreach (var line in lines.Where(x => !string.IsNullOrWhiteSpace(x)))
            {
                var keyValuePair = line.Split('=');
                iniData.Add(keyValuePair[0].Trim(), keyValuePair[1].Trim());
            }
            return iniData;
        }

        /// <summary>
        /// Start a backup of build files
        /// </summary>
        /// <returns>New backup</returns>
        public Backup StartBackup()
        {
            // Delete a backup if we're already at the count
            var backups = GetBackups();
            if (backups.Count >= _settingsService.AppSettings.BackupCount)
            {
                DeleteBackup(backups.OrderByDescending(x => x.TimeStamp).LastOrDefault());
            }
            // Start backup
            var backup = new Backup
            {
                BuildPath = _settingsService.AppSettings.BuildPath,
                Guid = Guid.NewGuid().ToString()
            };
            CurrentBackup = backup;
            return backup;
        }

        /// <summary>
        /// End backup of build files
        /// </summary>
        public void EndBackup()
        {
            if (CurrentBackup != null)
            {
                var backupJson = JsonConvert.SerializeObject(CurrentBackup, Formatting.Indented);
                File.WriteAllText(Path.Combine(Paths.BackupPath, $"{CurrentBackup.Guid}.json"), backupJson);
                CurrentBackup = null;
            }
        }

        /// <summary>
        /// Restore selected backup
        /// </summary>
        /// <param name="backup">Backup to restore</param>
        public void RestoreBackup(Backup backup)
        {
            var buildFiles = GetFiles(backup.BuildBackupPath, "*", SearchOption.AllDirectories);
            var textureFiles = GetFiles(backup.TextureBackupPath, "*", SearchOption.AllDirectories);
            // Restore build files
            foreach(var file in buildFiles)
            {
                var path = file.Replace(backup.BuildBackupPath, "");
                path = $"{_settingsService.AppSettings.BuildPath}\\{path}";
                CreateDirectory(Path.GetDirectoryName(path));
                File.Copy(file, path, true);
            }
            // Restore textures
            if (!string.IsNullOrEmpty(_settingsService.AppSettings.HDTextures) && _settingsService.AppSettings.ModifyHDTextures)
            {
                foreach (var file in textureFiles)
                {
                    var path = file.Replace(backup.TextureBackupPath, "");
                    path = $"{_settingsService.AppSettings.HDTextures}\\{path}";
                    CreateDirectory(Path.GetDirectoryName(path));
                    File.Copy(file, path, true);
                }
            }
            // Delete added files
            foreach(var file in backup.AddedFiles)
            {
                if (File.Exists($"{_settingsService.AppSettings.BuildPath}\\{file}"))
                {
                    File.Delete($"{_settingsService.AppSettings.BuildPath}\\{file}");
                }
                else if (File.Exists($"{_settingsService.AppSettings.HDTextures}\\{file}"))
                {
                    File.Delete($"{_settingsService.AppSettings.HDTextures}\\{file}");
                }
            }
        }

        /// <summary>
        /// Get a list of all saved backups
        /// </summary>
        /// <returns>List of backups</returns>
        public List<Backup> GetBackups()
        {
            var backups = new List<Backup>();
            foreach(var file in GetFiles(Paths.BackupPath, "*.json"))
            {
                var json = ReadTextFile(file);
                var backup = JsonConvert.DeserializeObject<Backup>(json);
                backups.Add(backup);
            }
            return backups.OrderByDescending(x => x.TimeStamp).ToList();
        }

        /// <summary>
        /// Delete selected backup
        /// </summary>
        /// <param name="backup">Backup to delete</param>
        public void DeleteBackup(Backup backup)
        {
            var backupFile = Path.Combine(Paths.BackupPath, $"{backup.Guid}.json");
            var backupFolder = Path.Combine(Paths.BackupPath, backup.Guid.ToString());
            if (File.Exists(backupFile))
            {
                File.Delete(backupFile);
            }
            if (Directory.Exists(backupFolder))
            {
                Directory.Delete(backupFolder, true);
            }
        }

        /// <summary>
        /// Backup a build file
        /// </summary>
        /// <param name="path">Path of file to backup</param>
        /// <param name="deleteFile">If file is being deleted</param>
        public void BackupBuildFile(string path, bool deleteFile = false)
        {
            if (CurrentBackup == null || string.IsNullOrEmpty(path))
            {
                return;
            }
            var backupPath = string.Empty;
            if (path.Contains(_settingsService.AppSettings.BuildPath) || (!string.IsNullOrEmpty(_settingsService.AppSettings.HDTextures) && path.Contains(_settingsService.AppSettings.HDTextures)))
            {
                if (path.Contains(_settingsService.AppSettings.BuildPath))
                {
                    backupPath = path.Replace(_settingsService.AppSettings.BuildPath, "");
                    backupPath = $"{CurrentBackup.BuildBackupPath}\\{backupPath}";
                }
                else if (!string.IsNullOrEmpty(_settingsService.AppSettings.HDTextures) && path.Contains(_settingsService.AppSettings.HDTextures))
                {
                    backupPath = path.Replace(_settingsService.AppSettings.HDTextures, "");
                    backupPath = $"{CurrentBackup.TextureBackupPath}\\{backupPath}";
                }
                if (!FileExists(backupPath) && FileExists(path) && !string.IsNullOrEmpty(backupPath))
                {
                    CreateDirectory(Path.GetDirectoryName(backupPath));
                    File.Copy(path, backupPath, true);
                }
                else if (!deleteFile && !FileExists(backupPath) && (path.Contains(_settingsService.AppSettings.BuildPath) || path.Contains(_settingsService.AppSettings.HDTextures)))
                {
                    if (!string.IsNullOrEmpty(_settingsService.AppSettings.BuildPath) && path.Contains(_settingsService.AppSettings.BuildPath))
                    {
                        CurrentBackup.AddedFiles.Add(path.Replace(_settingsService.AppSettings.BuildPath, ""));
                    }
                    else if (!string.IsNullOrEmpty(_settingsService.AppSettings.HDTextures) && path.Contains(_settingsService.AppSettings.HDTextures))
                    {
                        CurrentBackup.AddedFiles.Add(path.Replace(_settingsService.AppSettings.HDTextures, ""));
                    }
                }
            }
        }

        /// <summary>
        /// Backup a build folder
        /// </summary>
        /// <param name="path">Path of folder to backup</param>
        private void BackupBuildFolder(string path)
        {
            if (CurrentBackup == null)
            {
                return;
            }
            if (DirectoryExists(path) && (path.Contains(_settingsService.AppSettings.BuildPath) || (!string.IsNullOrEmpty(_settingsService.AppSettings.HDTextures) && path.Contains(_settingsService.AppSettings.HDTextures))))
            {
                var backupPath = string.Empty;
                var basePath = string.Empty;
                if (path.Contains(_settingsService.AppSettings.BuildPath))
                {
                    basePath = _settingsService.AppSettings.BuildPath;
                    backupPath = path.Replace(_settingsService.AppSettings.BuildPath, "");
                    backupPath = $"{CurrentBackup.BuildBackupPath}\\{backupPath}";
                }
                else if (!string.IsNullOrEmpty(_settingsService.AppSettings.HDTextures) && path.Contains(_settingsService.AppSettings.HDTextures))
                {
                    basePath = _settingsService.AppSettings.HDTextures;
                    backupPath = path.Replace(_settingsService.AppSettings.HDTextures, "");
                    backupPath = $"{CurrentBackup.TextureBackupPath}\\{backupPath}";
                }
                if (!string.IsNullOrEmpty(backupPath) && !string.IsNullOrEmpty(basePath))
                {
                    foreach (var file in GetFiles(path, "*", SearchOption.AllDirectories))
                    {
                        var newFilePath = file.Replace(basePath, "");
                        newFilePath = Path.Combine(backupPath, newFilePath);
                        if (!FileExists(newFilePath))
                        {
                            CreateDirectory(Path.GetDirectoryName(newFilePath));
                            File.Copy(path, newFilePath, true);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Load an XML file
        /// </summary>
        /// <param name="path">Path to XML file</param>
        /// <returns>XElement</returns>
        public XElement OpenXmlFile(string path)
        {
            var doc = XElement.Load(path);
            return doc;
        }

        /// <summary>
        /// Save an XML file
        /// </summary>
        /// <param name="xml">XML element to save</param>
        /// <param name="path">Path to save XMl to</param>
        public void SaveXmlFile(XElement xml, string path)
        {
            BackupBuildFile(path);
            if (!Directory.Exists(Path.GetDirectoryName(path)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(path));
            }
            xml.Save(path);
        }
    }
}
