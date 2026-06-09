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

            foreach (string rootFolder in prefabsRootFolders)
            {
                if (!AssetDatabase.IsValidFolder(rootFolder))
                {
                    Debug.LogWarning($"Pasta de prefabs inválida ou não encontrada: {rootFolder}");
                    continue; 
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

                    Sprite thumbnail = null;
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

                    // --- O SEGREDO ESTÁ AQUI: Chamamos a função de tradução para o DisplayName ---
                    FurnitureItemData item = new FurnitureItemData
                    {
                        DisplayName = TranslateToPortuguese(prefab.name, prefabPath),
                        Category = category,
                        Prefab = prefab,
                        Thumbnail = thumbnail
                    };

                    furnitureItems.Add(item);
                }
            }

            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();

            Debug.Log($"<color=cyan>Sucesso!</color> {furnitureItems.Count} itens carregados e traduzidos para a UI.");
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

        // --- DICIONÁRIO DE TRADUÇÃO PT-PT ---
        private string TranslateToPortuguese(string englishName, string path)
        {
            string lowerName = englishName.ToLower();

            // Furniture Collection
            if (lowerName.Contains("bed")) return "Cama";
            if (lowerName.Contains("chair")) return "Cadeira";
            if (lowerName.Contains("closet")) return "Roupeiro";
            if (lowerName.Contains("cushion")) return "Almofada";
            if (lowerName.Contains("drawer")) return "Cómoda";
            if (lowerName.Contains("sofa")) return "Sofá";
            if (lowerName.Contains("table")) return "Mesa";

            // Bathroom Collection
            if (lowerName.Contains("shower")) return "Chuveiro";
            if (lowerName.Contains("tissue")) return "Porta-rolos";
            if (lowerName.Contains("bathtub") || lowerName.Contains("bath")) return "Banheira";
            if (lowerName.Contains("toilet")) return "Sanita";
            if (lowerName.Contains("towel")) return "Toalheiro";
            if (lowerName.Contains("vanity")) return "Móvel de Lavatório";
            if (lowerName.Contains("wash basin") || lowerName.Contains("basin")) return "Lavatório";

            // Kitchen Collection
            if (lowerName.Contains("cabinet")) return "Armário";
            if (lowerName.Contains("exhaust")) return "Exaustor";
            if (lowerName.Contains("stove")) return "Fogão";
            if (lowerName.Contains("microwave")) return "Micro-ondas";
            if (lowerName.Contains("oven")) return "Forno";
            if (lowerName.Contains("refrigerator") || lowerName.Contains("fridge")) return "Frigorífico";
            if (lowerName.Contains("sink")) return "Lava-loiça";

            // Structural 
            if (path.Contains("Janelas")) return "Janela";
            if (path.Contains("Portas")) return "Porta";
            if (path.Contains("Divisorias")) return "Divisória";

            // Se não encontrar correspondência, devolve "Móvel" por defeito
            return "Móvel";
        }
#endif
    }
}