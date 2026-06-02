using UnityEngine;

namespace JogosEmRede
{
    // Comportamento do pinguim central.
    // Regras:
    // - No Start(), posiciona automaticamente na posição do bloco central (3,3).
    // - Monitora a existência do bloco central; se ele sumir, o pinguim "cai" e loga qual jogador perdeu.
    public class Pinguim : MonoBehaviour
    {
        // Coordenadas centrais fixas na grade 7x7
        private int centerX = 3;
        private int centerY = 3;

        // Flag para garantir que a lógica de queda execute apenas uma vez
        private bool caiu = false;

        void Start()
        {
            // Posiciona o pinguim no centro do tabuleiro, se possível
            if (GeradorDeTabuleiro.Instance == null)
            {
                Debug.LogWarning("GeradorDeTabuleiro.Instance não encontrado. Pinguim não foi posicionado no centro.");
                return;
            }

            GameObject blocoCentral = GeradorDeTabuleiro.Instance.GetBlock(centerX, centerY);
            if (blocoCentral != null)
            {
                transform.position = blocoCentral.transform.position;
            }
            else
            {
                Debug.LogWarning("Bloco central (3,3) não encontrado no Start() ao posicionar pinguim.");
            }
        }

        void Update()
        {
            if (caiu)
                return;

            if (GeradorDeTabuleiro.Instance == null)
                return;

            // Se o bloco central sumiu, o pinguim deve cair
            if (GeradorDeTabuleiro.Instance.GetBlock(centerX, centerY) == null)
            {
                Caiu();
            }
        }

        // Lógica que acontece quando o pinguim cai
        void Caiu()
        {
            caiu = true;

            int jogadorPerdedor = 0;
            if (GeradorDeTabuleiro.Instance != null)
            {
                jogadorPerdedor = GeradorDeTabuleiro.Instance.turnoAtual;
            }

            if (jogadorPerdedor == 0)
            {
                Debug.Log("Pinguim caiu! Não foi possível determinar o jogador que perdeu.");
            }
            else
            {
                Debug.Log($"Pinguim caiu! Jogador {jogadorPerdedor} perdeu o jogo!");
            }

            // Aqui você pode adicionar animações, efeitos e lógica de fim de jogo.
        }
    }
}

