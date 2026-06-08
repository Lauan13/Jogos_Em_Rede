using UnityEngine;

namespace JogosEmRede
{
    /// <summary>
    /// Controla o comportamento do pinguim quando ele perde o suporte do gelo.
    /// </summary>
    public class Pinguim : MonoBehaviour
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
        /// </summary>
        public void Desabar()
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

            // Destrói o objeto do pinguim após 2 segundos de queda para limpar a memória
            Destroy(gameObject, 2f);
        }
    }
}