# Sokoban Summer Assembly Definitions

This project uses Unity Assembly Definitions (asmdefs) to speed up compilation and enforce code boundaries.

## Assembly Boundaries

- **SokobanSummer.Core**: Core logic, services, and singletons. No UI or gameplay specifics. Referenced by all other assemblies.
- **SokobanSummer.Gameplay**: Gameplay logic, player, power-ups, and level scripts. References Core.
- **SokobanSummer.UI**: UI logic and components. References Core. Can reference Gameplay if UI needs gameplay data.
- **SokobanSummer.Effects**: Visual effects scripts. References Core and UnityEngine.UI.
- **SokobanSummer.Editor**: Editor-only utilities. References Core. Only included in Editor builds.

## Guidelines
- Place scripts in the correct folder/domain.
- Editor scripts must be in `Assets/Editor` and only reference runtime assemblies.
- Do not reference UI or Gameplay from Core.
- Use asmdef references, not direct script links.

## Unity References
- All runtime assemblies reference `UnityEngine.UI`, `Unity.TextMeshPro`, and `Unity.InputSystem` as needed.
- Editor assembly is restricted to Editor platform.

---

If you add new domains, create a new asmdef and update this README.
