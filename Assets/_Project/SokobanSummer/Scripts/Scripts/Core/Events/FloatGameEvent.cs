using UnityEngine;
using UnityEngine.Events;

namespace Core.Events
{
    /// <summary>
    /// Game event that passes a float value
    /// Use for: timer values, progress percentages, speed multipliers, etc.
    /// </summary>
    [CreateAssetMenu(fileName = "New Float Event", menuName = "Sokoban Summer/Events/Float Event")]
    public class FloatGameEvent : GenericGameEvent<float>
    {
    }
}
