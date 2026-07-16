using UnityEngine;
using Unity.Netcode;

namespace JogosEmRede
{
    /// <summary>
    /// Controla o estado de existência do pinguim.
    /// Quando ele perde a estabilidade, o servidor simplesmente o remove da rede.
    /// </summary>
    public class Pinguim : NetworkBehaviour
    {
        /// <summary>
        /// Faz o pinguim sumir da rede de forma síncrona.
        /// Roda exclusivamente no Servidor para garantir a autoridade da partida.
        /// </summary>
        public void Desabar()
        {
            if (!IsServer) return;

            Debug.Log("[Pinguim] Bloco de suporte destruído! Sumindo do tabuleiro...");

            if (NetworkObject != null && NetworkObject.IsSpawned)
            {
                // Remove o pinguim da tela de todos os jogadores na rede instantaneamente!
                NetworkObject.Despawn(true);
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }
}