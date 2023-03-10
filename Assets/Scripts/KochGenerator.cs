using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KochGenerator : MonoBehaviour
{

    protected enum _axis
    {
        Xaxis,
        Yaxis,
        Zaxis
    };

    [SerializeField]
    protected _axis axis = new _axis();

    protected enum _initiator
    {
        Triangle,
        Square,
        Pentagon,
        Hexagon,
        Heptagon,
        Octagon
    };

    public struct LineSegment
    {
        public Vector3 StartPosition { get; set; }
        public Vector3 EndPosition { get; set; }
        public Vector3 Direction { get; set; }
        public float Length { get; set; }


    }

    [SerializeField]
    protected _initiator initiator = new _initiator();

    [SerializeField]
    protected AnimationCurve _generator;
    protected Keyframe[] _keys;

    [SerializeField]
    protected bool _useBezierCurves;

    [SerializeField]
    [Range(8,24)]
    protected int _bezierVertexCount;

    protected int _generationCount;

    protected int _initiatorPointAmount;

    private Vector3[] _initiatorPoints;

    private Vector3 _rotateVector;
    private Vector3 _rotateAxis;

    [SerializeField]
    protected float _initiatorSize;

    private float _initialRotation;

    protected Vector3[] _position;
    protected Vector3[] _targetPosition;
    protected Vector3[] _bezierPosition;
    public List<LineSegment> _lineSegment;

    protected Vector3[] BezierCurve (Vector3[] points, int vertexCount)
    {
        var pointList = new List<Vector3>();
        for (int i = 0; i < points.Length; i += 2)
        {
            if (i+2 <= points.Length - 1)
            {
                for (float ratio = 0f; ratio <= 1f; ratio += 1.0f/vertexCount)
                {
                    var tangentLineVertex1 = Vector3.Lerp(points[i], points[i + 1], ratio);
                    var tangentLineVertex2 = Vector3.Lerp(points[i+1], points[i + 2], ratio);
                    var bezierpoint = Vector3.Lerp(tangentLineVertex1, tangentLineVertex2, ratio);
                    pointList.Add(bezierpoint);
                }
            }
        }
        return pointList.ToArray();
    }

    private void Awake()
    {
        GetInitiatorPoints();

        //assign lists & arrays
        _position = new Vector3[_initiatorPointAmount+1];
        _targetPosition = new Vector3[_initiatorPointAmount + 1];
        _lineSegment = new List<LineSegment>();
        _keys = _generator.keys;

        _rotateVector = Quaternion.AngleAxis(_initialRotation, _rotateAxis) * _rotateVector;

        for (int i = 0; i < _initiatorPointAmount; i++)
        {
            _position[i] = _rotateVector * _initiatorSize;
            _rotateVector = Quaternion.AngleAxis(360 / _initiatorPointAmount, _rotateAxis) * _rotateVector;

        }
        _position[_initiatorPointAmount] = _position[0];
        _targetPosition = _position;
    }

    protected void KochGenerate (Vector3[] positions, bool outwards, float generatorMultiplier)
    {
        //creating line segments
        _lineSegment.Clear();
        for (int i=0; i< positions.Length; i++)
        {
            LineSegment line = new LineSegment();
            line.StartPosition = positions[i];
            if (i == positions.Length - 1)
            {
                line.EndPosition = positions[0];
            } else
            {
                line.EndPosition = positions[i + 1];
            }
            line.Direction = (line.EndPosition - line.StartPosition).normalized;
            line.Length = Vector3.Distance(line.EndPosition, line.StartPosition);
            _lineSegment.Add(line);
        }

        //add the line segment points to a point array
        List<Vector3> newPos = new List<Vector3>();
        List<Vector3> targetPos = new List<Vector3>();

        for (int i=0; i< _lineSegment.Count; i++)
        {
            newPos.Add(_lineSegment[i].StartPosition);
            targetPos.Add(_lineSegment[i].StartPosition);

            for (int j = 1; j < _keys.Length; j++)
            {
                float moveAmount = _lineSegment[i].Length * _keys[j].time;
                float heightAmount = (_lineSegment[i].Length * _keys[j].value) * generatorMultiplier;
                Vector3 movePos = _lineSegment[i].StartPosition + (_lineSegment[i].Direction * moveAmount);

                Vector3 Dir;
                if (outwards)
                {
                    Dir = Quaternion.AngleAxis(-90, _rotateAxis) * _lineSegment[i].Direction;
                }
                else
                {
                    Dir = Quaternion.AngleAxis(90, _rotateAxis) * _lineSegment[i].Direction;
                }
                newPos.Add(movePos);
                targetPos.Add(movePos + (Dir * heightAmount));
            }
        }

        newPos.Add(_lineSegment[0].StartPosition);
        targetPos.Add(_lineSegment[0].StartPosition);
        _position = new Vector3[newPos.Count];
        _targetPosition = new Vector3[targetPos.Count];
        _position = newPos.ToArray();
        _targetPosition = targetPos.ToArray();
        _bezierPosition = BezierCurve(_targetPosition, _bezierVertexCount);

        _generationCount++;
    }

    private void OnDrawGizmos()
    {

        GetInitiatorPoints();
        _initiatorPoints = new Vector3[_initiatorPointAmount];

        _rotateVector = Quaternion.AngleAxis(_initialRotation, _rotateAxis) * _rotateVector;

        for (int i=0; i<_initiatorPointAmount; i++)
        {
            _initiatorPoints[i] = _rotateVector * _initiatorSize;
            _rotateVector = Quaternion.AngleAxis(360 / _initiatorPointAmount, _rotateAxis) * _rotateVector;
            
        }

        for (int i = 0; i < _initiatorPointAmount; i++)
        {
            Gizmos.color = Color.white;

            Matrix4x4 rotationMatrix = Matrix4x4.TRS(transform.position, transform.rotation, transform.lossyScale);
            Gizmos.matrix = rotationMatrix;

            if (i < _initiatorPointAmount - 1)
            {
                Gizmos.DrawLine(_initiatorPoints[i], _initiatorPoints[i + 1]);
            } else
            {
                Gizmos.DrawLine(_initiatorPoints[i], _initiatorPoints[0]);
            }
        }
    }

    private void GetInitiatorPoints ()
    {
        switch (initiator)
        {
            case _initiator.Triangle:
                _initiatorPointAmount = 3;
                _initialRotation = 0;
                break;
            case _initiator.Square:
                _initiatorPointAmount = 4;
                _initialRotation = 45;
                break;
            case _initiator.Pentagon:
                _initiatorPointAmount = 5;
                _initialRotation = 36;
                break;
            case _initiator.Hexagon:
                _initiatorPointAmount = 6;
                _initialRotation = 30;
                break;
            case _initiator.Heptagon:
                _initiatorPointAmount = 7;
                _initialRotation = 25.71428f;
                break;
            case _initiator.Octagon:
                _initiatorPointAmount = 8;
                _initialRotation = 22.5f;
                break;
            default:
                _initiatorPointAmount = 3;
                _initialRotation = 0;
                break;
        }

        switch (axis)
        {
            case _axis.Xaxis:
                _rotateVector = new Vector3(1, 0, 0);
                _rotateAxis = new Vector3(0, 0, 1);
                break;
            case _axis.Yaxis:
                _rotateVector = new Vector3(0, 1, 0);
                _rotateAxis = new Vector3(1, 0, 0);
                break;
            case _axis.Zaxis:
                _rotateVector = new Vector3(0, 0, 1);
                _rotateAxis = new Vector3(0, 1, 0);
                break;
            default:
                _rotateVector = new Vector3(0, 1, 0);
                _rotateAxis = new Vector3(1, 0, 0);
                break;

        }
    }
}
