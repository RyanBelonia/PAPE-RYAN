using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using InteriorPlanner.Systems.Placement;

public class PrefabAutoConfigurer : EditorWindow
{
    [MenuItem("Tools/Furniture/Auto Configure All Prefabs")]
    public static void ConfigurePrefabs()
    {
        // 1. Caminhos exatos das tuas pastas do projeto
        string furnitureFolder = "Assets/Prefabs/Furniture";
        string janelasFolder = "Assets/Prefabs/Structural/Janelas";
        string portasFolder = "Assets/Prefabs/Structural/Portas";
        string divisoriasFolder = "Assets/Prefabs/Structural/Divisorias";

        int count = 0;

        // Processar cada pasta APENAS com o caminho. O script descobre o resto sozinho!
        count += ProcessFolder(furnitureFolder);
        count += ProcessFolder(janelasFolder);
        count += ProcessFolder(portasFolder);
        count += ProcessFolder(divisoriasFolder);

        // Finalizar e atualizar a base de dados da Unity
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"<color=green>Sucesso!</color> {count} prefabs foram configurados automaticamente com base nas subpastas.");
    }

    // A função agora só pede a pasta. Acabaram-se os booleanos confusos!
    private static int ProcessFolder(string folderPath)
    {
        string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { folderPath });
        if (guids.Length == 0) return 0;

        int count = 0;

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject instance = PrefabUtility.LoadPrefabContents(path);

            // Garantir o componente PlaceableObject
            PlaceableObject placeable = instance.GetComponent<PlaceableObject>();
            if (placeable == null)
                placeable = instance.AddComponent<PlaceableObject>();

            // Coletar todos os Renderers
            MeshRenderer[] meshRenderers = instance.GetComponentsInChildren<MeshRenderer>(true);
            SkinnedMeshRenderer[] skinnedRenderers = instance.GetComponentsInChildren<SkinnedMeshRenderer>(true);

            List<Renderer> allRenderers = new List<Renderer>();
            allRenderers.AddRange(meshRenderers);
            allRenderers.AddRange(skinnedRenderers);

            // --- LÓGICA INTELIGENTE BASEADA NO NOME DA PASTA ---
            bool isWindow = folderPath.Contains("Janelas");
            bool isDoor = folderPath.Contains("Portas");
            bool isDivisoria = folderPath.Contains("Divisorias");

            PlaceableObjectType type = PlaceableObjectType.Furniture; 
            bool canMove = true;
            
            // Janelas e Portas não rodam sozinhas, Móveis e Divisórias rodam
            bool canRotate = !isWindow && !isDoor; 
            
            // SÓ as Divisórias é que podem ser esticadas com o rato
            bool canScale = isDivisoria; 
            
            // Janelas e Portas precisam de parede para colar
            bool requiresWallSupport = isWindow || isDoor;

            // Aplicar as definições automáticas ao script
            placeable.Configure(type, canMove, canRotate, canScale, requiresWallSupport, allRenderers.ToArray());

            // Lógica do BoxCollider automática baseada no tamanho do modelo 3D
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

            // Salvar e fechar o Prefab em memória
            PrefabUtility.SaveAsPrefabAsset(instance, path);
            PrefabUtility.UnloadPrefabContents(instance);
            count++;
        }

        return count;
    }
}