# Guia de Setup Multiplayer com Unity Netcode

## Pré-requisitos

1. **Instale os Pacotes Netcode**
   - Abra `Window → Package Manager`
   - Clique em "+" e selecione "Add package by name"
   - Adicione os seguintes pacotes:
     - `com.unity.netcode.gameobjects`
     - `com.unity.transport`

2. **Preparação da Cena**

### Passo 1: Preparar o Prefab Pinguim

1. Abra o menu `Netcode` (criado automaticamente pela ferramenta de Editor)
2. Selecione `Netcode → Prepare Pinguim Prefab (Add NetworkObject)`
   - Isso adiciona um componente `NetworkObject` ao prefab `Pinguim.prefab`
   - Verifique o console para confirmação

### Passo 2: Criar NetworkManager

1. Abra o menu `Netcode`
2. Selecione `Netcode → Create NetworkManager GameObject in Scene`
   - Isso cria um GameObject `NetworkManager` com o componente `NetworkManager`
   - Seleciona automaticamente o GameObject para que você configure

### Passo 3: Configurar o NetworkManager

1. Selecione o GameObject `NetworkManager` na cena
2. No Inspector, vá para o componente `NetworkManager`
3. Configure o **Transport**:
   - Arraste o componente `Unity Transport` para o campo `NetworkTransport`
   - Se não houver um, crie um GameObject vazio, adicione o componente `Unity Transport`
4. **Registre os NetworkPrefabs**:
   - No componente `NetworkManager`, expanda a lista `NetworkPrefabs`
   - Clique no "+" para adicionar um novo prefab
   - Arraste `Assets/Prefabs/Pinguim.prefab` para o campo
   - Repita para outros prefabs que precisem de sincronização (ex: `BlocoDeGelo.prefab`)

### Passo 4: Adicionar a UI de Controle de Rede

1. Na cena, crie um **Canvas** (Right-click in Hierarchy → UI → Legacy → Panel)
2. Adicione dois **Buttons** filhos do Canvas
3. Renomeie um para "StartHostButton" e outro para "StartClientButton"
4. Crie um GameObject vazio e adicione o componente `NetworkUI`
5. Para cada botão:
   - Selecione o botão
   - No Inspector, clique o "+" abaixo do componente Button → OnClick ()
   - Arraste o GameObject com `NetworkUI` para o campo
   - Na dropdown, selecione `NetworkUI → StartHost()` ou `StartClient()`

### Passo 5: Garantir um único EventSystem

1. Na cena, procure por `EventSystem` no Hierarchy
2. Se houver múltiplos, delete os extras (deixe apenas um)
3. Alternativamente, adicione o componente `EventSystemSingleton` a um GameObject (este script garante automaticamente que há apenas um)

## Como Testar

### Teste Local (Host + Client no mesmo Editor)

1. No Editor, clique no botão **StartHost**
   - Você verá um console log: `[NetworkManager] Started as Host.`
2. Abra uma segunda janela do jogo (Player):
   - Build → Build and Run (ou use Play + Build)
3. Na segunda janela, clique no botão **StartClient**
   - Ambas as instâncias devem estar sincronizadas
   - O host (servidor) inicializa o tabuleiro
   - O client deve ver a grade sincronizada

### Teste de Sincronização

1. **No Host**: Clique nos blocos
   - O bloco deve se quebrar no host
   - Observe o console para logs como `[Bloco Destruído] Bloco em (x, y) removido da grade.`
2. **No Client**: Você deve ver os mesmos blocos desaparecerem
   - A NetworkList `gridState` sincroniza as mudanças
   - O cliente reconstrói visualmente os blocos com base em `gridState`

### Teste de Turno

1. **No Host**: Um bloco quebrado alterna o turno
   - Observe o console: `[Turno] Agora é o turno do Jogador X. Força: Y`
   - A UI deve atualizar em ambos os clientes
2. **No Client**: Você vê a mudança de turno sincronizada

### Teste de GameOver

1. Continue quebrando blocos até que o tabuleiro desabe
2. Quando o bloco central perder a conexão com as bordas:
   - O servidor dispara `OnGameOver`
   - O pinguim executa `Desabar()` de forma sincronizada
   - Ambos os clientes devem ver a UI de fim de jogo

## Troubleshooting

### Erro: "There can only be one active Event System"

**Causa**: Múltiplos EventSystems ativos na cena.

**Solução**:
1. Adicione o componente `EventSystemSingleton` a um GameObject (qualquer um)
2. Ele detectará e desativará automaticamente os EventSystems duplicados

### Erro: "NetworkVariable is written to, but doesn't know its NetworkBehaviour yet"

**Causa**: Tentativa de modificar um `NetworkVariable` antes do `NetworkObject` ser spawned.

**Solução**:
- Verificamos que apenas o **servidor** modifica NetworkVariables no `ControladorDeUI`
- Movemos a inicialização da grade para o método `OnNetworkSpawn()` no `GeradorDeTabuleiro`
- Clientes apenas leem os valores sincronizados

### Erro: "NetworkManager.Singleton is null"

**Causa**: Nenhum `NetworkManager` foi criado ou spawnado na cena.

**Solução**:
1. Use o menu `Netcode → Create NetworkManager GameObject in Scene`
2. Ou crie manualmente um GameObject vazio e adicione o componente `NetworkManager`

### Bloco não desaparece no cliente após ser quebrado

**Causa**: O `gridState` não foi sincronizado corretamente.

**Solução**:
1. Verifique que `GeradorDeTabuleiro` é um `NetworkBehaviour` (deve ser, foi convertido)
2. Verifique que o `NetworkObject` do `GeradorDeTabuleiro` está registrado no `NetworkManager`
3. Verifique que `BlocoDeGelo.QuebrarBloco()` chamava `GeradorDeTabuleiro.Instance.ReportBlockDestroyed()`
4. Abra o console e procure por logs de sincronização

### Múltiplas instâncias do jogo (Host + Client) não sincronizam

**Causa**: Transport ou NetworkManager não configurado corretamente.

**Solução**:
1. Verifique que o `NetworkTransport` é do tipo `Unity Transport`
2. No `NetworkManager`, verifique que o prefab `GeradorDeTabuleiro` está registrado
3. Teste em loopback (ambas na mesma máquina) primeiro
4. Se funcionar localmente, então teste em LAN ou via Unity Relay

## Estrutura de Rede Implementada

### Server Authoritative (Servidor Autoritário)

- **GeradorDeTabuleiro** (NetworkBehaviour)
  - `turnoAtual` (NetworkVariable<int>): sincroniza turno entre clientes
  - `forcaDoTurnoAtual` (NetworkVariable<int>): sincroniza força do golpe
  - `gridState` (NetworkList<int>): representa presença/ausência de blocos (1/0)
  - `AlternarTurno()`: ServerRpc se chamado de cliente
  - `ReportBlockDestroyed()`: ServerRpc se chamado de cliente

- **Pinguim** (NetworkBehaviour)
  - `Desabar()`: ServerRpc se chamado de cliente
  - Despawn via `NetworkObject.Despawn()` após 2 segundos

- **BlocoDeGelo**: Não é NetworkObject (sincronizado via gridState)
  - Notifica `GeradorDeTabuleiro.ReportBlockDestroyed()` quando quebrado
  - Cliente ou servidor chama, servidor valida e atualiza `gridState`

### Fluxo de Dados

1. **Cliente clica em bloco**
   - → `ControleDeClique` chama `BlocoDeGelo.ReceberDano()`
2. **BlocoDeGelo quebra**
   - → `BlocoDeGelo.QuebrarBloco()` chama `GeradorDeTabuleiro.ReportBlockDestroyed(x, y)`
3. **Se cliente**: ServerRpc encaminha para servidor
   - → `ReportBlockDestroyedServerRpc(x, y)` executa no servidor
4. **Servidor processa**
   - → Atualiza `grade` e `gridState`
   - → `gridState` é sincronizado para todos os clientes via `NetworkList`
5. **Todos os clientes**
   - → Recebem `OnGridStateChanged` event
   - → Atualizam os visuais (destroem/criam blocos)

## Próximos Passos (Opcional)

- **Registro Automático**: Usar script de editor para registrar prefabs automaticamente no NetworkManager
- **Validação no Servidor**: Adicionar verificações (ex: somente no seu turno pode quebrar blocos)
- **Reconexão**: Implementar lógica para que clientes reconectem se desconectarem
- **UI Melhorada**: Mostrar estado de conexão, ping, host/client role
- **Relay/Lobby**: Integrar Unity Relay/Lobby para testes remotos (não locais)

## Referências

- [Unity Netcode for GameObjects](https://docs.unity.com/netcode/current/learn)
- [Unity Transport](https://docs.unity.com/transport/current/getting-started)
- [Multiplayer Samples](https://github.com/Unity-Technologies/netcode-samples)

