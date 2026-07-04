using System.Globalization;
using TMPro;
using UnityEngine;
using InteriorPlanner.Core;
using InteriorPlanner.Data;

namespace InteriorPlanner.Systems.ProjectSetup
{
    /// <summary>
    /// Controlador da Interface de Utilizador (UI) responsável pelo menu de criação de uma nova sala.
    /// Valida os inputs matemáticos do utilizador antes de gerar o espaço 3D.
    /// </summary>
    public class NewProjectUIController : MonoBehaviour
    {
        [SerializeField] private TMP_InputField widthInput;
        [SerializeField] private TMP_InputField lengthInput;
        [SerializeField] private TMP_InputField heightInput;
        [SerializeField] private TMP_Text errorText;

        // Medidas padrão (em metros) sugeridas ao utilizador quando o ecrã abre
        [SerializeField] private float defaultWidth = 4f;
        [SerializeField] private float defaultLength = 5f;
        [SerializeField] private float defaultHeight = 2.8f;

        private void Start()
        {
            // Preenche os campos de texto com os valores padrão.
            // O CultureInfo.InvariantCulture garante que os números usam ponto (.) em vez de vírgula,
            // evitando bugs de formatação entre computadores portugueses e americanos.
            widthInput.text = defaultWidth.ToString("0.0", CultureInfo.InvariantCulture);
            lengthInput.text = defaultLength.ToString("0.0", CultureInfo.InvariantCulture);
            heightInput.text = defaultHeight.ToString("0.0", CultureInfo.InvariantCulture);

            // Limpa mensagens de erro residuais
            if (errorText != null)
                errorText.text = "";
        }

        /// <summary>
        /// Acionado pelo botão "Criar" na interface gráfica.
        /// </summary>
        public void OnClickCreateRoom()
        {
            // Validação de Segurança 1: Tenta converter o texto em números (floats).
            // Se o utilizador escrever letras ou símbolos inválidos, a conversão falha e mostra erro.
            if (!TryReadFloat(widthInput.text, out float width) ||
                !TryReadFloat(lengthInput.text, out float length) ||
                !TryReadFloat(heightInput.text, out float height))
            {
                ShowError("Preenche os campos com valores válidos.");
                return;
            }

            // Validação de Segurança 2: Impede a criação de paredes com tamanho negativo ou zero,
            // o que quebraria o gerador 3D (RoomGenerator).
            if (width <= 0 || length <= 0 || height <= 0)
            {
                ShowError("Os valores devem ser maiores que zero.");
                return;
            }

            // Encapsula as medidas limpas num pacote de dados (RoomData)
            RoomData roomData = new RoomData(width, length, height);
            
            // Envia os dados para a memória central (AppManager) e regista o novo projeto
            AppManager.Instance.ProjectSession.CreateNewProject(roomData);
            
            // Avança para o ecrã de carregamento
            SceneController.LoadLoading();
        }

        public void OnClickBack()
        {
            SceneController.LoadMainMenu();
        }

        /// <summary>
        /// Função auxiliar que processa a string recebida e lida com erros comuns de digitação.
        /// </summary>
        private bool TryReadFloat(string value, out float result)
        {
            // Remove espaços em branco e substitui vírgulas por pontos para o conversor matemático não falhar
            value = value.Trim().Replace(',', '.');
            return float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out result);
        }

        private void ShowError(string message)
        {
            if (errorText != null)
                errorText.text = message;
        }
    }
}