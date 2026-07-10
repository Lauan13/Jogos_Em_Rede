# ⚡ Quick Start: Teste Multiplayer em 5 Minutos

## 1️⃣ Instalar Pacotes (30 segundos)

```
Window → Package Manager → "+" → Add by name
├─ com.unity.netcode.gameobjects
└─ com.unity.transport
Aguarde download/import
```

## 2️⃣ Preparar Prefab Pinguim (20 segundos)

No Editor Unity:
```
Menu superior → Netcode → Prepare Pinguim Prefab (Add NetworkObject)
✓ Console: "Adicionado NetworkObject ao prefab Pinguim."
```

## 3️⃣ Criar NetworkManager (30 segundos)

No Editor Unity:
```
Menu superior → Netcode → Create NetworkManager GameObject in Scene
✓ Novo GameObject "NetworkManager" aparece na Hierarchy
```

## 4️⃣ Registrar Prefab (1 minuto)

Na Hierarchy:
```
Selecione → NetworkManager
↓ Inspector:
   NetworkManager component → NetworkPrefabs → "+"
   Arraste: Assets/Prefabs/Pinguim.prefab
✓ Prefab registrado
```

## 5️⃣ Testar Host+Client (2 minutos)

**Terminal/PowerShell:**
```powershell
# Terminal 1: Inicie o Editor
cd "C:\Users\20231170150053\Documents\GitHub\Jogos_Em_Rede\Project"
# Abra o Editor (ou use Play se já estiver aberto)

# Terminal 2: Build para Client (opcional, pode ser Editor+Build)
# File → Build and Run (builds para Builds/BreakIce.exe)
```

**No Editor (Host):**
```
Play (teclado: Space ou botão Play)
✓ Tabuleiro 7x7 aparece
✓ Console: "[NetworkManager] Started as Host."
```

**No Build (Client):**
```
Build executa
✓ Mesma cena, mesma grade
✓ Console: "[NetworkManager] Started as Client."
```

**Teste de sincronização:**
```
Host: Clique em um bloco (não o centro)
✓ Bloco desaparece no Host
✓ Após ~1 frame, desaparece no Client também
Repita com 3 blocos → tudo sincronizado? ✅

Host: Turno muda
✓ Console Host: "[Turno] Agora é o turno do Jogador X. Força: Y"
✓ UI Client mostra mesmo turno
```

---

## ✅ Resultado Esperado

```
[✓] Ambas instâncias veem o mesmo tabuleiro
[✓] Cliques em blocos sincronizam instantaneamente
[✓] Turno alterna e sincroniza
[✓] Sem crashes ou erros no Console
[✓] Pinguim cai quando tabuleiro desaba (em ambas instâncias)
```

---

## 🆘 Se Algo Quebrar

### Erro: "NetworkVariable is written to..."
**Solução:** Clique em Play/Stop no Editor para resetar

### Erro: "EventSystem: There can only be one active"
**Solução:** 
```
Hierarchy: Procure "EventSystem"
Se múltiplos: Delete extras (deixe 1)
OU: Selecione um GameObject e Add Component → EventSystemSingleton
```

### Erro: "NetworkManager not found"
**Solução:** 
```
Hierarchy: Clique direito → Create Empty → Rename "NetworkManager"
Add Component → NetworkManager
```

### Bloco não some no Client
**Solução:** Verifica que:
- [ ] Pinguim.prefab tem NetworkObject
- [ ] Pinguim.prefab está registrado em NetworkManager
- [ ] Transport está configurado
- [ ] Console não mostra erros

---

## 📊 Checklist Rápido

```
[ ] Pacotes instalados (Netcode + Transport)
[ ] Editor menu "Netcode" funciona
[ ] Prefab Pinguim tem NetworkObject
[ ] NetworkManager criado e selecionado
[ ] Transport configurado
[ ] Pinguim registrado em NetworkPrefabs
[ ] Play no Editor = Host funciona
[ ] Build & Run = Client conecta
[ ] Blocos sincronizam
[ ] Turno sincroniza
[ ] Nenhum erro no Console
```

---

## 🎮 Próximo Passo

Se tudo passou:
1. Leia `MULTIPLAYER_SETUP.md` para entender a arquitetura
2. Leia `TESTING_CHECKLIST.md` para testes mais rigorosos
3. Comece a adicionar validações (turno, força, etc.)

Se algo falhou:
1. Consulte "🆘 Se Algo Quebrar" acima
2. Abra o Console (Window → General → Console)
3. Procure por erros com prefixo "[" (ex: "[Netcode]", "[Network]")

---

**Tempo total: ~5 minutos para teste funcional básico** ⚡

