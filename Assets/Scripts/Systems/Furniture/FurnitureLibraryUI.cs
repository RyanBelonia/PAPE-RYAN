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
        [SerializeField] private string prefabsRootFolder = "Assets/Prefabs/Furniture";
        [SerializeField] private string iconsRootFolder = "Assets/Art/Icons/Furniture";

        [Header("Generated Data")]
        [SerializeField] private List<FurnitureItemData> furnitureItems = new();

        private void Start()
        {
            ShowAll();
        }

        // --- MÉTODOS DE FILTRO ---

        /// <summary>
        /// Mostra todos os itens da biblioteca.
        /// </summary>
        public void ShowAll()
        {
            GenerateFurnitureButtons(furnitureItems);
        }

        /// <summary>
        /// Filtra usando o Enum diretamente (Útil para botões configurados via Inspector).
        /// </summary>
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

        /// <summary>
        /// Filtra usando o índice inteiro (Útil se você estiver usando Dropdowns ou IDs numéricos).
        /// </summary>
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

            // Limpa os botões antigos antes de gerar novos
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

            if (!AssetDatabase.IsValidFolder(prefabsRootFolder))
            {
                Debug.LogError($"Pasta de prefabs inválida: {prefabsRootFolder}");
                return;
            }

            string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab", new[] { prefabsRootFolder });

            for (int i = 0; i < prefabGuids.Length; i++)
            {
                string prefabPath = AssetDatabase.GUIDToAssetPath(prefabGuids[i]);
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

                if (prefab == null) continue;

                // Detecta a categoria baseada no nome da pasta onde o prefab está
                string categoryFolderName = GetImmediateCategoryFolder(prefabPath, prefabsRootFolder);

                if (!TryParseCategory(categoryFolderName, out FurnitureCategory category))
                {
                    Debug.LogWarning($"Pasta '{categoryFolderName}' não corresponde a nenhuma categoria no Enum. Ignorado: {prefabPath}");
                    continue;
                }

                // Busca o ícone correspondente na pasta de ícones
                string iconPath = BuildIconPath(prefab.name, categoryFolderName);
                Sprite thumbnail = AssetDatabase.LoadAssetAtPath<Sprite>(iconPath);

                FurnitureItemData item = new FurnitureItemData
                {
                    DisplayName = prefab.name,
                    Category = category,
                    Prefab = prefab,
                    Thumbnail = thumbnail
                };

                furnitureItems.Add(item);
            }

            // Marca o objeto como "sujo" para que a Unity salve as alterações na lista
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();

            Debug.Log($"Sucesso! {furnitureItems.Count} itens carregados automaticamente.");
        }

        private string GetImmediateCategoryFolder(string assetPath, string rootFolder)
        {
            string normalizedAssetPath = assetPath.Replace("\\", "/");
            string normalizedRoot = rootFolder.Replace("\\", "/").TrimEnd('/');

            if (!normalizedAssetPath.StartsWith(normalizedRoot)) return string.Empty;

            string relative = normalizedAssetPath.Substring(normalizedRoot.Length).TrimStart('/');
            string[] parts = relative.Split('/');

            // Retorna o nome da primeira pasta dentro da raiz (ex: "Chairs")
            return parts.Length > 1 ? parts[0] : string.Empty;
        }

        private bool TryParseCategory(string folderName, out FurnitureCategory category)
        {
            return System.Enum.TryParse(folderName, true, out category);
        }

        private string BuildIconPath(string prefabName, string categoryFolder)
        {
            string path = iconsRootFolder;
            if (!string.IsNullOrEmpty(categoryFolder)) path += $"/{categoryFolder}";
            
            return $"{path}/{prefabName}.png";
        }
#endif
    }
}