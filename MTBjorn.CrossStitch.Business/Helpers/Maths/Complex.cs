using System;

namespace MTBjorn.CrossStitch.Business.Helpers.Maths
{
	class complex
	{
		public float real = 0.0;
		public float imag = 0.0;
		//Empty constructor
		public complex()
		{
		}
		public complex(float real, float im)
		{
			this.real = real;
			this.imag = imag;
		}
		public string ToString()
		{
			string data = real.ToString() + " " + imag.ToString() + "i";
			return data;
		}
		//Convert from polar to rectangular
		public static complex from_polar(double r, double radians)
		{
			complex data = new complex(r * Math.Cos(radians), r * Math.Sin(radians));
			return data;
		}
		//Override addition operator
		public static complex operator +(complex a, complex b)
		{
			complex data = new complex(a.real + b.real, a.imag + b.imag);
			return data;
		}
		//Override subtraction operator
		public static complex operator -(complex a, complex b)
		{
			complex data = new complex(a.real - b.real, a.imag - b.imag);
			return data;
		}
		//Override multiplication operator
		public static complex operator *(complex a, complex b)
		{
			complex data = new complex((a.real * b.real) - (a.imag * b.imag),
			(a.real * b.imag + (a.imag * b.real));
			return data;
		}
		//Return magnitude of complex number
		public float magnitude {
			get {
				return Math.Sqrt(Math.Pow(real, 2) + Math.Pow(imag, 2));
			}
		}
		public float phase {
			get {
				return Math.Atan(imag / real);
			}
		}
	}
}
