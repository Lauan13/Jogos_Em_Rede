using UnityEngine;

namespace JogosEmRede
{
    public class BlocoDeGelo : MonoBehaviour
    {
        // Coordenadas do bloco na grade (preenchidas pelo Gerador)
        [HideInInspector] public int gridX;
        [HideInInspector] public int gridY;
        [HideInInspector] public bool protegido = false;

        [Header("Configurações de Rachadura")]
        // Arraste suas 4 imagens aqui no Inspector (Element 0, Element 1, Element 2, Element 3)
        public Sprite[] spritesRachadura; 
        
        private int batidasAtuais = 0;
        private SpriteRenderer spriteRenderer;

        void Awake()
        {
            // Pega o componente que desenha a imagem do bloco
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        void Start()
        {
            // Garante que o bloco começa com a primeira imagem (intacto) se houver sprites na lista
            if (spritesRachadura != null && spritesRachadura.Length > 0)
            {
                spriteRenderer.sprite = spritesRachadura[0];
            }
        }

        /// <summary>
        /// Aplica dano ao bloco. Se o bloco não estiver protegido, incrementa o contador de batidas,
        /// atualiza o sprite de rachadura e destrói o bloco se exceder o limite de imagens.
        /// </summary>
        /// <param name="dano">Quantidade de dano a aplicar (padrão: 1)</param>
        public void ReceberDano(int dano = 1)
        {
            // Se for o bloco central protegido, não faz nada
            if (protegido) return;

            // Avança para a próxima batida/rachadura
            batidasAtuais += dano;

            // Se ainda não estourou o limite de imagens, troca o visual
            if (batidasAtuais < spritesRachadura.Length)
            {
                spriteRenderer.sprite = spritesRachadura[batidasAtuais];
                Debug.Log($"[Bloco] Sofreu dano! Batidas: {batidasAtuais}/{spritesRachadura.Length}");
            }
            else
            {
                // Se passou da última imagem, o bloco quebra de verdade!
                QuebrarBloco();
            }
        }

        private void OnMouseDown()
        {
            // Chama o método público de dano
            ReceberDano(1);
        }

        private void QuebrarBloco()
        {
            Debug.Log($"[Bloco] Bloco em ({gridX}, {gridY}) foi totalmente destruído!");
            
            // Avisa o tabuleiro para remover este bloco da lógica e checar a estabilidade
            if (GeradorDeTabuleiro.Instance != null)
            {
                GeradorDeTabuleiro.Instance.ReportBlockDestroyed(gridX, gridY);
                GeradorDeTabuleiro.Instance.AlternarTurno();
            }

            // Remove o objeto do jogo
            Destroy(gameObject);
        }
    }
}