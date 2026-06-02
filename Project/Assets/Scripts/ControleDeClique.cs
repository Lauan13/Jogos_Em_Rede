using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace JogosEmRede
{
    // Detecta cliques do jogador sobre blocos de gelo usando o novo Input System
    // e aplica dano/alternância de turno.
    // Observação: cada bloco precisa ter um Collider2D para ser detectado por Physics2D.
    public class ControleDeClique : MonoBehaviour
    {
        void Update()
        {
            // 1) Detecta clique com o botão esquerdo usando o novo Input System
            if (Mouse.current == null)
                return; // sem dispositivo de mouse disponível

            if (!Mouse.current.leftButton.wasPressedThisFrame)
                return; // nenhum clique detectado neste frame

            // Opcional: evita clicar por cima de UI (se houver)
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                return;

            // 2) Converte posição do mouse (tela) para o mundo 2D
            if (Camera.main == null)
            {
                Debug.LogWarning("Nenhuma Camera.main encontrada para converter ScreenToWorldPoint.");
                return;
            }

            Vector2 screenPos = Mouse.current.position.ReadValue();
            Vector3 mouseWorld3 = Camera.main.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, Camera.main.nearClipPlane));
            Vector2 mouseWorld2 = new Vector2(mouseWorld3.x, mouseWorld3.y);

            // 3) Dispara um Raycast 2D na posição do mouse.
            // Em 2D, para detecção pontual usamos OverlapPoint como fallback.
            RaycastHit2D hit = Physics2D.Raycast(mouseWorld2, Vector2.zero);

            // Fallback: se Raycast não encontrou nada, tente OverlapPoint (mais confiável para cliques pontuais)
            Collider2D hitCollider = hit.collider != null ? hit.collider : Physics2D.OverlapPoint(mouseWorld2);

            if (hitCollider == null)
                return; // nada clicado

            // 4) Verifica se o objeto clicado tem BlocoDeGelo
            BlocoDeGelo bloco = hitCollider.GetComponent<BlocoDeGelo>();
            if (bloco == null)
                return; // o objeto não é um bloco de gelo

            // Se o bloco estiver protegido (centro), ignore
            if (bloco.protegido)
            {
                Debug.Log("Bloco protegido (centro) foi clicado, ignorando.");
                return;
            }

            // Aplica 1 ponto de dano
            bloco.ReceberDano(1);

            // Alterna o turno após a jogada (checa se o singleton existe)
            if (GeradorDeTabuleiro.Instance != null)
            {
                GeradorDeTabuleiro.Instance.AlternarTurno();
            }
            else
            {
                Debug.LogWarning("GeradorDeTabuleiro.Instance é nulo ao tentar alternar turno.");
            }
        }
    }
}


