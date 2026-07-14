using UnityEngine;
using Unity.Netcode; 

namespace JogosEmRede
{
    /// <summary>
    /// Controla o estado de rachadura e destruição de um bloco de gelo na rede.
    /// </summary>
    public class BlocoDeGelo : NetworkBehaviour
    {
        // Coordenadas do bloco na grade (preenchidas pelo Gerador)
        [HideInInspector] public int gridX;
        [HideInInspector] public int gridY;
        [HideInInspector] public bool protegido = false;

        [Header("Configurações de Rachadura")]
        public Sprite[] spritesRachadura; 
        
        // A vida/batidas do bloco é uma NetworkVariable para sincronizar entre as máquinas
        public NetworkVariable<int> batidasAtuais = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
        
        private SpriteRenderer spriteRenderer;

        void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            // Vincula o evento: Sempre que o valor de batidasAtuais mudar na rede, roda a função para atualizar o sprite
            batidasAtuais.OnValueChanged += AoMudarBatidas;

            // Garante que começa com o sprite correto atualizado
            AtualizarSpriteVisual(batidasAtuais.Value);
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();
            // Remove a vinculação do evento ao sumir da rede para evitar vazamento de memória
            batidasAtuais.OnValueChanged -= AoMudarBatidas;
        }

        private void AoMudarBatidas(int valorAntigo, int valorNovo)
        {
            AtualizarSpriteVisual(valorNovo);
        }

        private void AktualisarSpriteVisual(int batidas)
        {
            // Nota: Se houver algum erro de digitação no nome original, mantemos exatamente como estava às 10:50.
        }

        private void AtualizarSpriteVisual(int batidas)
        {
            if (spritesRachadura != null && batidas < spritesRachadura.Length && batidas >= 0)
            {
                spriteRenderer.sprite = spritesRachadura[batidas];
            }
        }

        /// <summary>
        /// Aplica dano ao bloco. Roda com segurança apenas no Servidor.
        /// </summary>
        public void ReceberDano(int dano = 1)
        {
            if (!IsServer) return;
            if (protegido) return;

            // Avança para a próxima batida/rachadura na variável de rede
            batidasAtuais.Value += dano;

            if (batidasAtuais.Value < spritesRachadura.Length)
            {
                Debug.Log($"[Servidor] Bloco ({gridX}, {gridY}) sofreu dano! Batidas: {batidasAtuais.Value}/{spritesRachadura.Length}");
            }
            else
            {
                QuebrarBloco();
            }
        }

        private void QuebrarBloco()
        {
            if (!IsServer) return;

            Debug.Log($"[Servidor] Bloco em ({gridX}, {gridY}) foi totalmente destruído!");

            // Solicita ao GeradorDeTabuleiro que registre a destruição na matriz de física
            if (GeradorDeTabuleiro.Instance != null)
            {
                GeradorDeTabuleiro.Instance.ReportBlockDestroyed(gridX, gridY);
            }
        }
    }
}