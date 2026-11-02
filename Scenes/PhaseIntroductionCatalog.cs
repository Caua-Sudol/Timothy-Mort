using System;

namespace DontLikePoetry;

public static class PhaseIntroductionCatalog
{
    public static (string Title, string Text) Get(string phaseId)
    {
        return phaseId switch
        {
            ScenarioCatalog.AffordanceVisibilityScenarioId => (
                "Affordance e visibilidade",
                "Affordance é uma possibilidade de ação que surge da relação entre uma pessoa e um objeto. " +
                "Uma superfície pode, por exemplo, permitir apoio ou passagem.\n\n" +
                "A visibilidade ajuda a perceber essas possibilidades. A aparência pode oferecer pistas " +
                "sobre o que é possível fazer, mas nem sempre essas pistas são claras.\n\n" +
                "Nesta fase, observe o que as superfícies parecem permitir e o que acontece ao usá-las."),

            ScenarioCatalog.DamageFeedbackScenarioId => (
                "Feedback",
                "Feedback é a informação que o jogo apresenta em resposta a uma ação ou acontecimento. " +
                "Ele ajuda a perceber o resultado de uma interação.\n\n" +
                "Uma resposta pode comunicar tanto um evento quanto uma mudança de estado. " +
                "Para compreender o que aconteceu, precisamos perceber essa relação no momento certo.\n\n" +
                "Nesta fase, observe como o jogo comunica as consequências do contato com o perigo."),

            ScenarioCatalog.ConceptualModelsConstraintsScenarioId => (
                "Modelos conceituais",
                "Um modelo conceitual é uma explicação de como algo funciona. " +
                "Ao jogar, construímos uma compreensão das regras e a usamos para prever resultados.\n\n" +
                "Quando algo parece igual ao que já conhecemos, esperamos que funcione do mesmo jeito.\n\n" +
                "Nesta fase, compare suas expectativas com o comportamento das plataformas."),

            _ => throw new ArgumentException("A fase não possui introdução.", nameof(phaseId))
        };
    }
}
