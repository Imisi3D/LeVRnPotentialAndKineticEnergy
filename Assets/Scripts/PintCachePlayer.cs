using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class PintCachePlayer : MonoBehaviour
{
    //
    //POINT CACHE Vs
    //
    public string filePath;

    struct PointCacheFile
    {
        public char[] signature;
        public int fileVersion;
        public int numPoints;
        public float startFrame;
        public float sampleRate;
        public int numSamples;
        public List<Vector3> vertexCoords;
    }


    PointCacheFile pcFile;
    bool fileParsed = false;

    //
    //VERTEX POS Vs
    //
    public float fps = 24.0f;
    Mesh mesh;
    Vector3[] vertices;
    int counter = 0;
    int curFrame = 0;
    float tempTime = 0.0f;
    float timeMeasure = 0.0f;

    List<List<int>> verticesList;
    int i = 0;

    public bool drawGizmo = false;
    public int selectedVertex = 0;
    public bool animate = false;
    public int numberOfVerts = 0;

    List<int> temp;



    // Use this for initialization
    void Start()
    {
        pcFile = new PointCacheFile();
        ParsePCFile();
        timeMeasure = 1 / fps;
        mesh = this.GetComponent<MeshFilter>().mesh;
        vertices = mesh.vertices;
        tempTime = Time.time;
        SetupVerticesList();
        Debug.Log("finished start");

        //WriteToFile();
    }

    void FixedUpdate()
    {
        if (animate)
        {
            Animate();
        }
    }

    void OnDrawGizmos()
    {
        if (drawGizmo)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.TransformPoint(new Vector3(mesh.vertices[selectedVertex].x, mesh.vertices[selectedVertex].y, mesh.vertices[selectedVertex].z)), 0.2f);
            Debug.Log(new Vector3(mesh.vertices[selectedVertex].x, mesh.vertices[selectedVertex].y, mesh.vertices[selectedVertex].z));
        }
    }

    void Animate()
    {
        if (Time.time > tempTime + timeMeasure)
        {
            if (fileParsed)
            {
                for (i = 0; i < temp.Count; i++)
                {
                    for (int x = 0; x < verticesList[temp[i]].Count; x++)
                    {
                        vertices[verticesList[temp[i]][x]] = pcFile.vertexCoords[counter];
                    }
                    counter++;
                }

                mesh.vertices = vertices;
                mesh.RecalculateBounds();
                mesh.RecalculateNormals();
                curFrame++;

                if (curFrame == pcFile.numSamples)
                {
                    curFrame = 0;
                    counter = 0;
                }
            }
            tempTime = Time.time;
        }
    }

    void ParsePCFile()
    {
        FileStream fs = new FileStream(filePath, FileMode.Open);
        BinaryReader binReader = new BinaryReader(fs);
        Debug.Log("parsing file");
        //
        //SIGNATURE
        //

        pcFile.signature = new char[12];
        pcFile.signature = binReader.ReadChars(12);

        //
        //FILE VERSION
        //
        Debug.Log("file version");

        pcFile.fileVersion = binReader.ReadInt32();

        //
        //NUMBER OF POINTS
        //

        pcFile.numPoints = binReader.ReadInt32();
        Debug.Log("counting points: "+pcFile.numPoints);

        //
        //START FRAME
        //

        pcFile.startFrame = binReader.ReadSingle();

        Debug.Log("start frame: " + pcFile.startFrame);
        //
        //SAMPLE RATE
        //

        pcFile.sampleRate = binReader.ReadSingle();

        Debug.Log("sample rate: " + pcFile.sampleRate);
        //
        //NUMBER OF SAMPLES
        //

        pcFile.numSamples = binReader.ReadInt32();

        Debug.Log("num samples: " + pcFile.numSamples);
        //
        //GET VERTEX COORDS
        //

        pcFile.vertexCoords = new List<Vector3>();

        for (i = 0; i < /*pcFile.numSamples*/ 10; i++)
        {
            Debug.Log("looping on numsamples");
            for (int x = 0; x < pcFile.numPoints; x++)
            {
                Vector3 vPos = new Vector3();
                vPos.x = binReader.ReadSingle();
                vPos.y = binReader.ReadSingle();
                vPos.z = binReader.ReadSingle();
                pcFile.vertexCoords.Add(vPos);
            }
        }
        fileParsed = true;

        SortOutVertices();
    }

    void SortOutVertices()
    {
        temp = new List<int>();
        Debug.Log("sorting verts");
        for (i = (int)(numberOfVerts / 2) - 1; i >= 0; i--)
        {
            temp.Add(i);
        }
        Debug.Log("sorting verts 2 ");

        for (i = numberOfVerts - 1; i != (numberOfVerts / 2) - 1; i--)
        {
            temp.Add(i);
        }
        Debug.Log("sorting verts 3 ");

        foreach (int num in temp)
        {
            Debug.Log(num);
        }
        Debug.Log("logging");

    }

    void SetupVerticesList()
    {
        bool firstRun = true;
        int indexToAdd = 0;
        verticesList = new List<List<int>>();

        Debug.Log("adding verts to list");
        for (i = 0; i < pcFile.numPoints; i++)
        {
            verticesList.Add(new List<int>());
        }
        Debug.Log("finished adding verts to list");

        for (i = 0; i < verticesList.Count; i++)
        {
            verticesList[i].Add(i);
            Debug.Log("matrix");

            for (int x = pcFile.numPoints; x < 10 /*mesh.vertexCount*/; x++)
            {
                if (mesh.vertices[verticesList[i][0]].x == mesh.vertices[x].x &&
                        mesh.vertices[verticesList[i][0]].y == mesh.vertices[x].y &&
                        mesh.vertices[verticesList[i][0]].z == mesh.vertices[x].z)
                {
                    verticesList[i].Add(x);
                }
                Debug.Log("conditionned add");

            }
        }

        for (i = 0; i < verticesList.Count; i++)
        {
            Debug.Log(System.Convert.ToString(i) + "=================:");
            for (int x = 0; x <10 /*verticesList[i].Count*/; x++)
            {
                Debug.Log(verticesList[i][x]);
            }
        }
        Debug.Log("logged all");

    }
    //
    //JUST FOR DEBUGING PURPOSES
    //
    void WriteToFile()
    {
        StreamWriter sw = new StreamWriter(@"mesh_vertices.txt");
        StreamWriter writer = new StreamWriter(@"cache_vertices.txt");

        for (i = 0; i < mesh.vertexCount; i++)
        {
            sw.WriteLine(System.Convert.ToString(i) + "===>X:" + System.Convert.ToString(mesh.vertices[i].x) + " Y:" + System.Convert.ToString(mesh.vertices[i].y) + " Z:" + System.Convert.ToString(mesh.vertices[i].z));

        }

        for (i = 0; i < pcFile.numPoints; i++)
        {
            writer.WriteLine(System.Convert.ToString(i) + "===>X:" + System.Convert.ToString(pcFile.vertexCoords[i].x) + " Y:" + System.Convert.ToString(pcFile.vertexCoords[i].y) + " Z:" + System.Convert.ToString(pcFile.vertexCoords[i].z));
        }
        sw.Close();
        writer.Close();
    }
}