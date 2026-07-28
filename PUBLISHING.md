# Update the documentation

Simply push on the `docs` branch.

# Publishing to NuGet.org

The three packages — `Joufflu`, `Joufflu.Inputs`, `Joufflu.Navigation` — are published
by GitHub Actions using **Trusted Publishing** (OIDC). No API key is stored; each run
exchanges a short-lived GitHub token for a temporary nuget.org key valid for 1 hour.

One workflow per package, triggered by a tag:

| Package             | Workflow file                     | Tag pattern     |
| ------------------- | --------------------------------- | --------------- |
| `Joufflu`           | `publish-joufflu.yml`             | `v*`            |
| `Joufflu.Inputs`    | `publish-joufflu-inputs.yml`      | `inputs-v*`     |
| `Joufflu.Navigation`| `publish-joufflu-navigation.yml`  | `navigation-v*` |

## One-time setup

### On nuget.org

For **each** of the three packages, create a Trusted Publishing policy
(username → **Trusted Publishing** → add policy):

- **Repository owner:** `ndegheselle`
- **Repository:** `Joufflu`
- **Workflow file:** the matching file name only (e.g. `publish-joufflu.yml`) — no path
- **Environment:** leave empty

### On GitHub

Add one repository secret (Settings → Secrets and variables → Actions):

- **`NUGET_USER`** — your nuget.org **profile name** (not your email)

Nothing else is needed: `permissions: id-token: write` is already set in each workflow.

## Releasing a new version

Versions live in each project's `.csproj` (`<Version>`), and the tag drives the
published package version (passed via `-p:Version`). Keep them in sync.

1. **Bump the version** in Visual Studio — edit `<Version>` in the target
   `.csproj` (e.g. `Joufflu/Joufflu.csproj`).
2. **Commit and push** to `main`. The commit must be on the remote before tagging.
3. **Create the tag** matching the package, then push it:

   ```bash
   git tag v0.1.2                 # Joufflu
   git push origin v0.1.2
   ```

   | Package              | Example tag        |
   | -------------------- | ------------------ |
   | `Joufflu`            | `v0.1.2`           |
   | `Joufflu.Inputs`     | `inputs-v0.1.2`    |
   | `Joufflu.Navigation` | `navigation-v0.1.2`|

   From Visual Studio: Git → Manage Branches → right-click the commit →
   **New Tag…** → then expand **Tags** → right-click → **Push tag to remote**.
   A normal push does **not** send tags — pushing the tag is what triggers the workflow.
4. **Watch the Actions tab.** The package appears on nuget.org within a few minutes
   (search indexing can take longer).

## Notes

- **Release order for the dependents.** `Joufflu.Inputs` and `Joufflu.Navigation`
  depend on `Joufflu` at the `<Version>` in `Joufflu/Joufflu.csproj` at build time
  (not their own tag). When bumping the whole family, bump and release `Joufflu`
  first so that version exists on nuget.org.
- **Versions are immutable.** A published `0.1.2` can't be replaced — unlist it and
  release `0.1.3`.
- **Re-runs are safe.** `--skip-duplicate` means re-publishing an existing version
  won't fail the workflow.
- **Manual run.** Each workflow also has a `workflow_dispatch` button (Actions tab);
  triggered that way it uses the `<Version>` from the `.csproj` instead of a tag.
- **Per-package README.** Each package ships its own `README.md`, located next to
  its `.csproj` (`Joufflu/README.md`, `Joufflu.Inputs/README.md`,
  `Joufflu.Navigation/README.md`) and packed via `<PackageReadmeFile>`. This is what
  shows on the package's nuget.org page — edit the one next to the project, not the
  repo-root `readme.md` (which is the GitHub landing page). Use absolute `https://`
  URLs for images and links, since relative paths don't resolve on nuget.org.