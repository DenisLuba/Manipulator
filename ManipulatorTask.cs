using System;
using System.Linq;
using Avalonia;
using NUnit.Framework;
using static Manipulation.Manipulator;

namespace Manipulation;

public static class ManipulatorTask
{
    /// <summary>
    /// Возвращает массив углов (shoulder, elbow, wrist),
    /// необходимых для приведения эффектора манипулятора в точку x и y 
    /// с углом между последним суставом и горизонталью, равному alpha (в радианах)
    /// См. чертеж manipulator.png!
    /// </summary>
    public static double[] MoveManipulatorTo(double x, double y, double alpha)
    {
        var wristPosY = y + Math.Sin(alpha) * Palm;
        var wristPosX = x - Math.Cos(alpha) * Palm;

        var segmentFromShoulderToWrist = Math.Sqrt(wristPosY * wristPosY + wristPosX * wristPosX);

        var elbowAngle = TriangleTask.GetABAngle(UpperArm, Forearm, segmentFromShoulderToWrist);
        var shoulderAngle = TriangleTask.GetABAngle(UpperArm, segmentFromShoulderToWrist, Forearm) 
                            + Math.Atan2(wristPosY, wristPosX);
        var wristAngle = Math.PI * 2 - elbowAngle - shoulderAngle - alpha;

        return new[] { shoulderAngle, elbowAngle, wristAngle };
    }
}

[TestFixture]
public class ManipulatorTask_Tests
{
    [TestCase(330, 0, 0, new[] { 0, Math.PI, Math.PI })]
    [TestCase(0, 330, -Math.PI / 2, new[] { Math.PI / 2, Math.PI, Math.PI })]
    [TestCase(Forearm, UpperArm - Palm, Math.PI / 2, new[] { Math.PI / 2, Math.PI / 2, Math.PI / 2 })]
    public void SimpleTestMoveManipulatorTo(double x, double y, double alpha, double[] expected)
    {
        var result = ManipulatorTask.MoveManipulatorTo(x, y, alpha);
        for (var i = 0; i < expected.Length; i++)
        {
            Assert.AreEqual(expected[i], result[i], 1e-5);
        }
    }


    private const int UpperBorder = (int)((UpperArm + Forearm + Palm) / 1.5);
    [Test]
    public void TestMoveManipulatorTo()
    {
        Assert.True(Repeat(10_000, UpperBorder, new Random(), Check));
    }

    private static bool Repeat(int i, int upperBorder, Random random, Func<int, Random, bool> check)
    {
        for (var j = 0; j < i; j++)
            if (!check.Invoke(upperBorder, random)) return false;

        return true;
    }

    private static bool Check(int upperBorder, Random random)
    {
        var randomX = random.Next(-upperBorder, upperBorder);
        var randomY = random.Next(-upperBorder, upperBorder);
        var randomAlphaAngle = random.NextDouble() * random.Next(-100, 100);

        var angles = ManipulatorTask.MoveManipulatorTo(randomX, randomY, randomAlphaAngle);
        if (angles.Any(double.IsNaN))
            return true;
        
        var positions = AnglesToCoordinatesTask.GetJointPositions(angles[0], angles[1], angles[2]);
        return PointComparison.Equals(positions[2], new Point(randomX, randomY));
    }
}

internal static class PointComparison
{
    public static bool Equals(Point point, Point other)
        => point.X.IntEquals(other.X) && point.Y.IntEquals(other.Y);

    private static bool IntEquals(this double number, double other)
        => Math.Abs(number - other) < 1e-5 || (double.IsNaN(number) && double.IsNaN(other));
}