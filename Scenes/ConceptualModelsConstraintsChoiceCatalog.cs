using System;
using System.Collections.Generic;

namespace DontLikePoetry;

public sealed class ConceptualModelsConstraintsChoiceCatalog : IInterventionChoiceCatalog
{
    private static readonly IReadOnlyList<PresentedSolutionDefinition>
        NoPresentedSolutions = Array.Empty<PresentedSolutionDefinition>();
    private static readonly IReadOnlyList<PresentedJustificationDefinition>
        NoPresentedJustifications = Array.Empty<PresentedJustificationDefinition>();
    private static readonly IReadOnlyList<InterventionSolutionDefinition>
        NoRevisionSolutions = Array.Empty<InterventionSolutionDefinition>();

    public static ConceptualModelsConstraintsChoiceCatalog Instance { get; } =
        new ConceptualModelsConstraintsChoiceCatalog();

    public string JustificationQuestion
    {
        get { return string.Empty; }
    }

    public IReadOnlyList<InterventionSolutionDefinition> AvailableRevisionSolutions
    {
        get { return NoRevisionSolutions; }
    }

    private ConceptualModelsConstraintsChoiceCatalog()
    {
    }

    public IReadOnlyList<PresentedSolutionDefinition> CreateInitialSolutionPresentation(
        Random random)
    {
        return NoPresentedSolutions;
    }

    public IReadOnlyList<PresentedJustificationDefinition>
        CreateInitialJustificationPresentation(Random random)
    {
        return NoPresentedJustifications;
    }

    public InterventionSolutionDefinition GetSolution(string solutionId)
    {
        throw new InvalidOperationException(
            "O catálogo de soluções de modelos conceituais ainda não foi preenchido.");
    }

    public InterventionJustificationDefinition GetJustification(
        string justificationId)
    {
        throw new InvalidOperationException(
            "O catálogo de justificativas de modelos conceituais ainda não foi preenchido.");
    }
}
