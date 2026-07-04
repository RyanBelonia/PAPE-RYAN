using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using InteriorPlanner.Systems.Tools;
using TMPro;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace InteriorPlanner.UI
{
    /// <summary>
    /// Constrói a biblioteca visual de materiais. O utilizador clica num botão e a ferramenta 
    /// "PaintTool" fica armada com o material correspondente.
    /// </summary>
    public class MaterialLibraryUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PaintTool paintTool;
        [SerializeField] private Transform gridContainer; 
        [SerializeField] private GameObject materialButtonPrefab;

        [Header("Data (Auto Populated)")]
        [SerializeField] private List<Material> availableMaterials = new List<Material>();

        private void Start()
        {
            GenerateMaterialButtons();
        }

        private void GenerateMaterialButtons()
        {
            // Limpeza da grelha de botões antes de gerar
            foreach (Transform child in gridContainer) Destroy(child.gameObject);

            if (paintTool == null) paintTool = Object.FindFirstObjectByType<PaintTool>();

            foreach (Material mat in availableMaterials)
            {
                if (mat == null) continue;

                GameObject btnObj = Instantiate(materialButtonPrefab, gridContainer);
                
                // 1. TEXTO: Formata o nome do ficheiro (ex: Mat_Parede_Branca -> Parede Branca)
                string cleanName = mat.name.Replace("Mat_", "").Replace("_", " ");
                TMP_Text txt = btnObj.GetComponentInChildren<TMP_Text>();
                if (txt != null) txt.text = cleanName;

                // 2. FOTO: Tenta extrair a textura do material para mostrar no botão
                Image img = btnObj.GetComponent<Image>();
                if (img != null)
                {
                    Texture2D tex = mat.mainTexture as Texture2D;
                    if (tex != null)
                    {
                        // Converte a textura para Sprite e aplica como ícone
                        img.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
                        img.color = Color.white; 
                    }
                    else
                    {
                        // Fallback: Se não houver foto, mostra a cor base (ex: bloco de cor sólida)
                        if (mat.HasProperty("_BaseColor")) img.color = mat.GetColor("_BaseColor");
                        else if (mat.HasProperty("_Color")) img.color = mat.GetColor("_Color");
                    }
                }

                // Ação do Botão: Carrega o material no Pincel (PaintTool)
                Button btn = btnObj.GetComponent<Button>();
                if (btn != null) btn.onClick.AddListener(() => paintTool.SetActiveMaterial(mat));
            }
        }

#if UNITY_EDITOR
        // Automação: O programador não precisa de arrastar 50 materiais manualmente.
        [ContextMenu("Auto Populate Materials")]
        public void AutoPopulateMaterials()
        {
            availableMaterials.Clear();
            string folderPath = "Assets/Art/Textures"; 
            string[] guids = AssetDatabase.FindAssets("t:Material", new[] { folderPath });

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
                if (mat != null) availableMaterials.Add(mat);
            }

            EditorUtility.SetDirty(this);
        }
#endif
    }
}