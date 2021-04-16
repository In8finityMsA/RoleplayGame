using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game
{
    public class LivingWater : Artifact
    {
        public static readonly short SpellID = SpellIDManager.GetNextID();
        public override short GetClassID()
        {
            return SpellID;
        }

        public LivingWater() : base(false, 1)
        {
        }

        public override void MagicEffect(Magician user, Character target)
        {
            Charge--;
            
        }
    }
}
