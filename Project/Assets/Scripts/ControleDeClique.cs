using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace JogosEmRede
{
    // Detecta cliques do jogador sobre blocos de gelo usando o novo Input System
    // e possui uma trava de segurança para evitar cliques duplos/fantasmas em blocos destruídos.
    public class ControleDeClique : MonoBehaviour
    {
        [Header("Segurança")]
        [SerializeField] private float tempoDeEsperaEntreCliques = 0.3f; // 300 milissegundos de trava
        private float proximoCliquePermitido = 0f;

        void Update()
        {
            // TRAVA DE SEGURANÇA: Se o jogo estiver processando muito rápido, ignora o clique
            if (Time.time < proximoCliquePermitido)
                return;

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
            RaycastHit2D hit = Physics2D.Raycast(mouseWorld2, Vector2.zero);

            // Fallback: se Raycast não encontrou nada, tente OverlapPoint
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

            // --- ATIVA A TRAVA DE TEMPO ---
            // Diz para o script ignorar qualquer clique nos próximos 0.3 segundos
            proximoCliquePermitido = Time.time + tempoDeEsperaEntreCliques;

            // 5) BUSCA A FORÇA SORTEADA DO TURNO ATUAL
            int forcaAplicada = 1;
            if (GeradorDeTabuleiro.Instance != null)
            {
                forcaAplicada = GeradorDeTabuleiro.Instance.forcaDoTurnoAtual;
            }

            // 6) ALTERNA O TURNO PRIMEIRO
            if (GeradorDeTabuleiro.Instance != null)
            {
                GeradorDeTabuleiro.Instance.AlternarTurno();
            }
            else
            {
                Debug.LogWarning("GeradorDeTabuleiro.Instance é nulo ao tentar alternar turno.");
            }

            // 7) APLICA O DANO NO BLOCO POR ÚLTIMO
            bloco.ReceberDano(forcaAplicada);
        }
    }
}