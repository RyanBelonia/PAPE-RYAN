using UnityEngine;
using TMPro;
using UnityEngine.UI;
using InteriorPlanner.Systems.Placement;

namespace InteriorPlanner.Systems.UI
{
    /// <summary>
    /// Controlador da interface que permite a manipulação de precisão dos objetos selecionados na cena.
    /// Gere a sincronização entre os valores numéricos da UI e as propriedades de Transform do objeto 3D.
    /// </summary>
    public class SelectionUI : MonoBehaviour
    {
        [Header("System References")]
        [SerializeField] private SelectionManager selectionManager;

        [Header("Main UI")]
        [SerializeField] private GameObject uiPanel;
        [SerializeField] private TMP_Text objectNameText;
        [SerializeField] private Button closeButton;
        [SerializeField] private Button deleteButton;

        [Header("Position Inputs")]
        [SerializeField] private TMP_InputField posX;
        [SerializeField] private TMP_InputField posY;
        [SerializeField] private TMP_InputField posZ;

        [Header("Rotation Inputs")]
        [SerializeField] private TMP_InputField rotX;
        [SerializeField] private TMP_InputField rotY;
        [SerializeField] private TMP_InputField rotZ;

        [Header("Size Inputs (Meters)")]
        [SerializeField] private TMP_InputField scaleX;
        [SerializeField] private TMP_InputField scaleY;
        [SerializeField] private TMP_InputField scaleZ;

        private PlaceableObject currentObject;

        private Vector3 lastKnownPosition;
        private Vector3 lastKnownRotation;
        private Vector3 lastKnownSizeInMeters; // Armazena a dimensão em metros reais
        private Vector3 objectBaseSize; // Tamanho de referência do modelo 3D (para cálculo de escala)

        private void Start()
        {
            uiPanel.SetActive(false);

            // Registo de eventos para botões e inputs de edição
            if (closeButton) closeButton.onClick.AddListener(Deselect);
            if (deleteButton) deleteButton.onClick.AddListener(DeleteObject);

            // Ao terminar a edição de qualquer campo numérico, o sistema força a atualização do objeto
            posX.onEndEdit.AddListener(val => UpdateObjectTransform());
            posY.onEndEdit.AddListener(val => UpdateObjectTransform());
            posZ.onEndEdit.AddListener(val => UpdateObjectTransform());

            rotX.onEndEdit.AddListener(val => UpdateObjectTransform());
            rotY.onEndEdit.AddListener(val => UpdateObjectTransform());
            rotZ.onEndEdit.AddListener(val => UpdateObjectTransform());

            scaleX.onEndEdit.AddListener(val => UpdateObjectTransform());
            scaleY.onEndEdit.AddListener(val => UpdateObjectTransform());
            scaleZ.onEndEdit.AddListener(val => UpdateObjectTransform());
        }

        private void Update()
        {
            if (selectionManager == null || uiPanel == null) return;

            PlaceableObject selected = selectionManager.GetSelectedObject();

            // Alterna o estado da interface com base na seleção atual
            if (selected != currentObject)
            {
                currentObject = selected;
                if (currentObject != null) ShowUI();
                else HideUI();
            }

            // Monitoriza alterações externas para sincronizar com a UI
            if (currentObject != null)
            {
                CheckForExternalChanges();
            }
        }

        private void ShowUI()
        {
            uiPanel.SetActive(true);
            if (objectNameText) objectNameText.text = currentObject.name.Replace("(Clone)", "").Trim();

            // Extrai o tamanho base do objeto para conversão de escala métrica
            BoxCollider boxCol = currentObject.GetComponent<BoxCollider>();
            objectBaseSize = boxCol != null ? boxCol.size : Vector3.one;

            // Define se os campos são editáveis consoante as capacidades do objeto
            bool canMove = currentObject.CanMove;
            posX.interactable = canMove;
            posY.interactable = canMove;
            posZ.interactable = canMove;

            bool canRotate = currentObject.CanRotate;
            rotX.interactable = canRotate;
            rotY.interactable = canRotate;
            rotZ.interactable = canRotate;

            bool canScale = currentObject.CanScale;
            scaleX.interactable = canScale;
            scaleY.interactable = canScale;
            scaleZ.interactable = canScale;

            ForceReadValues();
        }

        private void HideUI()
        {
            uiPanel.SetActive(false);
        }

        private void ForceReadValues()
        {
            lastKnownPosition = currentObject.transform.position;
            lastKnownRotation = currentObject.transform.eulerAngles;
            
            // Cálculo: Metros = (Escala local * Tamanho original do modelo 3D)
            lastKnownSizeInMeters = new Vector3(
                objectBaseSize.x * currentObject.transform.localScale.x,
                objectBaseSize.y * currentObject.transform.localScale.y,
                objectBaseSize.z * currentObject.transform.localScale.z
            );

            UpdateUIFields();
        }

        private void CheckForExternalChanges()
        {
            Transform objT = currentObject.transform;

            // Atualiza campos de posição apenas se o objeto mudou e o input não está focado
            if (objT.position != lastKnownPosition && !posX.isFocused && !posY.isFocused && !posZ.isFocused)
            {
                lastKnownPosition = objT.position;
                SetInputTextWithoutNotify(posX, lastKnownPosition.x);
                SetInputTextWithoutNotify(posY, lastKnownPosition.y);
                SetInputTextWithoutNotify(posZ, lastKnownPosition.z);
            }

            // Atualiza campos de rotação
            if (objT.eulerAngles != lastKnownRotation && !rotX.isFocused && !rotY.isFocused && !rotZ.isFocused)
            {
                lastKnownRotation = objT.eulerAngles;
                SetInputTextWithoutNotify(rotX, lastKnownRotation.x);
                SetInputTextWithoutNotify(rotY, lastKnownRotation.y);
                SetInputTextWithoutNotify(rotZ, lastKnownRotation.z);
            }

            // Atualiza campos de escala
            Vector3 currentSizeMeters = new Vector3(
                objectBaseSize.x * objT.localScale.x,
                objectBaseSize.y * objT.localScale.y,
                objectBaseSize.z * objT.localScale.z
            );

            if (currentSizeMeters != lastKnownSizeInMeters && !scaleX.isFocused && !scaleY.isFocused && !scaleZ.isFocused)
            {
                lastKnownSizeInMeters = currentSizeMeters;
                SetInputTextWithoutNotify(scaleX, lastKnownSizeInMeters.x);
                SetInputTextWithoutNotify(scaleY, lastKnownSizeInMeters.y);
                SetInputTextWithoutNotify(scaleZ, lastKnownSizeInMeters.z);
            }
        }

        private void UpdateUIFields()
        {
            SetInputTextWithoutNotify(posX, lastKnownPosition.x);
            SetInputTextWithoutNotify(posY, lastKnownPosition.y);
            SetInputTextWithoutNotify(posZ, lastKnownPosition.z);

            SetInputTextWithoutNotify(rotX, lastKnownRotation.x);
            SetInputTextWithoutNotify(rotY, lastKnownRotation.y);
            SetInputTextWithoutNotify(rotZ, lastKnownRotation.z);

            SetInputTextWithoutNotify(scaleX, lastKnownSizeInMeters.x);
            SetInputTextWithoutNotify(scaleY, lastKnownSizeInMeters.y);
            SetInputTextWithoutNotify(scaleZ, lastKnownSizeInMeters.z);
        }

        private void SetInputTextWithoutNotify(TMP_InputField input, float value)
        {
            if (input == null) return;
            input.SetTextWithoutNotify(value.ToString("F2")); 
        }

        private void UpdateObjectTransform()
        {
            if (currentObject == null) return;

            // Extração de dados da UI com validação via TryParse
            float px = float.TryParse(posX.text, out float _px) ? _px : lastKnownPosition.x;
            float py = float.TryParse(posY.text, out float _py) ? _py : lastKnownPosition.y;
            float pz = float.TryParse(posZ.text, out float _pz) ? _pz : lastKnownPosition.z;

            float rx = float.TryParse(rotX.text, out float _rx) ? _rx : lastKnownRotation.x;
            float ry = float.TryParse(rotY.text, out float _ry) ? _ry : lastKnownRotation.y;
            float rz = float.TryParse(rotZ.text, out float _rz) ? _rz : lastKnownRotation.z;

            float sizeX = float.TryParse(scaleX.text, out float _sx) ? _sx : lastKnownSizeInMeters.x;
            float sizeY = float.TryParse(scaleY.text, out float _sy) ? _sy : lastKnownSizeInMeters.y;
            float sizeZ = float.TryParse(scaleZ.text, out float _sz) ? _sz : lastKnownSizeInMeters.z;

            // Aplicação de limites físicos para evitar geometrias inválidas
            sizeX = Mathf.Clamp(sizeX, 0.1f, 15f);
            sizeY = Mathf.Clamp(sizeY, 0.1f, 15f);
            sizeZ = Mathf.Clamp(sizeZ, 0.1f, 15f);

            // Cálculo da escala relativa ao tamanho original do Prefab
            float newScaleX = objectBaseSize.x > 0 ? sizeX / objectBaseSize.x : 1f;
            float newScaleY = objectBaseSize.y > 0 ? sizeY / objectBaseSize.y : 1f;
            float newScaleZ = objectBaseSize.z > 0 ? sizeZ / objectBaseSize.z : 1f;

            // Aplicação das transformações (respeitando permissões de cada objeto)
            if (currentObject.CanMove) currentObject.transform.position = new Vector3(px, py, pz);
            if (currentObject.CanRotate) currentObject.transform.eulerAngles = new Vector3(rx, ry, rz);
            if (currentObject.CanScale) currentObject.transform.localScale = new Vector3(newScaleX, newScaleY, newScaleZ);

            ForceReadValues();
        }

        private void Deselect()
        {
            selectionManager.ClearSelection();
        }

        private void DeleteObject()
        {
            if (currentObject != null)
            {
                Destroy(currentObject.gameObject);
                selectionManager.ClearSelection();
            }
        }
    }
}