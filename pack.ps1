param (
    [string]$Configuration = "Release",
    [string]$Output = "dist",
    [switch]$Help
)

if ($Help)
{
    echo "Usage: pack.ps1 [-configuration CONFIGURATION] [-output OUTPUT FOLDER]"
    exit 0
}

$Projects = "DDC.Extractor";

echo "> Packing projects: $Projects"
echo "> Configuration: $Configuration"
echo "> Output path: $Output"

if (Test-Path -Path $Output)
{
    echo "Cleaning output folder $Output..."
    rm $Output -r -force
}

echo "Creating output folder $Output..."
$null = md $Output

$InteropFolder = "Interop";
$InteropDlls = Get-ChildItem "$InteropFolder/*.dll" | % { Split-Path $_ -leaf } | % { $_.ToLowerInvariant() }
$Exceptions = @("newtonsoft.json.dll")

echo "Found $( $InteropDlls.Length ) interop DLLs."

foreach ($Project in $Projects)
{
    echo "Packing project $Project..."

    $OtherProjects = $Projects | Where-Object { $_ -ne $Project }

    $Dir = Join-Path $Output $Project
    $null = MkDir $Dir -Force
    foreach ($File in Get-ChildItem "$Project/bin/$Configuration/net6.0/publish/*.dll")
    {
        $Filename = Split-Path $File -leaf
        $FilenameLower = $Filename.ToLowerInvariant()
        
        if (-not $Exceptions.Contains($FilenameLower) -and $InteropDlls.Contains($FilenameLower))
        {
            continue;
        }

        echo "Copying $File to $Dir..."
        copy $File $Dir
    }

    $ResourcesFolder = "$Project/bin/$Configuration/net6.0/publish/Resources"
    if (Test-Path $ResourcesFolder) {
        copy "$ResourcesFolder" $Dir -Recurse
    }

    $RuntimesFolder = "$Project/bin/$Configuration/net6.0/publish/runtimes/win-x64"
    if (Test-Path $RuntimesFolder) {
        copy "$RuntimesFolder/**/*.dll" $Dir -Recurse
    }
}
