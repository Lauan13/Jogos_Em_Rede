using UnityEngine;
using UnityEngine.SceneManagement;
namespace JogosEmRede
{
    /// <summary>
    /// Carregador de cenas responsável por carregar a interface do jogo de forma aditiva.
    /// Verifica se a cena "CenaUI" já está carregada antes de fazer o carregamento.
    /// Uso do modo Additive permite que múltiplas cenas coexistam simultaneamente.
    /// </summary>
    public class CarregadorDeCenas : MonoBehaviour
    {
        void Start()
        {
            // Verifica se a cena "CenaUI" já está carregada
            if (!SceneManager.GetSceneByName("CenaUI").isLoaded)
            {
                // Carrega a cena de forma aditiva (sem descarregar outras cenas)
                SceneManager.LoadScene("CenaUI", LoadSceneMode.Additive);
                Debug.Log("[CarregadorDeCenas] Cena 'CenaUI' carregada com sucesso em modo Aditivo.");
            }
            else
            {
                Debug.Log("[CarregadorDeCenas] Cena 'CenaUI' já estava carregada. Nenhuma ação necessária.");
            }
        }
    }
}
