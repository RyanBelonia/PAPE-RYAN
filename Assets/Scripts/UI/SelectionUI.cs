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

        [Header("Scale Inputs")]
        [SerializeField] private TMP_InputField scaleX;
        [SerializeField] private TMP_InputField scaleY;
        [SerializeField] private TMP_InputField scaleZ;

        private PlaceableObject currentObject;

        // Variáveis para saber quando o objeto foi movido pelo rato e não pela UI
        private Vector3 lastKnownPosition;
        private Vector3 lastKnownRotation;
        private Vector3 lastKnownScale;

        private void Start()
        {
            uiPanel.SetActive(false);

            if (closeButton) closeButton.onClick.AddListener(Deselect);
            if (deleteButton) deleteButton.onClick.AddListener(DeleteObject);

            // Adiciona os "Ouvintes" para quando o utilizador acaba de digitar (OnEndEdit)
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

            // 1. Mudança de Seleção (Clicou num móvel novo ou clicou no vazio)
            if (selected != currentObject)
            {
                currentObject = selected;
                if (currentObject != null) ShowUI();
                else HideUI();
            }

            // 2. Atualização em Tempo Real (Bidirecional)
            if (currentObject != null)
            {
                CheckForExternalChanges();
            }
        }

        private void ShowUI()
        {
            uiPanel.SetActive(true);
            if (objectNameText) objectNameText.text = currentObject.name.Replace("(Clone)", "").Trim();

            // Bloqueia ou desbloqueia as caixas de texto consoante as regras do móvel
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

            // Força a primeira leitura dos valores
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
            lastKnownScale = currentObject.transform.localScale;

            UpdateUIFields();
        }

        // Verifica se o móvel foi arrastado pelo rato (ObjectMover)
        private void CheckForExternalChanges()
        {
            Transform objT = currentObject.transform;

            // Se a posição mudou no mundo E o utilizador não está a escrever no InputField
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

            if (objT.localScale != lastKnownScale && !scaleX.isFocused && !scaleY.isFocused && !scaleZ.isFocused)
            {
                lastKnownScale = objT.localScale;
                SetInputTextWithoutNotify(scaleX, lastKnownScale.x);
                SetInputTextWithoutNotify(scaleY, lastKnownScale.y);
                SetInputTextWithoutNotify(scaleZ, lastKnownScale.z);
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

            SetInputTextWithoutNotify(scaleX, lastKnownScale.x);
            SetInputTextWithoutNotify(scaleY, lastKnownScale.y);
            SetInputTextWithoutNotify(scaleZ, lastKnownScale.z);
        }

        // Função de segurança para não disparar eventos em loop
        private void SetInputTextWithoutNotify(TMP_InputField input, float value)
        {
            if (input == null) return;
            input.SetTextWithoutNotify(value.ToString("F2")); // "F2" arredonda para 2 casas decimais visualmente
        }

        // --- CHAMADO QUANDO O UTILIZADOR DIGITA UM NÚMERO ---
        private void UpdateObjectTransform()
        {
            if (currentObject == null) return;

            // Lemos os textos e convertemos para números. Se o utilizador escrever letras, assume 0.
            float px = float.TryParse(posX.text, out float _px) ? _px : currentObject.transform.position.x;
            float py = float.TryParse(posY.text, out float _py) ? _py : currentObject.transform.position.y;
            float pz = float.TryParse(posZ.text, out float _pz) ? _pz : currentObject.transform.position.z;

            float rx = float.TryParse(rotX.text, out float _rx) ? _rx : currentObject.transform.eulerAngles.x;
            float ry = float.TryParse(rotY.text, out float _ry) ? _ry : currentObject.transform.eulerAngles.y;
            float rz = float.TryParse(rotZ.text, out float _rz) ? _rz : currentObject.transform.eulerAngles.z;

            float sx = float.TryParse(scaleX.text, out float _sx) ? _sx : currentObject.transform.localScale.x;
            float sy = float.TryParse(scaleY.text, out float _sy) ? _sy : currentObject.transform.localScale.y;
            float sz = float.TryParse(scaleZ.text, out float _sz) ? _sz : currentObject.transform.localScale.z;

            // Aplica ao objeto
            if (currentObject.CanMove) currentObject.transform.position = new Vector3(px, py, pz);
            if (currentObject.CanRotate) currentObject.transform.eulerAngles = new Vector3(rx, ry, rz);
            if (currentObject.CanScale) currentObject.transform.localScale = new Vector3(sx, sy, sz);

            // Atualiza a nossa memória para não haver conflitos
            lastKnownPosition = currentObject.transform.position;
            lastKnownRotation = currentObject.transform.eulerAngles;
            lastKnownScale = currentObject.transform.localScale;
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