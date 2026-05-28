// Located at: Assets/Scripts/Core/Stats/CalculationStage.cs
namespace ProjectWitchcraft.Core
{
    /// <summary>
    /// Defines the distinct stages of the stat calculation pipeline.
    /// The order of the values in this enum determines the order of operations.
    /// </summary>
    public enum CalculationStage
    {
        /// <summary>
        /// The starting value before any modifications.
        /// A modifier at this stage will replace the base value entirely.
        /// </summary>
        Override = 0,

        /// <summary>
        /// Stage for flat additive bonuses (e.g., +10 Health). Applied to the base or override value.
        /// </summary>
        Flat = 100,

        /// <summary>
        /// Stage for additive percentage bonuses (e.g., +10% Speed).
        /// All bonuses in this stage are summed together before being applied.
        /// </summary>
        Additive = 200,

        /// <summary>
        /// Stage for multiplicative percentage bonuses (e.g., *1.5x Damage).
        /// Each bonus in this stage is applied as a separate multiplier.
        /// </summary>
        Multiplicative = 300
    }
}

