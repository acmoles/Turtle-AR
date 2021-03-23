using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TubeDrawer : MonoBehaviour
{ 
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
    private float _minSegmentLength = 0.1f;

    private DrawState[] _drawStates;

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
            Debug.LogWarning("No detectors were specified! TubeDraw can not draw any lines without detectors.");
        }
    }

    void Start()
    {
        _drawStates = new DrawState[_positionReporters.Length];
        for (int i = 0; i < _positionReporters.Length; i++)
        {
            _drawStates[i] = new DrawState(this);
        }
    }

    void Update()
    {
        for (int i = 0; i < _positionReporters.Length; i++)
        {
            var reporter = _positionReporters[i];
            var drawState = _drawStates[i];

            if (reporter.DidStart)
            {
                drawState.BeginNewLine();
            }

            if (reporter.DidStop)
            {
                drawState.FinishLine();
            }

            if (reporter.IsMoving)
            {
                drawState.UpdateLine(reporter.Position);
            }
        }
    }

    private void OnDrawGizmos()
    {
        if (_drawStates is null) return;

        foreach (DrawState state in _drawStates)
        {
            foreach (Vector3 linePoint in state._points)
            {
                // Draw circle gizmo
                Gizmos.color = Color.red;
                Gizmos.DrawSphere(linePoint, 0.05f);
            }
        }

    }

    private class DrawState
    {
        private Vector3 _prevPoint = Vector3.zero;
        public List<Vector3> _points = new List<Vector3>();
        private List<Color> _colors = new List<Color>();
        private List<float> _radii = new List<float>();

        private TubeDrawer _parent;

        private Tube _tube;

        private Mesh _mesh;

        private int _startTaper = 3;
        private int _endTaper = 2;

        public DrawState(TubeDrawer parent)
        {
            _parent = parent;
        }

        public GameObject BeginNewLine()
        {
            _prevPoint = Vector3.zero;
            _points.Clear();
            _colors.Clear();
            _radii.Clear();

            // Create empty tube
            _tube = new Tube();

            _mesh = new Mesh();
            _mesh.name = "Line Mesh";
            _mesh.MarkDynamic();

            GameObject lineObj = new GameObject("Line Object");
            lineObj.transform.position = Vector3.zero;
            lineObj.transform.rotation = Quaternion.identity;
            lineObj.transform.localScale = Vector3.one;
            lineObj.AddComponent<MeshFilter>().mesh = _mesh;
            lineObj.AddComponent<MeshRenderer>().sharedMaterial = _parent._material;

            return lineObj;
        }

        public void UpdateLine(Vector3 position)
        {

            bool shouldAdd = false;

            shouldAdd |= _points.Count == 0;
            shouldAdd |= Vector3.Distance(_prevPoint, position) >= _parent._minSegmentLength;
            // TODO should add on angular change i.e. turtle rotating? Need to get turtle direction from reporter.
            // OR flag smooth needed (after rotate) and smooth last five points?
            // OR do dot product line post-process? Looking at Kandinsky line smoothing.

            if (shouldAdd)
            {
                _points.Add(position);
                _colors.Add(_parent.DrawColor); // TODO interface this with tube class
                _radii.Add(_parent.DrawRadius);

                if (_points.Count >= 2)
                {
                    UpdateRadii(); // TODO sets radius curve
                    _tube.Create(
                        _points.ToArray(),        // Polyline points
                        1f,                       // Decimation (not used currently)
                        1f,                       // Scale (not used currently)
                        _radii.ToArray(),         // Radius at point
                        _parent._drawResolution   // Circle resolution
                    );
                    UpdateMesh();
                }
                _prevPoint = position;
            }
        }

        public void FinishLine()
        {
            _mesh.UploadMeshData(true);
        }

        private void UpdateMesh()
        {
            _mesh.SetVertices(_tube.vertices);
            _mesh.SetColors(_tube.colors);
            _mesh.SetUVs(0, _tube.uv);
            _mesh.SetIndices(_tube.tris, MeshTopology.Triangles, 0);
            _mesh.RecalculateBounds();
            _mesh.RecalculateNormals();
            // Modify normals
            Vector3[] normals = _mesh.normals;

            for (int i = _tube.resolution - 1; i < normals.Length; i += _tube.resolution)
            {
                normals[i] = normals[i - _tube.resolution + 1];
            }
            // assign the array of normals to the mesh
            _mesh.normals = normals;
        }

        float UpdateRadii()
        {
            // Iterate polyline length
            for (int i = 0; i < _radii.Count; i++)
            {
                float amount = Mathf.Min(Mathf.Min(1f, (float)i / _startTaper), Mathf.Min(1f, (float)(_radii.Count - i - 1) / _endTaper));
                amount -= 1f;
                amount *= amount;
                // TODO extra ring in first and last segment?
                amount = Mathf.Max(1f - amount, 0.1f);
                //amount = 1f - amount;
                Debug.Log("taper amount: " + amount);

                float progress = (float)i / _radii.Count;
                //Debug.Log("progress: " + progress);
                //_radii[i] = (1f - progress) * _parent.DrawRadius;
                //_radii[i] = Mathf.Sin(50 * progress) * _parent.DrawRadius;
                //_radii[i] = (1f - Mathf.Pow(Mathf.Abs(progress - 0.5f) * 2f, 2f)) * 0.2f;
                _radii[i] = amount * _parent.DrawRadius;
                //Debug.Log(_radii[i]);
            }


            return 1f;
        }

        void GenerateSmooth() // Not used
        {
            Debug.Log("Before smooth: " + _points.Count);
            Vector3[] smoothPoints = LineSmoother.SmoothLine(_points.ToArray(), 1f);
            Debug.Log("After smooth: " + smoothPoints.Length);

            float[] smoothedRadii = new float[smoothPoints.Length];

            for (int i = 0; i < smoothedRadii.Length; i++)
            {
                smoothedRadii[i] = _parent.DrawRadius;
            }

            _points.Clear();

            for (int i = 0; i < smoothPoints.Length; i++)
            {
                _points.Add(smoothPoints[i]);
            }

            _tube.Create(
                smoothPoints,             // Polyline points
                1f,                       // Decimation (not used currently)
                1f,                       // Scale (not used currently)
                smoothedRadii,            // Radius at point
                _parent._drawResolution   // Circle resolution
            );
            UpdateMesh();
        }
    }

}

