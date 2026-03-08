$ErrorActionPreference = "Stop"
$dir = "e:\game\legendofblood\legacyofblood_unity_mobile_portraitframe\Assets\Tests"
$files = Get-ChildItem -Path $dir -Recurse -Filter *.cs

# Pattern
# Matches:
# public void SetUp()
#    var avatarManagerObj = ...
#    ...
# {
$pattern = "(?s)(public\s+void\s+(Set[uU]p)\(\)\s+)var\s+avatarManagerObj\s*=\s*new\s+(UnityEngine\.)?GameObject\(`"TestAvatarManager`"\);\s+avatarManagerObj\.AddComponent<LegendOfBlood\.AvatarManager>\(\);\s*\{"

$count = 0

foreach ($file in $files) {
    if ($file.Length -gt 0) {
        $content = [System.IO.File]::ReadAllText($file.FullName)
        if ($content -match $pattern) {
            # Move the block inside the curly brace
            $replacement = "public void `$2()`r`n        {`r`n            var avatarManagerObj = new UnityEngine.GameObject(`"TestAvatarManager`");`r`n            avatarManagerObj.AddComponent<LegendOfBlood.AvatarManager>();"
            $newContent = [regex]::Replace($content, $pattern, $replacement)
            [System.IO.File]::WriteAllText($file.FullName, $newContent)
            Write-Host "Fixed: $($file.Name)"
            $count++
        }
    }
}

Write-Host "Total files fixed: $count"
