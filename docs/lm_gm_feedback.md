# Feedback (LM-GM)

Este documento mapeia a fase 2, `feedback_dano`, pelo modelo Learning Mechanics–Game Mechanics (LM-GM). Ele registra uma **hipótese de design**: como as mecânicas de jogo devem sustentar as atividades de aprendizagem da fase. Não demonstra, por si só, que a aprendizagem ocorreu; isso depende dos testes com participantes e da telemetria.

## Referência e uso do modelo

Arnab, S. et al. (2015). *Mapping learning and game mechanics for serious games analysis*. British Journal of Educational Technology, 46(2), 391–411. DOI: [10.1111/bjet.12113](https://doi.org/10.1111/bjet.12113)

Como nas outras fases, o mapeamento tem duas partes:

- uma **tabela**: game mechanic, learning mechanic, implementação e uso pelo jogador;
- um **mapa** do fluxo, organizado pelos níveis de Bloom do modelo: Retention, Understanding, Applying, Analysing, Evaluating e Creating.

Os nomes das mecânicas seguem os nós do artigo.

## Escopo da fase

As salas têm **rochas quentes**, com a mesma aparência das plataformas comuns. Ficar em contato com elas, no chão, na parede ou no teto, tira **um coração a cada 0,6 s**. Não há nenhum sinal: nem recuo, nem piscar, nem som. Só o HUD de corações muda. O jogador perde vida sem perceber de onde ela veio.

O objetivo de aprendizagem é **reconhecer que uma consequência sem resposta perceptível é um problema de feedback, e avaliar se uma intervenção comunica o evento (quando e onde), a perda (quanto) ou só um alerta genérico**.

## Fluxo da fase

```text
Introdução da fase (texto curto)
-> Sala 1: rochas quentes ferem sem sinal
   (o Mort comenta o primeiro dano, 1 s depois de o jogador sair da rocha)
-> Sala 2: se o jogador chegar sem se ferir, o Mort avisa que as rochas ferem;
   a passagem para a sala 3 fica bloqueada até a primeira escolha
-> Sala de intervenção (entrada com E na sala 2, ou na sala 1 após duas mortes):
     espada: testar soluções; plataformas quentes de exemplo na sala;
     saída: justificativa (3 opções) ou "Continuar testando soluções"
-> Sala 3: percurso com a solução aplicada; com a melhor solução,
   o Mort demonstra a combinação recuo + coração rachando
-> Tela "Fase concluída"
```

Regras de suporte:

- **Vida:** cinco corações. Sem corações, abre-se o menu de morte; no Release, "Tentar novamente" recomeça da sala 1.
- **Saída da intervenção:** os corações são restaurados ao sair da sala de intervenção.
- **Paredes:** as bordas laterais das salas são paredes invisíveis.

## Tabela LM-GM

| Game mechanic | Learning mechanic | Implementação | Uso pelo jogador |
|---|---|---|---|
| Movement; Simulate/Response | Explore; Experimentation | Rochas quentes iguais às plataformas comuns, com dano contínuo de 1 coração / 0,6 s e invulnerabilidade escondida. | Viver o problema: perder vida sem entender por quê. |
| Feedback (ausente); Status | Observation; Identify | Só o HUD de corações muda. O Mort comenta o primeiro dano ("Você se feriu e nada mostrou isso!") depois de um pequeno atraso, para o dano passar despercebido antes. | Notar que a informação existia, mas não foi comunicada no momento nem no lugar do evento. |
| Rewards/Penalties; Levels | Hypothesis | Perda de corações, menu de morte e passagem para a sala 3 bloqueada até a escolha. | Relacionar o custo à falta de feedback, e não à própria habilidade. |
| Selecting/Collecting; Simulate/Response | Experimentation; Analyse | A espada abre três soluções, **Recuo**, **Coração rachando** e **Tela tremendo**, que podem ser trocadas à vontade e testadas nas rochas da sala. | Comparar o que cada resposta comunica sobre o mesmo dano. |
| Feedback | Feedback; Reflect/Discuss | A espada pulsa forte depois da primeira escolha. O Mort comenta cada solução com uma pergunta ("mostra a perda... mas mostra de onde ela veio?"). | Ser convidado a testar outra solução. |
| Questions and Answers | Reflect/Discuss; Hypothesis | Na saída: "Que resultado você espera obter com essa intervenção?", com 3 justificativas e "Continuar testando soluções". | Explicitar o que a intervenção deveria comunicar. |
| Levels; Movement | Action/Task; Imitation | Sala 3 com a solução aplicada. Com o Recuo, o Mort demonstra o Recuo + Coração rachando. | Verificar o efeito no percurso e ver uma combinação de feedbacks. |
| Assessment; Feedback | Assessment; Reflect/Discuss | Tela "Fase concluída", com avaliação curta ("Pista de contato e direção", "Pista de perda de vida", "Pista de alerta geral") e explicação. | Comparar a hipótese com uma leitura de design. |

## Mapa do fluxo (níveis de Bloom)

| Momento | Nível | Learning mechanics | Game mechanics |
|---|---|---|---|
| Salas 1–2 | Understanding → Applying | Explore, Experimentation, Observation | Movement, Simulate/Response, Status |
| Fala do Mort e bloqueio | Analysing | Identify, Hypothesis | Cut scenes/Story, Rewards/Penalties |
| Espada (testar soluções) | Analysing → Evaluating | Experimentation, Analyse, Feedback | Selecting/Collecting, Simulate/Response, Feedback |
| Saída com justificativa | Evaluating | Reflect/Discuss, Hypothesis | Questions and Answers |
| Sala 3 e demonstração | Applying → Evaluating | Action/Task, Imitation | Levels, Movement |
| Fase concluída | Evaluating | Assessment, Reflect/Discuss | Assessment, Feedback |

## Leitura das soluções

A qualidade fica registrada na telemetria e **não é mostrada ao jogador**. Todas as soluções liberam a sala 3.

| Solução | Efeito | Qualidade registrada | Leitura |
|---|---|---|---|
| Recuo (`damage_knockback`) | Empurra o personagem para longe da superfície, com um pequeno salto e 0,2 s sem controle. | ADEQUATE (melhor) | Liga o dano ao contato: mostra quando, onde e de que direção veio, e ainda afasta do perigo. |
| Coração rachando (`damage_heart_crack`) | Um coração racha sobre o personagem. | PARTIAL | Mostra a perda perto do olhar, mas não a origem. |
| Tela tremendo (`damage_screen_shake`) | A tela treme e as bordas ficam vermelhas. | PARTIAL | Alerta intenso, mas genérico. Não aponta a origem e cansa quando se repete. |

As três justificativas são:

- `damage_source`: "Relacionar o dano à superfície que o causou";
- `damage_amount`: "Saber quanta vida foi perdida";
- `damage_attention`: "Chamar atenção para o dano, mesmo olhando para outro ponto da tela".

Elas correspondem, de forma aproximada, a cada uma das soluções.

## Evidências na telemetria

Os campos são os mesmos das outras fases. Por fase:

- a solução final, a justificativa e as posições apresentadas;
- `hadRevision` e o número de escolhas;
- o tempo, as mortes e os dados da demonstração.

Na atividade da sessão:

- tempo, entradas e mortes por sala;
- a última sala alcançada;
- os eventos da intervenção, em ordem.

Perguntas que esses dados permitem investigar:

- se as mortes da sala 1 caem depois da fala do Mort;
- se quem testa várias soluções prefere o Recuo;
- se a justificativa combina com a solução escolhida.

## Limites

- A qualidade das soluções é uma hipótese de design.
- **Invencibilidade:** passou de 1,0 s para 0,6 s em 04/11/2026, porque dava para atravessar o fogo.
- **Fala do Mort:** passou a esperar o jogador sair da rocha. Dados anteriores a essas mudanças não são diretamente comparáveis.
- **Justificativas:** passaram a ser pedidas na saída da intervenção, e as opções caíram de 6 para 3.
- O teste com especialistas mostrou que a dificuldade de plataforma pode competir com o problema de feedback.
