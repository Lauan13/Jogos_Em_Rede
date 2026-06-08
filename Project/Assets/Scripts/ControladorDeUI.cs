using UnityEngine;
using TMPro; // Necessário para usar o TextMeshPro
using UnityEngine.SceneManagement; // OBRIGATÓRIO para conseguir reiniciar a cena

namespace JogosEmRede
{
    public class ControladorDeUI : MonoBehaviour
    {
        [Header("Configurações do Turno")]
        public TextMeshProUGUI textoTurno; // Arraste seu texto de turno aqui

        [Header("Configurações de Fim de Jogo")]
        public GameObject painelFimDeJogo; // Arraste o Painel/Pop-up de Game Over aqui
        public TextMeshProUGUI textoResultado; // Arraste o texto que diz quem ganhou aqui

        void Start()
        {
            // Garante que a tela de Game Over comece escondida ao iniciar o jogo
            if (painelFimDeJogo != null)
            {
                painelFimDeJogo.SetActive(false);
            }

            // Se inscreve nos eventos do tabuleiro para atualizar a tela automaticamente
            if (GeradorDeTabuleiro.Instance != null)
            {
                GeradorDeTabuleiro.Instance.OnTurnoAlterado += AtualizarTextoTurno;
                GeradorDeTabuleiro.Instance.OnGameOver += MostrarTelaDeVitoria;
            }
        }

        void OnDestroy()
        {
            // Limpa as inscrições de eventos para evitar erros de memória
            if (GeradorDeTabuleiro.Instance != null)
            {
                GeradorDeTabuleiro.Instance.OnTurnoAlterado -= AtualizarTextoTurno;
                GeradorDeTabuleiro.Instance.OnGameOver -= MostrarTelaDeVitoria;
            }
        }

        // Atualiza o texto normal de quem está jogando
        private void AtualizarTextoTurno(int turno)
        {
            if (textoTurno != null)
            {
                textoTurno.text = $"Vez do Jogador {turno}";
            }
        }

        /// <summary>
        /// Ativa a tela de fim de jogo e anuncia o grande vencedor!
        /// </summary>
        private void MostrarTelaDeVitoria(int jogadorVencedor)
        {
            if (painelFimDeJogo != null)
            {
                painelFimDeJogo.SetActive(true); // Acende a tela com o botão de reiniciar
            }

            if (textoResultado != null)
            {
                textoResultado.text = $"O Pinguim caiu!\n🏆 JOGADOR {jogadorVencedor} VENCEU! 🏆";
            }
        }

        /// <summary>
        /// FUNÇÃO PÚBLICA DO BOTÃO: Recarrega a partida do zero.
        /// Ela vai aparecer no seu menu 'No Function' agora por causa do 'public void'.
        /// </summary>
        public void ReiniciarPartida()
        {
            Debug.Log("[UI] O botão foi clicado! Reiniciando o tabuleiro...");
            
            // Certifique-se de que o nome da sua cena na Unity seja exatamente "QuebraGelo"
            SceneManager.LoadScene("QuebraGelo");
        }
    }
}