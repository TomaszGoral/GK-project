using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace Engine
{
    class Scene
    {
        public bool isphong = false;
        public bool isblinn = false;
        public Vector3 camera;
        byte[] backBuffer;
        double[] depthBuffer;
        private object[] lockBuffer;
        WriteableBitmap bmp;
        int bmpWidth;
        int bmpHeight;
        double bmpWidthDouble;
        double bmpHeightDouble;
        public Scene(WriteableBitmap bmp)
        {
            this.bmp = bmp;
            backBuffer = new byte[bmp.PixelWidth * bmp.PixelHeight * 4];
            depthBuffer = new double[bmp.PixelWidth * bmp.PixelHeight];
            bmpWidth = bmp.PixelWidth;
            bmpHeight = bmp.PixelHeight;
            bmpWidthDouble = bmp.Width;
            bmpHeightDouble = bmp.Height;
            lockBuffer = new object[bmpWidth * bmpHeight];
            for (var i = 0; i < lockBuffer.Length; i++)
            {
                lockBuffer[i] = new object();
            }
        }
        public void Present()
        {
            bmp.WritePixels(
            new System.Windows.Int32Rect(0, 0, bmpWidth, bmpHeight),
            backBuffer, bmpWidth * 4, 0);

        }
        public void Clear(byte r, byte g, byte b, byte a)
        {
            for (var index = 0; index < backBuffer.Length; index += 4)
            {
                backBuffer[index] = b;
                backBuffer[index + 1] = g;
                backBuffer[index + 2] = r;
                backBuffer[index + 3] = a;
            }
            for (var index = 0; index < depthBuffer.Length; index++)
            {
                depthBuffer[index] = -double.MaxValue;
            }
        }
        public Vertex Project(Vertex coord, Matrix TransformMatrix)
        {
            Matrix point = Matrix.MultiplyMatrix(TransformMatrix, new Matrix(new double[,] { { coord.Coordinates.first, coord.Coordinates.second, coord.Coordinates.third, 1 } }));
            Vector3 pointWorld = coord.Coordinates;
            Vector3 normalWorld = coord.Normal;
            point[0, 0] = point[0, 0] / point[0, 3];
            point[0, 1] = point[0, 1] / point[0, 3];
            point[0, 2] = point[0, 2] / point[0, 3];
            //point[0, 3] = point[0, 3] / point[0, 3];
            point[0, 3] = 1;
            var x = (point[0, 0] * bmpWidth + bmpWidth) / 2.0;
            var y = (-point[0, 1] * bmpHeight + bmpHeight) / 2.0;
            return new Vertex
            {
                Coordinates = new Vector3(x, y, point[0,2]),
                Normal = normalWorld,
                WorldCoordinates = pointWorld
            };
        }
        public void Render(Matrix camera, params Obj3D[] objects)
        {
            List<Light[]> listlights = new List<Light[]>();
            foreach (Obj3D o in objects)
            {
                if (o.Lights != null)
                {
                    Light[] lights = new Light[o.Lights.Length];

                    for (int i = 0; i < o.Lights.Length; i++)
                    {
                        lights[i] = new Light();
                        lights[i].Coordinates = o.Lights[i].Coordinates.Rotation(o.Rotation).Translation(o.Position);
                        lights[i].Color = o.Lights[i].Color;
                        lights[i].Normal = o.Lights[i].Normal.Rotation(o.Rotation);
                        lights[i].angle = o.Lights[i].angle;
                    }
                    listlights.Add(lights);
                }
            }
            //Vector3 cam = new Vector3(camera[0,0], camera[1, 0], camera[2, 0]).Rotation(objects[0].Rotation).Translation(objects[0].Position);
            //Matrix newcamera = camera;
            //newcamera[0, 0] = cam.first;
            //newcamera[1, 0] = cam.second;
            //newcamera[2, 0] = cam.third;
            //camera = newcamera
            Matrix ViewMatrix = Matrix.ViewMatrix(camera);
            Matrix ProjectionMatrix = Matrix.ProjectionMatrix(bmpHeightDouble/bmpWidthDouble,1,100,45);
            foreach (Obj3D o in objects)
            {


                Matrix TransformMatrix = Matrix.MultiplyMatrix(ProjectionMatrix, ViewMatrix);
                Color4 color = new Color4(1, 1, 1, 1);
                

                    
              
                Parallel.ForEach(o.Triangles,triangle=>
                {
                    Vertex vA = new Vertex { Coordinates = o.Vertices[triangle.VerA].Coordinates.Rotation(o.Rotation).Translation(o.Position), Normal = o.Vertices[triangle.VerA].Normal.Rotation(o.Rotation) };
                    Vertex vB = new Vertex { Coordinates = o.Vertices[triangle.VerB].Coordinates.Rotation(o.Rotation).Translation(o.Position), Normal = o.Vertices[triangle.VerB].Normal.Rotation(o.Rotation) };
                    Vertex vC = new Vertex { Coordinates = o.Vertices[triangle.VerC].Coordinates.Rotation(o.Rotation).Translation(o.Position), Normal = o.Vertices[triangle.VerC].Normal.Rotation(o.Rotation) };
                    DrawTriangle(Project(vA, TransformMatrix), Project(vB, TransformMatrix), Project(vC, TransformMatrix) , color, listlights);
                    
                });

            }
        }
        public double[] Interpolate(double to, double from, int length)
        {
            double d = (to - from) / length;
            double[] tab = new double[length];
            for(int i = 0; i < tab.Length;i++)
            {
                tab[i] = from + i * d;
            }
            return tab;
        }
        public double Ia(int colorNumber,Color4 color, double ka)
        {
            switch (colorNumber){
                case 0:
                    return color.Red * ka;
                case 1:
                    return color.Green * ka;
                case 2:
                    return color.Blue * ka;
            }
            return 0;
        }
        public double IdAtPoint(int colorNumber, Vector3 reflectedLight, double kd, Vector3 Normal, Vector3 WorldCoordinates, Vector3 lightSource, out double dot)
        {
            dot = Vector3.DotProduct(WorldCoordinates, Normal, lightSource);
            return Math.Max(reflectedLight[colorNumber] * kd * dot,0);
        }
        public double IsAtPoint(int colorNumber, double ks, Vector3 Normal, Vector3 WorldCoordinates, Vector3 lightSource, Vector3 lightColor, Vector3 camera, int n)
        {
            Vector3 viewDirectionAtPoint = camera - WorldCoordinates;
            
            if (isblinn)
            {

                Vector3 LV = lightSource - WorldCoordinates + viewDirectionAtPoint;
                Vector3 H = LV / Math.Sqrt(LV.first * LV.first + LV.second * LV.second + LV.third * LV.third);
                return lightColor[colorNumber] * ks * Math.Pow(Math.Max(Vector3.DotProduct(Normal, H.ToVersor()), 0), n);
            }
            else {
            Vector3 lightDirectionAtPoint = (lightSource - WorldCoordinates).ToVersor();
            double NL = Vector3.DotProduct(lightDirectionAtPoint, Normal);
            Vector3 R = ((2 * NL) * Normal) - lightDirectionAtPoint;
            
            return lightColor[colorNumber] * ks * Math.Pow(Math.Max(Vector3.DotProduct(R.ToVersor(), viewDirectionAtPoint.ToVersor()), 0), n);
            }
        }
        public double ColorIluminationAtPoint(int colorNumber, Color4 color, double ka, double kd, double ks, Vertex point, Light[] lightData, Vector3 camera, int n)
        {
            
            double Iaval = Ia(colorNumber, color, ka);
            double returnedValue = Ia(colorNumber, color, ka);
            Vector3 pointNormal = point.Normal.ToVersor();
            double dot;
            foreach (Light light in lightData)
            {
                if (Vector3.DotProduct(light.Coordinates,light.Normal.ToVersor(),point.WorldCoordinates) >=light.angle) {
                    Vector3 reflectedLight = new Vector3(Math.Max(0, light.Color.first - 1 + color.Red), Math.Max(0, light.Color.second + color.Green - 1), Math.Max(0, light.Color.third + color.Blue - 1));
                    returnedValue += IdAtPoint(colorNumber, reflectedLight, kd, pointNormal, point.WorldCoordinates, light.Coordinates, out dot);
                    if (dot > 0) returnedValue += IsAtPoint(colorNumber, ks, pointNormal, point.WorldCoordinates, light.Coordinates, reflectedLight, camera, n);
                }
            }
            return Math.Min(returnedValue,1);
        }
        public void DrawTriangle(Vertex pointA, Vertex pointB, Vertex pointC, Color4 color, List<Light[]> lights)
        {
            Vertex tmp = new Vertex();

            if (pointA.Coordinates.second < pointB.Coordinates.second)
            {
                tmp = pointA;
                pointA = pointB;
                pointB = tmp;
            }
            if (pointA.Coordinates.second < pointC.Coordinates.second)
            {
                tmp = pointA;
                pointA = pointC;
                pointC = tmp;
            }
            if (pointB.Coordinates.second < pointC.Coordinates.second)
            {
                tmp = pointB;
                pointB = pointC;
                pointC = tmp;
            }
            double d= (pointB.Coordinates.first - pointA.Coordinates.first) *(pointC.Coordinates.second - pointA.Coordinates.second) -(pointB.Coordinates.second - pointA.Coordinates.second) *(pointC.Coordinates.first - pointA.Coordinates.first);
            int lightscount = 0;
            foreach(Light[] l in lights)
            {
                lightscount += l.Length;
            }
            Light[] lightSources = new Light[1+lightscount];
            lightSources[0] = new Light
            {
                //lightSources[1] = new Vector3[2];
                //lightSources[2] = new Vector3[2];
                Coordinates = new Vector3(10, 0, 10),
                Color = new Vector3(1, 0, 0),
                Normal = new Vector3(0, 0, -1),
                angle = 0
            };
            //lightSources[1][0] = new Vector3(0, 5, 0);
            //lightSources[1][1] = new Vector3(1, 1, 1);
            // lightSources[2][0] = new Vector3(0, 5, 5);
            // lightSources[2][1] = new Vector3(1, 1, 1);
            int index = 0;
            foreach (Light[] li in lights)
            {
                foreach (Light l in li)
                {
                    lightSources[1 + index] = new Light
                    {
                        Coordinates = l.Coordinates,
                        angle = l.angle,
                        Normal = l.Normal,
                        Color = l.Color
                    };
                    index++;
                }
            }


            int[] xtabAC = BresenhamLine_2_0((int)pointA.Coordinates.first, (int)pointA.Coordinates.second, (int)pointC.Coordinates.first, (int)pointC.Coordinates.second, color, false);
            int[] xtabAB = BresenhamLine_2_0((int)pointA.Coordinates.first, (int)pointA.Coordinates.second, (int)pointB.Coordinates.first, (int)pointB.Coordinates.second, color, false);
            int[] xtabBC = BresenhamLine_2_0((int)pointB.Coordinates.first, (int)pointB.Coordinates.second, (int)pointC.Coordinates.first, (int)pointC.Coordinates.second, color, false);
            int i = 0, j = 1;
            double IrA = 0;
            double IgA = 0;
            double IbA = 0;
            double IrB = 0;
            double IgB = 0;
            double IbB = 0;
            double IrC = 0;
            double IgC = 0;
            double IbC = 0;
            bool phong = isphong;
            int n = 40;
            double ka = 0.1, kd = 0.8, ks = 0.5;
            
                //Vector3 lightSource = lightSources[1][0];
                //Vector3 lightColor = lightSources[1][1];
            //Vector3 V = new Vector3(pointB.WorldCoordinates.first - pointA.WorldCoordinates.first, pointB.WorldCoordinates.second - pointA.WorldCoordinates.second, pointB.WorldCoordinates.third - pointA.WorldCoordinates.third);
            //Vector3 U = new Vector3(pointC.WorldCoordinates.first - pointA.WorldCoordinates.first, pointC.WorldCoordinates.second - pointA.WorldCoordinates.second, pointC.WorldCoordinates.third - pointA.WorldCoordinates.third);
            //Vector3 normalTriangle = new Vector3(U.second*V.third-U.third*V.second, U.third * V.first - U.first * V.third, U.first * V.second - U.second * V.first);
            //Vector3 normalTriangle = Vector3.CrossProduct(U, V);
            //Vector3 normalTriangle1 = new Vector3((pointA.Normal.first + pointB.Normal.first + pointC.Normal.first) / 3, (pointA.Normal.second + pointB.Normal.second + pointC.Normal.second) / 3, (pointA.Normal.third + pointB.Normal.third + pointC.Normal.third) / 3);
            //Vector3 centerPointTriangle = new Vector3((pointA.WorldCoordinates.first + pointB.WorldCoordinates.first + pointC.WorldCoordinates.first) / 3.0, (pointA.WorldCoordinates.second + pointB.WorldCoordinates.second + pointC.WorldCoordinates.second) / 3.0, (pointA.WorldCoordinates.third + pointB.WorldCoordinates.third + pointC.WorldCoordinates.third) / 3.0);
            //double dotA = Vector3.DotProduct(pointA.WorldCoordinates, pointA.Normal, lightSource);
            //double dotB = Vector3.DotProduct(pointB.WorldCoordinates, pointB.Normal, lightSource);
            //double dotC = Vector3.DotProduct(pointC.WorldCoordinates, pointC.Normal, lightSource);
            //double dot = Vector3.DotProduct(centerPointTriangle, normalTriangle1, lightSource);
            //if (normalTriangle.first != normalTriangle1.first || normalTriangle.second != normalTriangle1.second || normalTriangle.third != normalTriangle1.third) throw new Exception("elo");
            //color = new Color4(1 * dot, 1 * dot, 1 * dot, 1);




            

                

                IrA += ColorIluminationAtPoint(0, color, ka, kd, ks, pointA, lightSources, camera, n);
                IgA += ColorIluminationAtPoint(1, color, ka, kd, ks, pointA, lightSources, camera, n);
                IbA += ColorIluminationAtPoint(2, color, ka, kd, ks, pointA, lightSources, camera, n);
                IrB += ColorIluminationAtPoint(0, color, ka, kd, ks, pointB, lightSources, camera, n);
                IgB += ColorIluminationAtPoint(1, color, ka, kd, ks, pointB, lightSources, camera, n);
                IbB += ColorIluminationAtPoint(2, color, ka, kd, ks, pointB, lightSources, camera, n);
                IrC += ColorIluminationAtPoint(0, color, ka, kd, ks, pointC, lightSources, camera, n);
                IgC += ColorIluminationAtPoint(1, color, ka, kd, ks, pointC, lightSources, camera, n);
                IbC += ColorIluminationAtPoint(2, color, ka, kd, ks, pointC, lightSources, camera, n);
            
            double[] ACNormalx = new double[1]; double[] ACNormaly = new double[1]; double[] ACNormalz = new double[1];
            double[] ABNormalx = new double[1]; double[] ABNormaly = new double[1]; double[] ABNormalz = new double[1];
            double[] BCNormalx = new double[1]; double[] BCNormaly = new double[1]; double[] BCNormalz = new double[1];

            double[] ACWorldx = new double[1]; double[] ACWorldy = new double[1]; double[] ACWorldz = new double[1];
            double[] ABWorldx = new double[1]; double[] ABWorldy = new double[1]; double[] ABWorldz = new double[1];
            double[] BCWorldx = new double[1]; double[] BCWorldy = new double[1]; double[] BCWorldz = new double[1];
            if (phong) {
                ACNormalx = Interpolate(pointC.Normal.first, pointA.Normal.first, xtabAC.Length);
                ACNormaly = Interpolate(pointC.Normal.second, pointA.Normal.second, xtabAC.Length);
                ACNormalz = Interpolate(pointC.Normal.third, pointA.Normal.third, xtabAC.Length);
                ACWorldx = Interpolate(pointC.WorldCoordinates.first, pointA.WorldCoordinates.first, xtabAC.Length);
                ACWorldy = Interpolate(pointC.WorldCoordinates.second, pointA.WorldCoordinates.second, xtabAC.Length);
                ACWorldz = Interpolate(pointC.WorldCoordinates.third, pointA.WorldCoordinates.third, xtabAC.Length);
            }
            double[] axisACZ = Interpolate(pointC.Coordinates.third, pointA.Coordinates.third, xtabAC.Length);
            double[] ACColorR = Interpolate(IrC, IrA, xtabAC.Length);
            double[] ACColorG = Interpolate(IgC, IgA, xtabAC.Length);
            double[] ACColorB = Interpolate(IbC, IbA, xtabAC.Length);
            if (phong)
            {
                ABNormalx = Interpolate(pointB.Normal.first, pointA.Normal.first, xtabAB.Length);
                ABNormaly = Interpolate(pointB.Normal.second, pointA.Normal.second, xtabAB.Length);
                ABNormalz = Interpolate(pointB.Normal.third, pointA.Normal.third, xtabAB.Length);
                ABWorldx = Interpolate(pointB.WorldCoordinates.first, pointA.WorldCoordinates.first, xtabAB.Length);
                ABWorldy = Interpolate(pointB.WorldCoordinates.second, pointA.WorldCoordinates.second, xtabAB.Length);
                ABWorldz = Interpolate(pointB.WorldCoordinates.third, pointA.WorldCoordinates.third, xtabAB.Length);
            }
            double[] axisABZ = Interpolate(pointB.Coordinates.third, pointA.Coordinates.third, xtabAB.Length);
            double[] ABColorR = Interpolate(IrB, IrA, xtabAB.Length);
            double[] ABColorG = Interpolate(IgB, IgA, xtabAB.Length);
            double[] ABColorB = Interpolate(IbB, IbA, xtabAB.Length);
            if (phong)
            {
                BCNormalx = Interpolate(pointC.Normal.first, pointB.Normal.first, xtabBC.Length);
                BCNormaly = Interpolate(pointC.Normal.second, pointB.Normal.second, xtabBC.Length);
                BCNormalz = Interpolate(pointC.Normal.third, pointB.Normal.third, xtabBC.Length);
                BCWorldx = Interpolate(pointC.WorldCoordinates.first, pointB.WorldCoordinates.first, xtabBC.Length);
                BCWorldy = Interpolate(pointC.WorldCoordinates.second, pointB.WorldCoordinates.second, xtabBC.Length);
                BCWorldz = Interpolate(pointC.WorldCoordinates.third, pointB.WorldCoordinates.third, xtabBC.Length);
            }
            double[] axisBCZ = Interpolate(pointC.Coordinates.third, pointB.Coordinates.third, xtabBC.Length);
            double[] BCColorR = Interpolate(IrC, IrB, xtabBC.Length);
            double[] BCColorG = Interpolate(IgC, IgB, xtabBC.Length);
            double[] BCColorB = Interpolate(IbC, IbB, xtabBC.Length);

            double[] YXNormalx = new double[1];
            double[] YXNormaly = new double[1];
            double[] YXNormalz = new double[1];
            double[] YXWorldx = new double[1];
            double[] YXWorldy = new double[1];
            double[] YXWorldz = new double[1];
            
            //throw new Exception("elo1");
            if (d<0)
            {
                
                for (int y = (int)pointA.Coordinates.second; y >= (int)pointC.Coordinates.second; y--)
                {

                    if (y >= (int)pointB.Coordinates.second)
                    {
                        int length = Math.Abs(xtabAB[i] - xtabAC[i]) + 1;
                        double[] Ir = new double[length];
                        double[] Ig = new double[length];
                        double[] Ib = new double[length];
                        double[] axisYX = Interpolate(axisABZ[i], axisACZ[i], length);
                        if (phong)
                        {
                            YXNormalx = Interpolate(ABNormalx[i], ACNormalx[i], length);
                            YXNormaly = Interpolate(ABNormaly[i], ACNormaly[i], length);
                            YXNormalz = Interpolate(ABNormalz[i], ACNormalz[i], length);
                            YXWorldx = Interpolate(ABWorldx[i], ACWorldx[i], length);
                            YXWorldy = Interpolate(ABWorldy[i], ACWorldy[i], length);
                            YXWorldz = Interpolate(ABWorldz[i], ACWorldz[i], length);
                            for(int g=0; g < length; g++)
                            {
                                Vertex point = new Vertex { Coordinates = new Vector3(xtabAC[i] + g, y, axisYX[g]), Normal = new Vector3(YXNormalx[g], YXNormaly[g], YXNormalz[g]), WorldCoordinates = new Vector3(YXWorldx[g], YXWorldy[g], YXWorldz[g]) };
                                if (point.Coordinates.third >= depthBuffer[(int)(point.Coordinates.first + point.Coordinates.second * bmpWidth)])
                                {
                                    Ir[g] = ColorIluminationAtPoint(0, color, ka, kd, ks, point, lightSources, camera, n);
                                    Ig[g] = ColorIluminationAtPoint(1, color, ka, kd, ks, point, lightSources, camera, n);
                                    Ib[g] = ColorIluminationAtPoint(2, color, ka, kd, ks, point, lightSources, camera, n);
                                }
                            }
                        }
                                            
                        
                        double[] YXColorR = Interpolate(ABColorR[i], ACColorR[i], length);
                        double[] YXColorG = Interpolate(ABColorG[i], ACColorG[i], length);
                        double[] YXColorB = Interpolate(ABColorB[i], ACColorB[i], length);
                        
                        ProcessScanLine(y, xtabAC[i], xtabAB[i], axisYX, color, YXColorR, YXColorG, YXColorB, Ir, Ig, Ib, phong);
                        i++;
                    }
                    else
                    {

                        int length = Math.Abs(xtabBC[j] - xtabAC[i]) + 1;
                        double[] Ir = new double[length];
                        double[] Ig = new double[length];
                        double[] Ib = new double[length];
                        double[] axisYX = Interpolate(axisBCZ[j], axisACZ[i], length);
                        if (phong)
                        {
                            YXNormalx = Interpolate(BCNormalx[j], ACNormalx[i], length);
                            YXNormaly = Interpolate(BCNormaly[j], ACNormaly[i], length);
                            YXNormalz = Interpolate(BCNormalz[j], ACNormalz[i], length);
                            YXWorldx = Interpolate(BCWorldx[j], ACWorldx[i], length);
                            YXWorldy = Interpolate(BCWorldy[j], ACWorldy[i], length);
                            YXWorldz = Interpolate(BCWorldz[j], ACWorldz[i], length);
                            for (int g = 0; g < length; g++)
                            {
                                Vertex point = new Vertex { Coordinates = new Vector3(xtabAC[i] + g, y, axisYX[g]), Normal = new Vector3(YXNormalx[g], YXNormaly[g], YXNormalz[g]), WorldCoordinates = new Vector3(YXWorldx[g], YXWorldy[g], YXWorldz[g]) };
                                if (point.Coordinates.third >= depthBuffer[(int)(point.Coordinates.first + point.Coordinates.second * bmpWidth)])
                                {
                                    Ir[g] = ColorIluminationAtPoint(0, color, ka, kd, ks, point, lightSources, camera, n);
                                    Ig[g] = ColorIluminationAtPoint(1, color, ka, kd, ks, point, lightSources, camera, n);
                                    Ib[g] = ColorIluminationAtPoint(2, color, ka, kd, ks, point, lightSources, camera, n);
                                }
                            }
                        }

                        
                        double[] YXColorR = Interpolate(BCColorR[j], ACColorR[i], length);
                        double[] YXColorG = Interpolate(BCColorG[j], ACColorG[i], length);
                        double[] YXColorB = Interpolate(BCColorB[j], ACColorB[i], length);

                        ProcessScanLine(y, xtabAC[i], xtabBC[j], axisYX, color, YXColorR, YXColorG, YXColorB, Ir, Ig, Ib, phong);
                        j++; i++;
                    }
                }
            }
            else
            {
                
                for (int y = (int)pointA.Coordinates.second; y >= (int)pointC.Coordinates.second; y--)
                {
                    if (y >= (int)pointB.Coordinates.second)
                    {
                        int length = Math.Abs(xtabAC[i] - xtabAB[i]) + 1;
                        double[] Ir = new double[length];
                        double[] Ig = new double[length];
                        double[] Ib = new double[length];
                        double[] axisYX = Interpolate(axisACZ[i], axisABZ[i], length);
                        if (phong)
                        {
                            YXNormalx = Interpolate(ACNormalx[i], ABNormalx[i], length);
                            YXNormaly = Interpolate(ACNormaly[i], ABNormaly[i], length);
                            YXNormalz = Interpolate(ACNormalz[i], ABNormalz[i], length);
                            YXWorldx = Interpolate(ACWorldx[i], ABWorldx[i], length);
                            YXWorldy = Interpolate(ACWorldy[i], ABWorldy[i], length);
                            YXWorldz = Interpolate(ACWorldz[i], ABWorldz[i], length);
                            for (int g = 0; g < length; g++)
                            {
                                Vertex point = new Vertex { Coordinates = new Vector3(xtabAB[i] + g, y, axisYX[g]), Normal = new Vector3(YXNormalx[g], YXNormaly[g], YXNormalz[g]), WorldCoordinates = new Vector3(YXWorldx[g], YXWorldy[g], YXWorldz[g]) };
                                if (point.Coordinates.third >= depthBuffer[(int)(point.Coordinates.first + point.Coordinates.second * bmpWidth)])
                                {
                                    Ir[g] = ColorIluminationAtPoint(0, color, ka, kd, ks, point, lightSources, camera, n);
                                    Ig[g] = ColorIluminationAtPoint(1, color, ka, kd, ks, point, lightSources, camera, n);
                                    Ib[g] = ColorIluminationAtPoint(2, color, ka, kd, ks, point, lightSources, camera, n);
                                }
                            }
                        }

                       
                        double[] YXColorR = Interpolate(ACColorR[i], ABColorR[i], length);
                        double[] YXColorG = Interpolate(ACColorG[i], ABColorG[i], length);
                        double[] YXColorB = Interpolate(ACColorB[i], ABColorB[i], length);

                        ProcessScanLine(y, xtabAB[i], xtabAC[i], axisYX, color, YXColorR, YXColorG, YXColorB, Ir, Ig, Ib, phong);
                        i++;

                    }
                    else
                    {
                        int length = Math.Abs(xtabAC[i] - xtabBC[j]) + 1;
                        double[] Ir = new double[length];
                        double[] Ig = new double[length];
                        double[] Ib = new double[length];
                        double[] axisYX = Interpolate(axisACZ[i], axisBCZ[j], length);
                        if (phong)
                        {
                            YXNormalx = Interpolate(ACNormalx[i], BCNormalx[j], length);
                            YXNormaly = Interpolate(ACNormaly[i], BCNormaly[j], length);
                            YXNormalz = Interpolate(ACNormalz[i], BCNormalz[j], length);
                            YXWorldx = Interpolate(ACWorldx[i], BCWorldx[j], length);
                            YXWorldy = Interpolate(ACWorldy[i], BCWorldy[j], length);
                            YXWorldz = Interpolate(ACWorldz[i], BCWorldz[j], length);
                            for (int g = 0; g < length; g++)
                            {
                                Vertex point = new Vertex { Coordinates = new Vector3(xtabBC[j] + g, y, axisYX[g]), Normal = new Vector3(YXNormalx[g], YXNormaly[g], YXNormalz[g]), WorldCoordinates = new Vector3(YXWorldx[g], YXWorldy[g], YXWorldz[g]) };
                                if (point.Coordinates.third >= depthBuffer[(int)(point.Coordinates.first + point.Coordinates.second * bmpWidth)])
                                {
                                    Ir[g] = ColorIluminationAtPoint(0, color, ka, kd, ks, point, lightSources, camera, n);
                                    Ig[g] = ColorIluminationAtPoint(1, color, ka, kd, ks, point, lightSources, camera, n);
                                    Ib[g] = ColorIluminationAtPoint(2, color, ka, kd, ks, point, lightSources, camera, n);
                                }
                            }
                        }

                        
                        double[] YXColorR = Interpolate(ACColorR[i], BCColorR[j], length);
                        double[] YXColorG = Interpolate(ACColorG[i], BCColorG[j], length);
                        double[] YXColorB = Interpolate(ACColorB[i], BCColorB[j], length);
 

                        ProcessScanLine(y, xtabBC[j], xtabAC[i], axisYX, color, YXColorR, YXColorG, YXColorB, Ir, Ig, Ib, phong);
                        j++;i++;

                    }
                }
            }
            //BresenhamLine((int)pointA.Item1, (int)pointA.Item2, (int)pointB.Item1, (int)pointB.Item2, color);
            //BresenhamLine((int)pointB.Item1, (int)pointB.Item2, (int)pointC.Item1, (int)pointC.Item2, color);
            //BresenhamLine((int)pointC.Item1, (int)pointC.Item2, (int)pointA.Item1, (int)pointA.Item2, color);

        }
        public void ProcessScanLine(int y, int sx, int ex, double[] z, Color4 color, double[] dotsR, double[] dotsG, double[] dotsB, double[] Ir, double[] Ig, double[] Ib, bool phong)
        {
            for (int i=sx;i<=ex;i++)
            {
                if (phong)
                {
                    PutPixel(i, y, z[i - sx], new Color4(Ir[i - sx], Ig[i - sx], Ib[i - sx], 1));
                }
                else PutPixel(i, y, z[i-sx], new Color4(dotsR[i-sx], dotsG[i - sx], dotsB[i - sx], 1));
               // PutPixel(i, y, z[i - sx],color);
            }
        }
        public void PutPixel(int x, int y, double z, Color4 color)
        {
            
                if (x >= 0 && y >= 0 && x < bmpWidth && y < bmpHeight)
                {
                    int i = (x + y * bmpWidth);
                    int index = i * 4;
                    
                    lock (lockBuffer[i])
                    {
                        if (z < depthBuffer[x + y * bmpWidth]) return;
                        depthBuffer[x + y * bmpWidth] = z;
                        
                        backBuffer[index] = (byte)(color.Blue * 255);
                        backBuffer[index + 1] = (byte)(color.Green * 255);
                        backBuffer[index + 2] = (byte)(color.Red * 255);
                        backBuffer[index + 3] = (byte)(color.Alpha * 255);
                }
                }
            
        }
        public class Color4
        {
            public double Blue, Green, Red, Alpha;
            public Color4(double r, double g, double b, double a)
            {
                Blue = b; Green = g; Red = r; Alpha = a;
            }
        }
        public int[] BresenhamLine_2_0(int x1, int y1, int x2, int y2, Color4 color, bool more)
        {
           
            int d, ai, bi;
            int x = x1, y = y1;
            int dx = Math.Abs(x2 - x1);
            int dy = Math.Abs(y2 - y1);
            int xi = (x1 < x2) ? 1 : -1;
            int yi = (y1 < y2) ? 1 : -1;
            int[] xtable = new int[dy+1];
            //PutPixel(x, y, color);
            int i = 0;
            xtable[i] = x;
            
            if (dx > dy)
            {
                ai = (dy - dx) * 2;
                bi = dy * 2;
                d = bi - dx;
                while (x != x2)
                {
                    if (d >= 0)
                    {
                        x += xi;
                        y += yi;
                        d += ai;
                        xtable[++i] = x;
                        
                    }
                    else
                    {
                        d += bi;
                        x += xi;
                        //if (more) xtable[i] = x;
                    }
                    //PutPixel(x, y, color);
                   
                }
            }
            else
            {
                ai = (dx - dy) * 2;
                bi = dx * 2;
                d = bi - dy;
                while (y != y2)
                {
                    if (d >= 0)
                    {
                        x += xi;
                        y += yi;
                        d += ai;
                        xtable[++i] = x;
                        
                    }
                    else
                    {
                        d += bi;
                        y += yi;
                        xtable[++i] = x;
                        
                    }
                    //PutPixel(x, y, color);
                }
            }
            return xtable;
        }
    }
}
