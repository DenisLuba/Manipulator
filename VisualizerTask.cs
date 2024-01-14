using System;
using System.Drawing.Text;
using Avalonia;
using Avalonia.Input;
using Avalonia.Media;

namespace Manipulation;

public static class VisualizerTask
{
    public static double X = 220;
    public static double Y = -100;
    public static double Alpha = 0.05;
    public static double Wrist = 2 * Math.PI / 3;
    public static double Elbow = 3 * Math.PI / 4;
    public static double Shoulder = Math.PI / 2;

    private static readonly double DeltaAngel = 0.1;
    private static readonly double KneeRadius = 10;

    public static Brush UnreachableAreaBrush = new SolidColorBrush(Color.FromArgb(255, 255, 230, 230));
    public static Brush ReachableAreaBrush = new SolidColorBrush(Color.FromArgb(255, 230, 255, 230));
    public static Pen ManipulatorPen = new Pen(Brushes.Black, 3);
    public static Brush JointBrush = new SolidColorBrush(Colors.Gray);

    public static void KeyDown(IDisplay display, KeyEventArgs key)
    {
        switch (key)
        {
            case { Key: Key.Q }:
                Shoulder.ChangeAngle();
                break;
            case { Key: Key.A }:
                Shoulder.ChangeAngle(false);
                break;
            case { Key: Key.W }:
                Elbow.ChangeAngle();
                break;
            case { Key: Key.S }:
                Elbow.ChangeAngle(false);
                break;
        }

        Wrist = -Alpha - Shoulder - Elbow;

        display.InvalidateVisual(); // вызывает перерисовку канваса
    }

    private static void ChangeAngle(this ref double angle, bool add = true)
    {
        angle = add ? angle + DeltaAngel : angle - DeltaAngel;
    }
    
    public static void MouseMove(IDisplay display, PointerEventArgs e)
    {
        var windowPoint = e.GetPosition(display);
        var shoulderPos = GetShoulderPos(display);
        var mathPoint = ConvertWindowToMath(windowPoint, shoulderPos);
        X = mathPoint.X;
        Y = mathPoint.Y;

	    UpdateManipulator();
	    display.InvalidateVisual();
    }
    
    public static void MouseWheel(IDisplay display, PointerWheelEventArgs e)
    {
        //Alpha += e.Delta.Y;
        Alpha += e.Delta.Y > 0 ? 0.1 : -0.1;
	    UpdateManipulator();
	    display.InvalidateVisual();
    }

    public static void UpdateManipulator()
    {
        var values = ManipulatorTask.MoveManipulatorTo(X, Y, Alpha);
        if (!double.IsNaN(values[0])) Shoulder = values[0];
        if (!double.IsNaN(values[1])) Elbow = values[1];
        if (!double.IsNaN(values[2])) Wrist = values[2];
    }

    public static void DrawManipulator(DrawingContext context, Point shoulderPos)
    {
        var joints = AnglesToCoordinatesTask.GetJointPositions(Shoulder, Elbow, Wrist);
            
        DrawReachableZone(context, ReachableAreaBrush, UnreachableAreaBrush, shoulderPos, joints);

        var formattedText = new FormattedText($"X={X:0}, Y={Y:0}, Alpha={Alpha:0.00}", Typeface.Default, 18,
            TextAlignment.Center, TextWrapping.Wrap, Size.Empty);
        context.DrawText(Brushes.DarkRed, new Point(10, 10), formattedText);

        var elbowPos = ConvertMathToWindow(joints[0], shoulderPos);
        var wristPos = ConvertMathToWindow(joints[1], shoulderPos);
        var palmEndPos = ConvertMathToWindow(joints[2], shoulderPos);

        context.DrawLine(ManipulatorPen, shoulderPos, elbowPos);
        context.DrawLine(ManipulatorPen, elbowPos, wristPos);
        context.DrawLine(ManipulatorPen, wristPos, palmEndPos);
        
        context.DrawEllipse(JointBrush, null, shoulderPos, KneeRadius, KneeRadius);
        context.DrawEllipse(JointBrush, null, elbowPos, KneeRadius, KneeRadius);
        context.DrawEllipse(JointBrush, null, wristPos, KneeRadius, KneeRadius);
    }

    private static void DrawReachableZone(
        DrawingContext context,
        Brush reachableBrush,
        Brush unreachableBrush,
        Point shoulderPos,
        Point[] joints)
    {
        var rmin = Math.Abs(Manipulator.UpperArm - Manipulator.Forearm);
        var rmax = Manipulator.UpperArm + Manipulator.Forearm;
        var mathCenter = new Point(joints[2].X - joints[1].X, joints[2].Y - joints[1].Y);
        var windowCenter = ConvertMathToWindow(mathCenter, shoulderPos);
        context.DrawEllipse(reachableBrush, 
            null,
            new Point(windowCenter.X, windowCenter.Y), 
            rmax, rmax);
        context.DrawEllipse(unreachableBrush, 
            null,
            new Point(windowCenter.X, windowCenter.Y), 
            rmin, rmin);
    }
    
	public static Point GetShoulderPos(IDisplay display)
	{
		return new Point(display.Bounds.Width / 2, display.Bounds.Height / 2);
	}

    public static Point ConvertMathToWindow(Point mathPoint, Point shoulderPos)
    {
        return new Point(mathPoint.X + shoulderPos.X, shoulderPos.Y - mathPoint.Y);
    }

    public static Point ConvertWindowToMath(Point windowPoint, Point shoulderPos)
    {
        return new Point(windowPoint.X - shoulderPos.X, shoulderPos.Y - windowPoint.Y);
    }
}