# Affordance — três soluções e preparação de arte

> **Documento histórico.** Registro da preparação de arte de outubro/2026. O fluxo atual da sala de intervenção (testar na espada, justificar na saída) está em [lm_gm_affordance_visibilidade.md](lm_gm_affordance_visibilidade.md).

## Ciclo atual

- Aparência: usa os assets da camada Solucao_gelatina do LDtk.
- Marcação: usa os assets da camada Solucao_mark do LDtk.
- Deformação: deformar os terrenos estranhos. Reutiliza a pulsação contínua por código sobre a aparência atual.
- As três opções conservam a física, liberam a saída e podem ser escolhidas novamente na intervenção, usando E. A primeira escolha e justificativa são preservadas; escolhas posteriores atualizam a solução aplicada.
- A ordem inicial é embaralhada e permanece igual nas novas escolhas. As outras fases conservam suas regras de revisão.
- O rastro e a indicação de uso/recarga do dash são comuns às três opções, desde o início do affordance.
- A pergunta compara qual aparência permite prever melhor o comportamento. No registro existente, aparência usa ADEQUATE e as outras duas PARTIAL; nenhuma usa INADEQUATE. São categorias provisórias, não uma validação empírica da arte.
- Os novos IDs terrain_appearance, terrain_mark e terrain_motion distinguem as propostas das 11 antigas. Nenhum dado existente foi migrado ou alterado.

## Desenhos necessários

| Arte | Preparação | Papel |
|---|---|---|
| Terra normal, variação adicional | Mesma escala de detalhes e contorno dos blocos atuais | Variedade de composição, sem indicar comportamento especial |
| Bloco com feno / cabana com teto de feno | Separar a área de apoio da decoração; decidir se é um único objeto ou peças modulares | Composição da aldeia; não é uma quarta solução |
| Terra marcada | Manter a mesma silhueta e dimensões da terra de referência; acrescentar marca consistente | Identificar terrenos diferentes sem antecipar uma ação específica |
| Gelatina | Mesmas dimensões de referência; material legível no zoom escolhido | Oferecer pista sobre a propriedade do terreno |
| Fundo da intervenção | Composição em 16:9, referência lógica 1920×1080; pouca informação atrás de texto e pontos de interação | Dar contexto à sala sem competir com a interface |

Para cada formato usado no Tiled, preparar versões correspondentes de terra, terra marcada e gelatina. Se houver blocos curtos, longos ou verticais, preservar suas proporções: não depender de esticar uma imagem quadrada para todos os objetos. Pode-se preparar pontas e miolo repetível, mas a montagem modular ainda exigiria integração.

Usar fundo transparente nos blocos e manter o mesmo canvas, alinhamento e área visível entre variantes. Uma marca em PNG separado também é possível, caso se prefira reaproveitá-la em vários formatos; sua integração está pendente.

## Pulsação

Não são necessários frames de animação para fechar este ciclo. World/LevelMap.cs já possui GetGelatinVisualBounds: aplica uma deformação periódica pequena apenas ao retângulo de desenho, com coordenadas inteiras. A colisão e a física permanecem fixas; a renderização do cenário usa PointClamp.

A demonstração final já combina a arte de gelatina com essa pulsação na tela 3. A fala considera a escolha ativa (ou sua ausência), e a demonstração não substitui a escolha nos registros.

## Separação na telemetria

- Aparência permanece ADEQUATE; Marcação e Deformação são PARTIAL. Não existe opção INADEQUATE no catálogo atual de affordance. O enum continua existindo para as outras fases e dados históricos.
- Ao iniciar a fala da combinação, PhaseState captura cenário, horário UTC, escolha ativa (nula se ainda não escolheu), módulos demonstrados, tempo ativo e mortes acumulados.
- O tempo e as mortes após a exposição são acumulados separadamente, inclusive se o jogador voltar às salas anteriores. Esses valores não significam tempo exclusivo sob o efeito combinado.
- A demonstração é registrada uma vez por execução da fase, sem aumentar selection_count nem alterar primeira escolha, justificativa ou última escolha.
- Ao concluir a fase, o registro é salvo na tabela phase_demonstrations, na mesma transação da telemetria, e exportado em phases[].demonstration no JSON.
- Sessões antigas sem esse registro exportam demonstration nulo: isso significa ausência de registro, não prova de ausência de exposição. A nova tabela é criada sem reclassificar dados antigos.
- Como os demais dados da fase, este registro só é persistido na conclusão. Abandonar ou fechar o jogo antes disso não salva a demonstração.

## Pendências para fechar o ciclo visual

1. Entrada simples para a intervenção: proposta de portal, sem raízes.
2. Fundo simples para a intervenção.
3. Saída da intervenção.
4. Espada do soldado morto, usada por Mort para alterar o mapa.
5. Conferir a indicação visual da saída final da fase.

Não é preciso desenhar um asset separado de gelatina pulsante. Corações, seletor e fundos de pause/morte pertencem à lista de UI e ficam fora desta alteração.

## Validação manual

1. Iniciar uma tentativa nova de affordance e abrir a intervenção: conferir exatamente três propostas e justificativas sobre terrenos.
2. Escolher cada alternativa, retornar ao percurso e observar os assets de gelatina, terra marcada e pulsação. Confirmar física igual e saída disponível nas três.
3. Voltar à intervenção e trocar mais de uma vez, inclusive após escolher aparência. Conferir que os efeitos não se acumulam.
4. Entrar na tela 3 com cada escolha e sem escolha: conferir a fala correspondente e gelatina com pulsação. Retornar à tela 3 não deve iniciar outro registro.
5. Concluir e conferir primeira escolha, última escolha e o objeto demonstration no JSON exportado. ActiveSecondsAtStart + ActiveSecondsAfterExposure deve corresponder ao tempo ativo total; TotalDeathsAtStart + DeathsAfterExposure deve corresponder às mortes totais.
6. Conferir que as demais fases mantêm suas opções e regras anteriores.

Observação: a implementação atual da gelatina limita a velocidade de queda e restaura o dash ao contato. A associação visual com algo escorregadio precisa ser validada contra esse comportamento.

Exercício opcional (10–15 min): localizar uma descrição em Scenes/AffordanceChoiceCatalog.cs, reescrevê-la sem revelar o resultado e conferir sua legibilidade no cartão. Atualmente as três usam frases genéricas; o esperado é preservar essa neutralidade. Reverter somente a frase ao terminar, se preferir.

Nenhuma nova dívida de domínio foi identificada neste ajuste. A validação visual depende dos assets finais.
