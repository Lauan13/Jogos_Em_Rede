using UnityEngine;
using Unity.Netcode;

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

            // CORREÇÃO: Se o dano foi 4, batidasAtuais vai direto para 4. 
            // Se spritesRachadura.Length for 4, precisamos quebrar o bloco imediatamente!
            if (batidasAtuais < spritesRachadura.Length)
            {
                spriteRenderer.sprite = spritesRachadura[batidasAtuais];
                Debug.Log($"[Bloco] Sofreu dano! Batidas: {batidasAtuais}/{spritesRachadura.Length}");
            }
            else
            {
                // Se o dano atingiu ou passou do limite de sprites, o bloco quebra!
                QuebrarBloco();
            }
        }

        // CORREÇÃO: Removeu-se o método OnMouseDown() antigo que causava conflitos de clique duplicado!

        private void QuebrarBloco()
        {
            Debug.Log($"[Bloco] Bloco em ({gridX}, {gridY}) foi totalmente destruído!");

            // Solicita ao GeradorDeTabuleiro que registre a destruição (servidor faz a validação)
            if (GeradorDeTabuleiro.Instance != null)
            {
                GeradorDeTabuleiro.Instance.ReportBlockDestroyed(gridX, gridY);
                // Quem alterna o turno é o ControleDeClique; aqui apenas notificamos a remoção.
            }

            // Somente o servidor destrói fisicamente o GameObject localmente.
            if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsServer)
            {
                Destroy(gameObject);
            }
            // Clientes aguardam a atualização via NetworkList (gridState) que removerá o visual.
        }
    }
}