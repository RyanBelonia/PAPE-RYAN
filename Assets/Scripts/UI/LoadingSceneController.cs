using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace InteriorPlanner.Systems.UI
{
    /// <summary>
    /// Gere a transição entre cenas com um ecrã de carregamento.
    /// Utiliza corrotinas para carregar a cena do Planner em background (async), 
    /// impedindo o "congelamento" da aplicação enquanto os dados pesados são carregados.
    /// </summary>
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

            // Carregamento Assíncrono: O Unity carrega a cena "Planner" sem bloquear o interface.
            AsyncOperation operation = SceneManager.LoadSceneAsync("Planner");
            
            // Impede que a cena abra automaticamente antes de termos terminado a animação da barra
            operation.allowSceneActivation = false;

            float displayedProgress = 0f;

            // Loop de carregamento até 90% (o Unity bloqueia o progresso em 0.9 ao carregar cenas)
            while (operation.progress < 0.9f)
            {
                float targetProgress = Mathf.Clamp01(operation.progress / 0.9f);
                // Move o progresso visual suavemente (Lerp/MoveTowards) para evitar saltos bruscos na barra
                displayedProgress = Mathf.MoveTowards(displayedProgress, targetProgress, Time.deltaTime);

                UpdateUI(displayedProgress);
                yield return null;
            }

            // Simulação de finalização: Dá tempo ao utilizador para ler a mensagem de "Concluído"
            while (displayedProgress < 1f)
            {
                displayedProgress = Mathf.MoveTowards(displayedProgress, 1f, Time.deltaTime * 0.5f);
                UpdateUI(displayedProgress);
                yield return null;
            }

            if (loadingText != null)
                loadingText.text = "Concluído";

            yield return new WaitForSeconds(0.2f);

            // Agora sim, autoriza a troca de cenas
            operation.allowSceneActivation = true;
        }

        private void UpdateUI(float progress)
        {
            if (progressFill != null)
                progressFill.fillAmount = progress; // Preenchimento radial ou horizontal da imagem

            if (progressPercentText != null)
                progressPercentText.text = Mathf.RoundToInt(progress * 100f) + "%";
        }
    }
}