using UnityEngine;

/// <summary>
/// Interface for externally setting an object's colors.
/// </summary>
public interface IColorable
{
    /// <summary>
    /// Tells the object which colors to use in any of 3 given states.
    /// </summary>
    /// <param name="groundedColor">The color to use when grounded.</param>
    /// <param name="jumpingColor">The color to use when not grounded and jumping.</param>
    /// <param name="fallingColor">The color to use when not grounded and not jumping.</param>
    void SetColors(Color groundedColor, Color jumpingColor, Color fallingColor);

    /// <summary>
    /// Calculates the object's color based on its state. <see cref="SetColors(Color, Color, Color)"/> must be called prior to using this.
    /// </summary>
    /// <returns>One of the 3 available colors, as described in <see cref="SetColors(Color, Color, Color)"/>.</returns>
    Color GetCurrentColor();
}
