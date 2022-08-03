using System;

namespace MTBjorn.CrossStitch.Business.Helpers.Maths
{
	/// <summary>
	/// Class to compute Fourier transforms using the Cooley-Tukey algorithm
	/// Adapted from: https://www.egr.msu.edu/classes/ece480/capstone/fall11/group06/style/Application_Note_ChrisOakley.pdf
	/// </summary>
	public static class Fourier
	{
		public static Complex[] DiscreteFourierTransform(Complex[] x)
		{
			var N = x.Length;
			var X = new Complex[N];

			for (int k = 0; k < N; k++)
			{
				X[k] = new Complex(0, 0);

				for (var n = 0; n < N; n++)
				{
					var temp = Complex.GetFromPolarCoordinates(1, -2 * Math.PI * n * k / N);
					temp *= x[n];
					X[k] += temp;
				}
			}

			return X;
		}

		public static Complex[] FastFourierTransform(Complex[] x)
		{
			var N = x.Length;
			if (N == 1)
			{
				return new Complex[]
				{
					x[0]
				};
			}

			var X = new Complex[N];
			var e = new Complex[N / 2]; // TODO: handle when the input array has an odd number of numbers?
			var d = new Complex[N / 2];

			for (var k = 0; k < N / 2; k++)
			{
				e[k] = x[2 * k];
				d[k] = x[(2 * k) + 1];
			}

			var D = FastFourierTransform(d);
			var E = FastFourierTransform(e);

			for (var k = 0; k < N / 2; k++)
				D[k] *= Complex.GetFromPolarCoordinates(1, -2 * Math.PI * k / N);

			for (var k = 0; k < N / 2; k++)
			{
				X[k] = E[k] + D[k];
				X[k + (N / 2)] = E[k] - D[k];
			}

			return X;
		}
	}
}
