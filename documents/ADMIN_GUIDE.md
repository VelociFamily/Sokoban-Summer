# Repository Administration Guide

This guide covers repository administration tasks for Sokoban Summer, including branch protection configuration and admin permissions.

## Table of Contents
- [Branch Protection and Admin Bypass](#branch-protection-and-admin-bypass)
- [Deleting Protected Branches](#deleting-protected-branches)
- [Repository Settings Overview](#repository-settings-overview)
- [Troubleshooting](#troubleshooting)

## Branch Protection and Admin Bypass

### Understanding Branch Protection
Branch protection rules help prevent accidental deletions and enforce code review workflows. However, repository administrators sometimes need to bypass these protections for maintenance tasks like cleaning up stale branches.

### Enabling Admin Bypass for Branch Deletion

To allow repository admins to bypass branch protection rules when deleting branches:

#### Step 1: Access Branch Protection Settings
1. Navigate to your repository on GitHub
2. Click **Settings** in the top menu bar
3. Select **Branches** from the left sidebar
4. Under "Branch protection rules", find the rule for your protected branch (e.g., `main`)
5. Click **Edit** on the existing rule, or click **Add rule** to create a new one

#### Step 2: Configure Branch Protection Rule
1. In the "Branch name pattern" field, enter the branch name or pattern (e.g., `main`, `develop`, or `release/*`)
2. Enable the following options as needed for your workflow:
   - ☑ **Require a pull request before merging**
   - ☑ **Require status checks to pass before merging**
   - ☑ **Require conversation resolution before merging**
   - ☑ **Require signed commits**
   - ☑ **Require linear history**

#### Step 3: Enable Admin Bypass (Critical Step)
At the bottom of the branch protection settings, you'll find:

**"Do not allow bypassing the above settings"**

To allow admins to delete protected branches:
- ☐ **UNCHECK** this option (leave it unchecked)

OR, if this option doesn't exist in your GitHub plan:

**"Allow force pushes"** and **"Allow deletions"** sections:
- Under **Allow force pushes**, select one of:
  - **Everyone** (not recommended for production branches)
  - **Specify who can force push** → Add admin team or specific users
- Under **Allow deletions**, check:
  - ☑ **Allow deletions** (this permits deletion of the protected branch)

#### Step 4: Save Changes
1. Scroll to the bottom of the page
2. Click **Save changes** or **Create** (for new rules)

### Important Notes

**GitHub Plans and Features:**
- **Free/Team plans**: The "Allow deletions" checkbox is available
- **Enterprise plans**: Additional "Do not allow bypassing the above settings" option provides more granular control

**Best Practices:**
- Only grant admin bypass to trusted repository administrators
- Consider using CODEOWNERS file for automatic reviewer assignment (already configured in `.github/CODEOWNERS`)
- Document why branches are being deleted (e.g., in a tracking issue)
- For temporary feature branches, consider using automated cleanup workflows instead

**Security Considerations:**
- Admins with bypass permission can delete branches without pull requests
- Ensure your team has proper backup/recovery procedures
- Consider enabling branch protection rule insights to audit bypass actions

## Deleting Protected Branches

Once admin bypass is configured, administrators can delete protected branches using:

### Via GitHub Web Interface
1. Navigate to the repository
2. Click **Branches** (below the repository name)
3. Find the branch you want to delete
4. Click the trash can icon next to the branch name
5. Confirm the deletion

### Via Command Line
```bash
# Delete a local branch
git branch -d branch-name

# Force delete a local branch
git branch -D branch-name

# Delete a remote branch (requires admin permissions with bypass)
git push origin --delete branch-name
```

### Via GitHub CLI
```bash
# Delete a branch using gh CLI
gh api repos/{owner}/{repo}/git/refs/heads/{branch} -X DELETE
```

**Note:** Even with bypass permissions, you should generally:
1. Ensure the branch is merged or no longer needed
2. Notify team members before deletion
3. Verify no open pull requests depend on the branch

## Repository Settings Overview

### Current Configuration
This repository includes:
- **Branch protection**: Configured for `main` branch
- **CODEOWNERS**: Defined in `.github/CODEOWNERS` for automatic reviewer assignment
- **GitHub Actions**: CI/CD workflows in `.github/workflows/`
- **Required status checks**: Unity CI builds must pass (see `.github/workflows/unity-ci.yml`)

### Recommended Settings for Admins
For optimal repository management:

1. **Branch Protection Rules** (Settings → Branches)
   - Protect `main` branch
   - Require pull request reviews
   - Allow admin bypass for maintenance

2. **Repository Access** (Settings → Collaborators)
   - Assign appropriate roles (Read, Write, Admin)
   - Use teams for group permissions

3. **Webhooks & Services** (Settings → Webhooks)
   - Configure CI/CD integrations
   - Set up deployment notifications

4. **Actions Permissions** (Settings → Actions)
   - Control workflow execution permissions
   - Manage secrets securely

## Troubleshooting

### "Cannot delete protected branch" Error
**Problem:** You receive an error when trying to delete a protected branch, even as an admin.

**Solutions:**
1. Verify you have **Admin** role on the repository (Settings → Collaborators)
2. Check branch protection settings:
   - Ensure "Allow deletions" is checked, OR
   - Ensure "Do not allow bypassing the above settings" is unchecked
3. If using GitHub CLI or API, ensure your authentication token has `repo` scope
4. Try refreshing your browser and clearing cache (web interface only)

### Changes Not Taking Effect
**Problem:** You modified branch protection settings but can't delete branches.

**Solutions:**
1. Log out and log back in to GitHub
2. Clear browser cache and cookies
3. Wait a few minutes for settings to propagate
4. Verify the rule applies to the specific branch you're trying to delete

### Permission Denied Despite Admin Role
**Problem:** You're an admin but still get "permission denied" errors.

**Solutions:**
1. Check if your organization has additional policies:
   - Organization settings might override repository settings
   - Contact organization owners if needed
2. Verify your admin access wasn't recently changed
3. Check if 2FA/SSO is required and properly configured
4. For enterprise accounts, verify enterprise policies don't restrict admin actions

### Accidentally Deleted Important Branch
**Problem:** You deleted a branch that was needed.

**Solutions:**
1. Check the branch's last commit SHA in:
   - GitHub notifications
   - Pull request references
   - CI/CD logs
2. Restore the branch:
   ```bash
   # If you know the commit SHA
   git checkout -b branch-name <commit-sha>
   git push origin branch-name
   ```
3. Contact GitHub support for assistance (enterprise plans)
4. Check if your organization has backup policies

## Additional Resources

- [GitHub Docs: Managing Branch Protection Rules](https://docs.github.com/en/repositories/configuring-branches-and-merges-in-your-repository/managing-protected-branches/managing-a-branch-protection-rule)
- [GitHub Docs: About Protected Branches](https://docs.github.com/en/repositories/configuring-branches-and-merges-in-your-repository/managing-protected-branches/about-protected-branches)
- [GitHub Docs: Repository Roles](https://docs.github.com/en/organizations/managing-user-access-to-your-organizations-repositories/repository-roles-for-an-organization)

## Related Documentation
- [CONTRIBUTING.md](./CONTRIBUTING.md) - Contribution guidelines and Git workflow
- [.github/workflows/README.md](../.github/workflows/README.md) - CI/CD workflow documentation
- [README.md](../README.md) - Repository overview and setup

---

For questions or issues not covered in this guide, please:
1. Check existing GitHub issues for similar problems
2. Open a new issue with the `documentation` or `question` label
3. Contact the repository maintainers directly
