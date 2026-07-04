using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using InteriorPlanner.Systems.Placement;
using InteriorPlanner.Systems.Tools; 

/// <summary>
/// Ferramenta de Editor que automatiza a injeção de scripts (componentes), 
/// cálculos de colisão (colliders) e configuração lógica em todos os modelos 3D (Prefabs) do projeto.
/// </summary>
public class PrefabAutoConfigurer : EditorWindow
{
    /// <summary>
    /// Adiciona um botão ao menu superior do Unity (Tools -> Furniture) 
    /// que desencadeia a varredura e reconfiguração em massa dos objetos.
    /// </summary>
    [MenuItem("Tools/Furniture/Auto Configure All Prefabs")]
    public static void ConfigurePrefabs()
    {
        // Define as diretorias exatas onde os diferentes tipos de objetos 3D estão armazenados
        string furnitureFolder = "Assets/Prefabs/Furniture";
        string janelasFolder = "Assets/Prefabs/Structural/Janelas";
        string portasFolder = "Assets/Prefabs/Structural/Portas";
        string divisoriasFolder = "Assets/Prefabs/Structural/Divisorias";

        int count = 0;
        
        // Processa cada diretoria independentemente e acumula o número de ficheiros tratados
        count += ProcessFolder(furnitureFolder);
        count += ProcessFolder(janelasFolder);
        count += ProcessFolder(portasFolder);
        count += ProcessFolder(divisoriasFolder);

        // Força o motor do Unity a gravar as alterações no disco e a atualizar a base de dados
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"<color=green>Sucesso!</color> {count} prefabs foram configurados automaticamente e o ID original foi gravado.");
    }

    /// <summary>
    /// Lê, modifica e guarda todos os ficheiros do tipo Prefab encontrados numa pasta específica.
    /// </summary>
    /// <param name="folderPath">O caminho da diretoria a ser analisada.</param>
    /// <returns>O número total de Prefabs modificados com sucesso nesta pasta.</returns>
    private static int ProcessFolder(string folderPath)
    {
        // Localiza todos os identificadores únicos (GUIDs) de ficheiros ".prefab" dentro da pasta alvo
        string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { folderPath });
        if (guids.Length == 0) return 0; // Sai imediatamente se a pasta estiver vazia

        int count = 0;

        // Itera sobre cada modelo 3D encontrado
        foreach (string guid in guids)
        {
            // Converte o identificador interno do Unity num caminho físico no disco
            string path = AssetDatabase.GUIDToAssetPath(guid);
            
            // Carrega o modelo 3D para a memória num ambiente isolado para o podermos editar via código
            GameObject instance = PrefabUtility.LoadPrefabContents(path);

            // Garante que o objeto tem o script principal do nosso sistema ('PlaceableObject')
            PlaceableObject placeable = instance.GetComponent<PlaceableObject>();
            if (placeable == null)
                placeable = instance.AddComponent<PlaceableObject>(); // Adiciona-o se não existir

            // --- GRAVAR O ID ORIGINAL (NOME DO FICHEIRO) PARA O SAVE SYSTEM ---
            // Passo crítico: Guarda o nome limpo do prefab no próprio script. 
            // O nosso SaveManager usará esta string mais tarde para recriar o objeto a partir de um JSON.
            placeable.originalPrefabID = instance.name;
            // -----------------------------------------------------------------------------

            // Extrai todos os componentes gráficos do objeto (incluindo meshes filhos/agrupados)
            MeshRenderer[] meshRenderers = instance.GetComponentsInChildren<MeshRenderer>(true);
            SkinnedMeshRenderer[] skinnedRenderers = instance.GetComponentsInChildren<SkinnedMeshRenderer>(true);

            // Funde os renderizadores simples e animados numa única lista universal
            List<Renderer> allRenderers = new List<Renderer>();
            allRenderers.AddRange(meshRenderers);
            allRenderers.AddRange(skinnedRenderers);

            // Inferência Lógica: Deduz o tipo e comportamento da mobília com base na pasta onde ela reside
            bool isWindow = folderPath.Contains("Janelas");
            bool isDoor = folderPath.Contains("Portas");
            bool isDivisoria = folderPath.Contains("Divisorias");

            // Configuração das Regras Físicas de cada tipo de objeto:
            PlaceableObjectType type = PlaceableObjectType.Furniture; 
            bool canMove = true;
            bool canRotate = !isWindow && !isDoor; // Portas e janelas herdam a rotação da parede onde são coladas
            bool canScale = isDivisoria;           // Apenas divisórias podem ser esticadas pelo utilizador
            bool requiresWallSupport = isWindow || isDoor; // Obriga a que portas e janelas só existam encostadas a superfícies verticais

            // Injeta as regras lógicas configuradas diretamente no script do objeto
            placeable.Configure(type, canMove, canRotate, canScale, requiresWallSupport, allRenderers.ToArray());

            // --- LÓGICA AUTOMÁTICA DA ETIQUETA DE PINTURA ---
            Paintable paintComponent = instance.GetComponent<Paintable>();
            
            // As Divisórias são consideradas paredes internas, logo, recebem o script que permite pintá-las com o balde de tinta
            if (isDivisoria)
            {
                if (paintComponent == null)
                    instance.AddComponent<Paintable>();
            }
            // A mobília convencional não pode ser pintada pelo utilizador, por isso o script é removido caso exista
            else
            {
                if (paintComponent != null)
                    DestroyImmediate(paintComponent, true);
            }
            // -------------------------------------------------

            // Normalização das Layers Físicas
            // Passa o objeto e todas as suas peças filhas para a Layer "Placeable"
            // para que o Raycast do rato o reconheça ao clicar
            int placeableLayer = LayerMask.NameToLayer("Placeable");
            if (placeableLayer != -1)
            {
                instance.layer = placeableLayer;
                foreach (Transform child in instance.GetComponentsInChildren<Transform>(true))
                {
                    child.gameObject.layer = placeableLayer;
                }
            }

            // Garante a existência de uma caixa de colisão física para permitir seleção e colisão
            BoxCollider boxCollider = instance.GetComponent<BoxCollider>();
            if (boxCollider == null)
                boxCollider = instance.AddComponent<BoxCollider>();

            // Cálculo Dinâmico de Limites Físicos (Bounding Box)
            // Se o objeto for complexo e feito de várias partes 3D, este bloco calcula 
            // matematicamente uma caixa de colisão que abraça a totalidade de todas as peças.
            if (allRenderers.Count > 0)
            {
                Bounds combinedBounds = new Bounds(Vector3.zero, Vector3.zero);
                bool hasBounds = false;

                foreach (Renderer rend in allRenderers)
                {
                    if (rend != null)
                    {
                        // Inicia a caixa com o tamanho da primeira peça
                        if (!hasBounds)
                        {
                            combinedBounds = rend.bounds;
                            hasBounds = true;
                        }
                        // Expande a caixa de colisão para englobar as peças seguintes
                        else
                        {
                            combinedBounds.Encapsulate(rend.bounds);
                        }
                    }
                }

                // Aplica a dimensão total matematicamente convertida para o espaço local do objeto
                if (hasBounds)
                {
                    boxCollider.center = instance.transform.InverseTransformPoint(combinedBounds.center);
                    boxCollider.size = instance.transform.InverseTransformVector(combinedBounds.size);
                }
            }

            // Guarda permanentemente o modelo 3D modificado e descarrega a instância da memória
            PrefabUtility.SaveAsPrefabAsset(instance, path);
            PrefabUtility.UnloadPrefabContents(instance);
            count++; // Regista o sucesso do ficheiro
        }

        return count;
    }
}