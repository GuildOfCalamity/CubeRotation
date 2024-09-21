using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Windows.Foundation;
using Windows.Foundation.Collections;

namespace WinUI_Cube;

public sealed partial class MainPage : Page
{
    #region [Props]
    int _counter = 0;
    DispatcherTimer? _timer;
    static float x, y, z, ooz, xp, yp;
    static int idx;
    static float iterationSpeed = 0.9f;
    static float cubeWidth = 32;
    static int widthOffset = 127;
    static int heightOffset = 45;
    static float[] zBuffer = new float[widthOffset * heightOffset];
    static int[] buffer = new int[widthOffset * heightOffset];
    static int backgroundChar = ' ';
    static float A = 0.001f;
    static float B = 0.001f;
    static float C = 0.001f;
    static int distanceFromCam = 100;
    static float K1 = 30;
    static long frameCount = 0;
    static ValueStopwatch vsw = ValueStopwatch.StartNew();
    #endregion

    public MainPage()
    {
        this.InitializeComponent();
        this.Loaded += MainPage_Loaded;
    }

    void MainPage_Loaded(object sender, RoutedEventArgs e)
    {
        if (this._timer == null)
        {
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromMilliseconds(24);
            _timer.Tick += _timer_Tick;
            _timer.Start();
        }
    }

    void _timer_Tick(object? sender, object e)
    {
        vsw = ValueStopwatch.StartNew();
        // Blank the buffer's background.
        Array.Fill(buffer, backgroundChar);
        // Reset depth buffer.
        Array.Fill(zBuffer, 0);
        // Determine the char for each face of the cube.
        for (float cubeX = -cubeWidth; cubeX < cubeWidth; cubeX += iterationSpeed)
        {
            for (float cubeY = -cubeWidth; cubeY < cubeWidth; cubeY += iterationSpeed)
            {
                // 1st face
                calculateForSurface(cubeX, cubeY, -cubeWidth, '≈');
                // 2nd face
                calculateForSurface(cubeWidth, cubeY, cubeX, '⁞');
                // 3rd face
                calculateForSurface(-cubeWidth, cubeY, -cubeX, '|');
                // 4th face
                calculateForSurface(-cubeX, cubeY, cubeWidth, '⅓');
                // 5th face
                calculateForSurface(cubeX, -cubeWidth, -cubeY, '⁃');
                // 6th face
                calculateForSurface(cubeX, cubeWidth, cubeY, '•');
            }
        }

        StringBuilder sb = new StringBuilder();
        for (int k = 0; k < widthOffset * heightOffset; k++)
        {
            if (k % widthOffset == 0) // Add CRLF on end of row.
                sb.AppendLine();
            else
                sb.Append(((char)buffer[k]));
        }

        // Rotate matrix by some amount.
        A += 0.06f;
        B += 0.06f;
        C += 0.001f;

        //if (A >= 360) { A = 0; }
        //if (B >= 360) { B = 0; }
        //if (C >= 360) { C = 0; }


        DispatcherQueue.TryEnqueue(() =>
        {
            // Render the buffer to the TextBox.
            tbCube.Text = $"{sb}";
            // Update the frames/second.
            //if (++frameCount % 10 == 0)
            //    this.Title = $"{1000 / vsw.GetElapsedTime().Milliseconds} FPS";
        });
    }

    #region [Math Functions]
    static float calculateX(int i, int j, int k)
    {
        return j * MathF.Sin(A) * MathF.Sin(B) * MathF.Cos(C) - k * MathF.Cos(A) * MathF.Sin(B) * MathF.Cos(C) +
               j * MathF.Cos(A) * MathF.Sin(C) + k * MathF.Sin(A) * MathF.Sin(C) + i * MathF.Cos(B) * MathF.Cos(C);
    }
    static float calculateY(int i, int j, int k)
    {
        return j * MathF.Cos(A) * MathF.Cos(C) + k * MathF.Sin(A) * MathF.Cos(C) -
               j * MathF.Sin(A) * MathF.Sin(B) * MathF.Sin(C) + k * MathF.Cos(A) * MathF.Sin(B) * MathF.Sin(C) -
               i * MathF.Cos(B) * MathF.Sin(C);
    }
    static float calculateZ(int i, int j, int k)
    {
        return k * MathF.Cos(A) * MathF.Cos(B) - j * MathF.Sin(A) * MathF.Cos(B) + i * MathF.Sin(B);
    }
    static void calculateForSurface(float cubeX, float cubeY, float cubeZ, int ch)
    {
        x = calculateX((int)cubeX, (int)cubeY, (int)cubeZ);
        y = calculateY((int)cubeX, (int)cubeY, (int)cubeZ);
        z = calculateZ((int)cubeX, (int)cubeY, (int)cubeZ) + distanceFromCam;
        ooz = 1 / z;
        xp = (int)(widthOffset / 2 + K1 * ooz * x * 2); //xp = (int)(width / 2 - 2 * cubeWidth + K1 * ooz * x * 2);
        yp = (int)(heightOffset / 2 + K1 * ooz * y);
        idx = (int)(xp + yp * widthOffset);
        if (idx >= 0 && idx < widthOffset * heightOffset)
        {
            // Update the depth and draw buffers.
            if (ooz > zBuffer[idx])
            {
                zBuffer[idx] = ooz;
                buffer[idx] = ch;
            }
        }
    }
    #endregion
}
