using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine {
    public class Triangle
    {
        public int VerA, VerB, VerC;
        public Triangle(int a, int b, int c)
        {
            VerA = a; VerB = b; VerC = c;
        }
    }
    public class Vertex
    {
        public Vector3 Normal;
        public Vector3 Coordinates;
        public Vector3 WorldCoordinates;

    }
    public class Light
    {
        public Vector3 Normal;
        public Vector3 Coordinates;
        public Vector3 Color;
        public double angle;
    }
    public class Obj3D
    {
        public string Name { get; set; }
        public Vertex[] Vertices { get; private set; }
        public Triangle[] Triangles { get; private set; }
        public Vector3 Position { get; set; }
        public Vector3 Rotation { get; set; }
        public Light[] Lights { get; set; }
        public Obj3D(string name, int verticesCount, int triangleCount)
        {
            Vertices = new Vertex[verticesCount];
            Triangles = new Triangle[triangleCount];
            Position = new Vector3();
            Rotation = new Vector3();
           
            Name = name;
        }
        public static Obj3D LoadJSONFile(string fileName)
        {
            List<Obj3D> meshes = new List<Obj3D>();
            var file = System.IO.File.ReadAllText(fileName);
            
            dynamic jsonObject = Newtonsoft.Json.JsonConvert.DeserializeObject(file);

            for (int meshIndex = 0; meshIndex < jsonObject.meshes.Count; meshIndex++)
            {
                var verticesArray = jsonObject.meshes[meshIndex].vertices;
                // Faces
                var indicesArray = jsonObject.meshes[meshIndex].indices;

                var uvCount = jsonObject.meshes[meshIndex].uvCount.Value;
                var verticesStep = 1;

                // Depending of the number of texture's coordinates per vertex
                // we're jumping in the vertices array  by 6, 8 & 10 windows frame
                switch ((int)uvCount)
                {
                    case 0:
                        verticesStep = 6;
                        break;
                    case 1:
                        verticesStep = 8;
                        break;
                    case 2:
                        verticesStep = 10;
                        break;
                }

                // the number of interesting vertices information for us
                var verticesCount = verticesArray.Count / verticesStep;
                // number of faces is logically the size of the array divided by 3 (A, B, C)
                var facesCount = indicesArray.Count / 3;
                var mesh = new Obj3D(jsonObject.meshes[meshIndex].name.Value, verticesCount, facesCount);

                // Filling the Vertices array of our mesh first
                for (int index = 0; index < verticesCount; index++)
                {
                    double x = (double)verticesArray[index * verticesStep].Value;
                    double y = (double)verticesArray[index * verticesStep + 1].Value;
                    double z = (double)verticesArray[index * verticesStep + 2].Value;
                    // Loading the vertex normal exported by Blender
                    double nx = (double)verticesArray[index * verticesStep + 3].Value;
                    double ny = (double)verticesArray[index * verticesStep + 4].Value;
                    double nz = (double)verticesArray[index * verticesStep + 5].Value;
                    mesh.Vertices[index] = new Vertex { Coordinates = new Vector3(x, y, z), Normal = new Vector3(nx, ny, nz) };
                }

                // Then filling the Faces array
                for (var index = 0; index < facesCount; index++)
                {
                    int a = (int)indicesArray[index * 3].Value;
                    int b = (int)indicesArray[index * 3 + 1].Value;
                    int c = (int)indicesArray[index * 3 + 2].Value;
                    mesh.Triangles[index] = new Triangle( a, b, c );
                }

                // Getting the position you've set in Blender
                var position = jsonObject.meshes[meshIndex].position;
                mesh.Position = new Vector3((float)position[0].Value, (float)position[1].Value, (float)position[2].Value);
                meshes.Add(mesh);
            }
            return meshes.ToArray()[0];
        }
    }
    
}
