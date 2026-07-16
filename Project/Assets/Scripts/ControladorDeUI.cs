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

            // Tenta se inscrever nos eventos do Tabuleiro de forma segura
            InscreverNosEventosDoTabuleiro();
        }

        private void InscreverNosEventosDoTabuleiro()
        {
            if (GeradorDeTabuleiro.Instance != null)
            {
                // Nos inscrevemos no evento de mudança de turno
                GeradorDeTabuleiro.Instance.OnTurnoAlterado += AtualizarTextoTurno;
                GeradorDeTabuleiro.Instance.OnGameOver += MostrarTelaDeVitoria;

                // Inicializa a UI imediatamente com o turno que já está ativo
                AtualizarTextoTurno(GeradorDeTabuleiro.Instance.turnoAtual.Value);
                Debug.Log($"[UI] Inscrito com sucesso! Turno inicial: {GeradorDeTabuleiro.Instance.turnoAtual.Value}");
            }
            else
            {
                // Caso o Gerador ainda não tenha sido criado na rede, tentamos novamente no próximo frame
                Invoke(nameof(InscreverNosEventosDoTabuleiro), 0.1f);
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

        private void AtualizarTextoTurno(int turno)
        {
            if (textoTurno != null)
            {
                // Indica de quem é a vez de forma clara
                textoTurno.text = $"Vez do Jogador {turno}";
                Debug.Log($"[UI] Texto atualizado na tela: Vez do Jogador {turno}");
            }
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
                // Se for o servidor (Host), reinicia a cena de forma sincronizada na rede inteira
                NetworkManager.Singleton.SceneManager.LoadScene("QuebraGelo", LoadSceneMode.Single);
            }
            else if (NetworkManager.Singleton == null)
            {
                SceneManager.LoadScene("QuebraGelo");
            }
        }
    }
}