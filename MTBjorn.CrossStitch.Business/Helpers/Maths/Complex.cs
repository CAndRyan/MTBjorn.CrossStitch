using System;

namespace MTBjorn.CrossStitch.Business.Helpers.Maths
{
	public class Complex
	{
		private readonly double real;
		private readonly double imaginary;

		public Complex(double real, double imaginary)
		{
			this.real = real;
			this.imaginary = imaginary;
		}

		/// <summary>
		/// Convert from polar coordinates to cartesian
		/// </summary>
		public static Complex GetFromPolarCoordinates(double r, double radians) => new Complex(r * Math.Cos(radians), r * Math.Sin(radians));

		public double GetMagnitude() => Math.Sqrt(Math.Pow(real, 2) + Math.Pow(imaginary, 2));

		public double GetPhase() => Math.Atan(imaginary / real);

		public override string ToString() => $"{real} + {imaginary}i";

		public static Complex operator +(Complex first, Complex second) => new Complex(first.real + second.real, first.imaginary + second.imaginary);

		public static Complex operator -(Complex first, Complex second) => new Complex(first.real - second.real, first.imaginary - second.imaginary);

		public static Complex operator *(Complex first, Complex second)
		{
			var localPart = (first.real * second.real) - (first.imaginary * second.imaginary);
			var imaginaryParty = (first.real * second.imaginary) + (first.imaginary * second.real);

			return new Complex(localPart, imaginaryParty);
		}
	}
}
