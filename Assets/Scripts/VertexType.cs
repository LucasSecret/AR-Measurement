using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum VERTEX_SHAPE_TYPE
{
    LINE,
    CIRCLE,
    TRIANGLE,
    SQUARE,
    PENTAGONE,
    BOX,
    NOT_AFFECTED_YET
}
public class VertexType : MonoBehaviour
{
    public VERTEX_SHAPE_TYPE ShapeType { get; set; }
}
