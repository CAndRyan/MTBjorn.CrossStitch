# Generate test images (using ImageMagick)

## Examples

* A solid red 10x10 png

````powershell
& magick.exe convert -size 10x10 xc:"rgb(255,0,0)" solid-red-10x10.png
````

* A half red half blue (horizontal split) 10x10 png

````powershell
& magick.exe -size 5x10 xc:"rgb(255,0,0)" xc:"rgb(0,0,255)" +append half-red-blue-10x10.png
````

* A quarter red, orange, blue, & green (horizontal split) 10x20 png

````powershell
& magick.exe -size 5x10x15x20 xc:"rgb(255,0,0)" xc:"rgb(255,128,0)" xc:"rgb(0,0,255)" xc:"rgb(0,255,0)" +append quarter-red-orange-blue-green-20x10.png
````
