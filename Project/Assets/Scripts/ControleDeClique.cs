using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using Unity.Netcode;

namespace JogosEmRede
{
    public class ControleDeClique : NetworkBehaviour
    {
        [Header("Segurança")]
        [SerializeField] private float tempoDeEsperaEntreCliques = 0.3f; 
        private float proximoCliquePermitido = 0f;

        void Update()
        {
            if (!IsSpawned) return;

            // Bloqueia o clique se não for a vez do jogador local
            if (GeradorDeTabuleiro.Instance != null)
            {
                int meuNumeroDeJogador = IsServer ? 1 : 2;
                if (GeradorDeTabuleiro.Instance.turnoAtual.Value != meuNumeroDeJogador)
                    return;
            }

            if (Time.time < proximoCliquePermitido) return;
            if (Mouse.current == null || !Mouse.current.leftButton.wasPressedThisFrame) return; 
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;
            if (Camera.main == null) return;

            Vector2 screenPos = Mouse.current.position.ReadValue();
            Vector3 mouseWorld3 = Camera.main.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, Camera.main.nearClipPlane));
            Vector2 mouseWorld2 = new Vector2(mouseWorld3.x, mouseWorld3.y);

            RaycastHit2D hit = Physics2D.Raycast(mouseWorld2, Vector2.zero);
            Collider2D hitCollider = hit.collider != null ? hit.collider : Physics2D.OverlapPoint(mouseWorld2);

            if (hitCollider == null) return; 

            BlocoDeGelo bloco = hitCollider.GetComponent<BlocoDeGelo>();
            if (bloco == null || bloco.protegido) return; 

            proximoCliquePermitido = Time.time + tempoDeEsperaEntreCliques;

            // Envia para o servidor processar
            ProcessarCliqueNoServidorServerRpc(bloco.gridX, bloco.gridY);
        }

        // Correção do aviso: Por padrão, no Netcode moderno, omitir parâmetros 
        // ou usar apenas o atributo sem argumentos já é o suficiente e evita warnings.
        [ServerRpc]
        private void ProcessarCliqueNoServidorServerRpc(int x, int y)
        {
            if (GeradorDeTabuleiro.Instance == null) return;

            GameObject blocoObj = GeradorDeTabuleiro.Instance.GetBlock(x, y);
            if (blocoObj == null) return;

            BlocoDeGelo bloco = blocoObj.GetComponent<BlocoDeGelo>();
            if (bloco == null) return;

            // APLICAÇÃO DIRETA: Por enquanto, aplicando força fixa 1.
            bloco.ReceberDano(1);

            // Muda o turno para o próximo jogador
            GeradorDeTabuleiro.Instance.AlternarTurno();
        }
    }
}