namespace Ica.Utils.Editor
{
    public static class AssetUtils
    {
        public static GameObject FindAndInstantiateAsset(string name)
        {
            var paths = AssetDatabase.FindAssets(name);
            var asset = AssetDatabase.LoadMainAssetAtPath(AssetDatabase.GUIDToAssetPath(paths[0]));
            var obj = (GameObject)Object.Instantiate(asset);
            return obj;
        }
        
    }
}