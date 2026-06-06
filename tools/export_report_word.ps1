param(
    [Parameter(Mandatory = $true)][string]$InputPath,
    [Parameter(Mandatory = $true)][string]$OutputPath
)

$ErrorActionPreference = "Stop"
$wdAlignParagraphLeft = 0
$wdAlignParagraphCenter = 1
$wdAlignParagraphJustify = 3
$wdPageBreak = 7
$wdFormatDocumentDefault = 16
$wdFieldPage = 33
$wdCollapseEnd = 0
$wdBorderBottom = -3
$wdLineStyleSingle = 1

function Clean-Markdown([string]$text) {
    $value = $text.Trim()
    $value = $value -replace '\*\*(.*?)\*\*', '$1'
    $value = $value -replace '`([^`]*)`', '$1'
    return $value
}

function Add-Paragraph($document, [string]$text, [string]$style = "Normal", [int]$alignment = $wdAlignParagraphJustify) {
    $paragraph = $document.Paragraphs.Add()
    $paragraph.Range.Text = (Clean-Markdown $text)
    try { $paragraph.Range.Style = $style } catch { }
    $paragraph.Alignment = $alignment
    $paragraph.Range.InsertParagraphAfter()
    return $paragraph
}

$inputFull = [System.IO.Path]::GetFullPath($InputPath)
$outputFull = [System.IO.Path]::GetFullPath($OutputPath)
$lines = [System.IO.File]::ReadAllLines($inputFull, [System.Text.Encoding]::UTF8)

$word = New-Object -ComObject Word.Application
$word.Visible = $false
$word.DisplayAlerts = 0
$document = $word.Documents.Add()

try {
    $section = $document.Sections.Item(1)
    $section.PageSetup.TopMargin = $word.CentimetersToPoints(2.5)
    $section.PageSetup.BottomMargin = $word.CentimetersToPoints(2.5)
    $section.PageSetup.LeftMargin = $word.CentimetersToPoints(3.0)
    $section.PageSetup.RightMargin = $word.CentimetersToPoints(2.0)

    $normal = $document.Styles.Item("Normal")
    $normal.Font.Name = "Times New Roman"
    $normal.Font.Size = 13
    $normal.ParagraphFormat.Alignment = $wdAlignParagraphJustify
    $normal.ParagraphFormat.LineSpacingRule = 1
    $normal.ParagraphFormat.LineSpacing = 18
    $normal.ParagraphFormat.SpaceAfter = 6

    foreach ($styleName in @("Title", "Heading 1", "Heading 2", "Heading 3")) {
        $style = $document.Styles.Item($styleName)
        $style.Font.Name = "Times New Roman"
        $style.Font.Bold = $true
    }
    $document.Styles.Item("Title").Font.Size = 20
    $document.Styles.Item("Heading 1").Font.Size = 16
    $document.Styles.Item("Heading 2").Font.Size = 14
    $document.Styles.Item("Heading 3").Font.Size = 13

    $isCover = $true
    $inCode = $false
    $codeLines = New-Object System.Collections.Generic.List[string]
    $i = 0
    while ($i -lt $lines.Length) {
        $line = $lines[$i]

        if ($line.Trim().StartsWith('```')) {
            if (-not $inCode) {
                $inCode = $true
                $codeLines.Clear()
            } else {
                $paragraph = Add-Paragraph $document ($codeLines -join "`r") "Normal" $wdAlignParagraphLeft
                $paragraph.Range.Font.Name = "Consolas"
                $paragraph.Range.Font.Size = 9
                $paragraph.Range.Shading.BackgroundPatternColor = 15132390
                $inCode = $false
            }
            $i++
            continue
        }

        if ($inCode) {
            $codeLines.Add($line)
            $i++
            continue
        }

        if ($line.Trim() -eq "---") {
            $document.Range($document.Content.End - 1, $document.Content.End - 1).InsertBreak($wdPageBreak)
            $isCover = $false
            $i++
            continue
        }

        if ($line.TrimStart().StartsWith("|")) {
            $tableLines = New-Object System.Collections.Generic.List[string]
            while ($i -lt $lines.Length -and $lines[$i].TrimStart().StartsWith("|")) {
                $tableLines.Add($lines[$i])
                $i++
            }
            $dataLines = @($tableLines | Where-Object { $_ -notmatch '^\s*\|[\s:|-]+\|\s*$' })
            if ($dataLines.Count -gt 0) {
                $rows = New-Object System.Collections.Generic.List[object]
                foreach ($tableLine in $dataLines) {
                    $cells = @($tableLine.Trim().Trim("|").Split("|") | ForEach-Object { Clean-Markdown $_ })
                    $rows.Add($cells)
                }
                $columnCount = ($rows | ForEach-Object { $_.Count } | Measure-Object -Maximum).Maximum
                $range = $document.Range($document.Content.End - 1, $document.Content.End - 1)
                $table = $document.Tables.Add($range, $rows.Count, $columnCount)
                $table.Borders.Enable = 1
                $table.Range.Font.Name = "Times New Roman"
                $table.Range.Font.Size = 11
                for ($r = 1; $r -le $rows.Count; $r++) {
                    for ($c = 1; $c -le $columnCount; $c++) {
                        $value = if ($c -le $rows[$r - 1].Count) { $rows[$r - 1][$c - 1] } else { "" }
                        $table.Cell($r, $c).Range.Text = $value
                    }
                }
                $table.Rows.Item(1).Range.Bold = 1
                $table.Rows.Item(1).Shading.BackgroundPatternColor = 14277081
                $table.AutoFitBehavior(1)
                $document.Range($document.Content.End - 1, $document.Content.End - 1).InsertParagraphAfter()
            }
            continue
        }

        if ([string]::IsNullOrWhiteSpace($line)) {
            $document.Paragraphs.Add().Range.InsertParagraphAfter()
            $i++
            continue
        }

        if ($line -match '^###\s+(.+)$') {
            Add-Paragraph $document $Matches[1] "Heading 3" $(if ($isCover) { $wdAlignParagraphCenter } else { $wdAlignParagraphLeft }) | Out-Null
        } elseif ($line -match '^##\s+(.+)$') {
            Add-Paragraph $document $Matches[1] $(if ($isCover) { "Title" } else { "Heading 2" }) $(if ($isCover) { $wdAlignParagraphCenter } else { $wdAlignParagraphLeft }) | Out-Null
        } elseif ($line -match '^#\s+(.+)$') {
            Add-Paragraph $document $Matches[1] $(if ($isCover) { "Title" } else { "Heading 1" }) $(if ($isCover) { $wdAlignParagraphCenter } else { $wdAlignParagraphLeft }) | Out-Null
        } elseif ($line -match '^\-\s+(.+)$') {
            Add-Paragraph $document $Matches[1] "List Bullet" $wdAlignParagraphLeft | Out-Null
        } elseif ($line -match '^\d+\.\s+(.+)$') {
            Add-Paragraph $document $Matches[1] "List Number" $wdAlignParagraphLeft | Out-Null
        } else {
            $paragraph = Add-Paragraph $document $line "Normal" $(if ($isCover) { $wdAlignParagraphCenter } else { $wdAlignParagraphJustify })
            if ($isCover) { $paragraph.Range.Font.Bold = $true }
        }
        $i++
    }

    foreach ($currentSection in $document.Sections) {
        $footer = $currentSection.Footers.Item(1)
        $footer.Range.ParagraphFormat.Alignment = $wdAlignParagraphCenter
        $footer.Range.Fields.Add($footer.Range, $wdFieldPage) | Out-Null
        $footer.Range.Font.Name = "Times New Roman"
        $footer.Range.Font.Size = 11
    }

    $document.SaveAs2($outputFull, $wdFormatDocumentDefault)
} finally {
    if ($document) { $document.Close($false) }
    if ($word) { $word.Quit() }
    if ($document) { [System.Runtime.InteropServices.Marshal]::ReleaseComObject($document) | Out-Null }
    if ($word) { [System.Runtime.InteropServices.Marshal]::ReleaseComObject($word) | Out-Null }
    [GC]::Collect()
    [GC]::WaitForPendingFinalizers()
}

Write-Output $outputFull
