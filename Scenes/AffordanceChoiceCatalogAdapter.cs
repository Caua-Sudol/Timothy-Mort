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

    public IReadOnlyList<InterventionSolutionDefinition> AvailableRevisionSolutions
    {
        get { return AffordanceChoiceCatalog.AvailableRevisionSolutions; }
    }

    private AffordanceChoiceCatalogAdapter()
    {
    }

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
