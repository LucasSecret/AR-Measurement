using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


[RequireComponent(typeof(MeshFilter))]

public class Circle : MonoBehaviour
{
    [HideInInspector]
    public GameObject _centerPoint;
    [HideInInspector]
    public GameObject _radiusPoint;

    private UIManager _uiManager;

    private GameObject _radiusText;
    private GameObject _surfaceText;

    private LineRenderer _edgesLineRenderer;
    private LineRenderer _radiusLineRenderer;
    
    private float _radius;
    private int _numberOfPoints = 360;
    private float _vertexYPosition;

    private bool _textsAreCreated = false;

    private void Update()
    {
        if (_textsAreCreated)
            UpdateTexts();
    }


    public void Init()
    {
        this.transform.position = _centerPoint.transform.position;
        _edgesLineRenderer = GetComponent<LineRenderer>();
        _centerPoint.transform.SetParent(this.transform);
        _radiusPoint.transform.SetParent(this.transform);

        _vertexYPosition = _centerPoint.transform.position.y;

        _uiManager = FindObjectOfType<UIManager>();

        CreateTexts();
        UpdateRadius();
    }


    public void CreateTexts()
    {
        if (!_uiManager)
            return;

        _radiusText = Instantiate(_uiManager.textInfosPrefab, _uiManager.transform);
        _surfaceText = Instantiate(_uiManager.textSpecialeInfosPrefab, _uiManager.transform);

        _textsAreCreated = true;
    }


    private void UpdateRadius()
    {
        _radius = Vector3.Distance(_centerPoint.transform.position, _radiusPoint.transform.position);
    }


    public void Draw()
    {
        UpdateRadius();
        DrawMesh();
        DrawEdges();
        DrawRadius();
        PlaceVerticesOverMesh();
        UpdateTexts();
    }


    private void InitRadiusObject()
    {
        GameObject radiusLineObj = new GameObject("RadiusLine");
        radiusLineObj.transform.SetParent(this.transform);

        _radiusLineRenderer = radiusLineObj.AddComponent<LineRenderer>();
        _radiusLineRenderer.useWorldSpace = true;
        _radiusLineRenderer.sharedMaterial = _edgesLineRenderer.sharedMaterial;
        _radiusLineRenderer.startWidth = _radiusLineRenderer.endWidth = _edgesLineRenderer.startWidth;
        _radiusLineRenderer.startColor = _radiusLineRenderer.endColor = _edgesLineRenderer.startColor;

        _radiusLineRenderer.positionCount = 2;
    }

    private void DrawRadius()
    {
        if (!_radiusLineRenderer)
            InitRadiusObject();

        _radiusLineRenderer.SetPosition(0, _centerPoint.transform.position);
        _radiusLineRenderer.SetPosition(1, _radiusPoint.transform.position);
    }


    private void DrawEdges()
    {
        _edgesLineRenderer.positionCount = _numberOfPoints + 1; //Add one extra point for the radius line
        List<Vector3> pointsPosition = new List<Vector3> { };

        for (int i = 0; i < _numberOfPoints + 1; i++) //Compute position of all circle points to draw line renderer
        {
            float x = _radius * Mathf.Sin((2 * Mathf.PI * i) / _numberOfPoints);
            float z = _radius * Mathf.Cos((2 * Mathf.PI * i) / _numberOfPoints);
                       
            pointsPosition.Add(_centerPoint.transform.position + new Vector3(x, 0, z));
        }

        _edgesLineRenderer.SetPositions(pointsPosition.ToArray());
    }


    private void DrawMesh()
    {
        MeshFilter mf = GetComponent<MeshFilter>();
        Mesh mesh = new Mesh();
        mf.mesh = mesh;

        Vector3 center = _centerPoint.transform.position;

        //vertices
        List<Vector3> verticesList = new List<Vector3> { };

        for (int i = 0; i < _numberOfPoints; i++)
        {
            float x = _radius * Mathf.Sin((2 * Mathf.PI * i) / _numberOfPoints);
            float z = _radius * Mathf.Cos((2 * Mathf.PI * i) / _numberOfPoints);
            verticesList.Add(transform.InverseTransformPoint(new Vector3(x, 0.0f, z) + center));
        }

        Vector3[] vertices = verticesList.ToArray();

        //triangles
        List<int> trianglesList = new List<int> { };
        for (int i = 0; i < (_numberOfPoints - 2); i++)
        {
            trianglesList.Add(0);
            trianglesList.Add(i + 1);
            trianglesList.Add(i + 2);
        }
        int[] triangles = trianglesList.ToArray();

        mesh.vertices = vertices;
        mesh.triangles = triangles;
    }


    private void UpdateTexts()
    {
        UpdateRadiusText();
        UpdateSurfaceText();
    }


    private void UpdateRadiusText()
    {
        _radiusText.GetComponentInChildren<Text>().text = (_radius * 100).ToString() + "cm";
        _radiusText.transform.position = Camera.main.WorldToScreenPoint((_radiusPoint.transform.position + _centerPoint.transform.position) / 2);

        Quaternion rotationToBeAligned = Quaternion.FromToRotation(_radiusText.transform.right, 
                                                                   Camera.main.WorldToScreenPoint(_radiusPoint.transform.position) - Camera.main.WorldToScreenPoint(_centerPoint.transform.position));
        _radiusText.transform.rotation = rotationToBeAligned * _radiusText.transform.rotation;
    }


    private void UpdateSurfaceText()
    {
        _surfaceText.GetComponentInChildren<Text>().text = (Mathf.PI * Mathf.Pow(_radius,2)).ToString() + "m²";
        _surfaceText.transform.position = Camera.main.WorldToScreenPoint(_centerPoint.transform.position) + new Vector3(-50f, 50f, 0);
    }



    private void PlaceVerticesOverMesh()
    {
        _centerPoint.transform.position = new Vector3(_centerPoint.transform.position.x, _vertexYPosition + 0.01f, _centerPoint.transform.position.z);
        _radiusPoint.transform.position = new Vector3(_radiusPoint.transform.position.x, _vertexYPosition + 0.01f, _radiusPoint.transform.position.z);
    }


    public void MoveVertex(GameObject vertexToMove, Vector3 position)
    {
        if (vertexToMove == _centerPoint)
            transform.position = position;

        else
            vertexToMove.transform.position = position;

        Draw();
    }



    public void Delete()
    {
        foreach(Transform child in transform)
        {
            Destroy(child.gameObject);
        }
        Destroy(_radiusText);
        Destroy(_surfaceText);
        Destroy(this.gameObject);
    }
}
