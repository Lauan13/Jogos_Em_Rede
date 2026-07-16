using UnityEngine;
using UnityEngine.EventSystems;

namespace JogosEmRede
{
    /// <summary>
    /// Garante que há apenas um EventSystem ativo na cena.
    /// Se múltiplos EventSystems forem detectados, desativa os extras.
    /// </summary>
    public class EventSystemSingleton : MonoBehaviour
    {
        void Awake()
        {
            // Usamos diretamente o FindObjectsByType com FindObjectsSortMode.None para evitar avisos de obsolescência
            EventSystem[] eventSystems = Object.FindObjectsByType<EventSystem>(FindObjectsSortMode.None);
            
            if (eventSystems.Length > 1)
            {
                Debug.LogWarning($"[EventSystemSingleton] Múltiplos EventSystems detectados ({eventSystems.Length}). Desativando os extras...");
                
                // Mantém o primeiro ativo, desativa os demais
                for (int i = 1; i < eventSystems.Length; i++)
                {
                    eventSystems[i].gameObject.SetActive(false);
                    Debug.Log($"EventSystem em '{eventSystems[i].gameObject.name}' foi desativado.");
                }
            }
            else if (eventSystems.Length == 0)
            {
                Debug.LogWarning("[EventSystemSingleton] Nenhum EventSystem encontrado na cena. Criando um novo...");
                GameObject go = new GameObject("EventSystem");
                go.AddComponent<EventSystem>();
                go.AddComponent<StandaloneInputModule>();
            }
        }
    }
}