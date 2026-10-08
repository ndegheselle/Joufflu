# Regenerates docs/llms-full.txt: every page linked from docs/llms.txt, front matter removed,
# concatenated in the order of the index. Run after any change to a docs page or to llms.txt:
#   pwsh docs/build-llms-full.ps1
$docs = $PSScriptRoot
$prefix = 'https://raw.githubusercontent.com/ndegheselle/Joufflu/main/docs/'
$utf8 = New-Object System.Text.UTF8Encoding($false)

$index = [System.IO.File]::ReadAllText((Join-Path $docs 'llms.txt'), $utf8)
$out = New-Object System.Text.StringBuilder
[void]$out.AppendLine('# Joufflu WPF Components - full documentation')
[void]$out.AppendLine('')
[void]$out.AppendLine('> Every documentation page of Joufflu concatenated in one file. Index with one-line summaries: ' + $prefix + 'llms.txt')

$seen = @{}
foreach ($m in [regex]::Matches($index, '\]\(' + [regex]::Escape($prefix) + '([^)\s]+\.md)\)')) {
    $rel = $m.Groups[1].Value
    if ($seen.ContainsKey($rel)) { continue }
    $seen[$rel] = $true
    $text = [System.IO.File]::ReadAllText((Join-Path $docs $rel), $utf8).TrimStart([char]0xFEFF)
    $text = $text -replace "`r`n", "`n"
    $text = [regex]::Replace($text, '^---\n.*?\n---\n', '', 'Singleline')
    [void]$out.AppendLine('')
    [void]$out.AppendLine('---')
    [void]$out.AppendLine('')
    [void]$out.AppendLine('<!-- source: ' + $prefix + $rel + ' -->')
    [void]$out.AppendLine('')
    [void]$out.AppendLine($text.Trim())
}

[System.IO.File]::WriteAllText((Join-Path $docs 'llms-full.txt'), ($out.ToString() -replace "`r`n", "`n"), $utf8)
Write-Host ("llms-full.txt: {0} pages" -f $seen.Count)
