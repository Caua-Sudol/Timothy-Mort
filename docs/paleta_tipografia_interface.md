# Paleta e tipografia da interface

Referência das telas integradas até 18/09/2026. Documento de registro; não altera o jogo.
Layout de referência: 1920 × 1080 pixels lógicos.

## Cores

| Uso                          | Hex       | RGB           | Origem / aplicação                                                        |
|------------------------------|-----------|---------------|---------------------------------------------------------------------------|
| Fundo carvão                 | `#15191F` | 21, 25, 31    | Sessão, seleção de fases, explicações e finalizações.                     |
| Texto principal / marfim     | `#D8D0BB` | 216, 208, 187 | Títulos, conteúdo principal e opções selecionadas.                        |
| Texto secundário             | `#B5ADA1` | 181, 173, 161 | Instruções, navegação, opções não selecionadas e mensagem da tela inicial. |
| Texto sobre cards claros     | `#25282B` | 37, 40, 43    | Texto dos cards na seleção de fases.                                      |
| Progresso salvo              | `#A69BAE` | 166, 155, 174 | Mensagem de persistência bem-sucedida na conclusão da fase.               |
| Fundo da arte do seletor     | `#302833` | 48, 40, 51    | card selecionado                                                          |
| Papel do card não selecionado | `#B8AE91` | 184, 174, 145 | card não selecionado                                                      |
| Erro                         | `#FF4500` | 255, 69, 0    | `Color.OrangeRed`: erros de identificação/persistência.                   |
| Icones flutuantes            | `#A69BAE` | 166, 155, 174 | Icones em loop                                                            |

As cores de fundo e texto acima foram conferidas em `UI/ParticipantIdentificationScreen.cs`,
`UI/PhaseSelectMenu.cs`, `UI/PhaseIntroductionScreen.cs`, `UI/PhaseResultScreen.cs`,
`UI/TitleScreen.cs`, `UI/PauseMenu.cs` e `UI/DeathMenu.cs`.

As cores do seletor e do papel são referências fornecidas pelo autor, não amostragens dos PNGs.
A aparência do card selecionado e do fundo da tela de título está embutida nas imagens;
não há um único hexadecimal no código que descreva essas artes integralmente.
Os PNGs são desenhados com `Color.White`, que preserva suas cores originais.

Pausa e morte mantêm o cenário visível: usam textos e seletor, sem fundo próprio.
As folhas decorativas atuais usam `Color.ForestGreen` (`#228B22`) multiplicado por 0,5;
esse é um efeito provisório, não uma cor definida para os futuros símbolos místicos.

## Tipografia

| Função                                           | Família                 | Arquivo em Content           | Asset carregado | Tamanho-base no SpriteFont |
|--------------------------------------------------|-------------------------|------------------------------|-----------------|----------------------------|
| Principal: títulos e títulos dos cards           | Walter Turncoat Regular | `WalterTurncoat-Regular.ttf` | `walter`        | 64                         |
| Leitura: textos, opções, explicações e navegação | Patrick Hand Regular    | `PatrickHand-Regular.ttf`    | `patrick`       | 32                         |

Configurações: `Content/walter.spritefont` e `Content/patrick.spritefont`.
Ambas usam estilo Regular, espaçamento adicional 0 e kerning ativado.
Faixas incluídas: caracteres 32–126 e 160–255, incluindo acentos portugueses comuns.

Os tamanhos acima são os valores de compilação do SpriteFont, não uma garantia de altura
em pixels nem uma equivalência direta ao tamanho mostrado no Affinity. As telas aplicam
escalas e ajustes para caber nos limites; validar a aparência no jogo em 1920 × 1080.

Os arquivos TTF locais alimentam a compilação do conteúdo MonoGame. O jogo carrega
os SpriteFonts compilados: o jogador não precisa instalar essas fontes no sistema.
No Affinity, usar as mesmas famílias para aproximar o protótipo da apresentação no jogo.

A tela inicial usa a arte do título já no PNG e Patrick na mensagem para continuar.
O menu de soluções/justificativas ainda recebe o asset legado `font` em `App/Game1.cs`;
sua migração visual permanece pendente. Não presumir que todas as telas já foram migradas.
