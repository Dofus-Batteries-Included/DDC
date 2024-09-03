param (
    [string] $Directory = "./",
    [string] $Output = "maps.json"
)

$Directory = [IO.Path]::GetFullPath($Directory);
$Output = [IO.Path]::GetFullPath($Output);

echo "Reading map files in $Directory...";

$Files = Get-ChildItem -Path "$Directory/map_*.json" -Recurse
$Result = @{}

$IdRegex = "^map_(?<id>.*)\.json$"
foreach ($File in $Files) {
    
    if ($File.Name -notmatch $IdRegex) {
        continue;
    }
    
    $Result[$Matches["id"]] = Get-Content -Raw $File.FullName | ConvertFrom-Json
}

echo "Found a total of $($Result.Count) maps.";
echo "Writing result in $Output...";

echo ($Result | ConvertTo-Json -Compress) > $Output