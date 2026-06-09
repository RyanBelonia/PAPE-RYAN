using UnityEngine;
using System.IO;
using System.Collections.Generic;
using InteriorPlanner.Systems.Placement;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace InteriorPlanner.Systems.Save
{
    public class SaveManager : MonoBehaviour
    {
        [Header("Project Settings")]
        public string defaultProjectName = "A_Minha_Sala";

        [Header("Database (Auto Populated)")]
        [SerializeField] private List<GameObject> prefabDatabase = new List<GameObject>();
        [SerializeField] private List<Material> materialDatabase = new List<Material>();

        private string SaveDirectory => Application.persistentDataPath + "/Saves/";

        private void Start()
        {
            if (!Directory.Exists(SaveDirectory)) Directory.CreateDirectory(SaveDirectory);
        }

        // ==========================================
        // SISTEMA DE SAVE
        // ==========================================
        [ContextMenu("1. Guardar Projeto Agora!")]
        public void SaveCurrentRoom()
        {
            SaveProject(defaultProjectName);
        }

        public void SaveProject(string projectName)
        {
            RoomSaveData newSave = new RoomSaveData();
            newSave.projectName = projectName;
            newSave.lastSavedDate = System.DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss");

            PlaceableObject[] allObjects = Object.FindObjectsByType<PlaceableObject>(FindObjectsSortMode.None);

            foreach (PlaceableObject obj in allObjects)
            {
                FurnitureData data = new FurnitureData();
                
                // Agora usamos o ID absoluto de fábrica gravado pelo teu configurador.
                // A condição serve de segurança caso coloques na cena um móvel que não foi reconfigurado.
                data.prefabID = !string.IsNullOrEmpty(obj.originalPrefabID) ? obj.originalPrefabID : obj.gameObject.name.Replace("(Clone)", "").Trim();
                
                data.position = obj.transform.position;
                data.rotation = obj.transform.rotation;
                data.scale = obj.transform.localScale;

                Renderer rend = obj.GetComponentInChildren<Renderer>();
                if (rend != null && rend.sharedMaterial != null)
                {
                    data.materialName = rend.sharedMaterial.name.Replace(" (Instance)", "").Trim();
                }
                else
                {
                    data.materialName = "Default";
                }

                newSave.placedObjects.Add(data);
            }

            string jsonString = JsonUtility.ToJson(newSave, true);
            string fullPath = SaveDirectory + projectName + ".json";
            File.WriteAllText(fullPath, jsonString);

            Debug.Log($"<color=green><b>Guardado:</b></color> {fullPath}");
        }

        // ==========================================
        // SISTEMA DE LOAD 
        // ==========================================
        [ContextMenu("2. Carregar Projeto Agora!")]
        public void LoadCurrentRoom()
        {
            LoadProject(defaultProjectName);
        }

       public void LoadProject(string projectName)
        {
            string fullPath = SaveDirectory + projectName + ".json";

            if (!File.Exists(fullPath))
            {
                Debug.LogWarning($"Ficheiro de save não encontrado: {fullPath}");
                return;
            }

            // 1. Lê o ficheiro
            string jsonString = File.ReadAllText(fullPath);
            RoomSaveData loadedData = JsonUtility.FromJson<RoomSaveData>(jsonString);

            // 2. Limpa a sala
            PlaceableObject[] existingObjects = Object.FindObjectsByType<PlaceableObject>(FindObjectsSortMode.None);
            foreach (PlaceableObject obj in existingObjects)
            {
                DestroyImmediate(obj.gameObject); 
            }

            // 3. Reconstrói
            int loadedCount = 0;
            foreach (FurnitureData data in loadedData.placedObjects)
            {
                // Busca direta e infalível pelo nome original do ficheiro!
                GameObject prefabToSpawn = prefabDatabase.Find(p => p.name == data.prefabID);
                
                if (prefabToSpawn != null)
                {
                    GameObject newObj = Instantiate(prefabToSpawn, data.position, data.rotation);
                    newObj.name = data.prefabID; 
                    newObj.transform.localScale = data.scale; 

                    if (data.materialName != "Default")
                    {
                        Material savedMat = materialDatabase.Find(m => m.name == data.materialName);
                        if (savedMat != null)
                        {
                            PlaceableObject placeable = newObj.GetComponent<PlaceableObject>();
                            if (placeable != null) placeable.UpdateOriginalMaterial(savedMat);
                        }
                    }
                    loadedCount++;
                }
                else
                {
                    Debug.LogWarning($"❌ Prefab não encontrado na base de dados para: {data.prefabID}");
                }
            }

            Debug.Log($"<color=cyan><b>Carregado:</b></color> {loadedCount} objetos reconstruídos!");
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

            // Carrega todos os Prefabs de móveis/estruturas
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

            // Carrega todos os Materiais
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