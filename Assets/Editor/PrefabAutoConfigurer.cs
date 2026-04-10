using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using InteriorPlanner.Systems.Placement;

public class PrefabAutoConfigurer : EditorWindow
{
    [MenuItem("Tools/Furniture/Auto Configure All Prefabs")]
    public static void ConfigurePrefabs()
    {
        string folderPath = "Assets/Prefabs/Furniture";
        string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { folderPath });

        if (guids.Length == 0)
        {
            Debug.LogWarning("Nenhum prefab encontrado na pasta: " + folderPath);
            return;
        }

        int count = 0;

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject instance = PrefabUtility.LoadPrefabContents(path);

            // 1. Garantir o componente PlaceableObject
            PlaceableObject placeable = instance.GetComponent<PlaceableObject>();
            if (placeable == null)
                placeable = instance.AddComponent<PlaceableObject>();

            // 2. Coletar todos os Renderers para o Highlight e para calcular o tamanho
            MeshRenderer[] meshRenderers = instance.GetComponentsInChildren<MeshRenderer>(true);
            SkinnedMeshRenderer[] skinnedRenderers = instance.GetComponentsInChildren<SkinnedMeshRenderer>(true);

            List<Renderer> allRenderers = new List<Renderer>();
            allRenderers.AddRange(meshRenderers);
            allRenderers.AddRange(skinnedRenderers);

            // 3. Configurar os dados do PlaceableObject
            placeable.Configure(
                PlaceableObjectType.Furniture,
                true,  // canMove
                true,  // canRotate
                false, // canScale
                false, // requiresWallSupport
                allRenderers.ToArray()
            );

            // 4. Lógica Inteligente de Collider (Ajuste de tamanho e centro)
            BoxCollider boxCollider = instance.GetComponent<BoxCollider>();
            if (boxCollider == null)
                boxCollider = instance.AddComponent<BoxCollider>();

            if (allRenderers.Count > 0)
            {
                // Criamos um "encapsulamento" vazio
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
                    // Convertemos a posição global do cálculo para a posição local do Prefab
                    // Isso evita que o collider fique no "chão" se o móvel for alto
                    boxCollider.center = instance.transform.InverseTransformPoint(combinedBounds.center);
                    boxCollider.size = instance.transform.InverseTransformVector(combinedBounds.size);
                }
            }

            // 5. Salvar as alterações e fechar a instância temporária
            PrefabUtility.SaveAsPrefabAsset(instance, path);
            PrefabUtility.UnloadPrefabContents(instance);

            count++;
        }

        // Finalizar processo na Unity
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"<color=green>Sucesso!</color> {count} prefabs foram reconfigurados com Colliders ajustados.");
    }
}