using UnityEngine;
using TMPro;

namespace JogosEmRede
{
    /// <summary>
    /// Controlador de UI que gerencia a exibição de turnos.
    /// Usa o padrão Observer (Eventos) para ficar desacoplado da lógica do tabuleiro.
    /// Se inscreve no evento OnTurnoAlterado do GeradorDeTabuleiro para atualizar a UI.
    /// </summary>
    public class ControladorDeUI : MonoBehaviour
    {
        // Referência ao texto TextMeshPro que exibirá o turno atual
        public TextMeshProUGUI textoTurnoUI;

        void Start()
        {
            // Validar se a referência ao texto foi atribuída no Inspector
            if (textoTurnoUI == null)
            {
                Debug.LogError("[ControladorDeUI] TextMeshProUGUI não foi atribuído no Inspector!");
                return;
            }

            // Validar se o GeradorDeTabuleiro singleton existe
            if (GeradorDeTabuleiro.Instance == null)
            {
                Debug.LogError("[ControladorDeUI] GeradorDeTabuleiro.Instance não encontrado!");
                return;
            }

            // Se inscrever no evento de alternância de turno
            GeradorDeTabuleiro.Instance.OnTurnoAlterado += AtualizarTextoTurno;

            // Exibir o turno atual logo no início
            AtualizarTextoTurno(GeradorDeTabuleiro.Instance.turnoAtual);

            Debug.Log("[ControladorDeUI] UI de turnos inicializada com sucesso.");
        }

        /// <summary>
        /// Atualiza o texto da UI com o número do jogador atual.
        /// </summary>
        /// <param name="novoTurno">Número do novo turno (1 ou 2)</param>
        private void AtualizarTextoTurno(int novoTurno)
        {
            if (textoTurnoUI != null)
            {
                textoTurnoUI.text = "Vez do Jogador " + novoTurno;
                Debug.Log($"[ControladorDeUI] Texto atualizado: Vez do Jogador {novoTurno}");
            }
        }

        void OnDestroy()
        {
            // Se desinscrever do evento para evitar erros de memória (memory leaks)
            if (GeradorDeTabuleiro.Instance != null)
            {
                GeradorDeTabuleiro.Instance.OnTurnoAlterado -= AtualizarTextoTurno;
                Debug.Log("[ControladorDeUI] Desinscrição do evento de turno realizada.");
            }
        }
    }
}

