using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

namespace InteriorPlanner.UI
{
    [RequireComponent(typeof(TMP_InputField))]
    public class ScrollableInputField : MonoBehaviour, IScrollHandler
    {
        [Tooltip("Quanto o valor aumenta/diminui por cada 'clique' da roda do rato.")]
        [SerializeField] private float scrollStep = 0.1f;
        
        [Tooltip("Limites opcionais para não deixar o valor ficar gigante ou negativo.")]
        [SerializeField] private bool useLimits = false;
        [SerializeField] private float minValue = 0.1f;
        [SerializeField] private float maxValue = 10f;

        private TMP_InputField inputField;

        private void Awake()
        {
            // Apanha a caixinha de texto automaticamente
            inputField = GetComponent<TMP_InputField>();
        }

        // Esta função é chamada automaticamente pela Unity quando usas o scroll do rato em cima do objeto
        public void OnScroll(PointerEventData eventData)
        {
            // Ignora se o scroll for 0
            if (eventData.scrollDelta.y == 0) return;

            // Vê se rodaste para cima (+1) ou para baixo (-1)
            float scrollDirection = Mathf.Sign(eventData.scrollDelta.y);

            // Tenta converter o texto que lá está num número
            if (float.TryParse(inputField.text, out float currentValue))
            {
                // Soma ou subtrai o valor do passo
                float newValue = currentValue + (scrollDirection * scrollStep);

                // Aplica limites (se ativares isso no Inspector)
                if (useLimits)
                {
                    newValue = Mathf.Clamp(newValue, minValue, maxValue);
                }

                // Escreve o novo valor na caixinha (com 2 casas decimais)
                inputField.text = newValue.ToString("F2");

                // Força o InputField a avisar o resto do jogo que o valor mudou
                // Assim a divisória estica logo na hora, sem teres de carregar no Enter!
                inputField.onValueChanged.Invoke(inputField.text);
            }
        }
    }
}