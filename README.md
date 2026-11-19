# Timothy & Mort

Jogo sério de plataforma 2D (MonoGame) sobre princípios de design de interação. É desenvolvido como TCC, e os testes são feitos com especialistas em IHC e com estudantes.

O jogo testa o ciclo:

> Problema → intervenção → justificativa → consequência → reflexão

Em cada fase, o jogador vive um problema de design no próprio percurso. Depois, testa três intervenções numa sala própria, justifica a escolha e vê a consequência no restante da fase.

## Fluxo do estudo

```text
Identificação da sessão (código TM-XXXX)
-> Seleção de fases (cada fase libera a próxima; fase concluída não reabre)
-> Tutorial (só antes da primeira Affordance da sessão; fora da pesquisa)
-> Fase 1: Affordance e visibilidade
-> Fase 2: Feedback
-> Fase 3: Modelos conceituais
-> Tela final: resumo e passos para enviar o arquivo da sessão
```

Cada fase segue o mesmo formato:

```text
Introdução -> Sala 1 -> Sala 2 (passagem para a sala 3 bloqueada até a escolha)
-> Sala de intervenção (espada: testar soluções; saída: justificativa)
-> Sala 3 (solução aplicada; demonstração combinada nas fases 1 e 2)
-> Fase concluída
```

| Fase | Problema | Soluções (a melhor primeiro) |
|---|---|---|
| Affordance e visibilidade | Gelatinas que recarregam o dash têm a mesma aparência da terra. | Aparência, Marcação, Deformação |
| Feedback | Rochas quentes ferem sem nenhum sinal. | Recuo, Coração rachando, Tela tremendo |
| Modelos conceituais | Impostoras com a aparência de plataformas conhecidas agem de outro jeito. | Diferenciar, Restaurar, Animar |

A qualidade de cada solução é registrada na telemetria, mas não é mostrada ao jogador.

O mapeamento pedagógico das fases (modelo LM-GM, Arnab et al., 2015) está no texto do TCC.

## Controles

Todas as teclas funcionam ao mesmo tempo. O "esquema" só muda os nomes de teclas mostrados no tutorial; o padrão é Espaço/Shift.

| Controle | Ação |
|---|---|
| `A` / `D` ou `←` / `→` | Andar e navegar nos menus. |
| `W` ou `↑` | Mirar o impulso para cima (não pula). |
| `Espaço`, `C` ou `K` | Pular. **No ar**, apertar de novo faz o impulso para cima (na diagonal, com um lado segurado) e gasta o impulso. |
| `Shift`, `X` ou `J` | Impulso (dash), **só no ar**: para os lados, para cima e na diagonal para cima. A direção pode vir logo depois da tecla (0,06 s). |
| `E` | Interagir: entrada da intervenção, espada, saída da intervenção, placa de controles. |
| `Enter` | Confirmar nos menus. |
| `Esc` | Pausa. |
| `Ctrl + P` / `Ctrl + R` | Debug visual / recarregar o mapa (só em builds Debug). |

## Mapas (LDtk)

As salas das três fases, as salas de intervenção e o tutorial estão num único projeto: `Content/Maps/Affordance.ldtk`. As camadas, os valores de colisão e as entidades estão documentados em [Content/Maps/README.md](Content/Maps/README.md).

Resta um mapa TMX (Tiled), `SimpleCutsceneMap.tmx`: o id dele só serve de sinal para "voltar à seleção", e o carregador TMX ainda existe no código. Os outros TMX saíram do projeto.

## Dados da pesquisa

- **Banco local:** a sessão fica num SQLite local (`timothy-mort.db`, na pasta de dados do usuário).
- **Arquivo exportado:** também é exportada como JSON em `Documentos/TimothyMort`.
- **Quando o arquivo é atualizado:** ao entrar em cada sala, a cada escolha na intervenção, ao concluir uma fase e ao fechar o jogo. Uma sessão abandonada também deixa o arquivo.
- **O que o arquivo registra:**
  - versão do jogo;
  - por fase: solução, justificativa, posições apresentadas, trocas, tempo e mortes;
  - por sala: tempo, entradas e mortes;
  - tutorial, esquema de controle e eventos da intervenção.
- **Versão:** a constante `ResearchStudyConfiguration.GameVersion` deve ser atualizada antes de cada build.

Configurações em `App/ResearchStudyConfiguration.cs`:

- **`BetaAffordanceOnly`:** só a Affordance fica jogável.
- **`ResearchEmail`:** o e-mail mostrado na tela final.
- **MEEGA+:** `MeegaFormUrl` e `SessionFieldName` (formulário dos estudantes; o código da sessão vai pré-preenchido). Com a URL vazia, a tela final não mostra o questionário.

## Estrutura de código

| Pasta | Responsabilidade |
|---|---|
| `App` | Loop principal (`Game1`), sessão de estudo, telemetria em andamento, configuração. |
| `Core` | Câmera, escala e debug visual. |
| `Gameplay` | Player, input, colisão, espada da intervenção e constantes de cada fase. |
| `Scenes` | Estado e fluxo da fase (`PhaseRun`, `PhaseState`, `Scene`), catálogos de soluções e de falas do Mort. |
| `UI` | Menus, tela de escolhas, telas de introdução, resultado e conclusão. |
| `World` | Leitura e desenho dos mapas LDtk e TMX. |
| `Persistence` | SQLite e exportação JSON da sessão. |
| `docs` | Régua de movimento (`metricas_mov.md`) e paleta (`paleta_tipografia_interface.md`). |

## Executar

```bash
dotnet run                 # Debug: todas as fases abertas e ferramentas de desenvolvimento
dotnet publish -c Release  # build do estudo: ferramentas ocultas e fases em sequência
```
