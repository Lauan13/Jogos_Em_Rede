using UnityEngine;
using Unity.Netcode;

namespace JogosEmRede
{
    /// <summary>
    /// Controla o comportamento do pinguim quando ele perde o suporte do gelo.
    /// Agora preparado para rodar com Netcode: Desabar é autoritativo no servidor.
    /// </summary>
    public class Pinguim : NetworkBehaviour
    {
        private Rigidbody2D rb;
        private bool caiu = false;

        void Start()
        {
            // Tenta pegar o Rigidbody2D do pinguim
            rb = GetComponent<Rigidbody2D>();
            
            // Garante que ele comece parado (sem cair sozinho no início)
            if (rb != null)
            {
                rb.bodyType = RigidbodyType2D.Kinematic;
            }
        }

        /// <summary>
        /// Ativa a gravidade real para o pinguim despencar da tela.
        /// Em multiplayer: clientes pedem ao servidor para executar a queda.
        /// </summary>
        public void Desabar()
        {
            if (!IsServer)
            {
                DesabarServerRpc();
                return;
            }

            DesabarInternal();
        }

        [ServerRpc]
        private void DesabarServerRpc(ServerRpcParams rpcParams = default)
        {
            DesabarInternal();
        }

        private void DesabarInternal()
        {
            if (caiu) return;
            caiu = true;

            Debug.Log("[Pinguim] Socorro! Estou caindo!");

            if (rb != null)
            {
                // Muda para Dynamic para a gravidade da Unity puxá-lo para baixo
                rb.bodyType = RigidbodyType2D.Dynamic;
            }
            
            // Opcional: Desativa o colisor para ele não trombar em nada enquanto cai
            Collider2D col = GetComponent<Collider2D>();
            if (col != null) col.enabled = false;

            // Servidor despawna o objeto em 2 segundos (para que a animação ocorra)
            if (NetworkObject != null)
            {
                // Schedule despawn on server
                Invoke(nameof(DespawnSelf), 2f);
            }
            else
            {
                Destroy(gameObject, 2f);
            }
        }

        private void DespawnSelf()
        {
            if (NetworkObject != null && NetworkManager.Singleton != null && NetworkManager.Singleton.IsServer)
            {
                NetworkObject.Despawn(true);
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }
}