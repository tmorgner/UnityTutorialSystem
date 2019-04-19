#reference "tools/Cake.Unity/lib/net46/Cake.Unity.dll"
#reference "tools/AutoCake.Build/tools/AutoCake.Build.dll"
#reference "tools/AutoCake.Unity/tools/AutoCake.Unity.dll"
#load      "tools/AutoCake.Build/content/dependencies.cake"
#load      "tools/AutoCake.Unity/tools/tasks.cake"

CreateDirectory("build-artefacts/");

var buildType = Argument("buildType", "development");
var targetDir = Argument("targetdir", "build-artefacts/current-build");

UnityBuildActions.Arguments.BuildTargetExecutable = "VirginTrainsKitchenSimulator";
UnityBuildActions.Arguments.BuildTarget = UnityBuildTargetType.Windows64Player;
UnityBuildActions.Arguments.BuildTargetPath = DirectoryPath.FromString(targetDir).Combine("bin");
UnityBuildActions.Arguments.LogFile = DirectoryPath.FromString(targetDir).CombineWithFilePath("unity-log.txt");

UnityBuildActions.PackageFilePath = DirectoryPath.FromString(targetDir).Combine("zip").CombineWithFilePath("VirginTrainsKitchenSimulator.zip");


CreateDirectory(targetDir);

//////////////////////////////////////////////////////////////////////
// TASK TARGETS
//////////////////////////////////////////////////////////////////////



Task("Default")
    .IsDependentOn("Build")
   .Does(() => {

   });

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

var target = Argument("target", "Default");
RunTarget(target);
