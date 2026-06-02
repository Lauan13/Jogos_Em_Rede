using UnityEngine;
using System.Collections.Generic;

namespace JogosEmRede
{
    /// <summary>
    /// Gera e controla a grade 7x7 de blocos de gelo, controla turnos e verifica estabilidade.
    /// Usa BFS para determinar quais blocos estão conectados às bordas seguras (bordas extremas da grade).
    /// Blocos desconectados são destruídos automaticamente.
    /// </summary>
    public class GeradorDeTabuleiro : MonoBehaviour
    {
        // Instância singleton para que os blocos possam avisar quando quebram.
        public static GeradorDeTabuleiro Instance;

        // Prefab do bloco de gelo (deve conter o componente BlocoDeGelo)
        public GameObject blocoPrefab;

        // Tamanho fixo da grade
        private int colunas = 7;
        private int linhas = 7;

        // Espaçamento entre blocos (configurável no Inspector)
        public float espacamento = 1f;

        // Matriz que guarda referências aos blocos instanciados
        private GameObject[,] grade;

        // Controle de turnos: 1 = Jogador 1, 2 = Jogador 2
        public int turnoAtual = 1;

        // Flag para evitar verificações em cascata simultâneas
        private bool emVerificacao = false;

        // Coordenadas do bloco central
        private int centerX;
        private int centerY;

        /// <summary>
        /// Evento disparado quando o turno é alternado. Permite que outros scripts se inscrevam para ser notificados.
        /// Padrão Observer para desacoplamento entre lógica de jogo e UI.
        /// </summary>
        public event System.Action<int> OnTurnoAlterado;

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
        }

        void Start()
        {
            // Instancia todos os blocos da grade 7x7
            for (int x = 0; x < colunas; x++)
            {
                for (int y = 0; y < linhas; y++)
                {
                    // Calcula posição para centralizar a grade em torno da origem (0,0)
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

                        // Protege o bloco central (onde ficará o pinguim)
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
                }
            }
        }

        /// <summary>
        /// Alterna o turno entre os dois jogadores.
        /// </summary>
        public void AlternarTurno()
        {
            turnoAtual = (turnoAtual == 1) ? 2 : 1;
            Debug.Log($"[Turno] Agora é o turno do Jogador {turnoAtual}.");

            // Dispara evento para notificar inscritos sobre a mudança de turno
            OnTurnoAlterado?.Invoke(turnoAtual);
        }

        /// <summary>
        /// Chamado por um bloco quando ele se quebra ou é removido da grade.
        /// Remove a referência do bloco, verifica estabilidade e valida o estado do jogo.
        /// </summary>
        public void ReportBlockDestroyed(int x, int y)
        {
            // Validar coordenadas
            if (x < 0 || x >= colunas || y < 0 || y >= linhas)
            {
                Debug.LogWarning($"ReportBlockDestroyed chamado com coordenadas inválidas: ({x}, {y})");
                return;
            }

            // Remove referência ao bloco da grade
            if (grade[x, y] != null)
            {
                grade[x, y] = null;
                Debug.Log($"[Bloco Destruído] Bloco em ({x}, {y}) removido da grade.");
            }

            // Verifica estabilidade: remove blocos desconectados das bordas
            ChecarEstabilidade();

            // Valida se o bloco central ainda existe
            if (grade[centerX, centerY] == null)
            {
                Debug.Log($"[Game Over] Bloco central caiu! Jogador {turnoAtual} PERDEU!");
            }
        }

        /// <summary>
        /// Verifica a estabilidade da grade usando sistema de física com âncoras duplas.
        /// 
        /// ALGORITMO:
        /// 1. Identifica "ilhas" (grupos de blocos conectados via flood fill)
        /// 2. Para cada ilha, verifica quantas das 4 bordas extremas ela toca
        /// 3. Se uma ilha tocar MENOS de 2 bordas diferentes E NÃO contiver o bloco central,
        ///    ela é considerada instável e todos seus blocos são destruídos
        /// 4. Gera logs detalhados sobre avaliação de ilhas em português
        /// </summary>
        public void ChecarEstabilidade()
        {
            // Evita verificações simultâneas em cascata
            if (emVerificacao)
                return;

            emVerificacao = true;

            try
            {
                // Array para marcar blocos já processados em ilhas
                bool[,] processado = new bool[colunas, linhas];

                // Lista para armazenar todas as ilhas encontradas
                List<List<Vector2Int>> ilhas = new List<List<Vector2Int>>();

                // PASSO 1: Identifica todas as ilhas (componentes conectadas de blocos)
//                Debug.Log("[Estabilidade] Iniciando varredura de ilhas...");

                for (int x = 0; x < colunas; x++)
                {
                    for (int y = 0; y < linhas; y++)
                    {
                        // Se encontrou um bloco não processado, inicia flood fill para formar uma ilha
                        if (grade[x, y] != null && !processado[x, y])
                        {
                            List<Vector2Int> ilhaAtual = new List<Vector2Int>();
                            FloodFillIlha(x, y, processado, ilhaAtual);
                            ilhas.Add(ilhaAtual);
                        }
                    }
                }

                Debug.Log($"[Estabilidade] Total de ilhas identificadas: {ilhas.Count}");

                // PASSO 2: Avalia cada ilha segundo regras de física (pressão lateral - âncoras duplas)
                List<Vector2Int> blocosCaem = new List<Vector2Int>();

                for (int i = 0; i < ilhas.Count; i++)
                {
                    List<Vector2Int> ilha = ilhas[i];

                    // Verifica quais bordas esta ilha toca
                    HashSet<string> bordasTocadas = new HashSet<string>();
                    bool contemBlocoCentral = false;

                    foreach (var coord in ilha)
                    {
                        // Verifica bordas extremas (4 direções)
                        if (coord.x == 0) bordasTocadas.Add("Oeste");
                        if (coord.x == colunas - 1) bordasTocadas.Add("Leste");
                        if (coord.y == 0) bordasTocadas.Add("Sul");
                        if (coord.y == linhas - 1) bordasTocadas.Add("Norte");

                        // Verifica se contém o bloco central (pinguim)
                        if (coord.x == centerX && coord.y == centerY)
                        {
                            contemBlocoCentral = true;
                        }
                    }

                    // Log de avaliação da ilha
                    string bordasString = bordasTocadas.Count > 0 ? string.Join(", ", bordasTocadas) : "Nenhuma";
//                    Debug.Log($"[Física] Ilha #{i + 1} avaliada: {ilha.Count} blocos. Conectada às bordas: [{bordasString}]");

                    // PASSO 3: Aplica regra de pressão lateral (âncoras duplas)
                    // Regra: Ilha precisa de 2+ bordas OU conter bloco central para sobreviver
                    if (bordasTocadas.Count < 2 && !contemBlocoCentral)
                    {
                        // Ilha instável - deve cair por falta de pressão lateral
//                        Debug.Log($"[Física] Ilha destruída por instabilidade! Tocava apenas {bordasTocadas.Count} borda(s).");

                        foreach (var coord in ilha)
                        {
                            blocosCaem.Add(coord);
                        }
                    }
                    else if (contemBlocoCentral && bordasTocadas.Count < 2)
                    {
                        // Ilha salva por conter bloco central (pinguim)
                        Debug.Log($"[Física] Ilha central salva por conter o Pinguim. Blocos protegidos: {ilha.Count}");
                    }
                    else
                    {
                        // Ilha estável (2+ bordas)
//                        Debug.Log($"[Física] Ilha #{i + 1} estável com {bordasTocadas.Count} âncoras.");
                    }
                }

                // PASSO 4: Destrói todos os blocos das ilhas instáveis
                if (blocosCaem.Count > 0)
                {
                    Debug.Log($"[Estabilidade] Destruindo {blocosCaem.Count} blocos instáveis...");

                    foreach (var coord in blocosCaem)
                    {
                        GameObject go = grade[coord.x, coord.y];
                        grade[coord.x, coord.y] = null;

                        if (go != null)
                        {
                            BlocoDeGelo bloco = go.GetComponent<BlocoDeGelo>();

                            // Log especial se for o bloco central
                            if (bloco != null && bloco.gridX == centerX && bloco.gridY == centerY)
                            {
                                Debug.Log($"[Instabilidade CRÍTICA] Bloco CENTRAL destruído em ({coord.x}, {coord.y})!");
                            }

                            Destroy(go);
                        }
                    }
                }
                else
                {
                    Debug.Log("[Estabilidade] Nenhum bloco caiu nesta verificação. Todas as ilhas estão estáveis.");
                }
            }
            finally
            {
                emVerificacao = false;
            }
        }

        /// <summary>
        /// Executa flood fill (BFS) para identificar uma ilha de blocos conectados.
        /// Marca todos os blocos da ilha como processados.
        /// </summary>
        private void FloodFillIlha(int startX, int startY, bool[,] processado, List<Vector2Int> ilha)
        {
            Queue<Vector2Int> fila = new Queue<Vector2Int>();
            fila.Enqueue(new Vector2Int(startX, startY));
            processado[startX, startY] = true;

            int[] dx = { 1, -1, 0, 0 };
            int[] dy = { 0, 0, 1, -1 };

            while (fila.Count > 0)
            {
                Vector2Int atual = fila.Dequeue();
                ilha.Add(atual);

                // Explora 4 direções (cima, baixo, esquerda, direita)
                for (int i = 0; i < 4; i++)
                {
                    int nx = atual.x + dx[i];
                    int ny = atual.y + dy[i];

                    if (nx >= 0 && nx < colunas && ny >= 0 && ny < linhas)
                    {
                        if (!processado[nx, ny] && grade[nx, ny] != null)
                        {
                            processado[nx, ny] = true;
                            fila.Enqueue(new Vector2Int(nx, ny));
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Retorna o GameObject do bloco na posição (x, y) da grade.
        /// Retorna null se as coordenadas forem inválidas ou o bloco não existir.
        /// </summary>
        public GameObject GetBlock(int x, int y)
        {
            if (x < 0 || x >= colunas || y < 0 || y >= linhas)
                return null;

            return grade[x, y];
        }

        /// <summary>
        /// Retorna a dimensão horizontal da grade.
        /// </summary>
        public int GetColunas()
        {
            return colunas;
        }

        /// <summary>
        /// Retorna a dimensão vertical da grade.
        /// </summary>
        public int GetLinhas()
        {
            return linhas;
        }

        /// <summary>
        /// Retorna as coordenadas do bloco central (pinguim).
        /// </summary>
        public Vector2Int GetCenterCoordinates()
        {
            return new Vector2Int(centerX, centerY);
        }
    }
}

