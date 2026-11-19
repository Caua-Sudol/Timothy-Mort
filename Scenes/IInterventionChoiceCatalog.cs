using System;
using System.Collections.Generic;

namespace DontLikePoetry;

public interface IInterventionChoiceCatalog
{
    string JustificationQuestion { get; }

    IReadOnlyList<PresentedSolutionDefinition> CreateInitialSolutionPresentation(
        Random random);

    IReadOnlyList<PresentedJustificationDefinition>
        CreateInitialJustificationPresentation(Random random);

    InterventionSolutionDefinition GetSolution(string solutionId);
    InterventionJustificationDefinition GetJustification(string justificationId);

    // Room-format phases: question above the solutions and result-screen texts.
    // Null keeps the generic text (old-format phases).
    string SolutionQuestion => null;
    string GetResultAssessment(string solutionId) => null;
    string GetResultExplanation(string solutionId) => null;
}
