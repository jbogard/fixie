﻿using System;
using System.Collections.Generic;

namespace Fixie.Tests
{
    public class StubListener : Listener
    {
        readonly List<string> log = new List<string>();

        public void CasePassed(Case @case)
        {
            log.Add(string.Format("{0} passed.", @case.Name));
        }

        public void CaseFailed(Case @case, Exception ex)
        {
            log.Add(string.Format("{0} failed: {1}", @case.Name, ex.Message));
        }

        public IEnumerable<string> Entries
        {
            get { return log; }
        }
    }
}