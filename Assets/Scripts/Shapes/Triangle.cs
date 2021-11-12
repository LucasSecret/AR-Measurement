using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(MeshFilter))]
public class Triangle : MonoBehaviour
{
    private List<GameObject> _verticesList;
    private List<GameObject> _textsList;
    private GameObject _surfaceText;
    private LineRenderer _lineRenderer;
    private UIManager _uiManager;

    private bool _textsAreCreated = false;


    private void Update()
    {
        if (!_textsAreCreated)
            return;

        UpdateTexts();
    }


    public void Init(List<GameObject> vertices)
    {
        _verticesList = new List<GameObject>();
        _textsList = new List<GameObject>();
        _uiManager = FindObjectOfType<UIManager>();
        foreach (GameObject vertex in vertices)
        {
            _verticesList.Add(vertex);
            vertex.transform.SetParent(this.transform);
        }


        _lineRenderer = GetComponent<LineRenderer>();

        CreateTexts();
        Draw();
    }

    private void Draw()
    {
        DrawEdges();
        DrawMesh();
    }

    private void DrawEdges()
    {
        _lineRenderer.positionCount = _verticesList.Count+1;

        for (int i = 0; i < _verticesList.Count+1; i++)
            _lineRenderer.SetPosition(i, _verticesList[i % _verticesList.Count].transform.position);
    }

    private void DrawMesh()
    {
        Mesh mesh = GetComponent<MeshFilter>().mesh;
        mesh.Clear();

        List<Vector3> vertices = new List<Vector3>();
        foreach (GameObject vertex in _verticesList)
            vertices.Add(transform.InverseTransformPoint(vertex.transform.position));

        List<int> triangles = new List<int>();
        triangles.Add(0);
        triangles.Add(1);
        triangles.Add(2);

        //Duplicate triangles for double side
        triangles.Add(2);
        triangles.Add(1);
        triangles.Add(0);





        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
    }


    private void CreateTexts()
    {
        if (!_uiManager)
            return;

        for (int i = 0; i < _verticesList.Count ; i++)
            _textsList.Add(Instantiate(_uiManager.textInfosPrefab, _uiManager.transform));

        _textsAreCreated = true;

        _surfaceText = Instantiate(_uiManager.textSpecialeInfosPrefab, _uiManager.transform);
    }


    private void UpdateTexts()
    {
        Quaternion rotationToBeAligned;

        for (int i = 0; i < _textsList.Count; i++)
        {
            float distance = Vector3.Distance(_verticesList[i].transform.position, _verticesList[(i + 1) % _textsList.Count].transform.position);
            _textsList[i].GetComponentInChildren<Text>().text = (distance * 100).ToString() + " cm";
            _textsList[i].transform.position = Camera.main.WorldToScreenPoint((_verticesList[i].transform.position + _verticesList[(i + 1) % _textsList.Count].transform.position) / 2);

            rotationToBeAligned = Quaternion.FromToRotation(_textsList[i].transform.right, 
                                                            Camera.main.WorldToScreenPoint(_verticesList[(i + 1) % _textsList.Count].transform.position) - 
                                                            Camera.main.WorldToScreenPoint(_verticesList[i].transform.position));

            _textsList[i].transform.rotation = rotationToBeAligned * _textsList[i].transform.rotation;
        }


        float surface = 0.0f;
        for (int i = 0; i < _verticesList.Count; i++)
            surface += Vector3.Distance(_verticesList[i].transform.position, _verticesList[(i + 1) % _verticesList.Count].transform.position);

        surface /= 2;

        float surfaceMultipliedBySide = 1.0f;
        Vector3 surfaceTextPosition = Vector3.zero;
        for (int i = 0; i < _verticesList.Count; i++)
        {
            surfaceMultipliedBySide *= surface - Vector3.Distance(_verticesList[i].transform.position, _verticesList[(i + 1) % _verticesList.Count].transform.position);
            surfaceTextPosition += _verticesList[i].transform.position;
        }

        surface = Mathf.Sqrt(surface * surfaceMultipliedBySide);


        _surfaceText.GetComponentInChildren<Text>().text = surface.ToString() + " m²";
        _surfaceText.transform.position = Camera.main.WorldToScreenPoint(surfaceTextPosition / 3);

    }

    
    public void Delete()
    {
        foreach (Transform child in transform)
            Destroy(child.gameObject);

        foreach (GameObject text in _textsList)
            Destroy(text);

        Destroy(_surfaceText);


        Destroy(this.gameObject);
    }


    public void MoveVertex(GameObject vertex, Vector3 position)
    {
        vertex.transform.position = position;
        Draw();
    }
}
