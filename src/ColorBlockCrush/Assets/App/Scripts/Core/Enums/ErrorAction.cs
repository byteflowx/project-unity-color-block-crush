using System;

namespace App.Scripts.Core.Enums
{
    [Flags]
    public enum ErrorAction
    {
        Retry = 1,
        Skip = 2
    }
}