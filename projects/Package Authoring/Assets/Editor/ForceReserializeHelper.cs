using UnityEditor;
using UnityEngine;

namespace Doji.PackageAuthoring.Utilities {
    public class ForceReserializeHelper : MonoBehaviour {
        [MenuItem("Tools/Force Reserialize Assets")]
        public static void ForceReserializeAssets() {
            AssetDatabase.ForceReserializeAssets();
        }
    }
}
