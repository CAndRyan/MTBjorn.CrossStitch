using System;
using System.IO;
using System.Linq;
using MTBjorn.CrossStitch.Business.Helpers;
using Newtonsoft.Json;
using Plotly.NET.CSharp;

namespace MTBjorn.CrossStitch.Visualize
{
	class Program
	{
		static void Main(string[] args)
		{
			var diagnosticsFilePath = @"D:\Chris\Downloads\cross-stitch-test-diagnostics.json";
			var diagnostics = File.ReadAllText(diagnosticsFilePath);
            var rebalanceHistory = JsonConvert.DeserializeObject<RebalanceHistory>(diagnostics);

			var chart = Chart.Point3D<int, int, int, string>(
				x: rebalanceHistory.Centroids[1].Select(c => (int)c.R),
				y: rebalanceHistory.Centroids[1].Select(c => (int)c.G),
				z: rebalanceHistory.Centroids[1].Select(c => (int)c.B)
			).WithTraceInfo("Centroids (0th)", ShowLegend: true)
			.WithXAxisStyle<int, int, string>(Title: Plotly.NET.Title.init("Red"))
			.WithYAxisStyle<int, int, string>(Title: Plotly.NET.Title.init("Green"));
            //.WithZAxisStyle(Title: Plotly.NET.Title.init("Blue"))
            chart.Show();

   //         Chart.Point<double, double, string>(
			//	x: new double[] { 1, 2 },
			//	y: new double[] { 5, 10 }
			//)
			//.WithTraceInfo("Hello from C#", ShowLegend: true)
			//.WithXAxisStyle<double, double, string>(Title: Plotly.NET.Title.init("xAxis"))
			//.WithYAxisStyle<double, double, string>(Title: Plotly.NET.Title.init("yAxis"))
			//.Show();
		}
	}
}
