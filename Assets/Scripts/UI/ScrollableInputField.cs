using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

namespace InteriorPlanner.UI
{
    /// <summary>
    /// Componente de UX "Qualidade de Vida". Permite ao utilizador alterar números 
    /// na UI apenas rolando a roda do rato, evitando a digitação constante.
    /// </summary>
    [RequireComponent(typeof(TMP_InputField))]
    public class ScrollableInputField : MonoBehaviour, IScrollHandler
    {
        [Tooltip("Quanto o valor aumenta/diminui por cada clique da roda do rato.")]
        [SerializeField] private float scrollStep = 0.1f;
        
        [SerializeField] private bool useLimits = false;
        [SerializeField] private float minValue = 0.1f;
        [SerializeField] private float maxValue = 10f;

        private TMP_InputField inputField;

        private void Awake()
        {
            inputField = GetComponent<TMP_InputField>();
        }

        // Interface IScrollHandler: deteta quando o utilizador roda o rato sobre este objeto
        public void OnScroll(PointerEventData eventData)
        {
            if (eventData.scrollDelta.y == 0) return;

            float scrollDirection = Mathf.Sign(eventData.scrollDelta.y);

            if (float.TryParse(inputField.text, out float currentValue))
            {
                float newValue = currentValue + (scrollDirection * scrollStep);

                if (useLimits)
                {
                    newValue = Mathf.Clamp(newValue, minValue, maxValue);
                }

                // Formatação: "F2" garante que o número aparece com 2 casas decimais (ex: 4.00)
                inputField.text = newValue.ToString("F2");

                // Invoca o evento de mudança. O sistema reage em tempo real 
                // sem o utilizador ter de carregar em "Enter".
                inputField.onValueChanged.Invoke(inputField.text);
            }
        }
    }
}