using System;

namespace MTBjorn.CrossStitch.Business.Helpers.Maths
{
	public class Complex
	{
		public double real;
		public double imag;
		//Empty constructor
		public Complex()
		{
		}
		public Complex(double real, double im)
		{
			this.real = real;
			this.imag = im;
		}
		public string ToString()
		{
			string data = real.ToString() + " " + imag.ToString() + "i";
			return data;
		}
		//Convert from polar to rectangular
		public static Complex from_polar(double r, double radians)
		{
			Complex data = new Complex(r * Math.Cos(radians), r * Math.Sin(radians));
			return data;
		}
		//Override addition operator
		public static Complex operator +(Complex a, Complex b)
		{
			Complex data = new Complex(a.real + b.real, a.imag + b.imag);
			return data;
		}
		//Override subtraction operator
		public static Complex operator -(Complex a, Complex b)
		{
			Complex data = new Complex(a.real - b.real, a.imag - b.imag);
			return data;
		}
		//Override multiplication operator
		public static Complex operator *(Complex a, Complex b)
		{
			Complex data = new Complex((a.real * b.real) - (a.imag * b.imag), a.real * b.imag + (a.imag * b.real));
			return data;
		}
		//Return magnitude of complex number
		public double magnitude {
			get {
				return Math.Sqrt(Math.Pow(real, 2) + Math.Pow(imag, 2));
			}
		}
		public double phase {
			get {
				return Math.Atan(imag / real);
			}
		}
	}
}
