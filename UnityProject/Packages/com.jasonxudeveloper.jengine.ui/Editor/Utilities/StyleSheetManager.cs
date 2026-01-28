// StyleSheetManager.cs
//
//  Author:
//        JasonXuDeveloper <jason@xgamedev.net>
//
//  Copyright (c) 2025 JEngine
//
//  Permission is hereby granted, free of charge, to any person obtaining a copy
//  of this software and associated documentation files (the "Software"), to deal
//  in the Software without restriction, including without limitation the rights
//  to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//  copies of the Software, and to permit persons to whom the Software is
//  furnished to do so, subject to the following conditions:
//
//  The above copyright notice and this permission notice shall be included in
//  all copies or substantial portions of the Software.
//
//  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
//  THE SOFTWARE.

using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using PackageInfo = UnityEditor.PackageManager.PackageInfo;

namespace JEngine.UI.Editor.Utilities
{
    /// <summary>
    /// Manages loading and caching of USS stylesheets for JEngine UI components.
    /// </summary>
    public static class StyleSheetManager
    {
        private static StyleSheet _tokensStyleSheet;
        private static StyleSheet _baseStyleSheet;
        private static StyleSheet _componentsStyleSheet;
        private static string _packagePath;

        /// <summary>
        /// Gets the tokens stylesheet containing CSS variables.
        /// </summary>
        public static StyleSheet Tokens => _tokensStyleSheet ??= LoadStyleSheet("tokens.uss");

        /// <summary>
        /// Gets the base stylesheet with reset and common styles.
        /// </summary>
        public static StyleSheet Base => _baseStyleSheet ??= LoadStyleSheet("base.uss");

        /// <summary>
        /// Gets the components stylesheet with component-specific styles.
        /// </summary>
        public static StyleSheet Components => _componentsStyleSheet ??= LoadStyleSheet("components.uss");

        /// <summary>
        /// Applies all JEngine UI stylesheets to a visual element.
        /// </summary>
        /// <param name="element">The element to apply stylesheets to.</param>
        public static void ApplyAllStyleSheets(VisualElement element)
        {
            if (Tokens != null)
                element.styleSheets.Add(Tokens);
            if (Base != null)
                element.styleSheets.Add(Base);
            if (Components != null)
                element.styleSheets.Add(Components);
        }

        /// <summary>
        /// Gets the package asset path for this assembly.
        /// </summary>
        private static string GetPackagePath()
        {
            if (!string.IsNullOrEmpty(_packagePath))
                return _packagePath;

            var packageInfo = PackageInfo.FindForAssembly(Assembly.GetExecutingAssembly());
            if (packageInfo != null)
            {
                _packagePath = packageInfo.assetPath;
                return _packagePath;
            }

            Debug.LogWarning("[JEngine.UI.Editor] Could not find package path for assembly");
            return null;
        }

        /// <summary>
        /// Loads a stylesheet from the package Styles folder.
        /// </summary>
        /// <param name="fileName">The USS file name.</param>
        /// <returns>The loaded stylesheet, or null if not found.</returns>
        private static StyleSheet LoadStyleSheet(string fileName)
        {
            var packagePath = GetPackagePath();
            if (string.IsNullOrEmpty(packagePath))
                return null;

            var ussPath = $"{packagePath}/Editor/Styles/{fileName}";
            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(ussPath);

            if (styleSheet == null)
            {
                Debug.LogWarning($"[JEngine.UI.Editor] Could not load stylesheet: {ussPath}");
            }

            return styleSheet;
        }

        /// <summary>
        /// Clears the cached stylesheets. Useful for development/hot-reload.
        /// </summary>
        public static void ClearCache()
        {
            _tokensStyleSheet = null;
            _baseStyleSheet = null;
            _componentsStyleSheet = null;
            _packagePath = null;
        }
    }
}
