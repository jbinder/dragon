﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dragon.CPR.Errors
{
    public class ErrorBase 
    {
        public string PropertyBase { get; set; }
        public int? Index { get; set; }

        public string PropertyName { get; set; }
        public string Message { get; set; }
    }
}
