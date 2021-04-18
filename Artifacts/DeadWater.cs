using Artifacts;
using game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Artifacts
{
    public sealed class DeadWater: Water
    {
        public DeadWater(BottleSize size) : base(size) { }

        protected sealed override void ActionsWithHealth(Character target)
        {
            target.Health -= (int)Size;
        }
    }
}
