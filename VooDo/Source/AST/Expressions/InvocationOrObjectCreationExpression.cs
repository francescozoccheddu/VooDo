﻿using System;

namespace VooDo.AST.Expressions
{

    public abstract record InvocationOrObjectCreationExpression : Expression
    {

        public abstract override InvocationOrObjectCreationExpression ReplaceNodes(Func<NodeOrIdentifier?, NodeOrIdentifier?> _map);

    }

}
