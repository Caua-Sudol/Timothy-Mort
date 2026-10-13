# Affordance — três soluções e preparação de arte

## Ciclo atual

- Aparência: alterar a aparência dos terrenos estranhos. Provisoriamente aplica cor azul; falta a arte de gelatina.
- Marcação: marcar os terrenos estranhos. Provisoriamente desenha uma marca quadrada; falta o bloco marcado.
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

A mesma técnica poderá ser usada na gelatina da demonstração final. Uma animação desenhada quadro a quadro fica como alternativa se o esticamento dos detalhes não ficar bom. Verificar nitidez, tremulação e alinhamento visual com a colisão no zoom real.

## Pendências para fechar o ciclo visual

1. Produzir e integrar as variantes de terreno; a cor azul atual não substitui a leitura do material.
2. Conferir os formatos efetivamente usados nos mapas antes de exportar todas as peças.
3. Produzir e integrar o fundo da intervenção.
4. Preparar o texto introdutório de Mort sobre terrenos estranhos e o uso do dash; a fala ainda não foi implementada.
5. Implementar a demonstração final proposta: Mort sugere combinar gelatina com pulsação, depois da comparação individual. Ainda não há essa demonstração visual.

Não é preciso desenhar um asset separado de gelatina pulsante. Corações, seletor e fundos de pause/morte pertencem à lista de UI e ficam fora desta alteração.

## Validação manual

1. Iniciar uma tentativa nova de affordance e abrir a intervenção: conferir exatamente três propostas e justificativas sobre terrenos.
2. Escolher cada alternativa, retornar ao percurso e observar cor provisória, marca e pulsação. Confirmar física igual e saída disponível nas três.
3. Voltar à intervenção e trocar mais de uma vez, inclusive após escolher aparência. Conferir que os efeitos não se acumulam.
4. Concluir e verificar primeira escolha, última escolha e explicação correspondente.
5. Conferir que as demais fases mantêm suas opções e regras anteriores.

Observação: a implementação atual da gelatina limita a velocidade de queda e restaura o dash ao contato. A associação visual com algo escorregadio precisa ser validada contra esse comportamento.

Exercício opcional (10–15 min): localizar uma descrição em Scenes/AffordanceChoiceCatalog.cs, reescrevê-la sem revelar o resultado e conferir sua legibilidade no cartão. Atualmente as três usam frases genéricas; o esperado é preservar essa neutralidade. Reverter somente a frase ao terminar, se preferir.

Nenhuma nova dívida de domínio foi identificada neste ajuste. A validação visual depende dos assets finais.
