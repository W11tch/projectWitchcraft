// Located at: Assets/Scripts/Core/Stats/StatModifier.cs
namespace ProjectWitchcraft.Core
{
    /// <summary>
    /// Represents a single modification to a stat. This is a plain C# class, not a MonoBehaviour or ScriptableObject.
    /// </summary>
    public class StatModifier
    {
        /// <summary>
        /// The numerical value of the modification.
        /// </summary>
        public readonly float Value;

        /// <summary>
        /// The stage in the calculation pipeline where this modifier is applied.
        /// </summary>
        public readonly CalculationStage Stage;

        /// <summary>
        /// The object that is the source of this modifier (e.g., an item, a skill, a temporary buff).
        /// Used as a key to prevent stacking and to allow for easy removal.
        /// </summary>
        public readonly object Source;

        /// <summary>
        /// Creates a new stat modifier.
        /// </summary>
        /// <param name="value">The numerical value of the modification.</param>
        /// <param name="stage">The calculation stage (e.g., Flat, Additive, Multiplicative).</param>
        /// <param name="source">The object providing this modifier. Must not be null.</param>
        public StatModifier(float value, CalculationStage stage, object source)
        {
            Value = value;
            Stage = stage;
            Source = source;
        }
    }
}

    