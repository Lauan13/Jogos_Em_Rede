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

            // --- CONVERSÃO DE POSIÇÃO 2D ---
            Vector2 screenPos = Mouse.current.position.ReadValue();
            float zDistance = Mathf.Abs(Camera.main.transform.position.z);
            Vector3 mouseWorld3 = Camera.main.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, zDistance));
            Vector2 mouseWorld2 = new Vector2(mouseWorld3.x, mouseWorld3.y);

            Collider2D hitCollider = Physics2D.OverlapPoint(mouseWorld2);
            if (hitCollider == null)
            {
                RaycastHit2D hit = Physics2D.Raycast(mouseWorld2, Vector2.zero);
                hitCollider = hit.collider;
            }

            if (hitCollider == null) return; 

            BlocoDeGelo bloco = hitCollider.GetComponent<BlocoDeGelo>();
            if (bloco == null || bloco.protegido) return; 

            // Pega o componente NetworkObject do bloco clicado
            NetworkObject targetNetworkObject = bloco.GetComponent<NetworkObject>();
            if (targetNetworkObject == null) return;

            proximoCliquePermitido = Time.time + tempoDeEsperaEntreCliques;

            // Em vez de passar X e Y, passamos a referência DIRETA do objeto de rede clicado!
            ProcessarCliqueNoServidorRpc(targetNetworkObject);
        }

        [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
        private void ProcessarCliqueNoServidorRpc(NetworkObjectReference targetBlockRef, RpcParams rpcParams = default)
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

            // Resolve a referência enviada pelo Cliente de volta para o objeto no Servidor
            if (targetBlockRef.TryGet(out NetworkObject netObj))
            {
                BlocoDeGelo bloco = netObj.GetComponent<BlocoDeGelo>();
                if (bloco != null)
                {
                    // Aplica o dano EXATAMENTE no bloco que o Cliente clicou
                    bloco.ReceberDano(1);

                    // Alterna o turno
                    GeradorDeTabuleiro.Instance.AlternarTurno();
                }
            }
        }
    }
}