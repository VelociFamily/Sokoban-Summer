using UnityEngine;
using UnityEngine.Events;

namespace Core.Events
{
    /// <summary>
    /// Game event that passes an integer value
    /// Use for: move counts, scores, level numbers, etc.
    /// </summary>
    [CreateAssetMenu(fileName = "New Int Event", menuName = "Sokoban Summer/Events/Int Event")]
    public class IntGameEvent : GenericGameEvent<int>
    {
    }
}
