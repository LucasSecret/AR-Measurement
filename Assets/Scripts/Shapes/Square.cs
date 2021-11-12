using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(MeshFilter))]
public class Square : MonoBehaviour
{
    private GameObject _centerPoint;
    private GameObject _sidePoint;

    private List<Vector3> _invisibleVerticesList;

    private List<GameObject> _textsList;

    private LineRenderer _lineRenderer;
    private GameObject _surfaceText;
    private GameObject _sideText;

    private UIManager _uiManager;
    private bool _textsAreCreated;

    private float _vertexYPosition;

    // Update is called once per frame
    void Update()
    {
        if (!_textsAreCreated)
            return;

        UpdateTexts();
    }




    public void Init(GameObject centerPoint, GameObject sidePoint)
    {
        this.transform.position = centerPoint.transform.position;

        _centerPoint = centerPoint;
        _centerPoint.transform.SetParent(this.transform);
        _sidePoint = sidePoint;
        _sidePoint.transform.SetParent(this.transform);

        _textsList = new List<GameObject>();
        _uiManager = FindObjectOfType<UIManager>();

        _invisibleVerticesList = new List<Vector3>();

        _lineRenderer = GetComponent<LineRenderer>();

        _vertexYPosition = _centerPoint.transform.position.y;

        CreateInvisiblesVertices();
        CreateTexts();
        Draw();
    }




    private void CreateInvisiblesVertices()
    {
        for(int i=0; i<4;i++)
            _invisibleVerticesList.Add(Vector3.zero);

        UpdateInvisibleVerticesPosition();
    }



    private void UpdateInvisibleVerticesPosition()
    {
        Vector3 centerPos = _centerPoint.transform.position;
        Vector3 sidePos = _sidePoint.transform.position;

        Vector3 sideDirection = (centerPos - sidePos).normalized;
        Vector3 upDirection = (Quaternion.Euler(0, 90, 0) * sideDirection).normalized;

        float demiSideLength = Vector3.Distance(centerPos, sidePos);
        
        _invisibleVerticesList[0] = sidePos - (upDirection * demiSideLength);
        _invisibleVerticesList[1] = sidePos - (upDirection * demiSideLength) + (2 * demiSideLength * sideDirection);
        _invisibleVerticesList[2] = sidePos + (upDirection * demiSideLength) + (2 * demiSideLength * sideDirection);
        _invisibleVerticesList[3] = sidePos + (upDirection * demiSideLength);

    }




    private void CreateTexts()
    {
        if (!_uiManager)
            return;

        _sideText = Instantiate(_uiManager.textInfosPrefab, _uiManager.transform);
        _surfaceText = Instantiate(_uiManager.textSpecialeInfosPrefab, _uiManager.transform);
        _textsAreCreated = true;

    }




    private void UpdateTexts()
    {
        Vector3 surfaceTextPosition = Vector3.zero;

        for (int i = 0; i < _invisibleVerticesList.Count; i++)
            surfaceTextPosition += _invisibleVerticesList[i];
      
        //Update side distance text
        float distance = Vector3.Distance(_invisibleVerticesList[0], _invisibleVerticesList[1]);
        _sideText.GetComponentInChildren<Text>().text = (distance * 100).ToString() + " cm";
        _sideText.transform.position = Camera.main.WorldToScreenPoint((_invisibleVerticesList[0] + _invisibleVerticesList[1]) / 2);
        Quaternion rotationToBeAligned = Quaternion.FromToRotation(_sideText.transform.right, Camera.main.WorldToScreenPoint(_invisibleVerticesList[1]) - Camera.main.WorldToScreenPoint(_invisibleVerticesList[0]));
        _sideText.transform.rotation = rotationToBeAligned * _sideText.transform.rotation;

        //Update surface text
        float surface = Mathf.Pow(Vector3.Distance(_invisibleVerticesList[0], _invisibleVerticesList[1]), 2);
        _surfaceText.GetComponentInChildren<Text>().text = surface.ToString() + " m²";
        _surfaceText.transform.position = Camera.main.WorldToScreenPoint(surfaceTextPosition / 4);

    }



    private void Draw()
    {
        UpdateInvisibleVerticesPosition();
        DrawEdges();
        DrawMesh();
        PlaceVerticesOverMesh();
        UpdateVertexScale();
    }



    private void DrawEdges()
    {
        _lineRenderer.positionCount = _invisibleVerticesList.Count + 1;

        for (int i = 0; i < _invisibleVerticesList.Count + 1; i++)
            _lineRenderer.SetPosition(i, _invisibleVerticesList[i % _invisibleVerticesList.Count]);
    }



    private void DrawMesh()
    {
        Mesh mesh = GetComponent<MeshFilter>().mesh;
        mesh.Clear();

        List<Vector3> vertices = new List<Vector3>();
        foreach (Vector3 vertexPosition in _invisibleVerticesList)
            vertices.Add(transform.InverseTransformPoint(vertexPosition));


        List<int> triangles = new List<int> { 0, 1, 2,
                                              0, 2, 3,
                                              2, 1, 0,
                                              3, 2, 0};

        List<Vector3> normalsList = new List<Vector3>();

        for (int i = 0; i < vertices.Count; i++)
            normalsList.Add(transform.up);
        

        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.normals = normalsList.ToArray();
    }

    

    private void PlaceVerticesOverMesh()
    {
        _centerPoint.transform.position = new Vector3(_centerPoint.transform.position.x, _vertexYPosition + 0.01f, _centerPoint.transform.position.z);
        _sidePoint.transform.position = new Vector3(_sidePoint.transform.position.x, _vertexYPosition + 0.01f, _sidePoint.transform.position.z);
    }


    private void UpdateVertexScale()
    {
        float demiSideDistance = Vector3.Distance(_centerPoint.transform.position, _sidePoint.transform.position);

        _centerPoint.transform.localScale = _sidePoint.transform.localScale = 0.35f * demiSideDistance * Vector3.one;
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


    public void MoveVertex(GameObject vertexToMove, Vector3 position)
    {
        if (vertexToMove == _centerPoint)
            transform.position = position;

        else
            vertexToMove.transform.position = position;

        Draw();
    }

}
