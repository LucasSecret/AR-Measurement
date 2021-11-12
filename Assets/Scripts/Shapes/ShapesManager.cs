using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//Enum used to define which type of shape user want to draw
public enum WANTED_SHAPE
{
    LINE,
    CIRCLE,
    TRIANGLE,
    SQUARE,
    POLYGON,
    BOX
}

public class ShapesManager : MonoBehaviour
{
    public GameObject _linePrefab;
    public GameObject _circlePrefab;
    public GameObject _trianglePrefab;
    public GameObject _squarePrefab;
    public GameObject _boxPrefab;
    public GameObject _polygonPrefab;
     
    public GameObject _shapeVerticePrefabs; 
    public GameObject _shapesContainer; 

    public Material _classicVertexMaterial; //The classic material of vertex (black one)
    public Material _hoverVertexMaterial; //The material set to the vertex when the user hovers it (green one)
    public Material _movingVertexMaterial; //The material set to the vertex when the user moves it (blue one)

    private List<GameObject> _verticesCreated = new List<GameObject>();

    private WANTED_SHAPE _wantedShape;

    private Line _currentLine;
    private Polygon _currentPolygon;



    //Called to instantiate a vertex, and create a shape if the number of vertices created correspond to the selected shape
    public void PlaceVertex(Vector3 hitPosition)
    {
        GameObject vertex = Instantiate(_shapeVerticePrefabs, hitPosition, Quaternion.Euler(90, 0, 0));
        _verticesCreated.Add(vertex);

        if (_verticesCreated.Count > 1)
        {
            foreach (GameObject v in _verticesCreated)
                v.GetComponent<VertexType>().ShapeType = (VERTEX_SHAPE_TYPE)_wantedShape;
        }

        else vertex.GetComponent<VertexType>().ShapeType = VERTEX_SHAPE_TYPE.NOT_AFFECTED_YET;

        //Create a shape according to the wanted shape and the number of vertices created
        switch (_wantedShape)
        {
            case WANTED_SHAPE.LINE:
                if (_verticesCreated.Count == 2 && !_currentLine)
                    CreateLine();

                else if (_verticesCreated.Count > 2 &&  _currentLine) //If a line is currently been drawing
                    _currentLine.AddVertice(vertex);
                break;

            case WANTED_SHAPE.CIRCLE:
                if (_verticesCreated.Count == 2)
                    CreateCircle();
                break;

            case WANTED_SHAPE.TRIANGLE:
                if (_verticesCreated.Count == 3)
                    CreateTriangle();
                break;

            case WANTED_SHAPE.SQUARE:
                if (_verticesCreated.Count == 2)
                    CreateSquare();
                break;

            case WANTED_SHAPE.POLYGON:
                if (_verticesCreated.Count == 2 && !_currentPolygon)
                    CreatePolygon();

                else if (_verticesCreated.Count > 2 && _currentPolygon) //If a polygon is currently been drawing
                    _currentPolygon.AddVertice(vertex);
                break;

            case WANTED_SHAPE.BOX:
                if (_verticesCreated.Count == 2)
                    CreateBox();
                break;
            default:
                break;
        }

       
    }

    //Return true if the user can end the current line or polygon (i.e the user created a sufficent number of vertex)
    public bool UserCanEndLine() 
    {
        return (_wantedShape == WANTED_SHAPE.LINE || _wantedShape == WANTED_SHAPE.POLYGON) && _verticesCreated.Count >= 2;
    }

    //Called when user want to end the current line or polygon
    public void EndLine()
    {
        _verticesCreated.Clear();
        _currentLine = null;

        if (_wantedShape == WANTED_SHAPE.POLYGON && _currentPolygon)
            _currentPolygon.EndShape();
        
        _currentPolygon = null;

    }



    private void CreateLine()
    {
        GameObject line = Instantiate(_linePrefab, _shapesContainer.transform);
        Line lineScript = line.GetComponent<Line>();
        lineScript.Init(_verticesCreated);
        _currentLine = lineScript;
    }



    private void CreateCircle()
    {
        GameObject circle = Instantiate(_circlePrefab, _shapesContainer.transform);
        circle.transform.position = Vector3.zero; //  _verticesCreated[0].transform.position;
        Circle circleScript = circle.GetComponent<Circle>();
        circleScript._centerPoint = _verticesCreated[0];
        circleScript._radiusPoint = _verticesCreated[1];

        circleScript.Init();
        circleScript.Draw();
        _verticesCreated.Clear();
    }



    private void CreateTriangle()
    {
        GameObject triangle = Instantiate(_trianglePrefab, _shapesContainer.transform);
        triangle.transform.position = Vector3.zero;
        Triangle triangleScript = triangle.GetComponent<Triangle>();
        
        triangleScript.Init(_verticesCreated);
        _verticesCreated.Clear();
    }


    private void CreateSquare()
    {
        GameObject square = Instantiate(_squarePrefab, _shapesContainer.transform);
        square.transform.position = Vector3.zero;
        Square squareScript = square.GetComponent<Square>();

        squareScript.Init(_verticesCreated[0], _verticesCreated[1]);
        _verticesCreated.Clear();
    }



    private void CreatePolygon()
    {
        GameObject polygon = Instantiate(_polygonPrefab, _shapesContainer.transform);
        Polygon polygonScript = polygon.GetComponent<Polygon>();
        polygonScript.Init(_verticesCreated);
        _currentPolygon = polygonScript;
    }



    private void CreateBox()
    {
        GameObject box = Instantiate(_boxPrefab, _shapesContainer.transform);
        box.transform.position = Vector3.zero;
        Box boxScript = box.GetComponent<Box>();

        boxScript.Init(_verticesCreated[0], _verticesCreated[1]);
        _verticesCreated.Clear();
    }


    public void ChangeWantedShape(WANTED_SHAPE wantedShape)
    {
        _wantedShape = wantedShape;

        foreach (GameObject vertex in _verticesCreated)
            Destroy(vertex);

        _verticesCreated.Clear();
    }


    public void VertexHover(GameObject vertex, bool hover)
    {
        if (!vertex)
            return;

        if (hover)
            vertex.GetComponent<MeshRenderer>().sharedMaterial = _hoverVertexMaterial;

        else vertex.GetComponent<MeshRenderer>().sharedMaterial = _classicVertexMaterial;
    }

    //Find the correct shape type of which vertex is a part of, to call the correct class 
    public void MoveVertex(GameObject vertex, Vector3 position)
    {
        switch(vertex.GetComponent<VertexType>().ShapeType)
        {
            case VERTEX_SHAPE_TYPE.LINE:
                vertex.GetComponentInParent<Line>().MoveVertex(vertex, position);
                break;

            case VERTEX_SHAPE_TYPE.CIRCLE:
                vertex.GetComponentInParent<Circle>().MoveVertex(vertex, position);
                break;

            case VERTEX_SHAPE_TYPE.TRIANGLE:
                vertex.GetComponentInParent<Triangle>().MoveVertex(vertex, position);
                break;

            case VERTEX_SHAPE_TYPE.SQUARE:
                vertex.GetComponentInParent<Square>().MoveVertex(vertex, position);
                break;

            case VERTEX_SHAPE_TYPE.PENTAGONE:
                vertex.GetComponentInParent<Polygon>().MoveVertex(vertex, position);
                break;

            case VERTEX_SHAPE_TYPE.BOX:
                vertex.GetComponentInParent<Box>().MoveVertex(vertex, position);
                break;

            case VERTEX_SHAPE_TYPE.NOT_AFFECTED_YET:
                vertex.transform.position = position;
                break;

            default:
                break;
        }
        
    }

    //Called when user press the hand button to move a vertex
    public void StartMovingVertex(GameObject vertex)
    {
        vertex.GetComponent<Renderer>().material = _movingVertexMaterial;
        vertex.GetComponent<MeshCollider>().enabled = false;
    }

    //Called when user release the hand button to move a vertex
    public void StopMovingVertex(GameObject vertex)
    {
        vertex.GetComponent<Renderer>().material = _classicVertexMaterial;
        vertex.GetComponent<MeshCollider>().enabled = true;
    }


    public void DeleteVertex(GameObject vertex)
    {
        switch (vertex.GetComponent<VertexType>().ShapeType)
        {
            case VERTEX_SHAPE_TYPE.LINE:
                vertex.GetComponentInParent<Line>().DeleteVertex(vertex);
                break;

            case VERTEX_SHAPE_TYPE.CIRCLE:
                vertex.GetComponentInParent<Circle>().Delete();
                break;

            case VERTEX_SHAPE_TYPE.TRIANGLE:
                vertex.GetComponentInParent<Triangle>().Delete();
                break;

            case VERTEX_SHAPE_TYPE.SQUARE:
                vertex.GetComponentInParent<Square>().Delete();
                break;

            case VERTEX_SHAPE_TYPE.PENTAGONE:
                vertex.GetComponentInParent<Polygon>().DeleteVertex(vertex);
                break;

            case VERTEX_SHAPE_TYPE.BOX:
                vertex.GetComponentInParent<Box>().Delete();
                break;

            case VERTEX_SHAPE_TYPE.NOT_AFFECTED_YET:
                Destroy(vertex);
                break;

            default:
                break;
        }

        //If the user is building a shape and want to delete a vertex in this non-ended shape, destroy the vertex inside the building vertices list
        if (_verticesCreated.Count > 0)
            _verticesCreated.Remove(vertex);
    }
}
