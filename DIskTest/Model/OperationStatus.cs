using System;
using System.Collections.Generic;
using System.Text;

namespace DIskTest.Model
{
    public enum OperationStatus
    {
        NotStarted,
        Started,
        InitMemBuffer,
        Running,
        Completed,
        NotEnoughMemory,
    };
}
