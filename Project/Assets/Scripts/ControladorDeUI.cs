using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using Unity.Netcode;

namespace JogosEmRede
{
    public class ControladorDeUI : MonoBehaviour
    {
        [Header("Configurações do Turno")]
        public TextMeshProUGUI textoTurno; 

        [Header("Configurações de Fim de Jogo")]
        public GameObject painelFimDeJogo; 
        public TextMeshProUGUI textoResultado; 

        void Start()
        {
            if (painelFimDeJogo != null) painelFimDeJogo.SetActive(false);

            if (GeradorDeTabuleiro.Instance != null)
            {
                GeradorDeTabuleiro.Instance.OnTurnoAlterado += AtualizarTextoTurno;
                GeradorDeTabuleiro.Instance.OnGameOver += MostrarTelaDeVitoria;

                // Inicializa a UI com o estado atual do tabuleiro
                AtualizarTextoTurno(GeradorDeTabuleiro.Instance.turnoAtual.Value);
            }
        }

        void OnDestroy()
        {
            if (GeradorDeTabuleiro.Instance != null)
            {
                GeradorDeTabuleiro.Instance.OnTurnoAlterado -= AtualizarTextoTurno;
                GeradorDeTabuleiro.Instance.OnGameOver -= MostrarTelaDeVitoria;
            }
        }

        // Exibição super limpa, sem depender de dados aleatórios
        private void UpdateTextoTurnoLocal(int turno)
        {
            if (textoTurno != null)
            {
                textoTurno.text = $"Vez do Jogador {turno}";
            }
        }

        private void AtualizarTextoTurno(int turno)
        {
            UpdateTextoTurnoLocal(turno);
        }

        private void MostrarTelaDeVitoria(int jogadorVencedor)
        {
            if (painelFimDeJogo != null) painelFimDeJogo.SetActive(true);
            if (textoResultado != null)
            {
                textoResultado.text = $"       O Pinguim caiu!\n JOGADOR {jogadorVencedor} VENCEU! ";
            }
        }

        public void ReiniciarPartida()
        {
            if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsServer)
            {
                // Se for o servidor, reinicia a cena para a rede inteira
                NetworkManager.Singleton.SceneManager.LoadScene("QuebraGelo", LoadSceneMode.Single);
            }
            else if (NetworkManager.Singleton == null)
            {
                SceneManager.LoadScene("QuebraGelo");
            }
        }
    }
}