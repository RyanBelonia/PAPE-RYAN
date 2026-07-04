using UnityEngine;
using System.IO;
using System.Collections.Generic;
using InteriorPlanner.Systems.Placement;
using SFB; // Biblioteca externa: Standalone File Browser (Interface de ficheiros nativa do Windows)

#if UNITY_EDITOR
using UnityEditor; // Necessário apenas para funções de automação no Editor (como o 'Auto Populate')
#endif

namespace InteriorPlanner.Systems.Save
{
    /// <summary>
    /// O motor de persistência do projeto. É responsável por converter a cena 3D viva (RAM) 
    /// num ficheiro JSON estático (Disco) e vice-versa.
    /// </summary>
    public class SaveManager : MonoBehaviour
    {
        [Header("Database (Auto Populated)")]
        // Bases de dados que o script preenche sozinho para saber o que pode instanciar ao carregar
        [SerializeField] private List<GameObject> prefabDatabase = new List<GameObject>();
        [SerializeField] private List<Material> materialDatabase = new List<Material>();
        
        [Header("Materiais Padrao")]
        // Referências para aplicar texturas automaticamente caso o save seja antigo ou vazio
        [SerializeField] private Material defaultWallMaterial;
        [SerializeField] private Material defaultFloorMaterial;
        
        // Define o caminho de gravação (pasta %AppData% do Unity)
        private string SaveDirectory => Application.persistentDataPath + "/Saves/";

        private void Start()
        {
            // Cria a pasta "Saves" se esta ainda não existir
            if (!Directory.Exists(SaveDirectory)) Directory.CreateDirectory(SaveDirectory);

            // Pequeno delay para garantir que o RoomGenerator termina de construir as paredes 
            // antes de tentarmos carregar um projeto guardado.
            Invoke(nameof(CheckAutoLoad), 0.2f);
        }

        private void CheckAutoLoad()
        {
            // O Menu Inicial comunica com esta cena através do PlayerPrefs.
            // Se o utilizador carregou em "Abrir Projeto" no Menu, o caminho está aqui guardado.
            if (PlayerPrefs.HasKey("ProjectToLoad"))
            {
                string path = PlayerPrefs.GetString("ProjectToLoad");

                // Apaga a chave da memória para evitar um loop de carregamento infinito
                PlayerPrefs.DeleteKey("ProjectToLoad");

                if (!string.IsNullOrEmpty(path))
                {
                    LoadProjectFromFile(path);
                }
            }
        }

        // ==========================================
        // SISTEMA DE SAVE (Serialização JSON)
        // ==========================================
        public void SaveProjectWithBrowser()
        {
            // Abre o explorador do Windows para o utilizador escolher onde guardar
            string path = StandaloneFileBrowser.SaveFilePanel("Guardar Projeto Como...", SaveDirectory, "Meu_Projeto", "json");
            if (string.IsNullOrEmpty(path)) return;

            // Cria o objeto "Envelope" (RoomSaveData) que conterá toda a lista de móveis
            RoomSaveData newSave = new RoomSaveData();
            newSave.projectName = Path.GetFileNameWithoutExtension(path);
            newSave.lastSavedDate = System.DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss");

            // Procura todos os objetos na sala que possuem o script 'PlaceableObject' (o nosso componente de inteligência)
            PlaceableObject[] allObjects = Object.FindObjectsByType<PlaceableObject>(FindObjectsSortMode.None);

            foreach (PlaceableObject obj in allObjects)
            {
                FurnitureData data = new FurnitureData();
                // Regista o ID do prefab para saber qual objeto reconstruir no 'Load'
                data.prefabID = !string.IsNullOrEmpty(obj.originalPrefabID) ? obj.originalPrefabID : obj.gameObject.name.Replace("(Clone)", "").Trim();
                data.position = obj.transform.position;
                data.rotation = obj.transform.rotation;
                data.scale = obj.transform.localScale;

                // Captura o nome do material (cor) para salvar a personalização do utilizador
                Renderer rend = obj.GetComponentInChildren<Renderer>();
                data.materialName = (rend != null && rend.sharedMaterial != null) ? rend.sharedMaterial.name.Replace(" (Instance)", "").Trim() : "Default";

                newSave.placedObjects.Add(data);
            }

            // Converte o objeto em texto JSON e grava no disco
            string jsonString = JsonUtility.ToJson(newSave, true);
            File.WriteAllText(path, jsonString);

            Debug.Log($"<color=green><b>Projeto Guardado:</b></color> {path}");
        }

        // ==========================================
        // SISTEMA DE LOAD (Desserialização)
        // ==========================================

        public void LoadProjectWithBrowser()
        {
            var paths = StandaloneFileBrowser.OpenFilePanel("Abrir Projeto...", SaveDirectory, "json", false);
            if (paths.Length == 0 || string.IsNullOrEmpty(paths[0])) return;

            LoadProjectFromFile(paths[0]);
        }

        private void LoadProjectFromFile(string fullPath)
        {
            if (!File.Exists(fullPath)) return;

            // Lê o texto JSON e transforma-o de volta em objetos de memória (C# Classes)
            string jsonString = File.ReadAllText(fullPath);
            RoomSaveData loadedData = JsonUtility.FromJson<RoomSaveData>(jsonString);

            // 1. LIMPEZA TOTAL: Apaga tudo o que está na sala antes de reconstruir (Evita sobreposição de móveis)
            PlaceableObject[] existingObjects = Object.FindObjectsByType<PlaceableObject>(FindObjectsSortMode.None);
            foreach (PlaceableObject obj in existingObjects)
            {
                DestroyImmediate(obj.gameObject);
            }

            // 2. RECONSTRUÇÃO COMPLETA: Itera pelos dados carregados e repõe a mobília
            int loadedCount = 0;
            foreach (FurnitureData data in loadedData.placedObjects)
            {
                if (string.IsNullOrEmpty(data.prefabID)) continue;

                // --- CASO A: ESTRUTURA DA SALA (Chão/Paredes gerados procedimentalmente) ---
                if (data.prefabID == "Floor" || data.prefabID.StartsWith("Wall_"))
                {
                    GameObject roomPart = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    roomPart.name = data.prefabID;

                    // Aplica as transformações de posição e escala guardadas
                    roomPart.transform.position = data.position;
                    roomPart.transform.rotation = data.rotation;
                    roomPart.transform.localScale = data.scale;

                    var placeable = roomPart.AddComponent<PlaceableObject>();
                    placeable.originalPrefabID = data.prefabID;

                    Renderer rend = roomPart.GetComponent<Renderer>();

                    // Aplica o material padrão definido no Inspector se o objeto não tiver sido pintado
                    if (data.prefabID == "Floor" && defaultFloorMaterial != null) 
                        rend.material = defaultFloorMaterial;
                    else if (data.prefabID.StartsWith("Wall_") && defaultWallMaterial != null) 
                        rend.material = defaultWallMaterial;

                    placeable.Configure(PlaceableObjectType.Furniture, false, false, false, false, new Renderer[] { rend });
                    roomPart.AddComponent<InteriorPlanner.Systems.Tools.Paintable>();

                    int layer = data.prefabID == "Floor" ? LayerMask.NameToLayer("Ground") : LayerMask.NameToLayer("Wall");
                    if (layer != -1) roomPart.layer = layer;

                    // Se o utilizador tinha pintado a parede com cor personalizada, recupera-a da base de dados
                    if (data.materialName != "Default")
                    {
                        Material savedMat = materialDatabase.Find(m => m.name == data.materialName);
                        if (savedMat != null) placeable.UpdateOriginalMaterial(savedMat);
                    }
                    continue;
                }

                // --- CASO B: MÓVEIS NORMIAIS (Instancia Prefabs do projeto) ---
                GameObject prefabToSpawn = prefabDatabase.Find(p => p.name == data.prefabID);
                if (prefabToSpawn != null)
                {
                    GameObject newObj = Instantiate(prefabToSpawn, data.position, data.rotation);
                    newObj.name = data.prefabID;
                    newObj.transform.localScale = data.scale;

                    // Se o móvel tinha uma cor personalizada, re-aplica-a
                    if (data.materialName != "Default")
                    {
                        Material savedMat = materialDatabase.Find(m => m.name == data.materialName);
                        if (savedMat != null) newObj.GetComponent<PlaceableObject>()?.UpdateOriginalMaterial(savedMat);
                    }
                    loadedCount++;
                }
            }
            Debug.Log($"<color=cyan><b>Carregado com Sucesso:</b></color> A estrutura e {loadedCount} móveis foram recriados!");
        }

        // ==========================================
        // AUTOMAÇÃO DA BASE DE DADOS (Edição de Conteúdo)
        // ==========================================
#if UNITY_EDITOR
        /// <summary>
        /// Ferramenta interna que varre as pastas do projeto à procura de Prefabs e Materiais.
        /// Isto evita que o programador tenha de arrastar 100+ objetos manualmente para listas na UI.
        /// </summary>
        [ContextMenu("3. Auto Populate Databases")]
        public void AutoPopulateDatabases()
        {
            prefabDatabase.Clear();
            materialDatabase.Clear();

            string[] prefabPaths = { "Assets/Prefabs/Furniture", "Assets/Prefabs/Structural" };
            foreach (string folder in prefabPaths)
            {
                string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { folder });
                foreach (string guid in guids)
                {
                    GameObject obj = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(guid));
                    if (obj != null) prefabDatabase.Add(obj);
                }
            }

            string[] matGuids = AssetDatabase.FindAssets("t:Material", new[] { "Assets/Art/Materials/Materiais" });
            foreach (string guid in matGuids)
            {
                Material mat = AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(guid));
                if (mat != null) materialDatabase.Add(mat);
            }

            EditorUtility.SetDirty(this);
            Debug.Log("<color=green>Bases de Dados atualizadas com sucesso!</color>");
        }
#endif
    }
}