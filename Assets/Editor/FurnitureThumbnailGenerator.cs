using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Sistema de Ferramenta do Editor da Unity para automatizar a extração e criação de imagens 
/// em miniatura (Thumbnails/Ícones) a partir dos Prefabs de mobiliário do projeto.
/// </summary>
public class FurnitureThumbnailGenerator : EditorWindow
{
    // Caminhos padrão para a leitura de modelos 3D (Prefabs) e escrita de imagens (Icons)
    private string prefabsRootFolder = "Assets/Prefabs/Furniture";
    private string outputRootFolder = "Assets/Art/Icons/Furniture";

    // Fila de processamento assíncrono para gerir o carregamento em memória e evitar congelamento da Unity
    private Queue<Object> pendingQueue = new Queue<Object>();
    
    // Dicionário mapeando cada objeto 3D ao seu respetivo caminho físico de destino da imagem .png
    private Dictionary<Object, string> assetOutputPaths = new Dictionary<Object, string>();

    /// <summary>
    /// Cria uma entrada no menu superior da Unity (Tools -> Furniture) para abrir a janela da ferramenta.
    /// </summary>
    [MenuItem("Tools/Furniture/Generate Thumbnails")]
    public static void OpenWindow()
    {
        // Instancia e abre a janela personalizada no Editor
        GetWindow<FurnitureThumbnailGenerator>("Furniture Thumbnails");
    }

    /// <summary>
    /// Renderiza a Interface Gráfica (GUI) da janela dentro do Editor da Unity.
    /// </summary>
    private void OnGUI()
    {
        GUILayout.Label("Gerador SEGURO de Thumbnails", EditorStyles.boldLabel);

        // Campos de texto para o utilizador alterar os caminhos das pastas diretamente na interface
        prefabsRootFolder = EditorGUILayout.TextField("Pasta raiz dos Prefabs", prefabsRootFolder);
        outputRootFolder = EditorGUILayout.TextField("Pasta raiz de saída", outputRootFolder);

        GUILayout.Space(10);

        // Botão para despoletar a varredura e preparação de imagens
        if (GUILayout.Button("Gerar Thumbnails"))
        {
            PrepareThumbnails();
        }
    }

    /// <summary>
    /// Varre as pastas à procura de ficheiros .prefab, cria as pastas de destino e enfileira os objetos.
    /// </summary>
    private void PrepareThumbnails()
    {
        // Validação preventiva contra caminhos incorretos ou inexistentes
        if (!AssetDatabase.IsValidFolder(prefabsRootFolder))
        {
            Debug.LogError("Pasta de prefabs inválida: " + prefabsRootFolder);
            return;
        }

        // Se a pasta de ícones não existir no projeto, cria a estrutura de forma dinâmica
        if (!AssetDatabase.IsValidFolder(outputRootFolder))
        {
            CreateFoldersRecursively(outputRootFolder);
        }

        // Encontra os GUIDs (identificadores únicos do motor da Unity) de todos os prefabs dentro da pasta
        string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab", new[] { prefabsRootFolder });

        // Limpa as estruturas de dados para prevenir acumulação de lixo em chamadas repetidas
        pendingQueue.Clear();
        assetOutputPaths.Clear();

        // Itera sobre cada prefab descoberto na pasta
        foreach (string guid in prefabGuids)
        {
            string prefabPath = AssetDatabase.GUIDToAssetPath(guid);
            Object asset = AssetDatabase.LoadAssetAtPath<Object>(prefabPath);

            if (asset == null)
                continue;

            // Mantém a organização original de subpastas do modelo 3D ao criar os ícones 2D
            string subfolder = GetRelativeSubfolder(prefabPath, prefabsRootFolder);
            string outputFolder = string.IsNullOrEmpty(subfolder)
                ? outputRootFolder
                : $"{outputRootFolder}/{subfolder}";

            if (!AssetDatabase.IsValidFolder(outputFolder))
            {
                CreateFoldersRecursively(outputFolder);
            }

            string filePath = $"{outputFolder}/{asset.name}.png";

            // Otimização: Apenas enfileira o objeto se o ícone ainda não tiver sido gerado em sessões anteriores
            if (!File.Exists(filePath))
            {
                pendingQueue.Enqueue(asset);
                assetOutputPaths[asset] = filePath;
            }
        }

        // Se nenhum novo prefab precisar de um ícone, encerra a execução
        if (pendingQueue.Count == 0)
        {
            Debug.Log("Todos os ícones já existem.");
            return;
        }

        // Subscreve a função de processamento de filas ao ciclo de atualização global do Editor (update)
        EditorApplication.update -= ProcessQueue;
        EditorApplication.update += ProcessQueue;

        Debug.Log($"Iniciando geração de {pendingQueue.Count} ícones...");
    }

    /// <summary>
    /// Executado a cada frame do Editor. Processa a fila um elemento de cada vez de forma assíncrona 
    /// para garantir estabilidade operacional e evitar estouros de memória RAM.
    /// </summary>
    private void ProcessQueue()
    {
        // Quando a fila é esvaziada, desliga a subscrição e atualiza a base de dados do projeto
        if (pendingQueue.Count == 0)
        {
            EditorApplication.update -= ProcessQueue;
            AssetDatabase.Refresh(); // Força a Unity a reconhecer os novos ficheiros .png em disco
            Debug.Log("Geração finalizada.");
            return;
        }

        // Obtém o primeiro elemento da fila sem o remover para validação inicial
        Object asset = pendingQueue.Peek();

        if (asset == null)
        {
            pendingQueue.Dequeue();
            return;
        }

        // Se a Unity ainda estiver a computar ou a renderizar o Preview interno do objeto 3D, aguarda o próximo frame
        if (AssetPreview.IsLoadingAssetPreview(asset.GetInstanceID()))
        {
            return;
        }

        // Solicita a miniatura gerada nativamente pelo motor da Unity
        Texture2D preview = AssetPreview.GetAssetPreview(asset);

        // Se o preview ainda for nulo, solicita-o e aguarda a renderização no próximo frame
        if (preview == null)
        {
            AssetPreview.GetAssetPreview(asset);
            return;
        }

        string outputPath = assetOutputPaths[asset];

        // Converte a textura interna do motor (que por padrão vem trancada para leitura) numa textura legível em código
        Texture2D readableCopy = CopyToReadableTexture(preview);
        if (readableCopy != null)
        {
            // Codifica a matriz de pixéis lida num ficheiro binário PNG e grava-o no disco rígido
            byte[] pngData = readableCopy.EncodeToPNG();
            File.WriteAllBytes(outputPath, pngData);
            DestroyImmediate(readableCopy); // Libertação imediata da textura temporária da memória

            Debug.Log($"Ícone criado para: {asset.name} ({pendingQueue.Count - 1} restantes)");
        }

        // Avança na fila de processamento
        pendingQueue.Dequeue();

        // Coleta de Lixo Forçada (Garbage Collection) para manter a performance do motor estável durante lotes grandes
        EditorUtility.UnloadUnusedAssetsImmediate();
        System.GC.Collect();
    }

    /// <summary>
    /// Transfere a imagem de uma textura protegida por hardware da Unity para uma área de buffer temporária (RenderTexture) 
    /// permitindo a leitura de pixéis via CPU para exportação física.
    /// </summary>
    private Texture2D CopyToReadableTexture(Texture2D source)
    {
        // Aloca uma textura temporária de renderização em memória gráfica (GPU)
        RenderTexture rt = RenderTexture.GetTemporary(source.width, source.height, 0, RenderTextureFormat.ARGB32);
        RenderTexture previous = RenderTexture.active;

        // Copia os pixéis da textura original para o buffer temporário
        Graphics.Blit(source, rt);
        RenderTexture.active = rt;

        // Cria uma nova textura em CPU configurada com suporte a leitura de pixéis e transparências (RGBA32)
        Texture2D copy = new Texture2D(source.width, source.height, TextureFormat.RGBA32, false);
        copy.ReadPixels(new Rect(0, 0, source.width, source.height), 0, 0); // Copia o bloco de pixéis ativos
        copy.Apply(); // Submete a alteração final de pixéis

        // Restaura o estado anterior da renderização do motor e liberta o buffer temporário da GPU
        RenderTexture.active = previous;
        RenderTexture.ReleaseTemporary(rt);

        return copy;
    }

    /// <summary>
    /// Calcula a estrutura de subpastas de um Prefab em relação à sua pasta de origem para espelhar a organização nos ícones.
    /// </summary>
    private string GetRelativeSubfolder(string assetPath, string rootFolder)
    {
        string assetDirectory = Path.GetDirectoryName(assetPath)?.Replace("\\", "/") ?? "";
        rootFolder = rootFolder.Replace("\\", "/");

        if (assetDirectory.StartsWith(rootFolder))
            return assetDirectory.Substring(rootFolder.Length).TrimStart('/');

        return string.Empty;
    }

    /// <summary>
    /// Função recursiva auxiliar para decompor e criar ficheiros de diretórios complexos no AssetDatabase da Unity.
    /// </summary>
    private void CreateFoldersRecursively(string folderPath)
    {
        string[] parts = folderPath.Split('/');
        string current = parts[0];

        for (int i = 1; i < parts.Length; i++)
        {
            string next = current + "/" + parts[i];
            if (!AssetDatabase.IsValidFolder(next))
            {
                // Cria fisicamente a pasta necessária no projeto da Unity
                AssetDatabase.CreateFolder(current, parts[i]);
            }
            current = next;
        }
    }
}