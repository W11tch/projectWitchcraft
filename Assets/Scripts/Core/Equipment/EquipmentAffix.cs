using System;

namespace ProjectWitchcraft.Core
{
    [Serializable]
    public class EquipmentAffix
    {
        public StatDefinition Stat;
        public float Value;
        // Flat = ADD, Additive = INCREASE/DECREASE, Multiplicative = MORE/LESS
        public CalculationStage Stage;
    }
}
