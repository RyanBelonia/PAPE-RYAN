using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using InteriorPlanner.Systems.Placement;
using InteriorPlanner.Systems.Tools; // ADICIONADO: Para aceder ao componente Paintable

public class PrefabAutoConfigurer : EditorWindow
{
    [MenuItem("Tools/Furniture/Auto Configure All Prefabs")]
    public static void ConfigurePrefabs()
    {
        string furnitureFolder = "Assets/Prefabs/Furniture";
        string janelasFolder = "Assets/Prefabs/Structural/Janelas";
        string portasFolder = "Assets/Prefabs/Structural/Portas";
        string divisoriasFolder = "Assets/Prefabs/Structural/Divisorias";

        int count = 0;
        count += ProcessFolder(furnitureFolder);
        count += ProcessFolder(janelasFolder);
        count += ProcessFolder(portasFolder);
        count += ProcessFolder(divisoriasFolder);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"<color=green>Sucesso!</color> {count} prefabs foram configurados automaticamente com base nas subpastas.");
    }

    private static int ProcessFolder(string folderPath)
    {
        string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { folderPath });
        if (guids.Length == 0) return 0;

        int count = 0;

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject instance = PrefabUtility.LoadPrefabContents(path);

            PlaceableObject placeable = instance.GetComponent<PlaceableObject>();
            if (placeable == null)
                placeable = instance.AddComponent<PlaceableObject>();

            MeshRenderer[] meshRenderers = instance.GetComponentsInChildren<MeshRenderer>(true);
            SkinnedMeshRenderer[] skinnedRenderers = instance.GetComponentsInChildren<SkinnedMeshRenderer>(true);

            List<Renderer> allRenderers = new List<Renderer>();
            allRenderers.AddRange(meshRenderers);
            allRenderers.AddRange(skinnedRenderers);

            bool isWindow = folderPath.Contains("Janelas");
            bool isDoor = folderPath.Contains("Portas");
            bool isDivisoria = folderPath.Contains("Divisorias");

            PlaceableObjectType type = PlaceableObjectType.Furniture; 
            bool canMove = true;
            bool canRotate = !isWindow && !isDoor; 
            bool canScale = isDivisoria; // Modifica isto mais tarde se quiseres outros móveis a esticar!
            bool requiresWallSupport = isWindow || isDoor;

            placeable.Configure(type, canMove, canRotate, canScale, requiresWallSupport, allRenderers.ToArray());

            // --- LÓGICA AUTOMÁTICA DA ETIQUETA DE PINTURA ---
            Paintable paintComponent = instance.GetComponent<Paintable>();
            if (isDivisoria)
            {
                // Se for da pasta Divisorias e não tiver o componente, adiciona-o
                if (paintComponent == null)
                    instance.AddComponent<Paintable>();
            }
            else
            {
                // Se estiver noutra pasta qualquer, remove para garantir que não se pinta por engano
                if (paintComponent != null)
                    DestroyImmediate(paintComponent, true);
            }
            // -------------------------------------------------

            int placeableLayer = LayerMask.NameToLayer("Placeable");
            if (placeableLayer != -1)
            {
                instance.layer = placeableLayer;
                foreach (Transform child in instance.GetComponentsInChildren<Transform>(true))
                {
                    child.gameObject.layer = placeableLayer;
                }
            }

            BoxCollider boxCollider = instance.GetComponent<BoxCollider>();
            if (boxCollider == null)
                boxCollider = instance.AddComponent<BoxCollider>();

            if (allRenderers.Count > 0)
            {
                Bounds combinedBounds = new Bounds(Vector3.zero, Vector3.zero);
                bool hasBounds = false;

                foreach (Renderer rend in allRenderers)
                {
                    if (rend != null)
                    {
                        if (!hasBounds)
                        {
                            combinedBounds = rend.bounds;
                            hasBounds = true;
                        }
                        else
                        {
                            combinedBounds.Encapsulate(rend.bounds);
                        }
                    }
                }

                if (hasBounds)
                {
                    boxCollider.center = instance.transform.InverseTransformPoint(combinedBounds.center);
                    boxCollider.size = instance.transform.InverseTransformVector(combinedBounds.size);
                }
            }

            PrefabUtility.SaveAsPrefabAsset(instance, path);
            PrefabUtility.UnloadPrefabContents(instance);
            count++;
        }

        return count;
    }
}