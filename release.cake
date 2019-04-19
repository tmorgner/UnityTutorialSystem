#reference "tools/AutoCake.Release/tools/AutoCake.Release.dll"
#load      "tools/AutoCake.Release/content/release-tasks.cake"
#load      "tools/AutoCake.Release/content/git-tasks.cake"

//////////////////////////////////////////////////////////////////////
// Configuration
//////////////////////////////////////////////////////////////////////

GitFlow.RunBuildTarget = () => 
{
  // See release-scripts/README.md for additional configuration options
  // and details on the syntax of this call.
  //
  // I prefer to add the version information to the build-output directory.
  // This way debugging gets a lot easier at to cost of some minimally larger
  // disk usage. Disks are cheap, developer hours are not.
  var versionInfo = GitVersioningAliases.FetchVersion();
  CakeRunnerAlias.RunCake(Context, new CakeSettings {
        Arguments = new Dictionary<string, string>()
        {
            { "targetdir", "build-artefacts/" + versionInfo.FullSemVer }
        }
  });
};


//////////////////////////////////////////////////////////////////////
// Standard boiler-plate code.
//////////////////////////////////////////////////////////////////////

Task("Default")
    .IsDependentOn("Attempt-Release");

var target = Argument("target", "Default");
RunTarget(target);
