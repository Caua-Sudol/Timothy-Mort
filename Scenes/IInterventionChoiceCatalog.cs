using System;
using System.Collections.Generic;

namespace DontLikePoetry;

public interface IInterventionChoiceCatalog
{
    string JustificationQuestion { get; }
    IReadOnlyList<InterventionSolutionDefinition> AvailableRevisionSolutions { get; }

    IReadOnlyList<PresentedSolutionDefinition> CreateInitialSolutionPresentation(
        Random random);

    IReadOnlyList<PresentedJustificationDefinition>
        CreateInitialJustificationPresentation(Random random);

    InterventionSolutionDefinition GetSolution(string solutionId);
    InterventionJustificationDefinition GetJustification(string justificationId);
}
