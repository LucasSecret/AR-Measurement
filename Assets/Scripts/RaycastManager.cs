using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class RaycastManager : MonoBehaviour
{
    public ShapesManager _shapesManager;
    public Button _placePointButton;
    public Button _movePointButton;
    public Button _deleteVertexButton;

    private RaycastHit _hit;
    private bool _isVertexHover = false;
    private GameObject _vertexHoverObj;

    private bool _moveVertexButtonPressed = false;

    private UIManager _uiManager;

    // Start is called before the first frame update
    void Start()
    {
        _uiManager = FindObjectOfType<UIManager>();
    }

    void Update()
    {
        Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

        if(Physics.Raycast(ray, out _hit))
        {
            //If Raycast hit ARPlane
            if (_hit.transform.CompareTag("ARPlane"))
            {
                if (_moveVertexButtonPressed)
                {
                    _shapesManager.MoveVertex(_vertexHoverObj, _hit.point);
                    _uiManager.RaycastHitPlane(true);
                }
                else
                {
                    //If the user was previously raycasting a vertex
                    if (_isVertexHover)
                    {
                        _isVertexHover = false;
                        _shapesManager.VertexHover(_vertexHoverObj, false);
                        _vertexHoverObj = null;
                    }

                    _uiManager.RaycastHitPlane(false);
                }
            }


            //If Raycast hit a vertex
            else if (_hit.transform.CompareTag("Vertices"))
            {
                //If user is already moving another vertex
                if (_moveVertexButtonPressed)
                    return;

                _uiManager.RaycastHitVertex();
                _isVertexHover = true;
                _vertexHoverObj = _hit.collider.gameObject;
                _shapesManager.VertexHover(_vertexHoverObj, true);
            }

            //If Raycast hit something but we don't care 
            else
            {
                _uiManager.RaycastHitNothing();
                if(_isVertexHover)
                {
                    _shapesManager.VertexHover(_vertexHoverObj, false);
                    _vertexHoverObj = null;
                }

                _isVertexHover = false;
            }
        }

        //If Raycast did not hit any object
        else
            _uiManager.RaycastHitNothing();

    }

    public void PlacePointButtonClicked()
    {
        _shapesManager.PlaceVertex(_hit.point);
    }


    public void MoveVertexButtonPressed()
    {
        _moveVertexButtonPressed = true;
        Debug.Log("Move button PRESSED");
        if (_vertexHoverObj)
            _shapesManager.StartMovingVertex(_vertexHoverObj);
    }


    public void MoveVertexButtonReleased()
    {
        _moveVertexButtonPressed = false;
        Debug.Log("Move button RELEASED");

        if (_vertexHoverObj)
            _shapesManager.StopMovingVertex(_vertexHoverObj);
    }


    public void DeleteButtonClicked()
    {
        if (_vertexHoverObj)
            _shapesManager.DeleteVertex(_vertexHoverObj);
    }

    public void EndLineButtonClicked()
    {
        _shapesManager.EndLine();
    }

}
