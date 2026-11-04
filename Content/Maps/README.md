# Mapas do jogo (LDtk)

Abra `Content/Maps/Affordance.ldtk` no LDtk. Apesar do nome, este projeto contém **todas** as salas das três fases, as três salas de intervenção e o tutorial. A pasta `Content/Maps/Affordance/backups/` guarda cópias automáticas do LDtk; não edite essas cópias. `EurekaTeste.ldtk` é usado só pela sala de teste do menu (ver `EurekaTeste.md`).

O `.csproj` copia `Content/Maps/*.ldtk` na compilação: depois de salvar no LDtk, recompile ou use `Ctrl + R` dentro da sala (só em Debug).

## Níveis

O jogo escolhe cada nível pelo **identificador**, e não pela posição no World. Mantenha os nomes.

| Fase | Salas | Intervenção |
|---|---|---|
| Tutorial (fora da pesquisa) | `Tutorial` | — |
| 1. Affordance e visibilidade | `AffordanceTela1`, `AffordanceTela2`, `AffordanceTela3` | `Aff_Sala_intervencao` |
| 2. Feedback | `FeedbackTela1`, `FeedbackTela2`, `FeedbackTela3` | `Feed_Sala_intervencao2` |
| 3. Modelos conceituais | `ModelosTela1`, `ModelosTela2`, `ModelosTela3` | `Concept_Sala_intervencao3` |

Cada fase tem a sua sala de intervenção, porque a colisão (IntGrid) não muda de uma fase para outra dentro de um mesmo nível. As três salas usam o mesmo fundo (`assets/mg/root/intervencao/fundo_intervencao.png`), mais uma luz azul suave vinda de baixo, desenhada por código.

## Regras das salas

- **Tamanho:** 2880 × 1600; terreno e colisão em grade de 96; entidades em grade de 16. As coordenadas são locais à sala.
- **Paredes:** as bordas laterais de todas as salas são paredes invisíveis (por código). Saídas encostadas na borda devem ficar **dentro** do mapa.
- **Passagens:**
  - **Contato:** `RoomExit` e `RoomBack` funcionam por contato nas salas de desafio.
  - **Intervenção:** a saída da sala de intervenção pede **E**. Antes da primeira solução, ela fica escura e encostar nela devolve o jogador ao início, com uma fala do Mort.
  - **Barreira:** a passagem da sala 2 para a sala 3 fica bloqueada até a primeira escolha. Entrar nela devolve o jogador ao início da sala.
- **Entrada da intervenção:** aparece na sala 2, ou na sala 1 depois de duas mortes. Some depois que a solução é escolhida.
- **Quedas:** custam um coração e devolvem o jogador ao início da sala. No tutorial, ele volta ao último chão tocado, sem custo.

## Colisão (`Colisao`, IntGrid)

| Valor | Nome | Comportamento |
|---|---|---|
| 1 | `solido` | Bloqueia. |
| 2 | `gelatina` | Atravessável; limita a queda e recarrega o dash. |
| 3 | `espinhos` | Dano: na Affordance, funciona como queda (perde um coração e volta ao início). |
| 4 | `rochaQuente` | Sólida; fere enquanto há contato (Feedback e Modelos). |
| 5 | `gelatinaTrampolim` | Fase 3, **mini-jump**: parece gelatina, lança para cima e não recarrega o dash. |
| 6 | `espinhoInofensivo` | Versão antiga da fase 3. Não é mais usado em nenhum nível. |
| 7 | `rochaImpostora` | Fase 3: parece rocha quente e tira 2 corações. |

A física vem **só** da `Colisao`. As camadas de arte não criam colisão.

## Camadas de arte por solução

As camadas abaixo só aparecem quando a solução correspondente está aplicada, mesmo que estejam ocultas no editor. Os tiles da camada de baixo que ficam sob a arte da solução, dentro de uma gelatina ou de uma impostora, são escondidos, para a transparência da arte nova funcionar.

| Fase | Camadas | Aparecem com |
|---|---|---|
| 1 | `Solucao_gelatina` | Aparência |
| 1 | `Solucao_mark` | Marcação |
| 3 | `Mini_jump_*`, `Blue_fire_*` (prefixos, sem distinguir maiúsculas) | Diferenciar |

Para preparar uma impostora na fase 3:

1. Pinte a arte **normal** embaixo (`Gelatine_*` para o mini-jump, `Fire_*` para a rocha).
2. Pinte a arte **diferenciada** por cima (`Mini_jump_*` / `Blue_fire_*`), nas mesmas células.

Sem a arte normal embaixo, a impostora fica sem arte quando o Diferenciar não está aplicado.

Efeitos por código, sem camada:

- **Deformação (fase 1):** a gelatina pulsa.
- **Animar (fase 3):** só o mini-jump estica, e só a rocha impostora solta faíscas vermelhas.

As escolhas entram com uma piscada (0,26 s fechando, 0,07 s fechado, 0,32 s abrindo).

## Entidades

| Entidade | Uso |
|---|---|
| `PlayerSpawn` | Início e checkpoint da sala (pivô nos pés). Obrigatória. |
| `RoomExit` | Avança para a próxima sala. Na sala 3, conclui a fase. Na intervenção, volta para a sala de origem (com E). |
| `RoomBack` | Volta para a sala anterior (contato). |
| `RoomReturn` | Posição dos pés ao voltar da sala seguinte. Mantenha fora de `RoomExit`. |
| `InterventionEntrance` | Entrada da intervenção (com E), mostrada com símbolos flutuantes. |
| `InterventionReturn` | Posição dos pés ao voltar da intervenção. |
| `InterventionSword` | Espada da intervenção: E abre as soluções. A fumaça vermelha e o pulso são desenhados por código. |
| `Texto` / `Seta` | Tutorial: texto e seta no cenário. Marcadores como `{dash}`, `{pular}`, `{cima}`, `{direita}`, `{esquerda}` e `{andar}` viram teclas entre colchetes, conforme o esquema escolhido. |
| `Controles` | Tutorial: placa que abre a escolha de esquema com E. |
| `FalaMort` | Área em que o Mort fala uma vez por sessão (campo `Texto`). |
| `Decor_*` / `Decoracao` | Peça visual que flutua de leve, sem colisão. Ver abaixo. |

O alcance do **E** é o corpo do jogador ampliado em 64 unidades em todas as direções. Spawns e retornos devem ficar com os pés sobre um sólido, fora de perigos e de passagens automáticas.

### Decorações flutuantes

As entidades `Decor_plat_fundo_1` a `Decor_plat_fundo_6` usam o próprio PNG como prévia e têm o campo `Frente` (marcado: na frente do jogador). Para criar uma peça nova:

1. Adicione o PNG como tileset.
2. Crie uma entidade com nome começando por `Decor_`, exibição "Tile" com esse tileset (render "Stretch"), pivô 0,0 e o campo Bool `Frente`.

Alternativa sem prévia: a entidade `Decoracao`, com o campo `Imagem` (o nome do PNG em `assets/eureka`). O PNG é esticado ao retângulo da entidade. Um nome errado não trava o jogo: a peça só não aparece.

## Cuidados

- Salve no LDtk antes de recompilar e não sobrescreva o arquivo com uma cópia antiga aberta em outro editor.
- Não é preciso cadastrar tilesets no `Content.mgcb`: o build copia os PNGs de `assets`.
- Erros de recarga (imagem ausente, recorte inválido) preservam o mapa anterior.
