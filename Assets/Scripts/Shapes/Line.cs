using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Line : MonoBehaviour
{
    private List<GameObject> _verticesList;

    private List<GameObject> _textsList;
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


    private void Draw()
    {
        _lineRenderer.positionCount = _verticesList.Count;

        for (int i = 0; i < _verticesList.Count; i++)
            _lineRenderer.SetPosition(i, _verticesList[i].transform.position);
    }

    private void CreateTexts()
    {
        if (!_uiManager)
            return;

        for(int i=0; i<_verticesList.Count -1; i++)
            _textsList.Add(Instantiate(_uiManager.textInfosPrefab, _uiManager.transform));
        
        _textsAreCreated = true;
    }


    private void UpdateTexts()
    {
        Quaternion rotationToBeAligned;

        for (int i=0; i<_textsList.Count; i++)
        {
            float distance = Vector3.Distance(_verticesList[i].transform.position, _verticesList[i + 1].transform.position);
            _textsList[i].GetComponentInChildren<Text>().text = (distance * 100).ToString() + " cm";
            _textsList[i].transform.position = Camera.main.WorldToScreenPoint((_verticesList[i].transform.position + _verticesList[i+1].transform.position) / 2);

            rotationToBeAligned = Quaternion.FromToRotation(_textsList[i].transform.right,
                                                         Camera.main.WorldToScreenPoint(_verticesList[i + 1].transform.position) -
                                                         Camera.main.WorldToScreenPoint(_verticesList[i].transform.position));

            _textsList[i].transform.rotation = rotationToBeAligned * _textsList[i].transform.rotation;
        }
    }

    public void AddVertice(GameObject vertex)
    {
        _verticesList.Add(vertex);
        vertex.transform.SetParent(this.transform);
        Draw();

        _textsList.Add(Instantiate(_uiManager.textInfosPrefab, _uiManager.transform));

     

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
