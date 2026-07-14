using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;

namespace JogosEmRede
{
    public class GeradorDeTabuleiro : NetworkBehaviour
    {
        public static GeradorDeTabuleiro Instance { get; private set; }

        [Header("Configurações do Tabuleiro")]
        public int largura = 7;
        public int altura = 7;
        public GameObject blocoPrefab; 
        public float espacamento = 1.1f; 

        [Header("Estado da Rede")]
        public NetworkVariable<int> turnoAtual = new NetworkVariable<int>(1, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        public System.Action<int> OnTurnoAlterado;
        public System.Action<int> OnGameOver;

        private Dictionary<Vector2Int, GameObject> matrizDeBlocos = new Dictionary<Vector2Int, GameObject>();
        private Vector2Int coordenadaPinguim;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);

            // O pinguim começa no centro do tabuleiro
            coordenadaPinguim = new Vector2Int(largura / 2, altura / 2);
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            if (IsServer)
            {
                GerarTabuleiroNoServidor();
                OnTurnoAlterado?.Invoke(turnoAtual.Value);
            }
        }

        private void GerarTabuleiroNoServidor()
        {
            int centroX = largura / 2;
            int centroY = altura / 2;
            Vector3 origem = new Vector3(-(largura - 1) * espacamento / 2f, -(altura - 1) * espacamento / 2f, 0);

            for (int x = 0; x < largura; x++)
            {
                for (int y = 0; y < altura; y++)
                {
                    Vector3 posicaoBloco = origem + new Vector3(x * espacamento, y * espacamento, 0);
                    GameObject novoBloco = Instantiate(blocoPrefab, posicaoBloco, Quaternion.identity);

                    BlocoDeGelo componenteBloco = novoBloco.GetComponent<BlocoDeGelo>();
                    if (componenteBloco != null)
                    {
                        componenteBloco.gridX = x;
                        componenteBloco.gridY = y;
                        if (x == centroX && y == centroY) componenteBloco.protegido = true;
                    }

                    NetworkObject netObj = novoBloco.GetComponent<NetworkObject>();
                    if (netObj != null) netObj.Spawn(true);

                    matrizDeBlocos[new Vector2Int(x, y)] = novoBloco;
                }
            }
        }

        public void AlternarTurno()
        {
            if (!IsServer) return;
            turnoAtual.Value = (turnoAtual.Value == 1) ? 2 : 1;
            OnTurnoAlterado?.Invoke(turnoAtual.Value);
            Debug.Log($"[Turno] Agora é a vez do Jogador {turnoAtual.Value}");
        }

        public GameObject GetBlock(int x, int y)
        {
            Vector2Int chave = new Vector2Int(x, y);
            return matrizDeBlocos.ContainsKey(chave) ? matrizDeBlocos[chave] : null;
        }

        public void ReportBlockDestroyed(int x, int y)
        {
            if (!IsServer) return;

            Vector2Int chave = new Vector2Int(x, y);
            if (matrizDeBlocos.ContainsKey(chave))
            {
                GameObject blocoObj = matrizDeBlocos[chave];
                matrizDeBlocos.Remove(chave);

                if (blocoObj != null)
                {
                    NetworkObject netObj = blocoObj.GetComponent<NetworkObject>();
                    if (netObj != null && netObj.IsSpawned) netObj.Despawn(true);
                }
            }

            // APÓS DESTRUIR UM BLOCO: O servidor recalcula a física e desaba o que estiver flutuando!
            VerificarEstruturaDoTabuleiro();
        }

        /// <summary>
        /// Varre o tabuleiro usando BFS (Busca em Largura) a partir das bordas para detectar blocos soltos e queda do pinguim.
        /// </summary>
        private void VerificarEstruturaDoTabuleiro()
        {
            if (!IsServer) return;

            HashSet<Vector2Int> blocosConectadosAsBordas = new HashSet<Vector2Int>();
            Queue<Vector2Int> filaDeBusca = new Queue<Vector2Int>();

            // 1. Encontra todos os blocos sobreviventes que estão nas bordas (X=0, X=largura-1, Y=0, Y=altura-1)
            foreach (var par in matrizDeBlocos)
            {
                Vector2Int pos = par.Key;
                if (pos.x == 0 || pos.x == largura - 1 || pos.y == 0 || pos.y == altura - 1)
                {
                    blocosConectadosAsBordas.Add(pos);
                    filaDeBusca.Enqueue(pos);
                }
            }

            // 2. Flood Fill / BFS: Espalha a conexão de segurança das bordas para o centro
            Vector2Int[] direcoes = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

            while (filaDeBusca.Count > 0)
            {
                Vector2Int atual = filaDeBusca.Dequeue();

                foreach (Vector2Int dir in direcoes)
                {
                    Vector2Int vizinho = atual + dir;

                    if (matrizDeBlocos.ContainsKey(vizinho) && !blocosConectadosAsBordas.Contains(vizinho))
                    {
                        blocosConectadosAsBordas.Add(vizinho);
                        filaDeBusca.Enqueue(vizinho);
                    }
                }
            }

            // 3. Identifica quais blocos ficaram isolados (não foram alcançados pela busca)
            List<Vector2Int> blocosParaDerrubar = new List<Vector2Int>();
            foreach (var par in matrizDeBlocos)
            {
                if (!blocosConectadosAsBordas.Contains(par.Key))
                {
                    blocosParaDerrubar.Add(par.Key);
                }
            }

            // 4. Derruba os blocos soltos
            foreach (Vector2Int posIsolada in blocosParaDerrubar)
            {
                GameObject blocoObj = matrizDeBlocos[posIsolada];
                if (blocoObj != null)
                {
                    Debug.Log($"[Física] Bloco em ({posIsolada.x}, {posIsolada.y}) ficou flutuando e vai cair!");
                    
                    // Remove da matriz física e despawna na rede
                    matrizDeBlocos.Remove(posIsolada);
                    NetworkObject netObj = blocoObj.GetComponent<NetworkObject>();
                    if (netObj != null && netObj.IsSpawned) netObj.Despawn(true);
                }
            }

            // 5. VERIFICAÇÃO DO PINGUIM: O bloco central (coordenadaPinguim) ainda está conectado às bordas?
            if (!blocosConectadosAsBordas.Contains(coordenadaPinguim))
            {
                DerrubarPinguim();
            }
        }

        private void DerrubarPinguim()
        {
            Pinguim pinguimObj = FindFirstObjectByType<Pinguim>();
            if (pinguimObj != null)
            {
                Debug.Log("[Física] O pinguim perdeu a conexão com as bordas e vai cair!");
                pinguimObj.Desabar();

                // Quem ganha é quem NÃO jogou na vez que causou a queda (ou seja, o jogador ativo atual)
                int jogadorVencedor = (turnoAtual.Value == 1) ? 1 : 2;
                FinalizarJogo(jogadorVencedor);
            }
        }

        public void FinalizarJogo(int jogadorVencedor)
        {
            if (!IsServer) return;
            OnGameOver?.Invoke(jogadorVencedor);
        }
    }
}