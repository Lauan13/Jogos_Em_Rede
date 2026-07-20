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
            if (Camera.main == null)
            {
                Debug.LogError("[ControleDeClique] Nenhuma câmera com a Tag 'MainCamera' foi encontrada na cena!");
                return;
            }

            // --- CONVERSÃO DE POSIÇÃO 2D PRECISA ---
            Vector2 screenPos = Mouse.current.position.ReadValue();
            
            // Usamos a distância Z real da câmera para não distorcer no espaço 2D
            float zDistance = Mathf.Abs(Camera.main.transform.position.z);
            Vector3 mouseWorld3 = Camera.main.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, zDistance));
            Vector2 mouseWorld2 = new Vector2(mouseWorld3.x, mouseWorld3.y);

            // Busca o Collider do bloco sob a ponta do cursor
            Collider2D hitCollider = Physics2D.OverlapPoint(mouseWorld2);

            // Fallback com Raycast 2D caso o OverlapPoint não encontre
            if (hitCollider == null)
            {
                RaycastHit2D hit = Physics2D.Raycast(mouseWorld2, Vector2.zero);
                hitCollider = hit.collider;
            }

            // --- TESTE DE DIAGNÓSTICO (Olhar no Console do Unity/Build) ---
            string nomeDoObjetoHit = hitCollider != null ? hitCollider.gameObject.name : "NENHUM";
            Debug.Log($"[DIAGNÓSTICO] Posição Clicada no Mundo: {mouseWorld2} | Objeto Encontrado: {nomeDoObjetoHit}");

            if (hitCollider == null) return; 

            BlocoDeGelo bloco = hitCollider.GetComponent<BlocoDeGelo>();
            if (bloco == null || bloco.protegido) return; 

            proximoCliquePermitido = Time.time + tempoDeEsperaEntreCliques;

            // Envia a solicitação do bloco exato clicado para o servidor
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

            // Aplica 1 de dano
            bloco.ReceberDano(1);

            // Alterna o turno
            GeradorDeTabuleiro.Instance.AlternarTurno();
        }
    }
}