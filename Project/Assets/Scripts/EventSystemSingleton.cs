using UnityEngine;
using UnityEngine.EventSystems;
#if UNITY_6_0_OR_NEWER
using UnityEngine.SceneManagement;
#endif

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
            #if UNITY_6_0_OR_NEWER
            EventSystem[] eventSystems = Object.FindObjectsByType<EventSystem>(FindObjectsSortMode.None);
            #else
            EventSystem[] eventSystems = Object.FindObjectsOfType<EventSystem>();
            #endif
            
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


