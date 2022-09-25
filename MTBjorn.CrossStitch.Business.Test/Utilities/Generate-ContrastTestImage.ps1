#
# Generate an image for testing contrast preservation from color reductoin algorithms
# Attempt to reduce an image with 98 shades of red, with 1 shade each of blue & green to 3 colors
#

function Get-ColorDistance {
	param(
		[Parameter(Mandatory=$true)][PSCustomObject]$FirstPixel,
		[Parameter(Mandatory=$true)][PSCustomObject]$SecondPixel
	)

	$redPortion = [System.Math]::Pow(($SecondPixel.r - $FirstPixel.r), 2)
	$greenPortion = [System.Math]::Pow(($SecondPixel.g - $FirstPixel.g), 2)
	$bluePortion = [System.Math]::Pow(($SecondPixel.b - $FirstPixel.b), 2)
	$sum = $redPortion + $greenPortion + $bluePortion

	return [System.Math]::Sqrt($sum)
}

function Get-RectangleDrawCommand {
	param(
		[Parameter(Mandatory=$true)][PSCustomObject]$color,
		[Parameter(Mandatory=$true)][int]$XCoordinate,
		[Parameter(Mandatory=$true)][int]$YCoordinate,
		[Parameter(Mandatory=$true)][int]$PlotHeight,
		[Parameter(Mandatory=$true)][int]$PlotWidth
	)

	$colorString = "rgb($($color.r),$($color.g),$($color.b))"
	$bottomCornerXCoordinate = $XCoordinate + $PlotWidth
	$bottomCornerYCoordinate = $YCoordinate + $PlotHeight
	$rectangle = "rectangle $XCoordinate,$YCoordinate $bottomCornerXCoordinate,$bottomCornerYCoordinate"
	$cmd = "-fill '$colorString' -draw '$rectangle'"

	return $cmd
}

$blueShade = New-Object PSCustomObject -Property @{ r=50; g=0; b=150; }
$greenShade = New-Object PSCustomObject -Property @{ r=50; g=150; b=0; }
$redBlueCutoff = New-Object PSCustomObject -Property @{ r=150; g=0; b=75; } # go more to blue 41
$redGreenCutoff = New-Object PSCustomObject -Property @{ r=150; g=75; b=0; } # go more to green by 41
$redPure = New-Object PSCustomObject -Property @{ r=255; g=0; b=0; } # go more blue by 7 and green by 8

$blueGroup = New-Object System.Collections.Generic.List[PSCustomObject];
$greenGroup = New-Object System.Collections.Generic.List[PSCustomObject];
$redGroup = New-Object System.Collections.Generic.List[PSCustomObject]; $blueGroup.Add($redPure)

for ($i = 0; $i -lt 41; $i++) {
	$blueGroup.Add($( New-Object PSCustomObject -Property @{ r=$redBlueCutoff.r; g=$redBlueCutoff.g; b=($redBlueCutoff.b + $i); } ))
	$greenGroup.Add($( New-Object PSCustomObject -Property @{ r=$redGreenCutoff.r; g=($redGreenCutoff.g + $i); b=$redGreenCutoff.b; } ))

	if ($i -lt 7) {
		$redGroup.Add($( New-Object PSCustomObject -Property @{ r=$redPure.r; g=($redPure.g + ($i * 8)); b=$redPure.b; } ))
		$redGroup.Add($( New-Object PSCustomObject -Property @{ r=$redPure.r; g=$redPure.g; b=($redPure.b + ($i * 8)); } ))
	}
}
$redGroup.Add($( New-Object PSCustomObject -Property @{ r=$redPure.r; g=($redPure.g + (8 * 8)); b=$redPure.b; } ))
$redGroup.AddRange($blueGroup)
$redGroup.AddRange($greenGroup)

# Ensure groupings are correct
if ($redGroup.Count -ne 98) {
	throw "Expected 98 shades of red, generated $($redGroup.Count)"
}

# $redGroupSpreadTowardsRed = Get-ColorDistance $redPure $redGroup[$redGroup.Count - 1]
# $redGroupSpreadTowardsBlue = 0
# $redGroupSpreadTowardsGreen = 0

# foreach ($redShade in $redGroup) {
# 	foreach ($shade in $blueGroup) {
# 		$distance = Get-ColorDistance $shade $redShade
# 		if ($distance -gt $redGroupSpreadTowardsBlue) {
# 			$redGroupSpreadTowardsBlue = $distance
# 		}
# 	}
# 	foreach ($shade in $greenGroup) {
# 		$distance = Get-ColorDistance $shade $redShade
# 		if ($distance -gt $redGroupSpreadTowardsGreen) {
# 			$redGroupSpreadTowardsGreen = $distance
# 		}
# 	}
# }

# Draw image as grid of 10x10 plots for each color -- use 2 columns of red shades, and one each for blue & green
# NOTE: to draw a rectangle, specify the top-left and bottom-right coordinates as (x,y) where y is from the top
$outputFile = "..\Resources\contrast-test-image.png"
$baseCommand = "& magick.exe convert -size 40x245 xc:white"
$drawCommands = New-Object System.Collections.Generic.List[string]

for ($i = 0; $i -lt 49; $i++) {
	$yCoord = $i * 10
	$leftColor = $redGroup[$i]
	$rightColor = $redGroup[$i + 49]

	$drawCommands.Add($(Get-RectangleDrawCommand $leftColor 0 $yCoord 10 10))
	$drawCommands.Add($(Get-RectangleDrawCommand $rightColor 10 $yCoord 10 10))
}
$drawCommands.Add($(Get-RectangleDrawCommand $greenShade 20 0 245 10))
$drawCommands.Add($(Get-RectangleDrawCommand $blueShade 30 0 245 10))

$drawCommandsString = [System.String]::Join(" ", $drawCommands)
$command = "$baseCommand $drawCommandsString $outputFile"

Invoke-Expression $command
