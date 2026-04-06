using System.Globalization;
using TMPro;
using UnityEngine;
using InteriorPlanner.Core;
using InteriorPlanner.Data;

namespace InteriorPlanner.Systems.ProjectSetup
{
    public class NewProjectUIController : MonoBehaviour
    {
        [SerializeField] private TMP_InputField widthInput;
        [SerializeField] private TMP_InputField lengthInput;
        [SerializeField] private TMP_InputField heightInput;
        [SerializeField] private TMP_Text errorText;

        [SerializeField] private float defaultWidth = 4f;
        [SerializeField] private float defaultLength = 5f;
        [SerializeField] private float defaultHeight = 2.8f;

        private void Start()
        {
            widthInput.text = defaultWidth.ToString("0.0", CultureInfo.InvariantCulture);
            lengthInput.text = defaultLength.ToString("0.0", CultureInfo.InvariantCulture);
            heightInput.text = defaultHeight.ToString("0.0", CultureInfo.InvariantCulture);

            if (errorText != null)
                errorText.text = "";
        }

        public void OnClickCreateRoom()
        {
            if (!TryReadFloat(widthInput.text, out float width) ||
                !TryReadFloat(lengthInput.text, out float length) ||
                !TryReadFloat(heightInput.text, out float height))
            {
                ShowError("Preenche os campos com valores válidos.");
                return;
            }

            if (width <= 0 || length <= 0 || height <= 0)
            {
                ShowError("Os valores devem ser maiores que zero.");
                return;
            }

            RoomData roomData = new RoomData(width, length, height);
            AppManager.Instance.ProjectSession.CreateNewProject(roomData);
            SceneController.LoadLoading();
        }

        public void OnClickBack()
        {
            SceneController.LoadMainMenu();
        }

        private bool TryReadFloat(string value, out float result)
        {
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