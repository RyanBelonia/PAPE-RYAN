using UnityEngine;
using System.IO;
using System.Collections.Generic;
using InteriorPlanner.Systems.Placement;
using SFB; // Biblioteca do explorador do Windows

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace InteriorPlanner.Systems.Save
{
    public class SaveManager : MonoBehaviour
    {
        [Header("Database (Auto Populated)")]
        [SerializeField] private List<GameObject> prefabDatabase = new List<GameObject>();
        [SerializeField] private List<Material> materialDatabase = new List<Material>();
        
        [Header("Materiais Padrao")]
        [SerializeField] private Material defaultWallMaterial;
        [SerializeField] private Material defaultFloorMaterial;
        
        private string SaveDirectory => Application.persistentDataPath + "/Saves/";

        private void Start()
        {
            if (!Directory.Exists(SaveDirectory)) Directory.CreateDirectory(SaveDirectory);

            // Espera 0.2 segundos para garantir que o RoomGenerator já fez as paredes
            // e depois vai verificar se viemos do "Abrir Projeto" do Menu Inicial!
            Invoke(nameof(CheckAutoLoad), 0.2f);
        }

        private void CheckAutoLoad()
        {
            // Verifica se o Menu Inicial deixou um ficheiro na memória
            if (PlayerPrefs.HasKey("ProjectToLoad"))
            {
                string path = PlayerPrefs.GetString("ProjectToLoad");

                // Apaga a memória imediatamente para não voltar a carregar sem querer
                PlayerPrefs.DeleteKey("ProjectToLoad");

                if (!string.IsNullOrEmpty(path))
                {
                    LoadProjectFromFile(path);
                }
            }
        }

        // ==========================================
        // SISTEMA DE SAVE (COM WINDOWS EXPLORER)
        // ==========================================
        public void SaveProjectWithBrowser()
        {
            string path = StandaloneFileBrowser.SaveFilePanel("Guardar Projeto Como...", SaveDirectory, "Meu_Projeto", "json");
            if (string.IsNullOrEmpty(path)) return;

            RoomSaveData newSave = new RoomSaveData();
            newSave.projectName = Path.GetFileNameWithoutExtension(path);
            newSave.lastSavedDate = System.DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss");

            PlaceableObject[] allObjects = Object.FindObjectsByType<PlaceableObject>(FindObjectsSortMode.None);

            foreach (PlaceableObject obj in allObjects)
            {
                FurnitureData data = new FurnitureData();
                data.prefabID = !string.IsNullOrEmpty(obj.originalPrefabID) ? obj.originalPrefabID : obj.gameObject.name.Replace("(Clone)", "").Trim();
                data.position = obj.transform.position;
                data.rotation = obj.transform.rotation;
                data.scale = obj.transform.localScale;

                Renderer rend = obj.GetComponentInChildren<Renderer>();
                data.materialName = (rend != null && rend.sharedMaterial != null) ? rend.sharedMaterial.name.Replace(" (Instance)", "").Trim() : "Default";

                newSave.placedObjects.Add(data);
            }

            string jsonString = JsonUtility.ToJson(newSave, true);
            File.WriteAllText(path, jsonString);

            Debug.Log($"<color=green><b>Projeto Guardado:</b></color> {path}");
        }

        // ==========================================
        // SISTEMA DE LOAD 
        // ==========================================

        // Esta função é chamada pelo teu botão "Carregar" dentro da cena 3D
        public void LoadProjectWithBrowser()
        {
            var paths = StandaloneFileBrowser.OpenFilePanel("Abrir Projeto...", SaveDirectory, "json", false);
            if (paths.Length == 0 || string.IsNullOrEmpty(paths[0])) return;

            LoadProjectFromFile(paths[0]);
        }

        // Esta é a função principal que faz o trabalho sujo (lê o texto e cria os móveis)
        private void LoadProjectFromFile(string fullPath)
        {
            if (!File.Exists(fullPath)) return;

            string jsonString = File.ReadAllText(fullPath);
            RoomSaveData loadedData = JsonUtility.FromJson<RoomSaveData>(jsonString);

            // 1. LIMPEZA TOTAL (Sem escudos! Apaga absolutamente tudo)
            PlaceableObject[] existingObjects = Object.FindObjectsByType<PlaceableObject>(FindObjectsSortMode.None);
            foreach (PlaceableObject obj in existingObjects)
            {
                DestroyImmediate(obj.gameObject);
            }

            // 2. RECONSTRUÇÃO COMPLETA (Paredes, Chão e Móveis)
            int loadedCount = 0;
            foreach (FurnitureData data in loadedData.placedObjects)
            {
                if (string.IsNullOrEmpty(data.prefabID)) continue;

                // --- CASO A: ESTRUTURA DA SALA (Cria do zero com o tamanho e posição guardados) ---
                if (data.prefabID == "Floor" || data.prefabID.StartsWith("Wall_"))
                {
                    // Cria o cubo primitivo do zero
                    GameObject roomPart = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    roomPart.name = data.prefabID;

                    // Aplica as transformações exatas que estavam no JSON (Tamanho e Posição certos!)
                    roomPart.transform.position = data.position;
                    roomPart.transform.rotation = data.rotation;
                    roomPart.transform.localScale = data.scale;

                    // Configura o componente PlaceableObject para o sistema o reconhecer no próximo Save
                    var placeable = roomPart.AddComponent<PlaceableObject>();
                    placeable.originalPrefabID = data.prefabID;

                    Renderer rend = roomPart.GetComponent<Renderer>();

                    // ----------------------------------------------------
                    // NOVO: Aplica os materiais padrão logo à nascença!
                    if (data.prefabID == "Floor" && defaultFloorMaterial != null) 
                        rend.material = defaultFloorMaterial;
                    else if (data.prefabID.StartsWith("Wall_") && defaultWallMaterial != null) 
                        rend.material = defaultWallMaterial;
                    // ----------------------------------------------------

                    placeable.Configure(PlaceableObjectType.Furniture, false, false, false, false, new Renderer[] { rend });

                    // Devolve a capacidade de ser pintado
                    roomPart.AddComponent<InteriorPlanner.Systems.Tools.Paintable>();

                    // Define as Layers corretas para o rato e o balde de tinta funcionarem
                    int layer = data.prefabID == "Floor" ? LayerMask.NameToLayer("Ground") : LayerMask.NameToLayer("Wall");
                    if (layer != -1) roomPart.layer = layer;

                    // Aplica a textura/material guardado (se o utilizador tiver pintado de outra cor)
                    if (data.materialName != "Default")
                    {
                        Material savedMat = materialDatabase.Find(m => m.name == data.materialName);
                        if (savedMat != null) placeable.UpdateOriginalMaterial(savedMat);
                    }
                    continue;
                }

                // --- CASO B: MÓVEIS NORMIAIS (Clona da Base de Dados) ---
                GameObject prefabToSpawn = prefabDatabase.Find(p => p.name == data.prefabID);
                if (prefabToSpawn != null)
                {
                    GameObject newObj = Instantiate(prefabToSpawn, data.position, data.rotation);
                    newObj.name = data.prefabID;
                    newObj.transform.localScale = data.scale;

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
        // AUTOMAÇÃO DA BASE DE DADOS
        // ==========================================
#if UNITY_EDITOR
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