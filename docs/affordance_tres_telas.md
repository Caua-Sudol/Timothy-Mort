# Affordance — blockout de três telas

Cada TMX usa 60 × 34 tiles de 32 px; a câmera enquadra 1920 × 1080.
A margem inferior de 8 px do TMX fica fora da área visível.

- `AffordanceVisibilidade.tmx`: saltos de 64 px de altura, porta E na primeira tela,
  travessia longa com duas gelatinas e um apoio quebrável opcional.
- `AffordanceTela2.tmx`: plataforma móvel, apoio quebrável e sequência de recargas.
- `AffordanceTela3.tmx`: sequência quebrável/móvel, recargas menores e espinhos no pouso.
- `AffordanceIntervencao.tmx`: espada retangular E perto do spawn, escada e trecho para
  experimentar a solução escolhida; saída retorna à sala pela qual o jogador entrou.

## Fluxo e checkpoints

Primeira tela → segunda → terceira → conclusão. A passagem entre as três telas é livre, mesmo sem escolher uma solução.
As bordas esquerdas das telas 2 e 3 retornam à tela anterior, permitindo revisitar a intervenção.
A conclusão na terceira tela continua exigindo solução parcial ou adequada; a espada permite revisão.
A intervenção mantém a transição com fade. Entre as três telas, a transição lateral
dura 0,30 s, sem fade; o jogador fica parado durante o deslocamento das telas.

Cada entrada de sala define o ponto de retorno no spawn. A vida começa com 5 corações
e é preservada nas trocas de sala. Quedas retiram um coração e teleportam
imediatamente ao checkpoint. O efeito dura 0,22 s sem bloquear o controle.
Espinhos e outros danos retiram um coração sem mudar a posição do jogador,
com recuo e 0,6 s de invulnerabilidade, indicada pelo jogador piscando.
Ao zerar, o menu de morte permite reiniciar a tela atual com 5 corações.
Quedas e perda do último coração contam como mortes no registro existente.
Danos não letais de espinhos não contam como morte nem reiniciam as plataformas.
As plataformas dinâmicas são restauradas no retorno ao checkpoint.

## Editar no Tiled

A porta da primeira sala aparece após duas mortes registradas nessa sala (quedas
também contam), na posição definida pelo autor. Na segunda sala há uma porta
disponível desde a chegada, em X = 144, Y = 704. Ambas usam E e retornam ao spawn
`retorno_intervencao` da sala de origem. A primeira sala continua com passagem livre.
Espinhos em retângulos mais altos que largos são desenhados lateralmente, apontando
para a direita e repetindo ao longo de toda a altura do volume de dano.

Abra os arquivos pelo projeto `Timothy-Mort.tiled-project`.
Salve e use Ctrl esquerdo + R no jogo para recarregar a sala.

| Layer | Objetos / propriedades |
| --- | --- |
| `mapaGeral` | Retângulos sólidos |
| `gelatina` | Volumes atravessáveis de recarga, não duplicar em sólidos |
| `plataformaMovel` | `deslocamentoX`, `deslocamentoY` em px; `periodo` em segundos (ida e volta) |
| `plataformaQuebravel` | `tempoQuebra` em segundos após o primeiro pouso |
| `dano` | Espinhos |
| `entradaIntervencao` | Porta E; manter `posicao=tardia` em Affordance |
| `botaoIntervencao` | Espada provisória: retângulo; interação E a curta distância |
| `saidaCena` | Trigger de transição à direita, com destino e spawn |

Movimento: A/D, pulo Espaço, dash Shift com direção W/A/S/D.
Plataformas móveis carregam o jogador quando ele está apoiado.
Os vãos centrais de 704–768 px usam gelatinas intermediárias para recarregar
o dash no ar. Não há piso sólido sob essas gelatinas.

## Validação no jogo

1. Conferir passagem por cada vão com recarga; ajustar gelatinas conforme sensação do dash.
2. Cair: perder exatamente um coração e retornar sem fade.
3. Trocar de tela: posição no novo spawn e vida preservada.
4. Entrar na porta e abrir escolhas somente ao pressionar E.
5. Testar escolha inadequada, revisão e retorno da intervenção.
6. Conferir apoio móvel, quebra, restauração ao cair e conclusão apenas na terceira tela.
7. Tocar em espinhos: perder um coração sem teleportar; ao perder o último, abrir o menu de morte.

Base de layout para ajustes do autor; ainda exige teste de jogabilidade em execução.

## Correção das saídas

As triggers laterais cobrem Y = 0 até 1080, inclusive a trajetória de um salto ou dash.
A intervenção tem uma porta E de retorno perto do spawn (X = 32, Y = 704),
visível após confirmar a primeira solução e justificativa, inclusive uma escolha inadequada.
A saída do percurso de teste à direita mantém sua condição de solução parcial/adequada.
A primeira tela não exige solução para avançar. O retorno pela esquerda nas telas 2 e 3
usa spawns seguros junto ao fim da sala anterior e a animação desliza na direção correspondente.
