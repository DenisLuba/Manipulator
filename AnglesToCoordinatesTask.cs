using System;
using Avalonia;
using NUnit.Framework;
using static Manipulation.Manipulator;

namespace Manipulation;

public static class AnglesToCoordinatesTask
{
	/// <summary>
	/// По значению углов суставов возвращает массив координат суставов
	/// в порядке new []{elbow, wrist, palmEnd}
	/// </summary>
	public static Point[] GetJointPositions(double shoulder, double elbow, double wrist)
    {
        var elbowX = Math.Cos(shoulder) * UpperArm;
        var elbowY = Math.Sin(shoulder) * UpperArm;
		var elbowPos = new Point(elbowX, elbowY);

        var wristX = Math.Cos(elbow + shoulder - Math.PI) * Forearm + elbowX;
        var wristY = Math.Sin(elbow + shoulder - Math.PI) * Forearm + elbowY;
		var wristPos = new Point(wristX, wristY);

        var palmX = Math.Cos(wrist + elbow + shoulder) * Palm + wristX;
        var palmY = Math.Sin(wrist + elbow + shoulder) * Palm + wristY;
		var palmEndPos = new Point(palmX, palmY);

		return new[]
		{
			elbowPos,
			wristPos,
			palmEndPos
		};
	}

    public static double Square(this double x) => x * x;
}

[TestFixture]
public class AnglesToCoordinatesTask_Tests
{
	// Доработайте эти тесты!
	// С помощью строчки TestCase можно добавлять новые тестовые данные.
	// Аргументы TestCase превратятся в аргументы метода.
	[TestCase(Math.PI / 2, Math.PI / 2, Math.PI, Forearm + Palm, UpperArm)]
	[TestCase(0, Math.PI, Math.PI, UpperArm + Forearm + Palm, 0)]
	[TestCase(Math.PI, 0, 0, Forearm - UpperArm - Palm, 0)]
    [TestCase(Math.PI, Math.PI / 2, Math.PI / 2, Palm - UpperArm, Forearm)]
    public void TestGetJointPositions(double shoulder, double elbow, double wrist, double palmEndX, double palmEndY)
	{
		var joints = AnglesToCoordinatesTask.GetJointPositions(shoulder, elbow, wrist);
		Assert.AreEqual(palmEndX, joints[2].X, 1e-5, "palm endX");
		Assert.AreEqual(palmEndY, joints[2].Y, 1e-5, "palm endY");

        var elbowPos = joints[0];
		var wristPos = joints[1];
        var palmPos = joints[2];

        var upperArm = Math.Sqrt(elbowPos.X.Square() + elbowPos.Y.Square());
        var forearm = Math.Sqrt((wristPos.X - elbowPos.X).Square() + (wristPos.Y - elbowPos.Y).Square());
        var palm = Math.Sqrt((palmPos.X - wristPos.X).Square() + (palmPos.Y - wristPos.Y).Square());

        if (Math.Abs(upperArm - UpperArm) > 1e-5 || Math.Abs(forearm - Forearm) > 1e-5 || Math.Abs(palm -= Palm) > 1e-5)
            Assert.Fail("Расстояния между точками не равны сегментам.");
    }
}