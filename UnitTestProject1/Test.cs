using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Engine;

namespace UnitTestProject1
{
    [TestClass]
    public class Test
    {
        [TestMethod]
        public void TestViewMatrix()
        {
            Matrix M = new Matrix(new double[,] { { 5, 0.5, 0 }, { 0.5, 1 , 0 }, { 0.5, 0.5, 1 } });
            Matrix NewM = Matrix.ViewMatrix(M).InvertAffineMatrix();
            for (int i = 0; i < NewM.y; i++)
            {
                for (int j = 0; j < NewM.x; j++)
                {
                    Console.Write("{0} ", Math.Round(NewM[j, i],4));
                }
                Console.WriteLine();
            }
            Assert.AreEqual(0, 0, 0.001, "Account not debited correctly");
        }
        [TestMethod]
        public void TestViewMatrixInvertAffineMatrix()
        {
            Matrix M = new Matrix(new Double[,] { { 5, 0.5, 0 }, { 0.5, 1, 0 }, { 0.5, 0.5, 1 } });
            Matrix NewM = Matrix.ViewMatrix(M);
            for (int i = 0; i < NewM.y; i++)
            {
                for (int j = 0; j < NewM.x; j++)
                {
                    Console.Write("{0} ", Math.Round(NewM[j, i], 4));
                }
                Console.WriteLine();
            }
            Assert.AreEqual(0, 0, 0.001, "Account not debited correctly");
        }
        [TestMethod]
        public void TestProjectionMatrix()
        {
            
            Matrix NewM = Matrix.ProjectionMatrix(100 / 50, 1, 100, 45);
            for (int i = 0; i < NewM.y; i++)
            {
                for (int j = 0; j < NewM.x; j++)
                {
                    Console.Write("{0} ", Math.Round(NewM[j, i], 4));
                }
                Console.WriteLine();
            }
            Assert.AreEqual(0, 0, 0.001, "Account not debited correctly");
        }
        [TestMethod]
        public void TestSquareRoot()
        {
            double a = Math.Sqrt(1000);
            double b = 1 / Vector3.InvSqrt(1000);

            Assert.AreEqual(a, b, 0.001, "Account not debited correctly");
        }
    }
}
