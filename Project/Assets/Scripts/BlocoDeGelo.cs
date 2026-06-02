using UnityEngine;

namespace JogosEmRede
{
    // Script que representa um único bloco de gelo na grade 7x7.
    // Contém vida, coordenadas na grade e lógica básica de receber dano/quebrar.
    public class BlocoDeGelo : MonoBehaviour
    {
        // Vida atual do bloco. Pode ser configurada no Inspector.
        public int vidaAtual = 4;

        // Coordenadas deste bloco na grade (0..6)
        public int gridX;
        public int gridY;

        // Se verdadeiro, o bloco não recebe dano (usado para proteger o bloco central onde fica o pinguim)
        public bool protegido = false;

        void Start()
        {
            // Atualiza o sprite inicial conforme a vida (stub para o futuro)
            AtualizarSpriteRachadura();
        }

        // Função pública para aplicar dano ao bloco
        public void ReceberDano(int dano)
        {
            if (protegido)
                return; // bloco protegido não sofre dano

            vidaAtual -= dano;
            AtualizarSpriteRachadura();

            if (vidaAtual <= 0)
            {
                QuebrarBloco();
            }
        }

        // Atualizar a aparência do bloco (stub).
        // Aqui você poderá trocar sprites/animator conforme 'vidaAtual' para mostrar rachaduras.
        void AtualizarSpriteRachadura()
        {
            // Exemplo: mudar SpriteRenderer.sprite ou parâmetros do Animator conforme vidaAtual.
            // Deixe este método preenchido mais tarde com suas imagens de gelo rachado.
        }

        // Lida com a destruição do bloco: avisa o gerador de tabuleiro e destrói o GameObject.
        void QuebrarBloco()
        {
            if (GeradorDeTabuleiro.Instance != null)
            {
                GeradorDeTabuleiro.Instance.ReportBlockDestroyed(gridX, gridY);
            }
            else
            {
                Debug.LogWarning("GeradorDeTabuleiro.Instance não encontrado ao quebrar bloco.");
            }

            Destroy(gameObject);
        }
    }
}


