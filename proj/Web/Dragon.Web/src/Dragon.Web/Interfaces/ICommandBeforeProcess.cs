﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dragon.Web.Interfaces
{
    public interface ICommandBeforeProcess<in T>
    {
        void BeforeProcess(T obj);
    }
}
