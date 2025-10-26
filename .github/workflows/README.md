# Unity CI Workflow with Self-Hosted Runner Support

## Overview

The Unity CI workflow (`unity-ci.yml`) has been enhanced to support self-hosted Unity runners with automatic fallback to Windows runners. This provides better performance when self-hosted runners are available while ensuring builds can still complete when they're not.

## How It Works

### 1. Automatic Flow (PR Events)
When a PR is closed to the main branch, the workflow:
1. **Attempts self-hosted runner first** - Tries to build on `[self-hosted, unity]` runners
2. **Requests approval for fallback** - If self-hosted fails, requires manual approval to use Windows runner
3. **Falls back to Windows runner** - After approval, builds on `windows-latest`
4. **Provides summary** - Shows the results of both attempts

### 2. Manual Controls (workflow_dispatch)
You can manually trigger the workflow with these options:
- **`branch`** - Specify which branch to run the workflow on (leave empty for current branch)
- **`force_windows: true`** - Skip self-hosted runner entirely and use Windows runner
- **`retry_self_hosted: true`** - Force retry of self-hosted runner (useful when you just turned it on)

## Runner Configuration

### Self-Hosted Runner Setup
Your self-hosted runner should be configured with:
- **Labels**: `self-hosted`, `unity` 
- **Unity Editor**: Version 6000.2.8f1 installed
- **Game CI tools**: Unity Builder dependencies

### Required Secrets
The workflow requires these repository secrets:
- `UNITY_EMAIL` - Your Unity account email
- `UNITY_PASSWORD` - Your Unity account password  
- `UNITY_LICENSE` - Your Unity license content (or use `UNITY_SERIAL`)

## Environment Protection

The workflow uses a protected environment called `windows-runner-approval` to gate the fallback to Windows runners. To set this up:

1. Go to Settings → Environments in your repository
2. Create environment named `windows-runner-approval`
3. Add required reviewers (yourself or team members)
4. Configure protection rules as needed

## Usage Scenarios

### Scenario 1: Self-hosted runner is online and working
- PR closes → Self-hosted build succeeds → Done ✅
- No approval needed, fastest path

### Scenario 2: Self-hosted runner is offline or build fails  
- PR closes → Self-hosted build fails → Approval requested
- Review the approval request → Approve → Windows build runs ✅
- Or cancel and wait for self-hosted runner to come online

### Scenario 3: You want to force Windows runner
- Use Manual workflow dispatch
- Set `force_windows: true`
- Windows runner used immediately ✅

### Scenario 4: You just turned on self-hosted runner
- Use Manual workflow dispatch  
- Set `retry_self_hosted: true`
- Self-hosted runner attempted again ✅

### Scenario 5: You want to test a specific branch
- Use Manual workflow dispatch
- Set `branch` to the branch name (e.g., `feature/my-branch`)
- Optionally combine with other options like `force_windows` or `retry_self_hosted`
- Workflow runs on the specified branch ✅

## Build Artifacts

Builds produce different artifact names based on the runner:
- Self-hosted builds: `unity-build-StandaloneWindows64-self-hosted`
- Windows builds: `unity-build-StandaloneWindows64-windows`

## Troubleshooting

### Self-hosted runner not being used
- Check runner is online and has correct labels: `[self-hosted, unity]`
- Verify Unity version 6000.2.8f1 is installed
- Check runner can access the repository

### Windows fallback not working
- Verify `windows-runner-approval` environment exists
- Check environment protection rules and reviewers
- Ensure required secrets are configured

### Build failures
- Check Unity license is valid and secrets are configured
- Verify Unity version matches project (6000.2.8f1)
- Review build logs in the failed job