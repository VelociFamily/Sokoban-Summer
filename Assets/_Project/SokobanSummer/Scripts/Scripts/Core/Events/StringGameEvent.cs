using UnityEngine;
using UnityEngine.Events;

namespace Core.Events
{
    /// <summary>
    /// Game event that passes a string value
    /// Use for: level names, power-up types, messages, etc.
    /// </summary>
    [CreateAssetMenu(fileName = "New String Event", menuName = "Sokoban Summer/Events/String Event")]
    public class StringGameEvent : GenericGameEvent<string>
    {
    }
}
