using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Microsoft.Graphics.Canvas.UI.Xaml;
using Windows.UI;
using Windows.Storage;




namespace IndependentProject
{
    public sealed partial class MainPage : Page
    {
        public int[] LayerSizes;
        public TransferFunction[] TransFuncs;
        public BackPropagation bp;
        public Point MouseCords;
        public int Stage;
        public bool[] MouseDown;
        public int[] GameState, OldGameState;
        public int MoveMemory;
        public bool Turn, GameOver, Tie;
        public double[][] Input = new double[1000][];
        public double[][] DesiredOutput = new double[1000][];
        public int TrainerCount;

        public MainPage()
        {
            this.InitializeComponent();
            Canvas1.Draw += Canvas1_Draw;
            Window.Current.CoreWindow.PointerPressed += CoreWindow_PointerPressed;
            Window.Current.CoreWindow.PointerReleased += CoreWindow_PointerReleased;
            Window.Current.CoreWindow.PointerMoved += CoreWindow_PointerMoved;

            LayerSizes = new int[3] { 9, 10, 1 }; // first is input, middle are the hidden layers, and end is an output
            TransFuncs = new TransferFunction[3] { TransferFunction.None, TransferFunction.Sigmoid, TransferFunction.Linear };
            bp = new BackPropagation(LayerSizes, TransFuncs);
            Stage = 0;
            MouseDown = new bool[2];
            GameState = new int[9];
            OldGameState = new int[9];
            for (int i = 0; i < 1000; i++)
            {
                Input[i] = new double[9];
                DesiredOutput[i] = new double[1];
            }

        }

        private void Canvas1_Draw(Microsoft.Graphics.Canvas.UI.Xaml.CanvasControl sender, Microsoft.Graphics.Canvas.UI.Xaml.CanvasDrawEventArgs args)
        {
            #region Menu
            if (Stage == 0)
            {
                Rect[] MenuRectangle = new Rect[2];
                for (int i = 0; i < 2; i++)
                {
                    MenuRectangle[i] = new Rect((int)((Canvas1.ActualWidth / 2) - 23), (int)((Canvas1.ActualHeight / 2) - 47 + (50 * i)), 50, 25);
                    if (!MenuRectangle[i].Contains(MouseCords))
                    {
                        args.DrawingSession.DrawRectangle(MenuRectangle[i], Colors.Black);
                    }
                    else
                    {
                        args.DrawingSession.DrawRectangle(MenuRectangle[i], Colors.Gold);
                    }

                }

                args.DrawingSession.DrawText("Train", (int)((Canvas1.ActualWidth / 2) - 20), (int)((Canvas1.ActualHeight / 2) - 50 + (50 * 0)), Colors.LightGreen);
                args.DrawingSession.DrawText("Fight", (int)((Canvas1.ActualWidth / 2) - 20), (int)((Canvas1.ActualHeight / 2) - 50 + (50 * 1)), Colors.LightGreen);

                if (MouseDown[0] && MenuRectangle[0].Contains(MouseCords))
                {
                    Stage = 1;
                    MouseDown[0] = false;
                }
                if (MouseDown[0] && MenuRectangle[1].Contains(MouseCords))
                {
                    Stage = 2;
                    MouseDown[0] = false;
                }
            }
            #endregion

            args.DrawingSession.DrawText(TrainerCount + "", 10, 10, Colors.LightGreen);
            #region Trainer
            if (Stage == 1)
            {

                #region BoardControl
                if (GameOver)
                {
                    if (Tie)
                    {
                        args.DrawingSession.DrawText("You both win!!!", (int)((Canvas1.ActualWidth / 2) - 40), (int)((Canvas1.ActualHeight / 2) - 50 - (50 * 4)), Colors.LightGreen);
                    }
                    else if (Turn)
                    {
                        args.DrawingSession.DrawText("O Won", (int)((Canvas1.ActualWidth / 2) - 30), (int)((Canvas1.ActualHeight / 2) - 50 - (50 * 4)), Colors.LightGreen);
                    }
                    else if (!Turn)
                    {
                        args.DrawingSession.DrawText("X Won", (int)((Canvas1.ActualWidth / 2) - 30), (int)((Canvas1.ActualHeight / 2) - 50 - (50 * 4)), Colors.LightGreen);
                    }
                }
                else if (Turn)
                {
                    args.DrawingSession.DrawText("X's Turn", (int)((Canvas1.ActualWidth / 2) - 30), (int)((Canvas1.ActualHeight / 2) - 50 - (50 * 4)), Colors.LightGreen);
                }
                else if (!Turn)
                {
                    args.DrawingSession.DrawText("O's Turn", (int)((Canvas1.ActualWidth / 2) - 30), (int)((Canvas1.ActualHeight / 2) - 50 - (50 * 4)), Colors.LightGreen);
                }
               


                args.DrawingSession.DrawLine((int)((Canvas1.ActualWidth / 2) - 50), (int)((Canvas1.ActualHeight / 2) - 150), (int)((Canvas1.ActualWidth / 2) - 50), (int)((Canvas1.ActualHeight / 2) + 150), Colors.Black);
                args.DrawingSession.DrawLine((int)((Canvas1.ActualWidth / 2) + 50), (int)((Canvas1.ActualHeight / 2) - 150), (int)((Canvas1.ActualWidth / 2) + 50), (int)((Canvas1.ActualHeight / 2) + 150), Colors.Black);
                args.DrawingSession.DrawLine((int)((Canvas1.ActualWidth / 2) - 150), (int)((Canvas1.ActualHeight / 2) - 50), (int)((Canvas1.ActualWidth / 2) + 150), (int)((Canvas1.ActualHeight / 2) - 50), Colors.Black);
                args.DrawingSession.DrawLine((int)((Canvas1.ActualWidth / 2) - 150), (int)((Canvas1.ActualHeight / 2) + 50), (int)((Canvas1.ActualWidth / 2) + 150), (int)((Canvas1.ActualHeight / 2) + 50), Colors.Black);

                Rect[] GameRectangle = new Rect[9];
                for (int i = 0; i < 9; i++)
                {
                    GameRectangle[i] = new Rect((int)((Canvas1.ActualWidth / 2) - 150 + (100 * (i % 3))), (int)((Canvas1.ActualHeight / 2) - 150 + (100 * (Math.Floor((double)(i / 3))))), 100, 100);
                    if (!GameOver)
                    {
                        if (MouseDown[0] && GameRectangle[i].Contains(MouseCords) && GameState[i] == 0 && Turn)
                        {
                            OldGameState[i] = GameState[i];
                            GameState[i] = 1;
                            MoveMemory = i;
                            Turn = !Turn;
                        }
                        else if (MouseDown[0] && GameRectangle[i].Contains(MouseCords) && GameState[i] == 0 && !Turn)
                        {
                            //OldGameState[i] = GameState[i];
                            GameState[i] = 2;
                            Turn = !Turn;
                        }
                    }

                    if (GameState[i] == 1)
                    {
                        args.DrawingSession.DrawLine((int)(GameRectangle[i].X + 5), (int)(GameRectangle[i].Y + 5), (int)(GameRectangle[i].X + GameRectangle[i].Width - 5), (int)(GameRectangle[i].Y + GameRectangle[i].Height - 5), Colors.Black);
                        args.DrawingSession.DrawLine((int)(GameRectangle[i].X + 5), (int)(GameRectangle[i].Y + GameRectangle[i].Height - 5), (int)(GameRectangle[i].X + GameRectangle[i].Width - 5), (int)(GameRectangle[i].Y + 5), Colors.Black);
                    }
                    if (GameState[i] == 2)
                    {
                        args.DrawingSession.DrawEllipse((int)(GameRectangle[i].X + 50), (int)(GameRectangle[i].Y + 50), 45, 45, Colors.Black);
                    }
                }

                if (GameOver)
                {
                    args.DrawingSession.DrawText("Rematch", (int)((Canvas1.ActualWidth / 2) - 20), (int)((Canvas1.ActualHeight / 2) - 50 + (50 * 4)), Colors.LightGreen);
                    args.DrawingSession.DrawText("Go Home", (int)((Canvas1.ActualWidth / 2) - 20), (int)((Canvas1.ActualHeight / 2) - 50 + (50 * 5)), Colors.LightGreen);
                    Rect[] MenuRectangle = new Rect[2];
                    for (int i = 0; i < 2; i++)
                    {
                        MenuRectangle[i] = new Rect((int)((Canvas1.ActualWidth / 2) - 23), (int)((Canvas1.ActualHeight / 2) - 47 + (50 * (i + 4))), 90, 25);
                        if (!MenuRectangle[i].Contains(MouseCords))
                        {
                            args.DrawingSession.DrawRectangle(MenuRectangle[i], Colors.Black);
                        }
                        else
                        {
                            args.DrawingSession.DrawRectangle(MenuRectangle[i], Colors.Gold);
                        }
                    }
                    if (MouseDown[0] && MenuRectangle[0].Contains(MouseCords))
                    {
                        for (int i = 0; i < 9; i++)
                        {
                            GameState[i] = 0;
                        }
                        GameOver = false;
                        Tie = false;
                    }
                    else if (MouseDown[0] && MenuRectangle[1].Contains(MouseCords))
                    {
                        for (int i = 0; i < 9; i++)
                        {
                            GameState[i] = 0;
                        }
                        GameOver = false;
                        Stage = 4;
                        Tie = false;
                    }
                }

                #region GameCheck
                for (int i = 0; i < 3; i++)
                {
                    if (GameState[i] != 0 && GameState[i] == GameState[i + 3] && GameState[i] == GameState[i + 6])
                    {
                        GameOver = true;
                        args.DrawingSession.DrawLine((int)((Canvas1.ActualWidth / 2) - 100 + (100 * (i % 3))), (int)((Canvas1.ActualHeight / 2) - 150), (int)((Canvas1.ActualWidth / 2) - 100 + (100 * (i % 3))), (int)((Canvas1.ActualHeight / 2) + 150), Colors.Red);
                    }
                    else if (GameState[i * 3] != 0 && GameState[i * 3] == GameState[(i * 3) + 1] && GameState[i * 3] == GameState[(i * 3) + 2])
                    {
                        GameOver = true;
                        args.DrawingSession.DrawLine((int)((Canvas1.ActualWidth / 2) - 150), (int)((Canvas1.ActualHeight / 2) - 100 + (100 * (i % 3))), (int)((Canvas1.ActualWidth / 2) + 150), (int)((Canvas1.ActualHeight / 2) - 100 + (100 * (i % 3))), Colors.Red);
                    }
                    else if (GameState[i * 2] != 0 && i != 2 && GameState[i * 2] == GameState[4] && GameState[i * 2] == GameState[8 - (i * 2)])
                    {
                        GameOver = true;
                        args.DrawingSession.DrawLine((int)((Canvas1.ActualWidth / 2) - (150 + (-300 * i))), (int)((Canvas1.ActualHeight / 2) - 150), (int)((Canvas1.ActualWidth / 2) + (150 + (-300 * i))), (int)((Canvas1.ActualHeight / 2) + 150), Colors.Red);
                    }
                    else if (!GameState.Contains(0))
                    {
                        GameOver = true;
                        Tie = true;

                    }
                }
                #endregion
                #endregion

                if (GameState != OldGameState)
                {
                    TrainerCount++;
                    for (int i = 0; i < 9; i++)
                    {
                        Input[TrainerCount][i] = OldGameState[i];
                    }
                    DesiredOutput[TrainerCount][0] = MoveMemory;
                    OldGameState = GameState;


                }
            }
            #endregion

            if (Stage == 4)
            {

                double[] Output = new double[1];
                double Error = 0.0, ErrorResult = 1.0, ErrorSum = 1.0, ErrorAverage = 0.0, PreviousErrorAverage = 0.0;
                int Unstuck = 0;
                while (ErrorResult > 0.001)
                { 
                    Error = 0.0;
                    for (int i = 0; i < TrainerCount; i++)
                    {
                        Error += bp.Train(ref Input[i], ref DesiredOutput[i], 0.15, 0.1);

                        ErrorResult = Error;
                        ErrorSum += Error;
                        

                        if ( (PreviousErrorAverage / ErrorSum) < 1.001) // Only run if error is increasing on average
                        {
                            bp.Stuck(0.1);
                            Unstuck++;                      
                        }
                        ErrorSum = 0.0;
                        PreviousErrorAverage = ErrorAverage;
                    }

                }
                Stage = 0;
            }

            #region Fight
            if (Stage == 2)
            {
                double[] SingleInput = new double[9];
                for (int i = 0; i < 9; i++)
                {
                    SingleInput[i] = OldGameState[i];
                }
                
                double[] Output = new double[1];
                bp.Run(ref SingleInput, out Output);
                #region BoardControl
                if (GameOver)
                {
                    if (Tie)
                    {
                        args.DrawingSession.DrawText("You both win!!!", (int)((Canvas1.ActualWidth / 2) - 40), (int)((Canvas1.ActualHeight / 2) - 50 - (50 * 4)), Colors.LightGreen);
                    }
                    else if (Turn)
                    {
                        args.DrawingSession.DrawText("O Won", (int)((Canvas1.ActualWidth / 2) - 30), (int)((Canvas1.ActualHeight / 2) - 50 - (50 * 4)), Colors.LightGreen);
                    }
                    else if (!Turn)
                    {
                        args.DrawingSession.DrawText("X Won", (int)((Canvas1.ActualWidth / 2) - 30), (int)((Canvas1.ActualHeight / 2) - 50 - (50 * 4)), Colors.LightGreen);
                    }
                }
                else if (Turn)
                {
                    args.DrawingSession.DrawText("X's Turn", (int)((Canvas1.ActualWidth / 2) - 30), (int)((Canvas1.ActualHeight / 2) - 50 - (50 * 4)), Colors.LightGreen);
                }
                else if (!Turn)
                {
                    args.DrawingSession.DrawText("O's Turn", (int)((Canvas1.ActualWidth / 2) - 30), (int)((Canvas1.ActualHeight / 2) - 50 - (50 * 4)), Colors.LightGreen);
                }

                args.DrawingSession.DrawLine((int)((Canvas1.ActualWidth / 2) - 50), (int)((Canvas1.ActualHeight / 2) - 150), (int)((Canvas1.ActualWidth / 2) - 50), (int)((Canvas1.ActualHeight / 2) + 150), Colors.Black);
                args.DrawingSession.DrawLine((int)((Canvas1.ActualWidth / 2) + 50), (int)((Canvas1.ActualHeight / 2) - 150), (int)((Canvas1.ActualWidth / 2) + 50), (int)((Canvas1.ActualHeight / 2) + 150), Colors.Black);
                args.DrawingSession.DrawLine((int)((Canvas1.ActualWidth / 2) - 150), (int)((Canvas1.ActualHeight / 2) - 50), (int)((Canvas1.ActualWidth / 2) + 150), (int)((Canvas1.ActualHeight / 2) - 50), Colors.Black);
                args.DrawingSession.DrawLine((int)((Canvas1.ActualWidth / 2) - 150), (int)((Canvas1.ActualHeight / 2) + 50), (int)((Canvas1.ActualWidth / 2) + 150), (int)((Canvas1.ActualHeight / 2) + 50), Colors.Black);

                Rect[] GameRectangle = new Rect[9];
                for (int i = 0; i < 9; i++)
                {
                    GameRectangle[i] = new Rect((int)((Canvas1.ActualWidth / 2) - 150 + (100 * (i % 3))), (int)((Canvas1.ActualHeight / 2) - 150 + (100 * (Math.Floor((double)(i / 3))))), 100, 100);
                    if (!GameOver)
                    {
                        if (MouseDown[0] && GameRectangle[i].Contains(MouseCords) && GameState[i] == 0 && Turn)
                        {
                            OldGameState[i] = GameState[i];
                            GameState[i] = 1;
                            Turn = !Turn;
                        }
                        else if (!Turn)
                        {
                            if (Output[0] > -0.5 && Output[0] < 8.5)
                            GameState[Convert.ToInt32(Output[0])] = 2;
                            Turn = !Turn;
                            MouseDown[0] = false;
                            args.DrawingSession.DrawText(Output[0] + "", 10, 10, Colors.LightGreen);
                        }
                    }

                    if (GameState[i] == 1)
                    {
                        args.DrawingSession.DrawLine((int)(GameRectangle[i].X + 5), (int)(GameRectangle[i].Y + 5), (int)(GameRectangle[i].X + GameRectangle[i].Width - 5), (int)(GameRectangle[i].Y + GameRectangle[i].Height - 5), Colors.Black);
                        args.DrawingSession.DrawLine((int)(GameRectangle[i].X + 5), (int)(GameRectangle[i].Y + GameRectangle[i].Height - 5), (int)(GameRectangle[i].X + GameRectangle[i].Width - 5), (int)(GameRectangle[i].Y + 5), Colors.Black);
                    }
                    if (GameState[i] == 2)
                    {
                        args.DrawingSession.DrawEllipse((int)(GameRectangle[i].X + 50), (int)(GameRectangle[i].Y + 50), 45, 45, Colors.Black);
                    }
                }

                if (GameOver)
                {
                    args.DrawingSession.DrawText("Rematch", (int)((Canvas1.ActualWidth / 2) - 20), (int)((Canvas1.ActualHeight / 2) - 50 + (50 * 4)), Colors.LightGreen);
                    args.DrawingSession.DrawText("Go Home", (int)((Canvas1.ActualWidth / 2) - 20), (int)((Canvas1.ActualHeight / 2) - 50 + (50 * 5)), Colors.LightGreen);
                    Rect[] MenuRectangle = new Rect[2];
                    for (int i = 0; i < 2; i++)
                    {
                        MenuRectangle[i] = new Rect((int)((Canvas1.ActualWidth / 2) - 23), (int)((Canvas1.ActualHeight / 2) - 47 + (50 * (i + 4))), 90, 25);
                        if (!MenuRectangle[i].Contains(MouseCords))
                        {
                            args.DrawingSession.DrawRectangle(MenuRectangle[i], Colors.Black);
                        }
                        else
                        {
                            args.DrawingSession.DrawRectangle(MenuRectangle[i], Colors.Gold);
                        }
                    }
                    if (MouseDown[0] && MenuRectangle[0].Contains(MouseCords))
                    {
                        for (int i = 0; i < 9; i++)
                        {
                            GameState[i] = 0;
                        }
                        GameOver = false;
                    }
                    else if (MouseDown[0] && MenuRectangle[1].Contains(MouseCords))
                    {
                        for (int i = 0; i < 9; i++)
                        {
                            GameState[i] = 0;
                        }
                        GameOver = false;
                        Stage = 0;
                    }
                }

                #region GameCheck
                for (int i = 0; i < 3; i++)
                {
                    if (GameState[i] != 0 && GameState[i] == GameState[i + 3] && GameState[i] == GameState[i + 6])
                    {
                        GameOver = true;
                        args.DrawingSession.DrawLine((int)((Canvas1.ActualWidth / 2) - 100 + (100 * (i % 3))), (int)((Canvas1.ActualHeight / 2) - 150), (int)((Canvas1.ActualWidth / 2) - 100 + (100 * (i % 3))), (int)((Canvas1.ActualHeight / 2) + 150), Colors.Red);
                    }
                    else if (GameState[i * 3] != 0 && GameState[i * 3] == GameState[(i * 3) + 1] && GameState[i * 3] == GameState[(i * 3) + 2])
                    {
                        GameOver = true;
                        args.DrawingSession.DrawLine((int)((Canvas1.ActualWidth / 2) - 150), (int)((Canvas1.ActualHeight / 2) - 100 + (100 * (i % 3))), (int)((Canvas1.ActualWidth / 2) + 150), (int)((Canvas1.ActualHeight / 2) - 100 + (100 * (i % 3))), Colors.Red);
                    }
                    else if (GameState[i * 2] != 0 && i != 2 && GameState[i * 2] == GameState[4] && GameState[i * 2] == GameState[8 - (i * 2)])
                    {
                        GameOver = true;
                        args.DrawingSession.DrawLine((int)((Canvas1.ActualWidth / 2) - (150 + (-300 * i))), (int)((Canvas1.ActualHeight / 2) - 150), (int)((Canvas1.ActualWidth / 2) + (150 + (-300 * i))), (int)((Canvas1.ActualHeight / 2) + 150), Colors.Red);
                    }
                    else if (!GameState.Contains(0))
                    {
                        GameOver = true;

                    }
                }
                #endregion
                #endregion

            }
            #endregion

            /*
            double[][] Input = new double[4][];
            double[][] DesiredOutput = new double[4][];
            for (int i = 0; i < 4; i++)
            {
                Input[i] = new double[2];
                DesiredOutput[i] = new double[1];
            }
            Input[0][0] = 0.0; Input[0][1] = 0.0; DesiredOutput[0][0] = 0.0;
            Input[1][0] = 1.0; Input[1][1] = 0.0; DesiredOutput[1][0] = 1.0;
            Input[2][0] = 0.0; Input[2][1] = 1.0; DesiredOutput[2][0] = 1.0;
            Input[3][0] = 1.0; Input[3][1] = 1.0; DesiredOutput[3][0] = 2.0;


            double[] Output = new double[1];
            double Error = 0.0, ErrorResult = 1.0, ErrorSum = 1.0, ErrorAverage = 0.0, PreviousErrorAverage = 0.0;
            int Count = 0, Unstuck = 0;
            while (ErrorResult > 0.001)
            {
                Count++;
                Error = 0.0;
                for (int j = 0; j < 4; j++)
                {
                    Error += bp.Train(ref Input[j], ref DesiredOutput[j], 0.15, 0.1);
                    // bp.Run(ref Input[j], out Output);                
                }
                ErrorResult = Error;
                ErrorSum += Error;
                if (Count % 1000 == 0)
                {
                    //Console.WriteLine("Trial:" + Count + " Error:" + Math.Round(Error, 3));
                }
                //Compareing average error change to detect being stuck
                if (Count % 100 == 0)
                {
                    ErrorAverage = ErrorSum / 100;
                    ErrorSum = 0.0;
                    //  Console.WriteLine("ErrorChange: " + PreviousErrorAverage / ErrorAverage);

                    if (Count != 100 && (PreviousErrorAverage / ErrorAverage) < 1.001) // Only run if error is increasing on average
                    {
                        bp.Stuck(0.1);
                        Unstuck++;

                    }
                    PreviousErrorAverage = ErrorAverage;
                }

            }
            //Console.WriteLine("Finished on Trial:" + Count + " Error:" + Math.Round(Error, 3) + " TimesStuck:" + Unstuck);

            for (int i = 0; i < 4; i++)
            {
                bp.Run(ref Input[i], out Output);
                //  Console.WriteLine(Input[i][0] + " + " + Input[i][1] + " = " + Output[0]);
            }


            // Console.ReadKey();
            */
            Canvas1.Invalidate();
        }

        #region MouseControl
        private void CoreWindow_PointerMoved(Windows.UI.Core.CoreWindow sender, Windows.UI.Core.PointerEventArgs args)
        {
            MouseCords = args.CurrentPoint.Position;
        }
        private void CoreWindow_PointerPressed(Windows.UI.Core.CoreWindow sender, Windows.UI.Core.PointerEventArgs args)
        {
            if (args.CurrentPoint.Properties.IsLeftButtonPressed)
            {
                MouseDown[0] = true;
            }
            if (args.CurrentPoint.Properties.IsRightButtonPressed)
            {
                MouseDown[1] = true;
            }
        }
        private void CoreWindow_PointerReleased(Windows.UI.Core.CoreWindow sender, Windows.UI.Core.PointerEventArgs args)
        {
            MouseDown[0] = false;
            MouseDown[1] = false;
        }
        #endregion 
    }

    #region Transfer Function Setup
    public enum TransferFunction
    {
        None,
        Sigmoid,
        Linear
    }
    public static class TransferFunctions
    {
        public static double Evaluate(TransferFunction transfunc, double Input)
        {
            // switch as long as result isn't None, in which case it will output 0
            switch (transfunc)
            {
                case TransferFunction.Sigmoid:
                    return Sigmoid(Input);
                case TransferFunction.Linear:
                    return Linear(Input);

                case TransferFunction.None:
                default:
                    return 0.0;
            }
        }
        public static double EvaluateDerivative(TransferFunction transfunc, double Input)
        {
            switch (transfunc)
            {
                case TransferFunction.Sigmoid:
                    return SigmoidDerivative(Input);
                case TransferFunction.Linear:
                    return LinearDerivative(Input);
                case TransferFunction.None:
                default:
                    return 0.0;
            }
        }
        private static double Sigmoid(double x)
        {
            return 1.0 / (1.0 + Math.Exp(-x));
        }
        private static double SigmoidDerivative(double x)
        {
            return Sigmoid(x) * (1 - Sigmoid(x));
        }
        private static double Linear(double x)
        {
            return x;
        }
        private static double LinearDerivative(double x)
        {
            return 1.0;
        }
    }
    #endregion

    public class BackPropagation
    {
        public BackPropagation(int[] LayerSizes, TransferFunction[] transferfunctions)
        {

            // Network Layers
            LayerCount = LayerSizes.Length - 1;
            InputSize = LayerSizes[0];
            LayerSize = new int[LayerCount];
            transferfunction = new TransferFunction[LayerCount];
            for (int i = 0; i < LayerCount; i++)
            {
                LayerSize[i] = LayerSizes[i + 1];
                transferfunction[i] = transferfunctions[i + 1];
            }

            // Given some Dimensions up in here
            Bias = new double[LayerCount][];
            PreviousBiasDelta = new double[LayerCount][];
            Delta = new double[LayerCount][];
            LayerOutput = new double[LayerCount][];
            LayerInput = new double[LayerCount][];
            Weight = new double[LayerCount][][];
            PreviousWeightDelta = new double[LayerCount][][];

            // Filling the 2D up
            for (int i = 0; i < LayerCount; i++)
            {
                Bias[i] = new double[LayerSize[i]];
                PreviousBiasDelta[i] = new double[LayerSize[i]];
                Delta[i] = new double[LayerSize[i]];
                LayerOutput[i] = new double[LayerSize[i]];
                LayerInput[i] = new double[LayerSize[i]];
                Weight[i] = new double[i == 0 ? InputSize : LayerSize[i - 1]][];
                PreviousWeightDelta[i] = new double[i == 0 ? InputSize : LayerSize[i - 1]][];

                // Finishing up 3D
                for (int j = 0; j < (i == 0 ? InputSize : LayerSize[i - 1]); j++)
                {
                    Weight[i][j] = new double[LayerSize[i]];
                    PreviousWeightDelta[i][j] = new double[LayerSize[i]];
                }
            }
            // start up the weights
            for (int i = 0; i < LayerCount; i++)
            {
                for (int j = 0; j < LayerSize[i]; j++)
                {
                    Bias[i][j] = Gaussian.GetRandomGaussian();
                    PreviousBiasDelta[i][j] = 0.0;
                    LayerOutput[i][j] = 0.0;
                    LayerInput[i][j] = 0.0;
                    Delta[i][j] = 0.0;
                }
                for (int j = 0; j < (i == 0 ? InputSize : LayerSize[i - 1]); j++)
                {
                    for (int k = 0; k < LayerSize[i]; k++)
                    {
                        Weight[i][j][k] = Gaussian.GetRandomGaussian();
                        PreviousWeightDelta[i][j][k] = 0.0;
                    }
                }
            }

        }

        #region Methods

        public void Run(ref double[] Input, out double[] Output)
        {


            // Dimensions for the output (2D)
            Output = new double[LayerSize[LayerCount - 1]];

            //Running the network
            for (int i = 0; i < LayerCount; i++)
            {
                for (int j = 0; j < LayerSize[i]; j++)
                {
                    double Sum = 0.0;
                    for (int k = 0; k < (i == 0 ? InputSize : LayerSize[i - 1]); k++)
                    {
                        // Summation of all incoming weights to calculate output
                        Sum += Weight[i][k][j] * (i == 0 ? Input[k] : LayerOutput[i - 1][k]);
                        Sum += Bias[i][j];
                        LayerInput[i][j] = Sum;
                        LayerOutput[i][j] = TransferFunctions.Evaluate(transferfunction[i], Sum);
                    }
                }
            }

            // Syncing to the array
            for (int i = 0; i < LayerSize[LayerCount - 1]; i++)
            {
                Output[i] = LayerOutput[LayerCount - 1][i];
            }
        }

        public double Train(ref double[] Input, ref double[] Desired, double TrainingRate, double Momentum)
        {
            //check
            if (Input.Length != InputSize)
            {
                throw new ArgumentException("Input is wrong, fix something!!!");
            }
            if (Desired.Length != LayerSize[LayerCount - 1])
            {
                throw new ArgumentException("Desired is wrong, fix something!!!");
            }

            //local vars
            double Error = 0.0, Sum = 0.0, WeightDelta = 0.0, BiasDelta = 0.0;
            double[] Output = new double[LayerSize[LayerCount - 1]];

            // Runing those things you spent ALOT of time on
            Run(ref Input, out Output);

            // A little backprop
            for (int i = LayerCount - 1; i >= 0; i--)
            {

                //Output Layer
                if (i == LayerCount - 1)
                {
                    for (int j = 0; j < LayerSize[i]; j++)
                    {
                        // Finding the difference to calc the percent error
                        Delta[i][j] = Output[j] - Desired[j];
                        Error += Math.Pow(Delta[i][j], 2); // Squareing the sums results in a smooth, convex graph
                        Delta[i][j] *= TransferFunctions.EvaluateDerivative(transferfunction[i], LayerInput[i][j]);
                    }
                }
                //Hidden Layer
                else
                {
                    for (int j = 0; j < LayerSize[i]; j++)
                    {
                        Sum = 0.0;
                        for (int k = 0; k < LayerSize[i + 1]; k++)
                        {
                            Sum += Weight[i + 1][j][k] * Delta[i + 1][k];
                        }
                        Sum *= TransferFunctions.EvaluateDerivative(transferfunction[i], LayerInput[i][j]);
                        Delta[i][j] = Sum;
                    }
                }
            }
            // Update weight/bias   (l i j --> i j k)
            for (int i = 0; i < LayerCount; i++)
            {
                for (int j = 0; j < (i == 0 ? InputSize : LayerSize[i - 1]); j++)
                {
                    for (int k = 0; k < LayerSize[i]; k++)
                    {
                        WeightDelta = TrainingRate * Delta[i][k] * (i == 0 ? Input[j] : LayerOutput[i - 1][j]) + (Momentum * PreviousWeightDelta[i][j][k]);
                        Weight[i][j][k] -= WeightDelta;

                        PreviousWeightDelta[i][j][k] = WeightDelta;
                    }
                }
            }
            for (int i = 0; i < LayerCount; i++)
            {
                for (int j = 0; j < LayerSize[i]; j++)
                {
                    BiasDelta = TrainingRate * Delta[i][j];
                    Bias[i][j] -= BiasDelta + Momentum * PreviousBiasDelta[i][j];
                    PreviousBiasDelta[i][j] = BiasDelta;
                }
            }
            return Error;
        }

        public void Stuck(double Magnitude)
        {
            // adjust weights and bias when stuck
            for (int i = 0; i < LayerCount; i++)
            {
                for (int j = 0; j < LayerSize[i]; j++)
                {
                    for (int k = 0; k < (i == 0 ? InputSize : LayerSize[i - 1]); k++)
                    {

                        double w = Weight[i][k][j];
                        Weight[i][k][j] += Gaussian.GetRandomGaussian(0.0, w * Magnitude);
                        PreviousWeightDelta[i][k][j] = 0.0;

                    }

                    double b = Bias[i][j];
                    Bias[i][j] += Gaussian.GetRandomGaussian(0.0, b * Magnitude);
                    PreviousBiasDelta[i][j] = 0.0;

                }

            }

        }
        #endregion

        #region Giving Dimensions
        // Private data to give an idea about the dimensions of the NN
        private int LayerCount;
        private int InputSize;
        private int[] LayerSize;
        private TransferFunction[] transferfunction;
        // 2D arrays
        private double[][] LayerOutput;
        private double[][] LayerInput;
        private double[][] Bias;
        private double[][] Delta;
        private double[][] PreviousBiasDelta;
        // 3D arrays
        private double[][][] Weight;
        private double[][][] PreviousWeightDelta;
        #endregion
    }
    public static class Gaussian //Get equally balanced randoms with the Gaussian Curve function
    {
        private static Random genorator = new Random();
        public static double GetRandomGaussian()
        {
            return GetRandomGaussian(0.0, 1.0);
        }
        public static double GetRandomGaussian(double mean, double stddeviation)
        {
            double rval1, rval2;

            GetRandomGaussian(mean, stddeviation, out rval1, out rval2);
            return rval1;
        }
        public static void GetRandomGaussian(double mean, double stddeviation, out double val1, out double val2)
        {
            double u = 0, v = 0, s, t;

            while (u * u + v * v > 1 || (u == 0 && v == 0))
            {
                u = 2 * genorator.NextDouble() - 1;
                v = 2 * genorator.NextDouble() - 1;
            }
            s = u * u + v * v;
            t = Math.Sqrt((-2.0 * Math.Log(s))) / s;
            val1 = stddeviation * u * t + mean;
            val2 = stddeviation * v * t + mean;
        }
    }





}

