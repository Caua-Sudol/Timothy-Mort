# Convenções de cenários

Este documento define o contrato mínimo entre os mapas do Tiled e o jogo.
Cada cenário é um arquivo TMX independente. O mapa define a geometria e os
pontos de fluxo; o código interpreta esses dados e decide qual cenário abrir.

## Object layers

| Layer | Quantidade | Finalidade |
| --- | ---: | --- |
| `mapaGeral` | 1 | Retângulos sólidos para colisão e blockout. |
| `gelatina` | 0 ou 1 | Volumes atravessáveis com comportamento especial. Inicialmente, só servem como dados e debug. |
| `spawnJogador` | 1 ou mais | Pontos nomeados de nascimento do jogador. O retângulo pode ser pequeno; sua posição superior esquerda é o spawn. |
| `saidaCena` | 0 ou mais | Áreas que levam o jogador a outro cenário. |

As layers futuras abaixo seguem a mesma convenção, mas não são necessárias em
todo mapa nem serão interpretadas antes de suas etapas próprias:

| Layer futura | Finalidade |
| --- | --- |
| `entradaIntervencao` | Área que abre a rota alternativa para as escolhas. |
| `opcoesSolucao` | Blocos de escolha de solução. |
| `opcoesJustificativa` | Blocos de escolha de justificativa. |
| `retornoPercurso` | Área que devolve o jogador ao trecho de teste. |
| `botaoIntervencao` | Botão pressionado ao ser pisado por cima; futuramente abre as escolhas. |
| `portaSaida` | Porta de retorno mantida fechada até uma solução permitir progressão. |
| `retornoIntervencao` | Saída condicionada da intervenção para o mapa do problema. |
| `rotaVerticalSolida` | Plataformas sólidas reveladas depois da primeira escolha. |
| `rotaVerticalGelatina` | Gelatinas reveladas depois da primeira escolha. |

## Objetos e propriedades

Todos os elementos jogáveis são objetos retangulares do Tiled, inclusive os
pontos. Isso mantém a edição do blockout simples e compatível com o leitor
atual do projeto.

Cada objeto dentro de `saidaCena` deve ter a propriedade customizada abaixo:

| Propriedade | Tipo | Exemplo | Uso |
| --- | --- | --- | --- |
| `destino` | texto | `affordance_visibilidade` | Identificador do cenário que deve ser carregado. |
| `spawnDestino` | texto | `spawn_inicial` | Nome do objeto em `spawnJogador` no cenário de destino. |
| `posicao` | texto | `tardia` | Variante de `entradaIntervencao`: `tardia` ou `antecipada`. |

Cada objeto da layer `spawnJogador` deve possuir um `name` único dentro do
mapa. Uma saída referencia esse nome por `spawnDestino`. Se a propriedade não
for informada ou não corresponder a um spawn existente, o jogo usa o primeiro
spawn do mapa e, por último, o fallback registrado para o cenário.

Os identificadores de destino usam somente letras minúsculas, números e
`_`. Eles não contêm caminhos de arquivo nem extensões. O registro central do
jogo faz a associação entre o identificador e seu TMX.

No futuro, os objetos de escolha também receberão propriedades customizadas
para seus identificadores. A regra será a mesma: o TMX informa o identificador
e a definição do cenário informa o significado e o efeito daquela escolha.

## Regras de organização

- A geometria de colisão fica apenas em `mapaGeral`.
- O volume de `gelatina` não é uma colisão sólida e não deve ser duplicado em
  `mapaGeral`.
- O mapa não decide qual solução é adequada, parcial ou inadequada.
- O mapa não contém lógica de tentativas, justificativas ou telemetria.
- Um mapa pode declarar entradas `tardia` e `antecipada`; o estado da fase
  decide qual delas fica visível e ativa.
- O mapa mínimo existente continua sendo um cenário de teste separado até a
  etapa de transição entre cenários.

## Convenção de coordenadas

O projeto usa tiles de `32 x 32` pixels. Para blockout, objetos devem ser
alinhados à grade sempre que possível. O player mede `16 x 16` pixels, então
spawns e volumes podem usar meio tile quando um ajuste fino for necessário.
