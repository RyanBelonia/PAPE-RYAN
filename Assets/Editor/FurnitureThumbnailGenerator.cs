using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class FurnitureThumbnailGenerator : EditorWindow
{
    private string prefabsRootFolder = "Assets/Prefabs/Furniture";
    private string outputRootFolder = "Assets/Art/Icons/Furniture";

    private Queue<Object> pendingQueue = new Queue<Object>();
    private Dictionary<Object, string> assetOutputPaths = new Dictionary<Object, string>();

    [MenuItem("Tools/Furniture/Generate Thumbnails")]
    public static void OpenWindow()
    {
        GetWindow<FurnitureThumbnailGenerator>("Furniture Thumbnails");
    }

    private void OnGUI()
    {
        GUILayout.Label("Gerador SEGURO de Thumbnails", EditorStyles.boldLabel);

        prefabsRootFolder = EditorGUILayout.TextField("Pasta raiz dos Prefabs", prefabsRootFolder);
        outputRootFolder = EditorGUILayout.TextField("Pasta raiz de saída", outputRootFolder);

        GUILayout.Space(10);

        if (GUILayout.Button("Gerar Thumbnails"))
        {
            PrepareThumbnails();
        }
    }

    private void PrepareThumbnails()
    {
        if (!AssetDatabase.IsValidFolder(prefabsRootFolder))
        {
            Debug.LogError("Pasta de prefabs inválida: " + prefabsRootFolder);
            return;
        }

        if (!AssetDatabase.IsValidFolder(outputRootFolder))
        {
            CreateFoldersRecursively(outputRootFolder);
        }

        string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab", new[] { prefabsRootFolder });

        pendingQueue.Clear();
        assetOutputPaths.Clear();

        foreach (string guid in prefabGuids)
        {
            string prefabPath = AssetDatabase.GUIDToAssetPath(guid);
            Object asset = AssetDatabase.LoadAssetAtPath<Object>(prefabPath);

            if (asset == null)
                continue;

            string subfolder = GetRelativeSubfolder(prefabPath, prefabsRootFolder);
            string outputFolder = string.IsNullOrEmpty(subfolder)
                ? outputRootFolder
                : $"{outputRootFolder}/{subfolder}";

            if (!AssetDatabase.IsValidFolder(outputFolder))
            {
                CreateFoldersRecursively(outputFolder);
            }

            string filePath = $"{outputFolder}/{asset.name}.png";

            if (!File.Exists(filePath))
            {
                pendingQueue.Enqueue(asset);
                assetOutputPaths[asset] = filePath;
            }
        }

        if (pendingQueue.Count == 0)
        {
            Debug.Log("Todos os ícones já existem.");
            return;
        }

        EditorApplication.update -= ProcessQueue;
        EditorApplication.update += ProcessQueue;

        Debug.Log($"Iniciando geração de {pendingQueue.Count} ícones...");
    }

    private void ProcessQueue()
    {
        if (pendingQueue.Count == 0)
        {
            EditorApplication.update -= ProcessQueue;
            AssetDatabase.Refresh();
            Debug.Log("Geração finalizada.");
            return;
        }

        Object asset = pendingQueue.Peek();

        if (asset == null)
        {
            pendingQueue.Dequeue();
            return;
        }

        if (AssetPreview.IsLoadingAssetPreview(asset.GetInstanceID()))
        {
            return;
        }

        Texture2D preview = AssetPreview.GetAssetPreview(asset);

        if (preview == null)
        {
            AssetPreview.GetAssetPreview(asset);
            return;
        }

        string outputPath = assetOutputPaths[asset];

        Texture2D readableCopy = CopyToReadableTexture(preview);
        if (readableCopy != null)
        {
            byte[] pngData = readableCopy.EncodeToPNG();
            File.WriteAllBytes(outputPath, pngData);
            DestroyImmediate(readableCopy);

            Debug.Log($"Ícone criado para: {asset.name} ({pendingQueue.Count - 1} restantes)");
        }

        pendingQueue.Dequeue();

        EditorUtility.UnloadUnusedAssetsImmediate();
        System.GC.Collect();
    }

    private Texture2D CopyToReadableTexture(Texture2D source)
    {
        RenderTexture rt = RenderTexture.GetTemporary(source.width, source.height, 0, RenderTextureFormat.ARGB32);
        RenderTexture previous = RenderTexture.active;

        Graphics.Blit(source, rt);
        RenderTexture.active = rt;

        Texture2D copy = new Texture2D(source.width, source.height, TextureFormat.RGBA32, false);
        copy.ReadPixels(new Rect(0, 0, source.width, source.height), 0, 0);
        copy.Apply();

        RenderTexture.active = previous;
        RenderTexture.ReleaseTemporary(rt);

        return copy;
    }

    private string GetRelativeSubfolder(string assetPath, string rootFolder)
    {
        string assetDirectory = Path.GetDirectoryName(assetPath)?.Replace("\\", "/") ?? "";
        rootFolder = rootFolder.Replace("\\", "/");

        if (assetDirectory.StartsWith(rootFolder))
            return assetDirectory.Substring(rootFolder.Length).TrimStart('/');

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