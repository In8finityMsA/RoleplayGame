﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game
{
    class Antidote : RemoveStateSpell
    {
        private const double MANA_COST = 20.0;
        public Antidote() : base(MANA_COST, false, true, State.POISONED) { }
    }
}
