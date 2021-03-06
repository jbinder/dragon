﻿using System.Collections.Generic;
using Dragon.CPR.Errors;
using Dragon.Data.Interfaces;

namespace Dragon.CPR
{
    public abstract class DispatcherBase<TDispatched> 
    {
        public abstract IEnumerable<ErrorBase> Dispatch(TDispatched o);
    }
}
