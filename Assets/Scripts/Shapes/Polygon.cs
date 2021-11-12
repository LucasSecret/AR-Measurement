using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(MeshFilter))]
public class Polygon : MonoBehaviour
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



    public void Init(List<GameObject> verticesList)
    {
        _verticesList = new List<GameObject>();
        _textsList = new List<GameObject>();

        _lineRenderer = GetComponent<LineRenderer>();
        _uiManager = FindObjectOfType<UIManager>();

        foreach (GameObject vertex in verticesList)
        {
            _verticesList.Add(vertex);
            vertex.transform.SetParent(this.transform);
        }
        Draw();
        CreateTexts();
    }


    public void EndShape()
    {
        Draw();
    }


    private void Draw()
    {
        DrawEdges();
        DrawMesh();
    }



    private void DrawEdges()
    {
        _lineRenderer.positionCount = (_verticesList.Count > 2) ? _verticesList.Count + 1 : _verticesList.Count;

        for (int i = 0; i < _lineRenderer.positionCount; i++)
            _lineRenderer.SetPosition(i, _verticesList[i%_verticesList.Count].transform.position);
    }


    private void DrawMesh()
    {
        Mesh mesh = GetComponent<MeshFilter>().mesh;
        mesh.Clear();

        if (_verticesList.Count < 3)
            return;

        List<Vector3> vertices = new List<Vector3>();

        for (int i = 0; i < _verticesList.Count; i++)
            vertices.Add(transform.InverseTransformPoint(_verticesList[i].transform.position));
        
        List<int> triangles = new List<int>();
        for (int i=0; i< vertices.Count -2; i++)
        {
            triangles.Add(0);
            triangles.Add(i + 1);
            triangles.Add(i + 2);

            //Duplicate triangles for double side
            triangles.Add(i + 2);
            triangles.Add(i + 1);
            triangles.Add(0);
        }

        List<Vector3> normalsList = new List<Vector3>();

        for (int i = 0; i < vertices.Count; i++)
            normalsList.Add(transform.up);


        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.normals = normalsList.ToArray();
    }



    private void CreateTexts()
    {
        if (!_uiManager)
            return;

        for (int i = 0; i < _verticesList.Count - 1; i++)
            _textsList.Add(Instantiate(_uiManager.textInfosPrefab, _uiManager.transform));

        if(_verticesList.Count > 2)
            _surfaceText = Instantiate(_uiManager.textSpecialeInfosPrefab, _uiManager.transform);

        _textsAreCreated = true;
    }


    private void UpdateTexts()
    {
        float surface = 0.0f;
        Vector3 surfaceTextPosition = Vector3.zero;

        for (int i = 0; i < _textsList.Count; i++)
        {
            float distance = Vector3.Distance(_verticesList[i].transform.position, _verticesList[(i + 1)%_verticesList.Count].transform.position);
            _textsList[i].GetComponentInChildren<Text>().text = (distance * 100).ToString() + " cm";
            _textsList[i].transform.position = Camera.main.WorldToScreenPoint((_verticesList[i].transform.position + _verticesList[(i + 1)%_verticesList.Count].transform.position) / 2);

            RotateTextToFitLine(_textsList[i], _verticesList[i], _verticesList[(i + 1) % _verticesList.Count]);

            if(i+2 < _textsList.Count)
                surface += TriangleArea(_verticesList[0], _verticesList[i + 1], _verticesList[i + 2]);

            surfaceTextPosition += _textsList[i].transform.position;
        }

        if (!_surfaceText.GetComponentInChildren<Text>())
            return;

        _surfaceText.GetComponentInChildren<Text>().text = surface.ToString() + " m²";
        _surfaceText.transform.position = surfaceTextPosition / _textsList.Count;
    }


    private void RotateTextToFitLine(GameObject text, GameObject vertexFrom, GameObject vertexTo)
    {
        Quaternion rotationToBeAligned;

        Vector3 positionFrom = Camera.main.WorldToScreenPoint(vertexFrom.transform.position);
        Vector3 positionTo = Camera.main.WorldToScreenPoint(vertexTo.transform.position);

        rotationToBeAligned = Quaternion.FromToRotation(text.transform.right, positionTo - positionFrom);

        text.transform.rotation = rotationToBeAligned * text.transform.rotation;
    }




    //Return the area of a triangle 
    private float TriangleArea(GameObject vertexA, GameObject vertexB, GameObject vertexC)
    { 

        float distanceAB = Vector3.Distance(vertexA.transform.position, vertexB.transform.position);
        float distanceBC = Vector3.Distance(vertexB.transform.position, vertexC.transform.position);
        float distanceCA = Vector3.Distance(vertexC.transform.position, vertexA.transform.position);

        float surface = distanceAB + distanceBC + distanceCA;
        surface /= 2;

        float surfaceMultipliedBySide = 1.0f;
        surfaceMultipliedBySide *= (surface - distanceAB);
        surfaceMultipliedBySide *= (surface - distanceBC);
        surfaceMultipliedBySide *= (surface - distanceCA);


        return Mathf.Sqrt(surface * surfaceMultipliedBySide);
    }



    public void AddVertice(GameObject vertex)
    {
        _verticesList.Add(vertex);
        vertex.transform.SetParent(this.transform);
        Draw();

        _textsList.Add(Instantiate(_uiManager.textInfosPrefab, _uiManager.transform));

        if (_verticesList.Count == 3)
        {
            _textsList.Add(Instantiate(_uiManager.textInfosPrefab, _uiManager.transform));
            _surfaceText = Instantiate(_uiManager.textSpecialeInfosPrefab, _uiManager.transform);
        }
    }
    
   

    public void MoveVertex(GameObject vertex, Vector3 position)
    {
        vertex.transform.position = position;
        Draw();
    }


    private void DestroyTexts()
    {
        foreach (GameObject text in _textsList)
            Destroy(text);

        Destroy(_surfaceText);
        _textsList.Clear();
        _textsAreCreated = false;

    }

    public void DeleteVertex(GameObject vertexToDestroy)
    {
        DestroyTexts();

        if (_verticesList.Count < 3)
        {
            foreach (GameObject vertex in _verticesList)
                Destroy(vertex);
            
            _verticesList.Clear();
            Destroy(this.gameObject);
        }

        else
        {
            _verticesList.Remove(vertexToDestroy);
            Destroy(vertexToDestroy);
            Draw();
            CreateTexts();
        }
    }
}
