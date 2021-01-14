﻿using System;
using System.IO;
using RIS.Cryptography.Hash;
using RIS.Cryptography.Hash.Methods;
using Environment = RIS.Environment;

namespace Memenim.Cryptography
{
    public static class HashManager
    {
        private static SHA512iCSP SHA512Provider { get; }

        public static HashService Service { get; }

        static HashManager()
        {
            SHA512Provider = new SHA512iCSP();
            Service = new HashService(SHA512Provider);
        }

        public static string GetLibrariesHash()
        {
            return Service.GetDirectoryHash(Environment.ExecAppDirectoryName,false, new[]
            {
                "storage",
                "logs",
                "hash",
                "downloads",
                "AppSettings.config",
                "PersistentSettings.store",
                "MEMENIM.exe",
                "MEMENIM.pdb",
                //dev
                "publish",
                "win-x64",
                "win-x86"
            });
        }

        public static string GetExeHash()
        {
            return Service.GetFileHash(Path.ChangeExtension(
                Environment.ExecProcessFilePath, "exe"));
        }

        public static string GetExePdbHash()
        {
            return Service.GetFileHash(Path.ChangeExtension(
                Environment.ExecProcessFilePath, "pdb"));
        }
    }
}
