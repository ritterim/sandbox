using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Threading;

namespace Sandbox
{
    public class Sandbox : Expando, IDisposable
    {
        public Sandbox()
        {
            Id = Path.GetRandomFileName();
            Description = string.Format("Sandbox <{0}>", Id);

            Location = Path.Combine(Path.GetTempPath(), string.Format("sandbox_{0}", Id));
            Principal = "Everyone";
        }

        public string Id { get; protected set; }

        public Sandbox SetPermissionPrinicipal(string user)
        {
            Principal = user;
            return this;
        }

        public string Principal { get; protected set; }
        public string Description { get; protected set; }
        public string Location { get; protected set; }

        public Sandbox Play()
        {
            CreateTemporaryDirectory();
            Grains.ToList().ForEach(x => x.Setup(this));

            return this;
        }

        private void CreateTemporaryDirectory()
        {
            if (!Directory.Exists(Location))
                Directory.CreateDirectory(Location);
        }

        public void Dispose()
        {
            Grains.ToList().ForEach(x => x.Dispose());
            TryDeleteFilesAndFoldersRecursively(Location);
        }

        public static void TryDeleteFilesAndFoldersRecursively(string directory)
        {
            for (var count = 0; count < 3; count++)
            {
                try
                {
                    Directory.Delete(directory, true);
                    return;
                }
                catch (IOException)
                {
                    count++;
                    Thread.Sleep(1);
                }
                catch (UnauthorizedAccessException)
                {
                    // shrug ?
                }
            }
        }

        public IList<IGrain> Grains
        {
            get
            {
                 return Properties
                    .Select(x => x.Value)
                    .Where(x => x is IGrain)
                    .Cast<IGrain>()
                    .ToList();
            }
        }

        public void SetDirectoryPermissions(string path, string user = null)
        {
            SetDirectoryPermissions(new DirectoryInfo(path), user);
        }

        public void SetDirectoryPermissions(DirectoryInfo directory, string user = null)
        {
            var security = directory.GetAccessControl();
            security.AddAccessRule(
            new FileSystemAccessRule(user ?? Principal,
                FileSystemRights.FullControl | FileSystemRights.Read | FileSystemRights.ReadAndExecute | FileSystemRights.Modify,
                InheritanceFlags.ObjectInherit | InheritanceFlags.ContainerInherit, PropagationFlags.InheritOnly,
                AccessControlType.Allow));
            directory.SetAccessControl(security);
        }

        public void CopyAll(DirectoryInfo source, DirectoryInfo target)
        {
            // Check if the target directory exists; if not, create it.
            if (!Directory.Exists(target.FullName))
            {
                Directory.CreateDirectory(target.FullName);
            }

            // Copy each file into the new directory.
            foreach (var fi in source.GetFiles())
            {
                Trace.WriteLine(string.Format(@"Copying {0}\{1}", target.FullName, fi.Name));
                fi.CopyTo(Path.Combine(target.FullName, fi.Name), true);
            }

            // Copy each subdirectory using recursion.
            foreach (var subDirectory in source.GetDirectories())
            {
                DirectoryInfo nextTargetSubDir =
                    target.CreateSubdirectory(subDirectory.Name);
                CopyAll(subDirectory, nextTargetSubDir);
            }
        }

        public Sandbox Add<TGrain>(TGrain grain)
           where TGrain : IGrain
        {
            if (!Exists(grain.Name, true))
            {
                this[grain.Name] = grain;
            }

            return this;
        }

        public bool Exists(string grainName, bool throwOnExists = false)
        {
            var exists = Properties.ContainsKey(grainName);

            if (throwOnExists && exists)
                throw new ArgumentException(string.Format("Grain with name \"{0}\" already exists in sandbox, please use another.", grainName), "grainName");

            return exists;
        }

        public TGrain Get<TGrain>(string name)
            where TGrain : IGrain
        {
            if (Exists(name))
            {
                return (TGrain)this[name];
            }

            return default(TGrain);
        }

        public string CreateGrainDirectory(IGrain grain)
        {
            var grainFolder = Path.Combine(Location, grain.Name);
            Directory.CreateDirectory(grainFolder);
            SetDirectoryPermissions(grainFolder);

            return grainFolder;
        }
    }
}
