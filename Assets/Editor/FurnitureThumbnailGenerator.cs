using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class FurnitureThumbnailGenerator : EditorWindow
{
    private string prefabsRootFolder = "Assets/Prefabs/Furniture";
    private string outputRootFolder = "Assets/Art/Icons/Furniture";

    private List<Object> pendingAssets;
    private Dictionary<Object, string> assetOutputPaths;
    private EditorApplication.CallbackFunction updateAction;

    [MenuItem("Tools/Furniture/Generate Thumbnails")]
    public static void OpenWindow()
    {
        GetWindow<FurnitureThumbnailGenerator>("Furniture Thumbnails");
    }

    private void OnGUI()
    {
        GUILayout.Label("Gerador Automático de Thumbnails", EditorStyles.boldLabel);

        prefabsRootFolder = EditorGUILayout.TextField("Pasta raiz dos Prefabs", prefabsRootFolder);
        outputRootFolder = EditorGUILayout.TextField("Pasta raiz de saída", outputRootFolder);

        GUILayout.Space(10);

        if (GUILayout.Button("Gerar Thumbnails"))
        {
            GenerateThumbnails();
        }
    }

    private void GenerateThumbnails()
    {
        if (!AssetDatabase.IsValidFolder(prefabsRootFolder))
        {
            Debug.LogError("A pasta de prefabs não existe: " + prefabsRootFolder);
            return;
        }

        if (!AssetDatabase.IsValidFolder(outputRootFolder))
        {
            CreateFoldersRecursively(outputRootFolder);
        }

        string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab", new[] { prefabsRootFolder });

        if (prefabGuids.Length == 0)
        {
            Debug.LogWarning("Nenhum prefab encontrado em: " + prefabsRootFolder);
            return;
        }

        pendingAssets = new List<Object>();
        assetOutputPaths = new Dictionary<Object, string>();

        foreach (string guid in prefabGuids)
        {
            string prefabPath = AssetDatabase.GUIDToAssetPath(guid);
            Object asset = AssetDatabase.LoadAssetAtPath<Object>(prefabPath);

            if (asset == null)
                continue;

            string relativeFolder = GetRelativeSubfolder(prefabPath, prefabsRootFolder);
            string outputFolder = string.IsNullOrEmpty(relativeFolder)
                ? outputRootFolder
                : $"{outputRootFolder}/{relativeFolder}";

            if (!AssetDatabase.IsValidFolder(outputFolder))
            {
                CreateFoldersRecursively(outputFolder);
            }

            string outputFilePath = $"{outputFolder}/{asset.name}.png";

            pendingAssets.Add(asset);
            assetOutputPaths[asset] = outputFilePath;
        }

        updateAction = ProcessPreviews;
        EditorApplication.update += updateAction;

        Debug.Log("A gerar thumbnails...");
    }

    private void ProcessPreviews()
    {
        bool stillLoading = false;
        bool createdAny = false;

        foreach (Object asset in pendingAssets)
        {
            if (asset == null)
                continue;

            Texture2D preview = AssetPreview.GetAssetPreview(asset);

            if (preview == null)
            {
                if (AssetPreview.IsLoadingAssetPreview(asset.GetInstanceID()))
                {
                    stillLoading = true;
                }
                continue;
            }

            if (!assetOutputPaths.TryGetValue(asset, out string filePath))
                continue;

            if (!File.Exists(filePath))
            {
                byte[] pngData = preview.EncodeToPNG();
                File.WriteAllBytes(filePath, pngData);
                createdAny = true;
                Debug.Log("Thumbnail criada: " + filePath);
            }
        }

        if (!stillLoading)
        {
            if (updateAction != null)
            {
                EditorApplication.update -= updateAction;
                updateAction = null;
            }

            if (createdAny)
            {
                AssetDatabase.Refresh();
            }

            Debug.Log("Geração de thumbnails terminada.");
        }
    }

    private string GetRelativeSubfolder(string assetPath, string rootFolder)
    {
        string assetDirectory = Path.GetDirectoryName(assetPath)?.Replace("\\", "/") ?? "";
        rootFolder = rootFolder.Replace("\\", "/");

        if (assetDirectory.StartsWith(rootFolder))
        {
            string relative = assetDirectory.Substring(rootFolder.Length).TrimStart('/');
            return relative;
        }

        return string.Empty;
    }

    private void CreateFoldersRecursively(string folderPath)
    {
        string[] parts = folderPath.Split('/');
        string current = parts[0];

        for (int i = 1; i < parts.Length; i++)
        {
            string next = current + "/" + parts[i];
            if (!AssetDatabase.IsValidFolder(next))
            {
                AssetDatabase.CreateFolder(current, parts[i]);
            }
            current = next;
        }
    }
}