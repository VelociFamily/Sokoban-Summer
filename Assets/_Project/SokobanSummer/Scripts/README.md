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

## Generated Input System (SokobanSummer.Input)

- The generated Input System class (`InputSystem_Actions.cs`) lives under `Assets/Settings/Input system things/` and is compiled into its own asmdef `SokobanSummer.Input` (`Assets/Settings/Input system things/Input.asmdef`).
- Assemblies that construct or reference `InputSystem_Actions` (for example `SokobanSummer.Core` and `SokobanSummer.UI`) should reference `SokobanSummer.Input` instead of `Assembly-CSharp` to ensure deterministic compile order.

## URP / Volume types

- If a script references URP-specific types such as `UnityEngine.Rendering.Volume` or `UnityEngine.Rendering.Universal.DepthOfField`, add the following references to the asmdef:
	- `Unity.RenderPipelines.Core.Runtime`
	- `Unity.RenderPipelines.Universal.Runtime`

This is necessary because those types are provided by the Universal Render Pipeline package assemblies.

## ModernAudioService convenience helpers

- The Audio assembly now includes small extension helpers that let callers use the `AudioChannelType` enum when interacting with `ModernAudioService`.
- Why: `CoreShared.IAudioManager` intentionally uses an `int` for channel identifiers to avoid a compile-time dependency on the Audio assembly. To make higher-level code ergonomic, the Audio assembly defines `ModernAudioService` extension methods that accept `Audio.AudioChannelType` and forward the call (with a cast) to the existing int-based API.
- Where: `Assets/Scripts/Audio/ModernAudioServiceExtensions.cs` (assembly `SokobanSummer.Audio`).
- Usage example:

```csharp
// Callers that have access to the Audio namespace can use the enum overloads directly
using Audio;

ModernAudioService.Instance.SetVolume(AudioChannelType.Master, 0.8f);
float master = ModernAudioService.Instance.GetVolume(AudioChannelType.Master);
```

Notes:
- These helpers are implemented inside the Audio assembly to avoid introducing a Core -> Audio dependency.
- If you prefer a different API surface (e.g., typed wrappers in another assembly), we can iterate on the design.

---

If you add new domains, create a new asmdef and update this README.
