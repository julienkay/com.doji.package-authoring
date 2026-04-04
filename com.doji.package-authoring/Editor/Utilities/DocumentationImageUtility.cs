using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Doji.PackageAuthoring.Utilities {
    /// <summary>
    /// Generates documentation image outputs from an optional source texture asset.
    /// </summary>
    internal static class DocumentationImageUtility {
        private const string LogoFileName = "logo.png";
        private const string FaviconFileName = "favicon.ico";

        private static readonly List<int> IcoSizes = new() {
            128, 64, 32, 16
        };

        /// <summary>
        /// Writes the documentation image outputs for whichever source textures are available.
        /// </summary>
        public static bool TryWriteDocumentationImages(
            Texture2D logoTexture,
            Texture2D faviconTexture,
            string imagesDirectoryPath) {
            if (string.IsNullOrWhiteSpace(imagesDirectoryPath) || (logoTexture == null && faviconTexture == null)) {
                return false;
            }

            Directory.CreateDirectory(imagesDirectoryPath);

            bool wroteAnyFile = false;
            bool succeeded = true;
            if (logoTexture != null) {
                wroteAnyFile = true;
                succeeded &= TryWriteLogoImage(logoTexture, imagesDirectoryPath);
            }

            if (faviconTexture != null) {
                wroteAnyFile = true;
                succeeded &= TryWriteFaviconImage(faviconTexture, imagesDirectoryPath);
            }

            return wroteAnyFile && succeeded;
        }

        private static bool TryWriteLogoImage(Texture2D sourceTexture, string imagesDirectoryPath) {
            Texture2D readableCopy = GetReadableCopy(sourceTexture);
            if (readableCopy == null) {
                return false;
            }

            try {
                string logoPath = Path.Combine(imagesDirectoryPath, LogoFileName);
                byte[] logoBytes = readableCopy.EncodeToPNG();
                if (logoBytes == null || logoBytes.Length == 0) {
                    return false;
                }

                File.WriteAllBytes(logoPath, logoBytes);
                return true;
            }
            finally {
                UnityEngine.Object.DestroyImmediate(readableCopy);
            }
        }

        private static bool TryWriteFaviconImage(Texture2D sourceTexture, string imagesDirectoryPath) {
            Texture2D readableCopy = GetReadableCopy(sourceTexture);
            if (readableCopy == null) {
                return false;
            }

            try {
                string faviconPath = Path.Combine(imagesDirectoryPath, FaviconFileName);
                return CreateIcoFile(readableCopy, IcoSizes, faviconPath);
            }
            finally {
                UnityEngine.Object.DestroyImmediate(readableCopy);
            }
        }

        private static bool CreateIcoFile(Texture2D largestTexture, IReadOnlyList<int> resolutions, string filePath) {
            if (largestTexture == null) {
                throw new ArgumentNullException(nameof(largestTexture));
            }

            if (resolutions == null) {
                throw new ArgumentNullException(nameof(resolutions));
            }

            if (resolutions.Any(size => size > 256)) {
                throw new ArgumentException("An image can not be larger than 256 x 256 (.ico max size).", nameof(resolutions));
            }

            List<Texture2D> icons = new(resolutions.Count);
            try {
                for (int i = 0; i < resolutions.Count; i++) {
                    Texture2D icon = CPUCopy(largestTexture);
                    int size = resolutions[i];
                    if (icon.width != size || icon.height != size) {
                        CPUResize(icon, size, size);
                    }

                    icons.Add(icon);
                }

                using FileStream outputStream = new(filePath, FileMode.Create, FileAccess.Write);
                using BinaryWriter iconWriter = new(outputStream);

                List<byte[]> pngImageData = new(icons.Count);
                for (int i = 0; i < icons.Count; i++) {
                    pngImageData.Add(icons[i].EncodeToPNG());
                }

                int offset = 6 + (16 * resolutions.Count);

                iconWriter.Write((short)0);
                iconWriter.Write((short)1);
                iconWriter.Write((short)resolutions.Count);

                for (int i = 0; i < resolutions.Count; i++) {
                    int size = resolutions[i];
                    iconWriter.Write((byte)(size == 256 ? 0 : size));
                    iconWriter.Write((byte)(size == 256 ? 0 : size));
                    iconWriter.Write((byte)0);
                    iconWriter.Write((byte)0);
                    iconWriter.Write((short)0);
                    iconWriter.Write((short)32);
                    iconWriter.Write(pngImageData[i].Length);
                    iconWriter.Write(offset);
                    offset += pngImageData[i].Length;
                }

                for (int i = 0; i < pngImageData.Count; i++) {
                    iconWriter.Write(pngImageData[i]);
                }
            }
            finally {
                foreach (Texture2D icon in icons) {
                    if (icon != null) {
                        UnityEngine.Object.DestroyImmediate(icon);
                    }
                }
            }

            return true;
        }

        private static Texture2D CPUCopy(Texture2D texture) {
            Texture2D copiedTexture = new(texture.width, texture.height, TextureFormat.RGBA32, false);
            copiedTexture.SetPixels32(texture.GetPixels32());
            copiedTexture.Apply(false, false);
            return copiedTexture;
        }

        private static void CPUResize(Texture2D texture, int width, int height) {
            Texture2D originalTexture = CPUCopy(texture);
            try {
                float ratioX = 1f / width;
                float ratioY = 1f / height;

#if UNITY_2021_2_OR_NEWER
                texture.Reinitialize(width, height, texture.format, false);
#else
                texture.Resize(width, height, texture.format, false);
#endif

                for (int y = 0; y < height; y++) {
                    for (int x = 0; x < width; x++) {
                        float u = x * ratioX;
                        float v = y * ratioY;
                        Color color = originalTexture.GetPixelBilinear(u, v);
                        texture.SetPixel(x, y, color);
                    }
                }

                texture.Apply(false, false);
            }
            finally {
                UnityEngine.Object.DestroyImmediate(originalTexture);
            }
        }

        private static Texture2D GetReadableCopy(Texture2D source) {
            if (source == null) {
                return null;
            }

            string assetPath = AssetDatabase.GetAssetPath(source);
            if (string.IsNullOrWhiteSpace(assetPath) || !File.Exists(assetPath)) {
                Debug.LogWarning("Documentation image generation requires a texture asset with a valid project path.");
                return null;
            }

            Texture2D readableCopy = new(2, 2, TextureFormat.RGBA32, false);
            byte[] imageData = File.ReadAllBytes(assetPath);
            if (readableCopy.LoadImage(imageData, markNonReadable: false)) {
#if UNITY_2022_1_OR_NEWER
                readableCopy.ignoreMipmapLimit = true;
#endif
                return readableCopy;
            }

            Object.DestroyImmediate(readableCopy);
            Debug.LogWarning($"Failed to load documentation image data from '{assetPath}'.");
            return null;
        }
    }
}
