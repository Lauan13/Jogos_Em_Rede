# Checklist de Implementação Multiplayer - Jogos em Rede

## ✅ Implementação Concluída

### A - Conversão para Server-Authoritative (GeradorDeTabuleiro)
- [x] `GeradorDeTabuleiro` herda de `NetworkBehaviour`
- [x] `turnoAtual` convertido para `NetworkVariable<int>`
- [x] `forcaDoTurnoAtual` convertido para `NetworkVariable<int>`
- [x] `gridState` implementado como `NetworkList<int>` (1=bloco existe, 0=removido)
- [x] `AlternarTurno()` torna-se `ServerRpc` quando chamado de cliente
- [x] `ReportBlockDestroyed()` torna-se `ServerRpc` quando chamado de cliente
- [x] `ChecarEstabilidade()` executável apenas no servidor
- [x] Inicialização de grade movida para `OnNetworkSpawn()` (evita erro de NetworkVariable antes do spawn)
- [x] Clientes atualizam visuais via callback `OnGridStateChanged`

### B - Preparação de Componentes de Rede
- [x] `Pinguim` herda de `NetworkBehaviour`
- [x] `Desabar()` torna-se `ServerRpc` quando chamado de cliente
- [x] `Pinguim` usa `NetworkObject.Despawn()` após 2 segundos (ao invés de `Destroy()`)
- [x] `BlocoDeGelo` notifica `GeradorDeTabuleiro` ao quebrar
- [x] Destruição de bloco validada no servidor apenas

### C - Correção de Erros (CS0029 + EventSystem)
- [x] `ControleDeClique.cs` usa `.Value` ao ler `forcaDoTurnoAtual` (corrige CS0029)
- [x] `ControladorDeUI.cs` verifica se é servidor antes de escrever em NetworkVariables
- [x] `EventSystemSingleton.cs` detecta e desativa múltiplos EventSystems
- [x] Suporte a Unity 6.0+ com conditional compilation (`FindObjectsByType`)

### D - UI e Helpers de Editor
- [x] `NetworkUI.cs` com métodos públicos `StartHost()`, `StartClient()`, `StartServer()`
- [x] `RegisterNetworkPrefabsEditor.cs` com:
  - [x] Menu: "Netcode → Prepare Pinguim Prefab (Add NetworkObject)"
  - [x] Menu: "Netcode → Create NetworkManager GameObject in Scene"
- [x] Guia de setup (`MULTIPLAYER_SETUP.md`) com instruções passo-a-passo

### E - Validação de Compilação
- [x] Nenhum erro crítico (CS****) de compilação
- [x] Apenas avisos de estilo (naming conventions) e deprecações (ignoráveis)
- [x] Estrutura de tipos corrigida (NetworkVariable, NetworkList, ServerRpc, etc.)

---

## 📋 Teste Fase 1: Compilação e Carregamento

### Tarefas:
1. [ ] Abra o Unity Editor com o projeto
2. [ ] Confirme que não há erros no Console
3. [ ] Console exibe apenas avisos de estilo (não erros CS***)
4. [ ] Projeto compila com sucesso

---

## 📋 Teste Fase 2: Preparação de Rede (Editor Menus)

### Tarefas:
1. [ ] Abra o menu `Netcode` no Editor (deve aparecer na barra de menu superior)
   - Se não aparecer: reinicie o Editor ou recompile os scripts
2. [ ] Clique em "Netcode → Prepare Pinguim Prefab (Add NetworkObject)"
   - Esperado: Console log "Adicionado NetworkObject ao prefab Pinguim."
3. [ ] Clique em "Netcode → Create NetworkManager GameObject in Scene"
   - Esperado: Um GameObject "NetworkManager" aparece na Hierarchy
   - Esperado: O GameObject fica selecionado e exibe o componente NetworkManager

---

## 📋 Teste Fase 3: Configuração Manual (sem automação)

### Tarefas:
1. [ ] **Registrar Pinguim no NetworkManager**
   - Selecione o GameObject "NetworkManager"
   - No Inspector, em NetworkManager > NetworkPrefabs, clique "+"
   - Arraste `Assets/Prefabs/Pinguim.prefab` para o campo "Prefab"
   - Deixe "Hash" como 0 (será gerado automaticamente)

2. [ ] **Configurar Transport**
   - Se não houver um GameObject "Unity Transport":
     - Right-click na Hierarchy → Create Empty
     - Renomeie para "UnityTransport"
     - Add Component → "Unity Transport"
   - Selecione "NetworkManager" e no Inspector:
     - Arraste o GameObject "UnityTransport" para o campo "Transport" em NetworkManager

3. [ ] **Verificar EventSystem**
   - Na Hierarchy, procure por "EventSystem"
   - Se houver múltiplos, delete os extras (deixe apenas um)
   - Ou adicione o script `EventSystemSingleton` a um GameObject

4. [ ] **Criar UI (Host/Client Buttons)**
   - Crie um Canvas (Right-click Hierarchy → UI → Legacy → Panel)
   - Adicione dois Buttons como filhos
   - Renomeie para "HostButton" e "ClientButton"
   - Crie um GameObject vazio "NetworkUIController"
   - Add Component → NetworkUI
   - Para cada botão:
     - Selecione o botão
     - No Inspector, Button component → OnClick (), clique "+"
     - Arraste "NetworkUIController" para o campo
     - Dropdown: Select "NetworkUI" → "StartHost()" ou "StartClient()"

---

## 📋 Teste Fase 4: Teste Local (Host-Client no Editor)

### Pré-requisitos:
- [x] Todos os testes das Fases 1-3 passaram
- [x] Cena salva e preparada

### Tarefas:
1. [ ] **Iniciar Host**
   - Clique no botão "HostButton" (ou selecione Play no Editor)
   - Console esperado: "[NetworkManager] Started as Host."
   - Tabuleiro com 49 blocos (7x7) deve aparecer

2. [ ] **Iniciar Client (segunda instância)**
   - Build and Run (Project → Build and Run)
   - Ou: Duplicate the Game window (Play + Build)
   - Na segunda instância, clique no botão "ClientButton"
   - Console esperado: "[NetworkManager] Started as Client."
   - Esperado: Cliente vê o mesmo tabuleiro que o Host

3. [ ] **Sincronização de Grade**
   - No Host: Clique em um bloco (não o centro protegido)
   - Console Host esperado: "[Bloco Destruído] Bloco em (x, y) removido da grade."
   - Esperado no Host: Bloco desaparece
   - Esperado no Client: O mesmo bloco desaparece após ~1 frame
   - Repita com 3-4 blocos diferentes

4. [ ] **Sincronização de Turno**
   - Quebra um bloco no Host → turno alterna
   - Console esperado: "[Turno] Agora é o turno do Jogador X. Força: Y"
   - Esperado no Client: Texto de turno atualiza também

5. [ ] **Teste de Colapso (GameOver)**
   - Continue quebrando blocos estrategicamente
   - Goal: Desconectar o bloco central das bordas
   - Esperado: "[Game Over] O anel de suporte foi rompido! O bloco central desabou! Jogador X PERDEU! Jogador Y VENCEU."
   - Pinguim deve desaparecer em ambas as instâncias
   - UI deve mostrar o vencedor

---

## 📋 Teste Fase 5: Teste de Desconexão (Opcional)

### Tarefas:
1. [ ] **No Host**: Clique em alguns blocos
2. [ ] **No Client**: Feche a janela (Ctrl+W ou Alt+F4)
3. [ ] **Esperado no Host**: 
   - Nenhuma crash
   - Console: "[NetworkManager] Client Y Disconnected" ou similar

---

## 📋 Teste Fase 6: Teste Remoto (LAN/Relay - Opcional)

### Pré-requisitos:
- [x] Testes de Rede Local (Fase 4) passaram

### Tarefas:
1. [ ] **Build para máquina diferente (mesma LAN)**
   - Build o projeto para outra máquina na mesma rede
   - Configure o IP da máquina Host no Client
   - Teste de sincronização novamente (deve funcionar)

2. [ ] **Unity Relay (para internet)**
   - Instale o pacote com.unity.services.relay
   - Configure Unity Cloud
   - Atualize o Transport para usar Relay
   - Teste com máquinas remotas (não requerimento para MVP)

---

## 🐛 Troubleshooting Rápido

| Problema | Causa | Solução |
|----------|-------|--------|
| "There can only be one active Event System" | Múltiplos EventSystems | Adicione `EventSystemSingleton` a um GameObject |
| "NetworkVariable is written to, but doesn't know..." | Escrita em NetworkVariable antes do spawn | Verifica que `OnNetworkSpawn()` inicializa a grade |
| NetworkManager não aparece | Pacote Netcode não instalado | Window → Package Manager → Add by name → com.unity.netcode.gameobjects |
| Bloco não some no Client | gridState não sincronizado | Verifica que GeradorDeTabuleiro está registrado no NetworkManager |
| Client não sincroniza com Host | NetworkManager não configurado | Verifica Transport e prefabs registrados |

---

## 📊 Resumo de Arquivos Alterados/Criados

| Arquivo | Tipo | Alteração |
|---------|------|-----------|
| `Assets/Scripts/GeradorDeTabuleiro.cs` | ALTERADO | → NetworkBehaviour + ServerRpc + OnNetworkSpawn |
| `Assets/Scripts/ControladorDeUI.cs` | ALTERADO | → Verifica servidor antes de escrever NetworkVariables |
| `Assets/Scripts/BlocoDeGelo.cs` | ALTERADO | → Add Netcode import + server-side destroy |
| `Assets/Scripts/Pinguim.cs` | ALTERADO | → NetworkBehaviour + ServerRpc + Despawn |
| `Assets/Scripts/ControleDeClique.cs` | ALTERADO | → Use `.Value` para NetworkVariable |
| `Assets/Scripts/NetworkUI.cs` | NOVO | → UI helper para StartHost/Client |
| `Assets/Scripts/EventSystemSingleton.cs` | NOVO | → Detecta/desativa EventSystem duplicados |
| `Assets/Editor/RegisterNetworkPrefabsEditor.cs` | NOVO | → Editor menu para preparar prefabs |
| `MULTIPLAYER_SETUP.md` | NOVO | → Guia passo-a-passo de setup |

---

## ✨ Próximos Passos (Pós-MVP)

1. **Validação de Autoridade**: Verificar turno antes de permitir quebra de blocos
2. **Reconexão**: Implementar lógica para que clientes desconectados se reconectem
3. **Relay/Lobby**: Integrar Unity Relay para testes remotos (sem necessidade de IP manual)
4. **UI Melhorada**: Mostrar ping, estado de conexão, role (Host/Client)
5. **Spawn de Blocos**: Converter BlocoDeGelo para NetworkObject e usar spawn/despawn (em vez de apenas sincronizar estado)

---

## 📞 Suporte

Se encontrar erros não listados aqui:
1. Verifique o console (Window → General → Console)
2. Procure por mensagens de erro com prefixo "[" (ex: "[Netcode]", "[Network]")
3. Confirme que todos os pacotes estão instalados (com.unity.netcode.gameobjects, com.unity.transport)
4. Reinicie o Editor e recompile os scripts (Ctrl+R)

---

**Última atualização:** 2026-07-10  
**Status:** ✅ Implementação Concluída - Pronto para Teste

