﻿using HybridCLR.Editor.ABI;
using HybridCLR.Editor.Template;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine;

namespace HybridCLR.Editor.Il2CppDef
{
    public class Il2CppDefGenerator
    {
        public class Options
        {
            public List<string> HotUpdateAssemblies { get; set; }

            public string UnityVersionTemplateFile { get; set; }

            public string UnityVersionOutputFile { get; set; }

            public string AssemblyManifestTemplateFile { get; set; }

            public string AssemblyManifestOutputFile { get; set; }

            public string UnityVersion { get; set; }
        }

        private readonly Options _options;
        public Il2CppDefGenerator(Options options)
        {
            _options = options;
        }


        private static readonly Regex s_unityVersionPat = new Regex(@"(\d+)\.(\d+)\.(\d+)");

        public void Generate()
        {
            GenerateIl2CppConfig();
            GeneratePlaceHolderAssemblies();
        }

        private void GenerateIl2CppConfig()
        {
            var frr = new FileRegionReplace(File.ReadAllText(_options.UnityVersionTemplateFile));

            List<string> lines = new List<string>();

            var match = s_unityVersionPat.Matches(_options.UnityVersion)[0];
            int majorVer = int.Parse(match.Groups[1].Value);
            int minorVer1 = int.Parse(match.Groups[2].Value);
            int minorVer2 = int.Parse(match.Groups[3].Value);

            lines.Add($"#define HYBRIDCLR_UNITY_VERSION {majorVer}{minorVer1.ToString("D2")}{minorVer2.ToString("D2")}");
            lines.Add($"#define HYBRIDCLR_UNITY_{majorVer} 1");
            for (int ver = 2019; ver <= 2023; ver++)
            {
                if (majorVer >= ver)
                {
                    lines.Add($"#define HYBRIDCLR_UNITY_{ver}_OR_NEW 1");
                }
            }
            for (int ver = 6000; ver <= 6100; ver++)
            {
                if (majorVer >= ver)
                {
                    lines.Add($"#define HYBRIDCLR_UNITY_{ver}_OR_NEW 1");
                }
            }

#if TUANJIE_1_1_OR_NEWER
            var tuanjieMatch = Regex.Matches(Application.tuanjieVersion, @"(\d+)\.(\d+)\.(\d+)");
            int tuanjieMajorVer = int.Parse(tuanjieMatch[0].Groups[1].Value);
            int tuanjieMinorVer1 = int.Parse(tuanjieMatch[0].Groups[2].Value);
            int tuanjieMinorVer2 = int.Parse(tuanjieMatch[0].Groups[3].Value);
            lines.Add($"#define HYBRIDCLR_TUANJIE_VERSION {tuanjieMajorVer}{tuanjieMinorVer1.ToString("D2")}{tuanjieMinorVer2.ToString("D2")}");
#elif TUANJIE_2022_3_OR_NEWER
            lines.Add($"#define HYBRIDCLR_TUANJIE_VERSION 10000");
#endif

            frr.Replace("UNITY_VERSION", string.Join("\n", lines));

            frr.Commit(_options.UnityVersionOutputFile);
            Debug.Log($"[HybridCLR.Editor.Il2CppDef.Generator] output:{_options.UnityVersionOutputFile}");
        }

        private void GeneratePlaceHolderAssemblies()
        {
            var frr = new FileRegionReplace(File.ReadAllText(_options.AssemblyManifestTemplateFile));

            List<string> lines = new List<string>();

            foreach (var ass in _options.HotUpdateAssemblies)
            {
                lines.Add($"\t\t\"{ass}\",");
            }

            frr.Replace("PLACE_HOLDER", string.Join("\n", lines));

            frr.Commit(_options.AssemblyManifestOutputFile);
            Debug.Log($"[HybridCLR.Editor.Il2CppDef.Generator] output:{_options.AssemblyManifestOutputFile}");
        }
    }
}
