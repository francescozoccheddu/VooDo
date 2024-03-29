﻿
using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace VooDo.Runtime
{

    public delegate void ProgramReturnedEventHandler<TReturn>(TReturn _value);

    public interface ITypedProgram<TReturn> : IProgram
    {

        event ProgramReturnedEventHandler<TReturn?>? OnReturn;

    }

    public interface ITypedProgram : IProgram
    {

        event ProgramReturnedEventHandler<object?>? OnReturn;

    }

    public interface IProgram
    {

        Loader Loader { get; }
        Type ReturnType { get; }

        bool IsRunRequested { get; }
        bool IsLocked { get; }
        bool IsStoringRequests { get; }
        ILocker Lock(bool _storeRequests = true);
        void RequestRun();
        void CancelRunRequest();
        void Freeze();

        ImmutableArray<IVariable> Variables { get; }
        ImmutableArray<IVariable> GetVariables(string _name);
        IEnumerable<Variable<TValue>> GetVariables<TValue>(string _name) where TValue : notnull;
        IVariable? GetVariable(string _name);
        Variable<TValue>? GetVariable<TValue>(string _name) where TValue : notnull;

    }

    public interface ILocker : IDisposable
    {

        bool IsDisposed { get; }
        bool AllowsStoringRequests { get; }
        IProgram Program { get; }

    }

}
