param($installPath, $toolsPath, $package, $project)

$frameworkVersionString = $project.Properties.Item('TargetFrameworkMoniker').Value
$frameworkVersionString -match 'Version=(v\d\.\d)'
$frameworkVersion = $matches[1]
$project.Save();

if ($frameworkVersion -ge 'v4.0')
{
    $project.Object.References | Where-Object { $_.EmbedInteropTypes -eq $true -and $_.Name -eq "Interop.UIAutomationClient" } | ForEach-Object { $_.EmbedInteropTypes = $false }
}

$project.Save()