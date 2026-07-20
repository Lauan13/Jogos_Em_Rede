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

            // 1. Trava local: Bloqueia a tentativa de clique se não for a vez do jogador local
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

            // Envia a solicitação de clique para o servidor
            ProcessarCliqueNoServidorRpc(bloco.gridX, bloco.gridY);
        }

        [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
        private void ProcessarCliqueNoServidorRpc(int x, int y, RpcParams rpcParams = default)
        {
            if (GeradorDeTabuleiro.Instance == null) return;

            // Trava de segurança no Servidor
            ulong idDoJogadorQueClicou = rpcParams.Receive.SenderClientId;
            int turnoDoJogador = (idDoJogadorQueClicou == NetworkManager.ServerClientId) ? 1 : 2;

            if (GeradorDeTabuleiro.Instance.turnoAtual.Value != turnoDoJogador)
            {
                Debug.LogWarning($"[Servidor] Jogador {turnoDoJogador} tentou clicar fora do seu turno!");
                return;
            }

            GameObject blocoObj = GeradorDeTabuleiro.Instance.GetBlock(x, y);
            if (blocoObj == null) return;

            BlocoDeGelo bloco = blocoObj.GetComponent<BlocoDeGelo>();
            if (bloco == null) return;

            // Aplica 1 de dano no bloco
            bloco.ReceberDano(1);

            // Alterna o turno para o próximo jogador
            GeradorDeTabuleiro.Instance.AlternarTurno();
        }
    }
}