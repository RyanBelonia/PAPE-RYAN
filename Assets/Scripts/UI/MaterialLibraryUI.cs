using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using InteriorPlanner.Systems.Tools;
using TMPro;

#if Unity_Editor
using UnityEditor;
#endif

namespace InteriorPlanner.UI
{
    public class MaterialLibraryUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PaintTool paintTool;
        [SerializeField] private Transform gridContainer; // Onde os botões vão nascer (Grid Layout Group)
        
        [Header("Prefab")]
        [SerializeField] private GameObject materialButtonPrefab; // O modelo do botão

        [Header("Data (Auto Populated)")]
        [SerializeField] private List<Material> availableMaterials = new List<Material>();

        private void Start()
        {
            GenerateMaterialButtons();
        }

        private void GenerateMaterialButtons()
        {
            // Limpa o contentor para não duplicar botões
            foreach (Transform child in gridContainer)
            {
                Destroy(child.gameObject);
            }

            if (paintTool == null)
            {
                paintTool = Object.FindFirstObjectByType<PaintTool>();
            }

            // Cria um botão para cada material da lista
            foreach (Material mat in availableMaterials)
            {
                if (mat == null) continue;

                GameObject btnObj = Instantiate(materialButtonPrefab, gridContainer);
                
                // Configura o nome no texto do botão (ex: remove o "Mat_" do nome para ficar limpo)
                string cleanName = mat.name.Replace("Mat_", "").Replace("_", " ");
                TMP_Text txt = btnObj.GetComponentInChildren<TMP_Text>();
                if (txt != null) txt.text = cleanName;

                // Tenta aplicar a cor do material ao fundo do botão como pré-visualização
                Image img = btnObj.GetComponent<Image>();
                if (img != null)
                {
                    // Se o material tiver uma cor principal, usa-a. Caso contrário, fica branco/padrão.
                    if (mat.HasProperty("_BaseColor")) img.color = mat.GetColor("_BaseColor");
                    else if (mat.HasProperty("_Color")) img.color = mat.GetColor("_Color");
                }

                // Configura o clique do botão para carregar o pincel
                Button btn = btnObj.GetComponent<Button>();
                if (btn != null)
                {
                    btn.onClick.AddListener(() => paintTool.SetActiveMaterial(mat));
                }
            }
        }

#if Unity_Editor
        // O teu botão mágico nos três pontinhos do Inspector!
        [ContextMenu("Auto Populate Materials")]
        public void AutoPopulateMaterials()
        {
            availableMaterials.Clear();
            
            // Procura todos os materiais na pasta específica de texturas/materiais
            // Podes mudar o caminho se os teus materiais estiverem noutra subpasta
            string folderPath = "Assets/Art/Textures"; 
            string[] guids = AssetDatabase.FindAssets("t:Material", new[] { folderPath });

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
                if (mat != null)
                {
                    availableMaterials.Add(mat);
                }
            }

            EditorUtility.SetDirty(this);
            Debug.Log($"<color=cyan>Palete de Cores:</color> {availableMaterials.Count} materiais encontrados e adicionados à UI!");
        }
#endif
    }
}