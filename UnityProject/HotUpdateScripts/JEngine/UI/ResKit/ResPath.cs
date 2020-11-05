//
// ResPath.cs
//
// Author:
//       L-Fone <275757115@qq.com>
//
// Copyright (c) 2020 JEngine
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
using System;
using System.Collections.Generic;
using JEngine.Core;
using UnityEngine;
using UnityEngine.UI;

namespace JEngine.UI.ResKit
{
    public enum ConfigType
    {
        Camera = 0,
        Map,
        Level,
        Table,
        Xml,
    }

    /// <summary>
    /// 加载类型
    /// </summary>
    public enum ResType
    {
        Battle,
        City,
        UI,
        Other,        
    }

    /// <summary>
    /// 加载的类型
    /// </summary>
    public enum AssetType
    {
        Custom = 0,    //自定义类型和路径
        Anim,          //.anim文件
        Controller,    //.controller文件
        Mat,           //.matrail文件
        TTF,           //.ttf字体文件
        Shader,        //.shader文件
        Prefab,        //.prefab文件        
        Scene,         //.unity场景
        Asset,         //.asset文件
        Txt,           //.txt文件
        Json,          //.json文件
        Bytes,         //.bytes文件
        Etc,           //.etc文件
        UI,            // UI图片

        UIPrefab,      //ui路径
        EntityPrefab,  //实体预置
        ScenePrefab,   //场景预置
    }

    /// <summary>
    /// 自定义的子集路径
    /// </summary>
    public enum CustomPath
    {
        None,
        Hero,
        PVE,
    }


    public class ResPath: Singleton<ResPath>
    {
        private const string ResRoot = "Assets/HotUpdateResources/";
        //根目录，
        private const string Controller = "Controller/";
        private const string Material = "Material/";
        private const string Other = "Other/";
        private const string Prefab = "Prefab/";
        private const string Scene = "Scene/";
        private const string ScriptableObject = "ScriptableObject/";
        private const string TextAsset = "TextAsset/";
        private const string UI = "UI/";

        //自定义路径
        private const string UIPrefab = "Prefab/ui/";
        private const string ScenePrefab = "Prefab/pve/";
        private const string EntityPrefab = "Prefab/hero/";


        /// <summary>
        /// 如果是自定义类型，需要填完整路径
        /// </summary>
        /// <param name="path">资源名字</param>
        /// <param name="type">要加载的类型</param>
        /// <returns></returns>
        public string GetPath(string name, AssetType type)
        {
            return GetResDir(type) + name + GetResSuffix(type);
        }

        private static string GetResDir(AssetType type)
        {
            switch (type)
            {
                //根目录
                case AssetType.Anim:        return ResRoot + Controller;
                case AssetType.Controller:  return ResRoot + Controller;
                case AssetType.Mat:         return ResRoot + Material;
                case AssetType.TTF:         return ResRoot + Other;
                case AssetType.Shader:      return ResRoot + Other;
                case AssetType.Prefab:      return ResRoot + Prefab;
                case AssetType.Scene:       return ResRoot + Scene;
                case AssetType.Asset:       return ResRoot + ScriptableObject;
                case AssetType.Txt:         return ResRoot + TextAsset;
                case AssetType.Json:        return ResRoot + TextAsset;
                case AssetType.Bytes:       return ResRoot + TextAsset;
                case AssetType.Etc:         return ResRoot + TextAsset;
                case AssetType.UI:          return ResRoot + UI;

                case AssetType.UIPrefab:      return ResRoot + UIPrefab;
                case AssetType.EntityPrefab:  return ResRoot + EntityPrefab;
                case AssetType.ScenePrefab:   return ResRoot + ScenePrefab;

                default: return "";
            }
        }

        private static string GetResSuffix(AssetType type)
        {
            switch (type)
            {
                case AssetType.Anim:         return ".anim";
                case AssetType.Controller:   return ".controller";
                case AssetType.Mat:          return ".mat";
                case AssetType.TTF:          return ".ttf";
                case AssetType.Shader:       return ".shader";
                case AssetType.Prefab:       return ".prefab";
                case AssetType.Scene:        return ".unity";
                case AssetType.Asset:        return ".asset";
                case AssetType.Txt:          return ".txt";
                case AssetType.Json:         return ".json";
                case AssetType.Bytes:        return ".bytes";
                case AssetType.Etc:          return ".etc";
                case AssetType.UI:           return ".png";

                case AssetType.UIPrefab: 
                case AssetType.EntityPrefab: 
                case AssetType.ScenePrefab:
                    return ".prefab";

                default: return "";
            }
        }


        public string TableConfig = "config/table/";

        public string GetTableConfig(string name) { return TableConfig + name; }

        public string GetResConfig(string name, ConfigType type)
        {
            if (type.Equals(ConfigType.Table))
                return GetTableConfig(name);
            return "";
        }

        public string GetPersistentDir(string dir)
        {
            return Application.persistentDataPath + "/" + dir;
        }


        public string GetConfig(string name, ConfigType type)
        {
            string curPath = "";
            if (type.Equals(ConfigType.Camera))
                curPath = string.Format("{0}.xml", GetResConfig(name, type));
            else if (type.Equals(ConfigType.Map))
                curPath = string.Format("{0}.xml", GetResConfig(name, type));
            else if (type.Equals(ConfigType.Level))
                curPath = string.Format("{0}.xml", GetResConfig(name, type));
            else if (type.Equals(ConfigType.Table))
                curPath = string.Format("{0}.txt", GetResConfig(name, type));
            else if (type.Equals(ConfigType.Xml))
                curPath = string.Format("{0}.xml", GetResConfig(name, type));
            return curPath;
        }
    }
}
