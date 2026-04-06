using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace InteriorPlanner.Systems.UI
{
    public class LoadingSceneController : MonoBehaviour
    {
        [SerializeField] private Image progressFill;
        [SerializeField] private TMP_Text progressPercentText;
        [SerializeField] private TMP_Text loadingText;

        private void Start()
        {
            StartCoroutine(LoadPlannerScene());
        }

        private IEnumerator LoadPlannerScene()
        {
            if (loadingText != null)
                loadingText.text = "A criar ambiente...";

            AsyncOperation operation = SceneManager.LoadSceneAsync("Planner");
            operation.allowSceneActivation = false;

            float displayedProgress = 0f;

            while (operation.progress < 0.9f)
            {
                float targetProgress = Mathf.Clamp01(operation.progress / 0.9f);
                displayedProgress = Mathf.MoveTowards(displayedProgress, targetProgress, Time.deltaTime);

                UpdateUI(displayedProgress);
                yield return null;
            }

            // Simular a parte final da criação
            while (displayedProgress < 1f)
            {
                displayedProgress = Mathf.MoveTowards(displayedProgress, 1f, Time.deltaTime * 0.5f);
                UpdateUI(displayedProgress);
                yield return null;
            }

            if (loadingText != null)
                loadingText.text = "Concluído";

            yield return new WaitForSeconds(0.2f);

            operation.allowSceneActivation = true;
        }

        private void UpdateUI(float progress)
        {
            if (progressFill != null)
                progressFill.fillAmount = progress;

            if (progressPercentText != null)
                progressPercentText.text = Mathf.RoundToInt(progress * 100f) + "%";
        }
    }
}