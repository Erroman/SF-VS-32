using System;
using static System.Console;
using System.Windows;
using System.Windows.Media;
static class TransformWorldToScreen
{
	private static Matrix WtoDMatrix, DtoWMatrix;
	public static void PrepareTransformations(
	double wxmin, double wxmax, double wymin, double wymax,
	double dxmin, double dxmax, double dymax, double dymin)
	{
		// Make WtoD.
		WtoDMatrix = Matrix.Identity;
		WtoDMatrix.Translate(-wxmin, -wymin);

		double xscale = (dxmax - dxmin) / (wxmax - wxmin);
		double yscale = (dymax - dymin) / (wymax - wymin);
		WtoDMatrix.Scale(xscale, yscale);

		WtoDMatrix.Translate(dxmin, dymin);

		// Make DtoW.
		DtoWMatrix = WtoDMatrix;
		DtoWMatrix.Invert();
	}

	// Transform a point from world to device coordinates.
	public static  Point WtoD(Point point)
	{
		return WtoDMatrix.Transform(point);
	}

	// Transform a point from device to world coordinates.
	public static Point DtoW(Point point)
	{
		return DtoWMatrix.Transform(point);
	}

		
}