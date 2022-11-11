const visualizeDiagnostics = (diagnostics) => {
    const centroidGroupData = diagnostics.Centroids.map((group, index) => {
        var finalCentroid = group[group.length - 1];
        return {
            x: group.map(p => p.R),
            y: group.map(p => p.G),
            z: group.map(p => p.B),
            name: `Group ${index}`,
            mode: 'lines+markers',
            marker: {
                size: 4,
                color: `rgb(${finalCentroid.R}, ${finalCentroid.G}, ${finalCentroid.B})`
            },
            type: 'scatter3d'
        };
    });
    const centroidData = diagnostics.Centroids.map((group, index) => {
        var finalCentroid = group[group.length - 1];
        return {
            x: [finalCentroid.R],
            y: [finalCentroid.G],
            z: [finalCentroid.B],
            name: `Centroid (group ${index})`,
            mode: 'markers',
            marker: {
                size: 6,
                color: `rgb(${finalCentroid.R}, ${finalCentroid.G}, ${finalCentroid.B})`
            },
            type: 'scatter3d'
        };
    });
    const groups = new Array(centroidGroupData.length);
    for (let i = 0; i < groups.length; i++) {
        groups[i] = [];
    }
    diagnostics.Pixels.forEach((pixelInfo) => {
        const finalGroupIndex = pixelInfo.Groups[pixelInfo.Groups.length - 1];
        groups[finalGroupIndex].push(pixelInfo.Pixel);
    });
    const pixelGroupData = groups.map((group, index) => ({
        x: group.map((pixel) => pixel.R),
        y: group.map((pixel) => pixel.G),
        z: group.map((pixel) => pixel.B),
        name: `Pixels (group ${index})`,
        mode: 'markers',
        marker: {
            size: 2
        },
        type: 'scatter3d'
    }));
    //const pixelData = {
    //    x: diagnostics.Pixels.map((pixelInfo) => pixelInfo.Pixel.R),
    //    y: diagnostics.Pixels.map((pixelInfo) => pixelInfo.Pixel.G),
    //    z: diagnostics.Pixels.map((pixelInfo) => pixelInfo.Pixel.B),
    //    name: 'Pixels',
    //    mode: 'markers',
    //    marker: {
    //        size: 2
    //    },
    //    type: 'scatter3d'
    //};
    const traces = [...centroidData, ...centroidGroupData, ...pixelGroupData];
    const layout = {
        title: 'Centroids',
        showlegend: true,
        autosize: true,
        width: 800,
        height: 800,
        scene: {
            xaxis: { title: 'Red' },
            yaxis: { title: 'Green' },
            zaxis: { title: 'Blue' }
        }
    };
    Plotly.newPlot('plot-container', traces, layout);
}

document.visualizeDiagnostics = visualizeDiagnostics;
