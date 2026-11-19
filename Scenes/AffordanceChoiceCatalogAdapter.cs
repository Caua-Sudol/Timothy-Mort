using System;
using System.Collections.Generic;

namespace DontLikePoetry;

public sealed class AffordanceChoiceCatalogAdapter : IInterventionChoiceCatalog
{
    public static AffordanceChoiceCatalogAdapter Instance { get; } =
        new AffordanceChoiceCatalogAdapter();

    public string JustificationQuestion
    {
        get { return AffordanceChoiceCatalog.JustificationQuestion; }
    }

    public string SolutionQuestion => AffordanceChoiceCatalog.SolutionQuestion;

    private AffordanceChoiceCatalogAdapter()
    {
    }

    public string GetResultAssessment(string solutionId) => solutionId switch
    {
        null => "Sem escolha",
        AffordanceChoiceCatalog.AppearanceSolutionId => "Pista de material",
        AffordanceChoiceCatalog.MarkSolutionId => "Pista de identificação",
        _ => "Pista de movimento"
    };

    public string GetResultExplanation(string solutionId) =>
        AffordanceChoiceCatalog.GetExplanation(solutionId);

    public IReadOnlyList<PresentedSolutionDefinition> CreateInitialSolutionPresentation(
        Random random)
    {
        return AffordanceChoiceCatalog.CreateInitialSolutionPresentation(random);
    }

    public IReadOnlyList<PresentedJustificationDefinition>
        CreateInitialJustificationPresentation(Random random)
    {
        return AffordanceChoiceCatalog.CreateInitialJustificationPresentation(random);
    }

    public InterventionSolutionDefinition GetSolution(string solutionId)
    {
        return AffordanceChoiceCatalog.GetSolution(solutionId);
    }

    public InterventionJustificationDefinition GetJustification(
        string justificationId)
    {
        return AffordanceChoiceCatalog.GetJustification(justificationId);
    }
}
