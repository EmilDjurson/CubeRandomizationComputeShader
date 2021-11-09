using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct Cube
{
    public Vector3 position;
    public Color color;
}

public class Randomizer : MonoBehaviour
{
    public Camera _camera;

    public Transform cubeParent;
    public GameObject create, randomizeGPU, randomizeCPU;

    public ComputeShader computeShader;

    public Mesh mesh;
    public Material material;

    public int count;
    public int repetitions;
    
    private List<GameObject> objects;
    private Cube[] data;

    public void CreateCubes()
    {
        objects = new List<GameObject>();
        data = new Cube[count * count];

        for (int x = 0; x < count; x++)
        {
            for (int y = 0; y < count; y++)
            {
                CreateCube(x, y);
            }
        }
    }

    public void CreateCube(int x, int y)
    {
        GameObject cube = new GameObject("Cube " + x * count + y, typeof(MeshFilter), typeof(MeshRenderer));
        cube.GetComponent<MeshFilter>().mesh = mesh;
        cube.GetComponent<MeshRenderer>().material = new Material(material);
        cube.transform.position = new Vector3(x, y, Random.Range(-0.25f, 0.25f));

        Color color = Random.ColorHSV();
        cube.GetComponent<MeshRenderer>().material.SetColor("_Color", color);

        objects.Add(cube);
        cube.transform.parent = cubeParent;

        Cube cubeData = new Cube();
        cubeData.position = cube.transform.position;
        cubeData.color = color;
        data[x * count + y] = cubeData;
    }

    public void OnRandomizeGpu()
    {
        int colorSize = sizeof(float) * 4;
        int vector3Size = sizeof(float) * 3;
        int totalSize = colorSize + vector3Size;

        ComputeBuffer cubesBuffer = new ComputeBuffer(data.Length, totalSize);
        cubesBuffer.SetData(data);

        computeShader.SetBuffer(0, "cubes", cubesBuffer);
        computeShader.SetFloat("resolution", data.Length);
        computeShader.SetFloat("repetitions", repetitions);
        computeShader.Dispatch(0, data.Length / 10, 1, 1);

        cubesBuffer.GetData(data);

        for (int i = 0; i < objects.Count; i++)
        {
            GameObject obj = objects[i];
            Cube cube = data[i];
            obj.transform.position = cube.position;
            obj.GetComponent<MeshRenderer>().material.SetColor("_Color", cube.color);
        }

        cubesBuffer.Dispose();
    }

    public void OnRandomizeCpu()
    {
        for (int i = 0; i < repetitions; i++)
        {
            for (int c = 0; c < objects.Count; c++)
            {
                GameObject obj = objects[c];
                obj.transform.position = new Vector3(obj.transform.position.x, obj.transform.position.y, Random.Range(-0.25f, 0.25f));
                obj.GetComponent<MeshRenderer>().material.SetColor("_Color", Random.ColorHSV());
            }
        }
    }

    public void Update()
    {
        if (objects == null || objects.Count / objects.Count == count)
        {
            randomizeCPU.SetActive(false);
            randomizeGPU.SetActive(false);
            create.SetActive(true);
        }
        else
        {
            randomizeCPU.SetActive(true);
            randomizeGPU.SetActive(true);
            create.SetActive(false);
        }

        _camera.gameObject.transform.position = new Vector3(((float) count - 1) / 2, ((float) count - 1) / 2, -50);
        _camera.orthographicSize = (float) count / 2;
    }
}
