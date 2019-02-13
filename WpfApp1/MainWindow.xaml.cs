using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Engine
{
    
    public partial class MainWindow : Window
    {
        
        Scene scene;
        DateTime previousTime;
        //Object3D object3D = new Object3D("Cube", 8, 12);
        
        
        Obj3D ob1 = Obj3D.LoadJSONFile("monkey.babylon");
        Obj3D ob2 = Obj3D.LoadJSONFile("sphere.babylon");
        Obj3D[] object3D = new Obj3D[2];
        
        Matrix camera = new Matrix(3,3);
        public bool? isphong;
        public bool? isblinn;
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            isphong = phongshading.IsChecked;
            isblinn = phongshading.IsChecked;
            WriteableBitmap bmp = new WriteableBitmap(360, 240, 96, 96, PixelFormats.Bgra32, null);
            scene = new Scene(bmp);
            object3D[0] = ob1;
            object3D[1] = ob2;
            object3D[1].Position += new Vector3(1, 0, 0);
            object3D[0].Position += new Vector3(-1.5, 0, 0);


            Light[] light = new Light[1];
            light[0] = new Light
            {
                Coordinates = new Vector3(0, 0, -1),
                Color = new Vector3(0, 0, 1),
                Normal = new Vector3(0, 0, -1),
                angle = 0.5
            };
            object3D[0].Lights = light;
            frontBuffer.Source = bmp;

            //object3D.Vertices[0] = new Vector3(-1, 1, 1);
            //object3D.Vertices[1] = new Vector3(1, 1, 1);
            //object3D.Vertices[2] = new Vector3(-1, -1, 1);
            //object3D.Vertices[3] = new Vector3(1, -1, 1);
            //object3D.Vertices[4] = new Vector3(-1, 1, -1);
            //object3D.Vertices[5] = new Vector3(1, 1, -1);
            //object3D.Vertices[6] = new Vector3(1, -1, -1);
            //object3D.Vertices[7] = new Vector3(-1, -1, -1);
            //object3D.Triangles[0] = new Triangle ( 0, 1, 2 );
            //object3D.Triangles[1] = new Triangle ( 1, 2, 3 );
            //object3D.Triangles[2] = new Triangle ( 1, 3, 6 );
            //object3D.Triangles[3] = new Triangle ( 1, 5, 6 );
            //object3D.Triangles[4] = new Triangle ( 0, 1, 4 );
            //object3D.Triangles[5] = new Triangle ( 1, 4, 5 );
            //object3D.Triangles[6] = new Triangle ( 2, 3, 7 );
            //object3D.Triangles[7] = new Triangle ( 3, 6, 7 );
            //object3D.Triangles[8] = new Triangle ( 0, 2, 7 );
            //object3D.Triangles[9] = new Triangle ( 0, 4, 7 );
            //object3D.Triangles[10] = new Triangle ( 4, 5, 6 );
            //object3D.Triangles[11] = new Triangle ( 4, 6, 7 );
             
            camera[0, 0] = 0; camera[1, 0] = 0; camera[2, 0] = 10;
            camera[0, 1] = 0; camera[1, 1] = 0; camera[2, 1] = 0;
            camera[0, 2] = 0; camera[1, 2] = -1; camera[2, 2] = 0;
            
            CompositionTarget.Rendering += CompositionTarget_Rendering;
        }
        void CompositionTarget_Rendering(object sender, object e)
        {
            scene.isphong = isphong==true?true:false;
            scene.isblinn = isblinn == true ? true : false;
            scene.camera = new Vector3(camera[0, 0], camera[1, 0], camera[2, 0]);
            DateTime now = DateTime.Now;
            double FPS = 1000 / (now - previousTime).TotalMilliseconds;
            previousTime = now;
            fps.Content = ((int)FPS).ToString()+"FPS";
            scene.Clear(255, 255, 255, 255);
            
                object3D[0].Rotation = new Vector3(object3D[0].Rotation[0] , object3D[0].Rotation[1] + 0.001, object3D[0].Rotation[2] );
            

            scene.Render(camera, object3D);
            scene.Present();
        }
        public MainWindow()
        {
            InitializeComponent();
            
        }

        

        private void Gouraud(object sender, RoutedEventArgs e)
        {
            isphong = false;
        }

        private void Phong(object sender, RoutedEventArgs e)
        {
            isphong = true;
        }
        private void BlinnIll(object sender, RoutedEventArgs e)
        {
            isblinn = true;
        }

        private void PhongIll(object sender, RoutedEventArgs e)
        {
            isblinn = false;
        }
    }

}
