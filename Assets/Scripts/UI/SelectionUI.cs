using UnityEngine;
using TMPro;
using UnityEngine.UI;
using InteriorPlanner.Systems.Placement;

namespace InteriorPlanner.Systems.UI
{
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
        private Vector3 lastKnownSizeInMeters; // Agora guarda o tamanho em METROS
        private Vector3 objectBaseSize; // O tamanho original do modelo 3D

        private void Start()
        {
            uiPanel.SetActive(false);

            if (closeButton) closeButton.onClick.AddListener(Deselect);
            if (deleteButton) deleteButton.onClick.AddListener(DeleteObject);

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

            if (selected != currentObject)
            {
                currentObject = selected;
                if (currentObject != null) ShowUI();
                else HideUI();
            }

            if (currentObject != null)
            {
                CheckForExternalChanges();
            }
        }

        private void ShowUI()
        {
            uiPanel.SetActive(true);
            if (objectNameText) objectNameText.text = currentObject.name.Replace("(Clone)", "").Trim();

            // Descobre o tamanho base original do modelo usando o BoxCollider
            BoxCollider boxCol = currentObject.GetComponent<BoxCollider>();
            objectBaseSize = boxCol != null ? boxCol.size : Vector3.one;

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
            
            // Calcula os Metros = Tamanho Original * Multiplicador de Escala
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

            if (objT.position != lastKnownPosition && !posX.isFocused && !posY.isFocused && !posZ.isFocused)
            {
                lastKnownPosition = objT.position;
                SetInputTextWithoutNotify(posX, lastKnownPosition.x);
                SetInputTextWithoutNotify(posY, lastKnownPosition.y);
                SetInputTextWithoutNotify(posZ, lastKnownPosition.z);
            }

            if (objT.eulerAngles != lastKnownRotation && !rotX.isFocused && !rotY.isFocused && !rotZ.isFocused)
            {
                lastKnownRotation = objT.eulerAngles;
                SetInputTextWithoutNotify(rotX, lastKnownRotation.x);
                SetInputTextWithoutNotify(rotY, lastKnownRotation.y);
                SetInputTextWithoutNotify(rotZ, lastKnownRotation.z);
            }

            // Verifica se a escala mudou de fora
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

            // POSIÇÃO
            float px = float.TryParse(posX.text, out float _px) ? _px : lastKnownPosition.x;
            float py = float.TryParse(posY.text, out float _py) ? _py : lastKnownPosition.y;
            float pz = float.TryParse(posZ.text, out float _pz) ? _pz : lastKnownPosition.z;

            // ROTAÇÃO
            float rx = float.TryParse(rotX.text, out float _rx) ? _rx : lastKnownRotation.x;
            float ry = float.TryParse(rotY.text, out float _ry) ? _ry : lastKnownRotation.y;
            float rz = float.TryParse(rotZ.text, out float _rz) ? _rz : lastKnownRotation.z;

            // TAMANHO EM METROS
            float sizeX = float.TryParse(scaleX.text, out float _sx) ? _sx : lastKnownSizeInMeters.x;
            float sizeY = float.TryParse(scaleY.text, out float _sy) ? _sy : lastKnownSizeInMeters.y;
            float sizeZ = float.TryParse(scaleZ.text, out float _sz) ? _sz : lastKnownSizeInMeters.z;

            // Limita o tamanho para não desaparecer (min: 10cm) nem ficar infinito (max: 15 metros)
            sizeX = Mathf.Clamp(sizeX, 0.1f, 15f);
            sizeY = Mathf.Clamp(sizeY, 0.1f, 15f);
            sizeZ = Mathf.Clamp(sizeZ, 0.1f, 15f);

            // Converte os metros de volta para Escala (Multiplicador)
            float newScaleX = objectBaseSize.x > 0 ? sizeX / objectBaseSize.x : 1f;
            float newScaleY = objectBaseSize.y > 0 ? sizeY / objectBaseSize.y : 1f;
            float newScaleZ = objectBaseSize.z > 0 ? sizeZ / objectBaseSize.z : 1f;

            // APLICA AO OBJETO
            if (currentObject.CanMove) currentObject.transform.position = new Vector3(px, py, pz);
            if (currentObject.CanRotate) currentObject.transform.eulerAngles = new Vector3(rx, ry, rz);
            if (currentObject.CanScale) currentObject.transform.localScale = new Vector3(newScaleX, newScaleY, newScaleZ);

            // Atualiza os valores conhecidos
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