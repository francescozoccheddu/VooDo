﻿
using VooDo.AST;

namespace VooDo.Problems
{

    public class AssignmentOfConstantProblem : Problem
    {

        internal AssignmentOfConstantProblem(Node _source)
            : base(EKind.Semantic, ESeverity.Error, "Cannot assign a constant", _source) { }

    }



}
