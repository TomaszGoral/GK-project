using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine
{   
    public class Vector3
    {
        
        public double first, second, third;
        public Vector3(double a, double b, double c)
        {

            first = a;
            second = b;
            third = c;
        }
        public Vector3()
        {

            first = 0;
            second = 0;
            third = 0;
        }
        public static Vector3 CrossProduct(Vector3 a, Vector3 b)
        {
            return new Vector3(a.second * b.third - a.third * b.second, a.third * b.first - a.first * b.third, a.first * b.second - a.second * b.first);
        }
        public static double DotProduct(Vector3 a, Vector3 b)
        {
            return a.first * b.first + a.second * b.second + a.third * b.third;
        }
        public static double DotProduct(Vector3 ver, Vector3 normal, Vector3 light)
        {
            Vector3 lightDirection = light - ver;
            return Math.Max(0,DotProduct(normal, lightDirection.ToVersor()));
        }
        unsafe public static double InvSqrt(double number)
        {
            double y = number;
            double x2 = y * 0.5;
            Int64 i = *(Int64*)&y;
            // The magic number is for doubles is from https://cs.uwaterloo.ca/~m32rober/rsqrt.pdf
            i = 0x5fe6eb50c7b537a9 - (i >> 1);
            y = *(double*)&i;
            y = y * (1.5 - (x2 * y * y));   // 1st iteration
                                            //      y  = y * ( 1.5 - ( x2 * y * y ) );   // 2nd iteration, this can be removed
            return y;
        }

        public Vector3 ToVersor()
        {
            double a = this.first, b = this.second, c = this.third;
           // double tmp = InvSqrt(a * a + b * b + c * c);

            double tmp = Math.Sqrt(a * a + b * b + c * c);
            //return tmp!=0?new Vector3(this[0] * tmp, this[1] * tmp, this[2] * tmp): new Vector3(0, 0, 0);
            
            if (tmp != 0) return new Vector3(a / tmp, b / tmp, c / tmp);
            //if (tmp != 0) return new Vector3(a * tmp, b * tmp, c * tmp);
            else return new Vector3(0, 0, 0);
        }
        public static Vector3 operator -(Vector3 a, Vector3 b)
        {
            return new Vector3(a.first - b.first, a.second - b.second, a.third - b.third);
        }
        public static Vector3 operator +(Vector3 a, Vector3 b)
        {
            return new Vector3(a.first + b.first, a.second + b.second, a.third + b.third);
        }
        public static Vector3 operator *(double a, Vector3 b)
        {
            return new Vector3(a * b.first, a * b.second, a * b.third);
        }
        public static Vector3 operator /(Vector3 a, double b)
        {
            return new Vector3(a.first / b, a.second / b, a.third / b);
        }
        public static Vector3 operator *(int a, Vector3 b)
        {
            return new Vector3(a * b.first, a * b.second, a * b.third);
        }
        
        public Vector3 Scale(Vector3 v)
        {
            return new Vector3(this.first * v.first, this.second * v.second, this.third * v.third);
        }
        public Vector3 Rotation(Vector3 v)
        {
            double px = this.first, py = this.second, pz = this.third;
            double px1 = px,py1 = Math.Cos(v.first) * py - Math.Sin(v.first) * pz, pz1 = Math.Sin(v.first) * py + Math.Cos(v.first) * pz;
            double px2 = Math.Cos(v.second) * px1 + Math.Sin(v.second) * pz1, py2 = py1, pz2 = -Math.Sin(v.second) * px1 + Math.Cos(v.second) * pz1;
            return new Vector3(Math.Cos(v.third) * px2 - Math.Sin(v.third) * py2, Math.Sin(v.third) * px2 + Math.Cos(v.third) * py2, pz2);
        }
        public Vector3 Translation(Vector3 v)
        {
            return new Vector3(this.first + v.first, this.second + v.second, this.third + v.third);
        }
        public double this[int key]
        {
            get
            {
                switch (key)
                {
                    case 0:
                        return first;
                    case 1:
                        return second;
                    case 2:
                        return third;
                }
                return 0;
            }

        }
    }
    
    public class Matrix
    {
        public int x, y;
        double[,] matrix;
        public Matrix(int _x, int _y)
        {
            x = _x;
            y = _y;
            matrix = new double[x,y];
        }
        public Matrix(double[,] m)
        {
            matrix = m;
            x = m.GetLength(0);
            y = m.GetLength(1);
        }
        public double this[int keyx,int keyy]{
            get{ return matrix[keyx,keyy]; }
            set{ matrix[keyx,keyy] = value; }
        }
        public static Matrix MultiplyMatrix(Matrix a, Matrix b)
        {
            if (a.x == b.y)
            {
                Matrix c = new Matrix(b.x, a.y);
                for (int i = 0; i < a.y; i++)
                {
                    for (int j = 0; j < b.x; j++)
                    {
                        c[j, i] = 0;
                        for (int k = 0; k < a.x; k++)
                        {
                            c[j, i] += a[k, i] * b[j, k];
                        }
                    }
                }
                return c;
            }
            else return null;
        }
        
        public Matrix InvertAffineMatrix()
        {
            double InvDetA = 1/(this[0,0] * this[1,1] * this[2,2] + this[0,1] * this[1,2] * this[2,0] + this[0,2] * this[1,0] * this[2,1] - this[0,0] * this[1,2] * this[2,1] - this[0,2] * this[1,1] * this[2,0] - this[0,1] * this[1,0] * this[2,2]);
            Matrix InvA = new Matrix(new double[,]{
                { InvDetA*(this[1,1] * this[2,2] - this[2,1] * this[1,2]), InvDetA*(this[2,1] * this[0,2] - this[0,1] * this[2,2]), InvDetA*(this[0,1] * this[1,2] - this[1,1] * this[0,2]) },
                { InvDetA*(this[2,0] * this[1,2] - this[1,0] * this[2,2]), InvDetA*(this[0,0] * this[2,2] - this[2,0] * this[0,2]), InvDetA*(this[1,0] * this[0,2] - this[0,0] * this[1,2]) },
                { InvDetA*(this[1,0] * this[2,1] - this[2,0] * this[1,1]), InvDetA*(this[2,0] * this[0,1] - this[0,0] * this[2,1]), InvDetA*(this[0,0] * this[1,1] - this[1,0] * this[0,1]) }
            });
            Vector3 v = new Vector3(
                -InvA[0, 0] * this[3, 0] - InvA[1, 0] * this[3, 1] - InvA[2, 0] * this[3, 2],
                -InvA[0, 1] * this[3, 0] - InvA[1, 1] * this[3, 1] - InvA[2, 1] * this[3, 2],
                -InvA[0, 2] * this[3, 0] - InvA[1, 2] * this[3, 1] - InvA[2, 2] * this[3, 2]
                );
            return new Matrix(new double[,]{
                { InvDetA*(this[1,1] * this[2,2] - this[2,1] * this[1,2]), InvDetA*(this[2,1] * this[0,2] - this[0,1] * this[2,2]), InvDetA*(this[0,1] * this[1,2] - this[1,1] * this[0,2]), 0 },
                { InvDetA*(this[2,0] * this[1,2] - this[1,0] * this[2,2]), InvDetA*(this[0,0] * this[2,2] - this[2,0] * this[0,2]), InvDetA*(this[1,0] * this[0,2] - this[0,0] * this[1,2]), 0 },
                { InvDetA*(this[1,0] * this[2,1] - this[2,0] * this[1,1]), InvDetA*(this[2,0] * this[0,1] - this[0,0] * this[2,1]), InvDetA*(this[0,0] * this[1,1] - this[1,0] * this[0,1]), 0 },
                { v.first, v.second, v.third, 1 }
            });
        }
        public static Matrix ViewMatrix(Matrix M)
        {
            Vector3 CameraPosition = new Vector3(M[0, 0], M[1, 0], M[2, 0]);
            Vector3 CameraTarget = new Vector3(M[0, 1], M[1, 1], M[2, 1]);
            Vector3 UpVector = new Vector3(M[0, 2], M[1, 2], M[2, 2]);
            UpVector = UpVector.ToVersor();
            Vector3 ZAxis = CameraPosition - CameraTarget;
            ZAxis = ZAxis.ToVersor();
            Vector3 XAxis = Vector3.CrossProduct(UpVector,ZAxis);
            XAxis = XAxis.ToVersor();
            Vector3 YAxis = Vector3.CrossProduct(ZAxis, XAxis);
            return new Matrix(new double[,] { { XAxis.first, XAxis.second, XAxis.third, 0 }, { YAxis.first, YAxis.second, YAxis.third, 0 }, { -ZAxis.first, -ZAxis.second, -ZAxis.third, 0 }, { -Vector3.DotProduct(XAxis, CameraPosition), -Vector3.DotProduct(YAxis, CameraPosition), Vector3.DotProduct(ZAxis, CameraPosition), 1 } });  
        }
        public static Matrix ProjectionMatrix(double a, double n, double f, double FOV)
        {
            double e = 1 / Math.Tan(FOV*Math.PI/360);
            return new Matrix(new double[,] { { e, 0, 0, 0 }, { 0, e/a, 0, 0 }, { 0, 0, -(f + n) / (f - n), -1 }, { 0, 0, -2 * (f * n) / (f - n), 0 } });
        }
    }
}
