# CrossStitch

Convert an image into a cross stitch pattern.

## Resizing

The first step is resizing an image to fit the desired aida cloth traits, typically measured in inches. The traits include heigh, width, and "points per inch," with 14 being standard.

To resize:
* Determine how many pixels can fit within the desired aida cloth dimensions
* Resize image to the assessed dimensions

## Color Reduction (RGB)

Naturally cross stitch becomes easier the fewer colors there are.

To reduce colors:
* Group all colors in the image according to how close they are in the 3D vector space defined by their R, G, B values
* Compute the central value, or centroid, of each color group
* Return the set of centroids

## Color Reduction (RGBA)

Handle the alpha channel by computing the reduced color set on just the RGB values, maintaining each pixel's original transparency value
