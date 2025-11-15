# Unity Version Upgrade to 6000.2.12f1

Target Editor: Unity 6.2 (6000.2.12f1)

## Actions
- Updated `ProjectSettings/ProjectVersion.txt` version lines.
- Placeholder revision hash used; will be replaced automatically after opening the project in the new Unity editor.

## Next Steps
1. Open the project in Unity 6000.2.12f1 so it regenerates the correct revision hash and any package updates.
2. Review the auto-updated `ProjectVersion.txt` and commit the real hash.
3. Check Package Manager for outdated / auto-upgraded packages and ensure no compile errors.
4. Run CI workflow to confirm successful build with new version.
5. Remove this file or convert to a changelog entry if desired.

## Validation Checklist
- [ ] Project opens with no domain reload errors.
- [ ] All scripts compile.
- [ ] Play Mode initializes via `GameInitializer` without warnings.
- [ ] CI passes on Unity 6000.2.12f1.
