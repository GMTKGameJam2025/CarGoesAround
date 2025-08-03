using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR
public class FixIconImportSettings
{
    [MenuItem("Tools/Fix Icon Import Settings")]
    public static void SetAllToSprite()
    {
        string iconPath = "Assets/_Project/Textures/Fixed_Icons";
        string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { iconPath });
        int fixedCount = 0;

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;

            if (importer == null) continue;

            bool dirty = false;

            if (importer.textureType != TextureImporterType.Sprite)
            {
                importer.textureType = TextureImporterType.Sprite;
                dirty = true;
            }

            if (importer.spriteImportMode != SpriteImportMode.Single)
            {
                importer.spriteImportMode = SpriteImportMode.Single;
                dirty = true;
            }

            if (dirty)
            {
                importer.SaveAndReimport();
                Debug.Log($"[FixSprite] Fixed: {path}");
                fixedCount++;
            }
        }

        Debug.Log($"[IconFix] Done. Updated {fixedCount} icons to Sprite.");
    }
}
#endif