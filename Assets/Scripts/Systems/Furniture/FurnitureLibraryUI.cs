using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace InteriorPlanner.Systems.Furniture
{
    public class FurnitureLibraryUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Transform furnitureContent;
        [SerializeField] private GameObject furnitureButtonPrefab;

        [Header("Auto Load Paths")]
        // LISTAS (ARRAYS) PARA LER VÁRIAS PASTAS
        [SerializeField] private string[] prefabsRootFolders = { 
            "Assets/Prefabs/Furniture", 
            "Assets/Prefabs/Structural" 
        };
        [SerializeField] private string[] iconsRootFolders = { 
            "Assets/Art/Icons/Furniture", 
            "Assets/Art/Icons/Structural",
            "Assets/Art/Icons/Structural/Janelas",
            "Assets/Art/Icons/Structural/Portas"

        };

        [Header("Generated Data")]
        [SerializeField] private List<FurnitureItemData> furnitureItems = new();

        private void Start()
        {
            ShowAll();
        }

        // --- MÉTODOS DE FILTRO ---

        public void ShowAll()
        {
            GenerateFurnitureButtons(furnitureItems);
        }

        public void FilterByCategory(FurnitureCategory category)
        {
            List<FurnitureItemData> filtered = new List<FurnitureItemData>();

            for (int i = 0; i < furnitureItems.Count; i++)
            {
                if (furnitureItems[i].Category == category)
                {
                    filtered.Add(furnitureItems[i]);
                }
            }

            GenerateFurnitureButtons(filtered);
        }

        public void FilterByCategoryInt(int categoryIndex)
        {
            FurnitureCategory category = (FurnitureCategory)categoryIndex;
            FilterByCategory(category);
        }

        // --- LÓGICA DE GERAÇÃO DE UI ---

        private void GenerateFurnitureButtons(List<FurnitureItemData> itemsToShow)
        {
            ClearButtons();

            if (furnitureButtonPrefab == null || furnitureContent == null)
            {
                Debug.LogError("Referências de UI faltando no FurnitureLibraryUI!");
                return;
            }

            for (int i = 0; i < itemsToShow.Count; i++)
            {
                GameObject buttonObject = Instantiate(furnitureButtonPrefab, furnitureContent);
                FurnitureButtonUI buttonUI = buttonObject.GetComponent<FurnitureButtonUI>();

                if (buttonUI != null)
                {
                    buttonUI.Setup(itemsToShow[i]);
                }
            }
        }

        private void ClearButtons()
        {
            if (furnitureContent == null) return;

            for (int i = furnitureContent.childCount - 1; i >= 0; i--)
            {
                Destroy(furnitureContent.GetChild(i).gameObject);
            }
        }

        // --- LÓGICA AUTOMÁTICA (APENAS NO EDITOR) ---

#if UNITY_EDITOR
        [ContextMenu("Auto Populate Furniture Items")]
        private void AutoPopulateFurnitureItems()
        {
            furnitureItems.Clear();

            // O código agora faz um loop por todas as pastas que definimos lá em cima
            foreach (string rootFolder in prefabsRootFolders)
            {
                if (!AssetDatabase.IsValidFolder(rootFolder))
                {
                    Debug.LogWarning($"Pasta de prefabs inválida ou não encontrada: {rootFolder}");
                    continue; // Salta para a próxima pasta em vez de parar tudo
                }

                string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab", new[] { rootFolder });

                for (int i = 0; i < prefabGuids.Length; i++)
                {
                    string prefabPath = AssetDatabase.GUIDToAssetPath(prefabGuids[i]);
                    GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

                    if (prefab == null) continue;

                    string categoryFolderName = GetImmediateCategoryFolder(prefabPath, rootFolder);

                    if (!TryParseCategory(categoryFolderName, out FurnitureCategory category))
                    {
                        Debug.LogWarning($"Pasta '{categoryFolderName}' não corresponde a nenhuma categoria no Enum. Ignorado: {prefabPath}");
                        continue;
                    }

                    // --- O NOVO SISTEMA DE ÍCONES (À Prova de Balas) ---
                    Sprite thumbnail = null;
                    
                    // Procura APENAS nas pastas que tu definiste no Inspector
                    string[] iconGuids = AssetDatabase.FindAssets($"{prefab.name} t:Sprite", iconsRootFolders);
                    
                    if (iconGuids.Length > 0)
                    {
                        string foundIconPath = AssetDatabase.GUIDToAssetPath(iconGuids[0]);
                        thumbnail = AssetDatabase.LoadAssetAtPath<Sprite>(foundIconPath);
                    }
                    else
                    {
                        Debug.LogWarning($"<color=orange>Aviso:</color> Ícone não encontrado para '{prefab.name}'. Confirma se a imagem existe nas pastas indicadas e se está como Sprite (2D and UI).");
                    }

                    FurnitureItemData item = new FurnitureItemData
                    {
                        DisplayName = prefab.name,
                        Category = category,
                        Prefab = prefab,
                        Thumbnail = thumbnail
                    };

                    furnitureItems.Add(item);
                }
            }

            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();

            Debug.Log($"<color=cyan>Sucesso!</color> {furnitureItems.Count} itens carregados automaticamente de todas as pastas.");
        }

        private string GetImmediateCategoryFolder(string assetPath, string rootFolder)
        {
            string normalizedAssetPath = assetPath.Replace("\\", "/");
            string normalizedRoot = rootFolder.Replace("\\", "/").TrimEnd('/');

            if (!normalizedAssetPath.StartsWith(normalizedRoot)) return string.Empty;

            string relative = normalizedAssetPath.Substring(normalizedRoot.Length).TrimStart('/');
            string[] parts = relative.Split('/');

            return parts.Length > 1 ? parts[0] : string.Empty;
        }

        private bool TryParseCategory(string folderName, out FurnitureCategory category)
        {
            return System.Enum.TryParse(folderName, true, out category);
        }
#endif
    }
}