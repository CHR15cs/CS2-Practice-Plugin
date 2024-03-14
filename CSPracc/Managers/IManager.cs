﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSPracc.Managers
{
    public interface IManager : IDisposable
    {
        public void RegisterCommands();
        public void DeregisterCommands();
    }
}