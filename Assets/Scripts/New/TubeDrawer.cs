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
            foreach (Vector3 linePoint in state._smoothPoints)
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
        public List<Vector3> _smoothPoints = new List<Vector3>();
        private List<Color> _colors = new List<Color>();
        private List<float> _radii = new List<float>();

        private TubeDrawer _parent;

        private Tube _tube;

        private Mesh _mesh;

        private int _startTaper = 3;
        private int _endTaper = 3;
        private int _amountToAverage = 3;

        public DrawState(TubeDrawer parent)
        {
            _parent = parent;
        }

        public GameObject BeginNewLine()
        {
            _prevPoint = Vector3.zero;
            _points.Clear();
            _smoothPoints.Clear();
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
                _smoothPoints.Clear();
                for (int i = 0; i < _points.Count; i++)
                {
                    AveragePoints(i);
                }
                _colors.Add(_parent.DrawColor); // TODO interface this with tube class
                _radii.Add(_parent.DrawRadius);

                if (_points.Count >= 2)
                {
                    UpdateRadii();
                    _tube.Create(
                        _smoothPoints.ToArray(),  // Polyline points
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
            // Post process end vertices
            for (int i = 0; i < _tube.resolution; i++)
            {
                // Start vertices
                _tube.vertices[i] = Vector3.Lerp(_smoothPoints[0], _smoothPoints[1], 0.6f);
            }
            int lastFullRingVerts = _tube.vertices.Length - 4 - _tube.resolution * 5;
            for (int i = lastFullRingVerts; i < _tube.vertices.Length; i++)
            {
                // End vertices
                _tube.vertices[i] = Vector3.Lerp(_smoothPoints[_points.Count - 2], _smoothPoints[_points.Count - 1], 0.4f);
            }

            _mesh.SetVertices(_tube.vertices);
            _mesh.SetColors(_tube.colors);
            _mesh.SetUVs(0, _tube.uv);
            _mesh.SetIndices(_tube.tris, MeshTopology.Triangles, 0);
            _mesh.RecalculateBounds();
            _mesh.RecalculateNormals();

            // Modify normals
            Vector3[] normals = _mesh.normals;

            int lastFullRing = (_tube.vertices.Length - 2) - _tube.resolution * 5;
            for (int i = _tube.resolution - 1; i < lastFullRing; i += _tube.resolution)
            {
                normals[i] = normals[i - _tube.resolution + 1];
            }
            // End point normal
            for (int i = lastFullRing; i < normals.Length; i++)
            {
                normals[i] = normals[lastFullRing - 1];
            }
            // Start point normal
            Vector3 startNormal = Vector3.zero;
            for (int i = 0; i < _tube.resolution; i++)
            {
                startNormal += normals[i];
                startNormal.Normalize();
            }
            for (int i = 0; i < _tube.resolution; i++)
            {
                normals[i] = startNormal;
            }
            // assign the array of normals to the mesh
            _mesh.SetNormals(normals);
        }

        float UpdateRadii()
        {
            // Iterate polyline length
            for (int i = 0; i < _radii.Count; i++)
            {
                float amount = Mathf.Min(Mathf.Min(1f, (float)i / _startTaper), Mathf.Min(1f, (float)(_radii.Count - i - 1) / _endTaper));
                amount -= 1f;
                amount *= amount;
                amount = 1f - amount;
                //Debug.Log("taper amount: " + amount);

                float progress = (float)i / _radii.Count;
                amount += 0.16f * (Mathf.Cos(progress) * Mathf.Cos(progress * 300f) * Mathf.Cos(progress * 800f));
                //Debug.Log("progress: " + progress);
                //_radii[i] = (1f - Mathf.Pow(Mathf.Abs(progress - 0.5f) * 2f, 2f)) * 0.2f;

                // Special case for end vertices
                if (i == 0 || i == _radii.Count - 1)
                {
                    amount = 0f;
                }

                _radii[i] = amount * _parent.DrawRadius;
                //Debug.Log(_radii[i]);
            }


            return 1f;
        }

        void AveragePoints(int index)
        {
            if (index <= _startTaper + _amountToAverage
                || index >= _points.Count - _endTaper)
            {
                _smoothPoints.Add(_points[index]);
                return;
            }

            Vector3 acumulator = Vector3.zero;
            Vector3 averageVector;

            for (int i = -1; i < _amountToAverage - 1; i++)
            {
                acumulator += _points[index + i];
            }
            averageVector = acumulator / (float)_amountToAverage;

            _smoothPoints.Add(averageVector);
        }
    }
}

