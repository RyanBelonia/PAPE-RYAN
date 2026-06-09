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
            foreach (Transform child in gridContainer) Destroy(child.gameObject);

            if (paintTool == null) paintTool = Object.FindFirstObjectByType<PaintTool>();

            foreach (Material mat in availableMaterials)
            {
                if (mat == null) continue;

                GameObject btnObj = Instantiate(materialButtonPrefab, gridContainer);
                
                // 1. TEXTO: Limpa o nome e escreve no botão
                string cleanName = mat.name.Replace("Mat_", "").Replace("_", " ");
                TMP_Text txt = btnObj.GetComponentInChildren<TMP_Text>();
                if (txt != null) txt.text = cleanName;

                // 2. FOTO: Vai buscar a imagem da textura e coloca no fundo do botão!
                Image img = btnObj.GetComponent<Image>();
                if (img != null)
                {
                    Texture2D tex = mat.mainTexture as Texture2D;
                    if (tex != null)
                    {
                        // Converte a textura 3D para um Sprite da UI
                        img.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
                        img.color = Color.white; // Limpa qualquer cor escura que estivesse no botão
                    }
                    else
                    {
                        // Se for um material sem foto (só tinta), usa a cor
                        if (mat.HasProperty("_BaseColor")) img.color = mat.GetColor("_BaseColor");
                        else if (mat.HasProperty("_Color")) img.color = mat.GetColor("_Color");
                    }
                }

                Button btn = btnObj.GetComponent<Button>();
                if (btn != null) btn.onClick.AddListener(() => paintTool.SetActiveMaterial(mat));
            }
        }

#if UNITY_EDITOR
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