# DeployableApp
Repository to experiment with CI/CD automatic deployment of new versions.

# Introduction
In a search for an effortless way to build and deploy an application that I intend to write, I ran accross [Squirrel](https://github.com/Squirrel/Squirrel.Windows).
However, I could not find any complete example. Therefore I decided to create this repository to do so myself.

# Try the result
Curious to see the result?

Go to the [Releases](https://github.com/leonvandermeer/DeployableApp/releases) page of this repo. Deliberately select an older version (e.g. [1.0.40-beta](https://github.com/leonvandermeer/DeployableApp/releases/tag/1.0.40-beta)). Download and run the **DeployableAppSetup.exe** installer. Observe that the application is installed, started and directly updated to the latest version.

# Goals
* Evaluate usage of Squirrel to assist in automatic updates of a Windows client application.
* Evaluate usage of [GitHub Actions](https://docs.github.com/en/actions) to automatically build, test and release software.
* Learn more about CI/CD and DevOps.
* Ensure that end users use the latest available version of my application whithout barely noticing updates.

# Scope and Requirements
* C#, .NET (Core), Windows, Wpf

# Design

## Building Installer with Squirrel
I want to publish my application as self-contained and to do that I can simply run `dotnet publish --sc`. Squirrel however, needs a NuGet package. Using [Microsoft.Build.NoTargets](https://github.com/microsoft/MSBuildSdks/blob/main/src/NoTargets/README.md) SDK, I created [DeployableApp.Publish](DeployableApp.Publish/DeployableApp.Publish.csproj) to do the work. At publish, it:
1. Publishes the application (via a project reference).
2. Collects all files from the main application's publish directory and marks those for packaging.
3. Creates a NuGet package (enabling `Generate NuGet package on uuild`).
4. In a custom build target calls Squirrel's releasify.

This also enables development testing as any developer can create an installer this way.

## Deploy with GitHub
I Created a GitHub workflow that:
1. Publishes the solution: `dotnet publish -c Release --self-contained`.
2. Uses [ncipollo/release-action](https://github.com/ncipollo/release-action) action to create a new release (on main branch only).

Simple and effective. GitHub's CI/CD do all the work on standard GitHub GitHub-hosted runners (Windows Server 2022[](https://github.com/actions/runner-images#available-images)).

# Conclusion
* Without good versioning process, software updates are impossible. [Nerdbank.GitVersioning](https://github.com/dotnet/Nerdbank.GitVersioning) assigns a unique version to any commit. It took only 10 minutes to integrate in this repository. It also integrates well with Squirrel.
* With Self Signed Certificates, it is very difficult to provide end users a simple download and installation experience.
* It is very Sad that Squirrel has no recent releases. See [issue 1470](https://github.com/Squirrel/Squirrel.Windows/issues/1470)). Thus I must conclude it does not support .NET (Core). Note: I did not evaluate [Clowd.Squirrel](https://github.com/clowd/Clowd.Squirrel).
* Keeping private key needed for code signing a secret is possible (but not easy - see below).

# Findings

<details><summary>Squirrel</summary>
  
* No .NET (Core) support
** Squirrel always installs .net framework runtime, this is not needed for .net core.
** Shortcut created for createdump.exe - an executable as part of .net runtime, as part of self contained installation
** I only got code compiling / working against a development version of Squirrel's nuget package (taken from https://github.com/Squirrel/Squirrel.Windows/tree/5e44cb4001a7d48f53ee524a2d90b3f5700a9920).
  
</details>

<details><summary>Code Signing and Certificates</summary>

## Code Signing

To bootstrap installation and updates, new end users must download an installer executable. This executable must be code signed:

* Today's browsers prevent download of any executable of which publisher identity and code integrity cannot be verified.
* Today's virus scanners prevent execution of any executable of which publisher identity and code integrity cannot be verified.

Without any Code signing, browsers refuse to download. And in case download succeeds (e.g. with curl), virus scanners directly quarantine the downloaded file. Using a Self Signed Certificate, download is possible. End users however must acknowledge several warnings when downloading / executing the installer. 

## Keep private keys secret
Using a code signing certificate (with private key) in a file does not work. The password is passed unencrypted to signtool's argument and thus appears in both squirrel and msbuild log files. GitHub's [Secret Redaction](https://docs.github.com/en/actions/security-guides/using-secrets-in-github-actions#redacting-secrets-from-workflow-run-logs) mechanisms are not enough.

Final solution is to use two GitHub secrets: one that contains the Certificate file and another that contains the password to access the private key. As a [build step](.github/workflows/dotnet.yml#L25-L32), the Certificate is imported in Windows. All without secret exposure. During the build, signtool uses the private key for signing in a secure way. See also [Using Secrets in GitHub](https://docs.github.com/en/actions/security-guides/using-secrets-in-github-actions#storing-base64-binary-blobs-as-secrets).

</details>
