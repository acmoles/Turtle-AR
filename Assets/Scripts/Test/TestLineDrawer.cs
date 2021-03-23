using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestLineDrawer : MonoBehaviour
{
#pragma warning disable 0649
    [Tooltip("Each detector can draw one line at a time.")]
    [SerializeField]
    private PositionReporter[] _positionReporters;

    [SerializeField]
    private Material _material;

    [SerializeField]
    private Color _drawColor = Color.white;

    [SerializeField]
    private float _drawRadius = 0.2f;

    [SerializeField]
    private int _drawResolution = 8;

    [SerializeField]
    private float _minSegmentLength = 0.05f;
#pragma warning restore 0649

    [Tooltip("Each detector can draw one line at a time.")]
    [SerializeField]
    //private PipeMeshGenerator _pipeDrawer;
    private TubeGenerator _pipeDrawer;

    private Vector3 _prevPoint = Vector3.zero;

    public Color DrawColor
    {
        get
        {
            return _drawColor;
        }
        set
        {
            _drawColor = value;
        }
    }

    public float DrawRadius
    {
        get
        {
            return _drawRadius;
        }
        set
        {
            _drawRadius = value;
        }
    }

    void OnValidate()
    {
        _drawRadius = Mathf.Max(0, _drawRadius);
        _drawResolution = Mathf.Clamp(_drawResolution, 3, 24);
        _minSegmentLength = Mathf.Max(0, _minSegmentLength);
    }

    void Awake()
    {
        if (_positionReporters.Length == 0)
        {
            Debug.LogWarning("No detectors were specified!  LineDraw can not draw any lines without detectors.");
        }
    }

    void Start()
    {

    }

    void Update()
    {
        for (int i = 0; i < _positionReporters.Length; i++)
        {
            var reporter = _positionReporters[i];

            if (reporter.DidStart)
            {
                //drawState.BeginNewLine();
               
            }

            if (reporter.DidStop)
            {
                //drawState.FinishLine();
               
            }

            if (reporter.IsMoving)
            {
                UpdateLine(reporter.Position);

                // If distance travelled is greater than limit, add new point and generate line
            }
        }
    }

    private List<Vector3> pointsList = new List<Vector3>();
    Vector3[][] polylines = new Vector3[1][];

    public void UpdateLine(Vector3 position)
    {

        bool shouldAdd = false;

        //shouldAdd |= _pipeDrawer.points.Count == 0;
        shouldAdd |= _prevPoint == Vector3.zero;
        shouldAdd |= Vector3.Distance(_prevPoint, position) >= _minSegmentLength;

        if (shouldAdd)
        {
            //_pipeDrawer.points.Add(position);
            //if (_pipeDrawer.points.Count >= 2)
            //{
            //    _pipeDrawer.RenderPipe();
            //}

            pointsList.Add(position);
            if (pointsList.Count >= 2)
            {
                polylines[0] = pointsList.ToArray();
                StartCoroutine(_pipeDrawer.Generate(polylines));
            }
        }

        _prevPoint = position;
    }
}
