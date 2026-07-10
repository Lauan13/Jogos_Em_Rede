using UnityEngine;
using System.Collections.Generic;
using Unity.Netcode;

namespace JogosEmRede
{
    /// <summary>
    /// Controla a grade de blocos de gelo, turnos e a integridade estrutural do anel de gelo.
    /// Se o bloco central perder a conexão com as bordas externas (paredes), a estrutura desaba e o pinguim cai.
    /// </summary>
    public class GeradorDeTabuleiro : NetworkBehaviour
    {
        public static GeradorDeTabuleiro Instance;

        public GameObject blocoPrefab;

        private int colunas = 7;
        private int linhas = 7;

        public float espacamento = 1f;

        // Estado de turno/criativo sincronizado pelo servidor
        public NetworkVariable<int> forcaDoTurnoAtual = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
        public NetworkVariable<int> turnoAtual = new NetworkVariable<int>(1, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        // Representação compacta da grade: 1 = bloco existe, 0 = removido
        public NetworkList<int> gridState;

        private GameObject[,] grade;


        private bool emVerificacao = false;

        private int centerX;
        private int centerY;

        public event System.Action<int> OnTurnoAlterado;
        // Evento disparado quando o jogo termina. O int é o número do jogador vencedor (1 ou 2).
        public event System.Action<int> OnGameOver;

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else if (Instance != this)
            {
                Destroy(gameObject);
            }

            grade = new GameObject[colunas, linhas];
            centerX = (colunas - 1) / 2; // 3
            centerY = (linhas - 1) / 2; // 3
            // Inicializa a lista de estado da grade (servidor preencherá os valores no Start)
            gridState = new NetworkList<int>();
        }

        void Start()
        {
            // Subscrição para mudanças na lista de estado da grade
            gridState.OnListChanged += OnGridStateChanged;
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            if (IsServer)
            {
                // Servidor inicializa a grade e cria os objetos locais após spawn
                for (int x = 0; x < colunas; x++)
                {
                    for (int y = 0; y < linhas; y++)
                    {
                        float posX = (x - centerX) * espacamento;
                        float posY = (y - centerY) * espacamento;

                        Vector3 pos = new Vector3(posX, posY, 0f);
                        GameObject go = Instantiate(blocoPrefab, pos, Quaternion.identity, transform);
                        go.name = $"Bloco_{x}_{y}";

                        BlocoDeGelo bloco = go.GetComponent<BlocoDeGelo>();
                        if (bloco != null)
                        {
                            bloco.gridX = x;
                            bloco.gridY = y;

                            if (x == centerX && y == centerY)
                            {
                                bloco.protegido = true;
                            }
                        }
                        else
                        {
                            Debug.LogWarning("O prefab do bloco não tem o componente BlocoDeGelo.");
                        }

                        grade[x, y] = go;
                        gridState.Add(1); // bloco presente
                    }
                }
            }
            else
            {
                // Cliente: se o servidor já preencheu a lista (modo de reconexão), cria visuais
                if (gridState.Count == colunas * linhas)
                {
                    CreateAllVisualsFromGridState();
                }
            }
        }

        public void AlternarTurno()
        {
            // Clientes pedem ao servidor para alternar o turno; servidor executa diretamente
            if (!IsServer)
            {
                AlternarTurnoServerRpc();
                return;
            }

            ToggleTurno();
        }

        [ServerRpc]
        private void AlternarTurnoServerRpc(ServerRpcParams rpcParams = default)
        {
            ToggleTurno();
        }

        private void ToggleTurno()
        {
            turnoAtual.Value = (turnoAtual.Value == 1) ? 2 : 1;
            forcaDoTurnoAtual.Value = Random.Range(1, 5);
            Debug.Log($"[Turno] Agora é o turno do Jogador {turnoAtual.Value}. Força: {forcaDoTurnoAtual.Value}");
            OnTurnoAlterado?.Invoke(turnoAtual.Value);
        }

        public void ReportBlockDestroyed(int x, int y)
        {
            // If called on a client, forward to server
            if (!IsServer)
            {
                ReportBlockDestroyedServerRpc(x, y);
                return;
            }

            if (x < 0 || x >= colunas || y < 0 || y >= linhas)
            {
                Debug.LogWarning($"ReportBlockDestroyed chamado com coordenadas inválidas: ({x}, {y})");
                return;
            }

            if (grade[x, y] != null)
            {
                grade[x, y] = null;
                Debug.Log($"[Bloco Destruído] Bloco em ({x}, {y}) removido da grade.");
            }

            // Atualiza o estado de grade sincronizado para todos os clients
            int idx = x + y * colunas;
            if (idx >= 0 && idx < gridState.Count)
            {
                gridState[idx] = 0;
            }
            else if (idx >= 0 && IsServer && gridState.Count == 0)
            {
                // Caso raro: se gridState ainda não foi populada, tenta proteger contra exceção
            }

            ChecarEstabilidade();
        }

        [ServerRpc]
        private void ReportBlockDestroyedServerRpc(int x, int y, ServerRpcParams rpcParams = default)
        {
            ReportBlockDestroyed(x, y);
        }

        /// <summary>
        /// Verifica se a estrutura do jogo desabou baseando-se nas conexões com as bordas externas estáveis.
        /// </summary>
        public void ChecarEstabilidade()
        {
            if (!IsServer) // Autoridade apenas no servidor
                return;
            if (emVerificacao)
                return;

            emVerificacao = true;

            try
            {
                // Se o bloco central já sumiu, o jogo já acabou
                if (grade[centerX, centerY] == null)
                    return;

                // --- PASSO 1: FLOOD FILL A PARTIR DAS BORDAS EXTERNAS (PAREDES FIXAS) ---
                bool[,] conectadoAsBordas = new bool[colunas, linhas];
                Queue<Vector2Int> fila = new Queue<Vector2Int>();

                // Insere todos os blocos das extremidades (borda do tabuleiro real) que ainda existem
                for (int x = 0; x < colunas; x++)
                {
                    if (grade[x, 0] != null) { fila.Enqueue(new Vector2Int(x, 0)); conectadoAsBordas[x, 0] = true; }
                    if (grade[x, linhas - 1] != null) { fila.Enqueue(new Vector2Int(x, linhas - 1)); conectadoAsBordas[x, linhas - 1] = true; }
                }
                for (int y = 1; y < linhas - 1; y++)
                {
                    if (grade[0, y] != null) { fila.Enqueue(new Vector2Int(0, y)); conectadoAsBordas[0, y] = true; }
                    if (grade[colunas - 1, y] != null) { fila.Enqueue(new Vector2Int(colunas - 1, y)); conectadoAsBordas[colunas - 1, y] = true; }
                }

                int[] dx = { 1, -1, 0, 0 };
                int[] dy = { 0, 0, 1, -1 };

                // Propaga a estabilidade de fora para dentro
                while (fila.Count > 0)
                {
                    Vector2Int atual = fila.Dequeue();

                    for (int i = 0; i < 4; i++)
                    {
                        int nx = atual.x + dx[i];
                        int ny = atual.y + dy[i];

                        if (nx >= 0 && nx < colunas && ny >= 0 && ny < linhas)
                        {
                            if (grade[nx, ny] != null && !conectadoAsBordas[nx, ny])
                            {
                                conectadoAsBordas[nx, ny] = true;
                                fila.Enqueue(new Vector2Int(nx, ny));
                            }
                        }
                    }
                }

                // --- PASSO 2: SE O BLOCO CENTRAL NÃO SE CONECTA ÀS BORDAS, TUDO DESABA ---
                if (!conectadoAsBordas[centerX, centerY])
                {
                    // CORREÇÃO: Como o turno muda antes do bloco quebrar, o 'turnoAtual' já é o vencedor legítimo!
                    int vencedor = turnoAtual.Value;
                    int perdedor = (vencedor == 1) ? 2 : 1;
                    Debug.Log($"[Game Over] O anel de suporte foi rompido! O bloco central desabou! Jogador {perdedor} PERDEU! Jogador {vencedor} VENCEU.");

                    // --- PASSO 2 (alterado): Encontra o pinguim (pílula verde) e o remove da cena imediatamente ---
                    GameObject pinguimObj = GameObject.Find("Pinguim");
                    if (pinguimObj == null)
                    {
                        try
                        {
                            pinguimObj = GameObject.FindWithTag("Player");
                        }
                        catch (System.Exception)
                        {
                            pinguimObj = null;
                        }
                    }

                    if (pinguimObj != null)
                    {
                        // Se o pinguim tiver o componente Pinguim (NetworkBehaviour), peça para ele 'Desabar' (server-authoritative)
                        var pComp = pinguimObj.GetComponent<Pinguim>();
                        if (pComp != null)
                        {
                            pComp.Desabar();
                        }
                        else
                        {
                            // Fallback: se for um NetworkObject, despawna; caso contrário, destrói a GameObject
                            var netObj = pinguimObj.GetComponent<NetworkObject>();
                            if (netObj != null && IsServer)
                            {
                                netObj.Despawn(true);
                            }
                            else
                            {
                                Destroy(pinguimObj);
                            }
                        }
                    }
                    else
                    {
                        Debug.LogWarning("[GeradorDeTabuleiro] Pinguim não encontrado para destruir (nome 'Pinguim' ou Tag 'Player').");
                    }

                    // Notifica a UI / gerenciador de jogo sobre o vencedor correto
                    OnGameOver?.Invoke(vencedor);

                    // Derruba o centro e todos os blocos órfãos que perderam sustentação externa
                    for (int x = 0; x < colunas; x++)
                    {
                        for (int y = 0; y < linhas; y++)
                        {
                            if (grade[x, y] != null && !conectadoAsBordas[x, y])
                            {
                                GameObject go = grade[x, y];
                                grade[x, y] = null;
                                Destroy(go);
                            }
                        }
                    }

                    return;
                }

                // --- PASSO 3: LIMPEZA DE BLOCOS ISOLADOS E PONTAS SOLTAS ---
                List<Vector2Int> blocosParaDestruir = new List<Vector2Int>();

                for (int x = 0; x < colunas; x++)
                {
                    for (int y = 0; y < linhas; y++)
                    {
                        if (grade[x, y] == null) continue;

                        if (!conectadoAsBordas[x, y])
                        {
                            blocosParaDestruir.Add(new Vector2Int(x, y));
                            continue;
                        }

                        if (x == centerX && y == centerY) continue;

                        bool topoLivre = (y == linhas - 1) || (grade[x, y + 1] == null);
                        if (topoLivre)
                        {
                            bool temVizinhoEsquerda = (x > 0) && (grade[x - 1, y] != null);
                            bool temVizinhoDireita = (x < colunas - 1) && (grade[x + 1, y] != null);

                            if (!temVizinhoEsquerda && !temVizinhoDireita)
                            {
                                blocosParaDestruir.Add(new Vector2Int(x, y));
                            }
                        }
                    }
                }

                if (blocosParaDestruir.Count > 0)
                {
                    Debug.Log($"[Física] Removendo {blocosParaDestruir.Count} blocos sem sustentação mecânica.");
                    foreach (var coord in blocosParaDestruir)
                    {
                        GameObject go = grade[coord.x, coord.y];
                        grade[coord.x, coord.y] = null;
                        if (go != null) Destroy(go);
                    }

                    emVerificacao = false;
                    ChecarEstabilidade();
                    return;
                }
            }
            finally
            {
                emVerificacao = false;
            }
        }

        public GameObject GetBlock(int x, int y)
        {
            if (x < 0 || x >= colunas || y < 0 || y >= linhas)
                return null;

            return grade[x, y];
        }

        // ----- Helpers para sincronização visual da grade -----
        private void OnGridStateChanged(NetworkListEvent<int> changeEvent)
        {
            // Simples: ao detectar qualquer mudança, refazemos os visuais para garantir consistência.
            CreateAllVisualsFromGridState();
        }

        private void CreateAllVisualsFromGridState()
        {
            for (int x = 0; x < colunas; x++)
            {
                for (int y = 0; y < linhas; y++)
                {
                    int idx = x + y * colunas;
                    if (idx < gridState.Count && gridState[idx] == 1)
                    {
                        if (grade[x, y] == null) CreateVisualBlock(x, y);
                    }
                    else
                    {
                        RemoveVisualBlock(x, y);
                    }
                }
            }
        }

        private void CreateVisualBlock(int x, int y)
        {
            if (grade[x, y] != null) return;
            float posX = (x - centerX) * espacamento;
            float posY = (y - centerY) * espacamento;
            Vector3 pos = new Vector3(posX, posY, 0f);
            GameObject go = Instantiate(blocoPrefab, pos, Quaternion.identity, transform);
            go.name = $"Bloco_{x}_{y}";
            BlocoDeGelo bloco = go.GetComponent<BlocoDeGelo>();
            if (bloco != null)
            {
                bloco.gridX = x;
                bloco.gridY = y;
                if (x == centerX && y == centerY) bloco.protegido = true;
            }
            grade[x, y] = go;
        }

        private void RemoveVisualBlock(int x, int y)
        {
            if (x < 0 || x >= colunas || y < 0 || y >= linhas) return;
            GameObject go = grade[x, y];
            grade[x, y] = null;
            if (go != null) Destroy(go);
        }

        public int GetColunas()
        {
            return colunas;
        }

        public int GetLinhas()
        {
            return linhas;
        }

        public Vector2Int GetCenterCoordinates()
        {
            return new Vector2Int(centerX, centerY);
        }
    }
}