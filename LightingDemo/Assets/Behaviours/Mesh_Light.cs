using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Used to contain two floats relating to X and Y coordinates
/// </summary>
public class Point
{
    public float x = 0f;
    public float y = 0f;

    public Point(float _x, float _y)
    {
        x = _x;
        y = _y;
    }
}

/// <summary>
/// Inherits from Point
/// Used for each endpoint on a segment
/// </summary>
public class EndPoint: Point
{
    //Is this the beginning of a segment
    public bool begin = false;
    //Which segment does this belong to
    public Segment segment = null;
    //Angle of the endpoint to the origin
    public float angle = 0f;
    /// <summary>
    /// Endpoint constructor
    /// </summary>
    /// <param name="_x">X coordinate</param>
    /// <param name="_y">Y coordinate</param>
    public EndPoint(float _x, float _y) : base(_x,_y)
    {
        x = _x;
        y = _y;
    }
}

/// <summary>
/// A segment is a container of two endpoints
/// </summary>
public class Segment
{
    //Which LightOccluder does this Segment belong to
    public LightOccluder parent;
    //First Endpoint
    public EndPoint p1;
    //Second Endpoint
    public EndPoint p2;
}

/// <summary>
/// Main class for mesh lighting system
/// Implementaion derived from http://www.redblobgames.com/articles/visibility/
/// </summary>
public class Mesh_Light : MonoBehaviour
{

    //Material to render to mesh
    public Material m_lightMaterial;

    //Light colour, opacity controls strength of light
    public Color m_lightColor = Color.white;

    //Size of the light in Unity units
    public float m_lightSize = 3f;

    //Occluders to ignore in lighting calculations
    public LightOccluder[] m_ignoredOccluders;
    
    //All segments
    private List<Segment> m_allSegments = new List<Segment>();

    //All endpoints
    private List<EndPoint> m_allEndPoints = new List<EndPoint>();

    //Position of the light last frame
    private Point m_lightPosition = new Point(0f, 0f);

    //Holds Segments in current sweep
    private LinkedList<Segment> m_sweepList = new LinkedList<Segment>();

    //Temporary list of Points
    public List<Point> m_outputPoints = new List<Point>();

    //Temporary list of Vector3 Points
    private List<Vector3> m_vector3PointList = new List<Vector3>();
    
    //Primary mesh renderer
    public MeshRenderer m_meshRenderer;

    //Primary mesh filter
    public MeshFilter m_meshFilter;

    //Instance of light material
    private Material m_lightMaterialInstance;

    //Secondary mesh renderer
    public MeshRenderer m_expandedMeshRenderer;

    //Secondary mesh filter
    public MeshFilter m_expandedMeshFilter;

    //Maximum vertices per mesh
    private const int m_meshVertexCount = 512;

    //Temporary vertex and triangle lists
    private Vector3[] m_tempMeshVertices = new Vector3[m_meshVertexCount];
    private int[] m_tempMeshTriangles = new int[m_meshVertexCount * 3];
    private Vector3[] m_tempExpandedMeshVertices = new Vector3[m_meshVertexCount];

    /// <summary>
    /// Run on light creation
    /// </summary>
    void Start ()
    {
        //Instantiate a new light material so we can change the light colour independantly of other lights
        m_lightMaterialInstance = new Material(m_lightMaterial);
        m_lightMaterialInstance.color = m_lightColor;

        //Create a new mesh, mark it as dynamic and give it default vertex and triangle arrays
        m_meshFilter.mesh = new Mesh();
        m_meshFilter.mesh.MarkDynamic();
        m_meshRenderer.material = m_lightMaterialInstance;
        m_meshFilter.mesh.vertices = new Vector3[m_meshVertexCount];
        m_meshFilter.mesh.vertices[0] = Vector3.zero;
        m_meshFilter.mesh.triangles = new int[m_meshVertexCount * 3];

        //Repeat for expanded mesh
        m_expandedMeshFilter.mesh = new Mesh();
        m_expandedMeshRenderer.material = m_lightMaterialInstance;
        m_expandedMeshFilter.mesh.MarkDynamic();
        m_expandedMeshFilter.mesh.vertices = new Vector3[m_meshVertexCount];
        m_expandedMeshFilter.mesh.vertices[0] = Vector3.zero;
        m_expandedMeshFilter.mesh.triangles = new int[m_meshVertexCount * 3];

        //First pass of colliders
        UpdateLightPosition();
    }

    /// <summary>
    /// Run every frame
    /// </summary>
    public void Update()
    {
        m_lightMaterialInstance.color = m_lightColor;
    }

    /// <summary>
    /// Run immediately after update
    /// </summary>
    void LateUpdate () {
        //Make sure the light does not rotate
        transform.rotation = Quaternion.identity;

        //Clear the endpoints and segments every frame
        m_allEndPoints.Clear();
        m_allSegments.Clear();

        //Five step process
        CreateSegmentsFromOccluders();
        UpdateLightPosition();
        Sweep();
        CreatePolyList();
        BuildMesh();
    }

    //Adds boundary segments and adjust segments to light position
    void UpdateLightPosition()
    {
        LoadSquare(m_lightSize);
        SetLightPosition(transform.position.x, transform.position.y);
    }

    /// <summary>
    /// Turns a list of type Points into a list of type Vector3
    /// </summary>
    void CreatePolyList()
    {
        m_vector3PointList.Clear();
        for (int i = 0; i < m_outputPoints.Count; i++)
        {
            Vector3 v = new Vector3();
            v.x = m_outputPoints[i].x;
            v.y = m_outputPoints[i].y;
            v.z = 0f;
            m_vector3PointList.Add(v);
        }
    }

    /// <summary>
    /// Convert list of vertices into UV coordinates
    /// </summary>
    /// <param name="vertices">List of vertices to be converted</param>
    /// <returns>UV list</returns>
    Vector2[] BuildUVs(Vector3[] vertices)
    {
        float xMin = m_lightSize;
        float yMin = m_lightSize;
        float xMax = -m_lightSize;
        float yMax = -m_lightSize;

        float xRange = xMax - xMin;
        float yRange = yMax - yMin;

        Vector2[] uvs = new Vector2[vertices.Length];
        for (int i = 0; i < vertices.Length; i++)
        {
            uvs[i].x = (vertices[i].x - xMin) / xRange;
            uvs[i].y = (vertices[i].y - yMin) / yRange;

        }
        return uvs;
    }

    /// <summary>
    /// Use the generated vertices to generate a two meshes
    /// </summary>
    void BuildMesh()
    {
        //If there aren't enough points to build the mesh then return
        if (m_vector3PointList == null || m_vector3PointList.Count < 3)
        {
            return;
        }

        //If there are more points than vertices in the mesh, throw an error and return
        if(m_vector3PointList.Count >= m_meshVertexCount)
        {
            Debug.Log("Error, mesh does not have enough vertices");
            return;
        }

        //First point is located at the lights origin
        Vector3 origin = transform.position;
        
        //Convert points to local space vertices
        //If the current vertex is not needed, set it to the last point in the list
        m_tempMeshVertices = m_meshFilter.mesh.vertices;
        for (int i = 0; i < m_meshFilter.mesh.vertices.Length; i++)
        {
            if (i < m_vector3PointList.Count)
            {
                m_tempMeshVertices[i + 1] = m_vector3PointList[i] - origin;
            }
            else
            {
                m_tempMeshVertices[i] = m_vector3PointList[m_vector3PointList.Count-1] - origin;
            }
        }

        //Create triangle list
        //If the triangle is not needed, set it to 0,0,0
        m_tempMeshTriangles = new int[m_tempMeshVertices.Length * 3];
        for (int i = 0; i < m_tempMeshVertices.Length; i++)
        {
            if (i < m_vector3PointList.Count)
            {
                m_tempMeshTriangles[i * 3] = i + 1;
                m_tempMeshTriangles[i * 3 + 1] = 0;
                m_tempMeshTriangles[i * 3 + 2] = i;
            }
            else
            {
                m_tempMeshTriangles[i * 3] = 0;
                m_tempMeshTriangles[i * 3 + 1] = 0;
                m_tempMeshTriangles[i * 3 + 2] = 0;
            }
        }

        m_tempMeshTriangles[(m_vector3PointList.Count) * 3] = 1;
        m_tempMeshTriangles[(m_vector3PointList.Count) * 3 + 1] = 0;
        m_tempMeshTriangles[(m_vector3PointList.Count) * 3 + 2] = m_vector3PointList.Count;

        //Assign verts to mesh vertices
        m_meshFilter.mesh.vertices = m_tempMeshVertices;
        m_meshFilter.mesh.triangles = m_tempMeshTriangles;

        //Recalculate mesh bounds
        m_meshFilter.mesh.RecalculateBounds();

        //Rebuild UVs
        m_meshFilter.mesh.uv = BuildUVs(m_meshFilter.mesh.vertices);

        //Convert points to local space vertices
        //If the current vertex is not needed, set it to the last point in the list
        m_tempExpandedMeshVertices = m_expandedMeshFilter.mesh.vertices;
        for (int i = 0; i < m_tempExpandedMeshVertices.Length; i++)
        {
            if (i < m_vector3PointList.Count)
            {
                m_tempExpandedMeshVertices[i + 1] = (m_vector3PointList[i] - origin) + ((m_vector3PointList[i] - origin).normalized * 0.15f); ;
            }
            else
            {
                m_tempExpandedMeshVertices[i] = m_vector3PointList[m_vector3PointList.Count - 1] - origin;
            }
        }

        //Assign verts to mesh vertices
        m_expandedMeshFilter.mesh.vertices = m_tempExpandedMeshVertices;
        m_expandedMeshFilter.mesh.triangles = m_tempMeshTriangles;

        //Recalculate mesh bounds
        m_expandedMeshFilter.mesh.RecalculateBounds();

        //Rebuild UVs
        m_expandedMeshFilter.mesh.uv = BuildUVs(m_expandedMeshFilter.mesh.vertices);
    }
    
    /// <summary>
    /// Loop through occluder list adding segments
    /// </summary>
    private void CreateSegmentsFromOccluders()
    {
        for (int i = 0; i < OccluderManager.Instance.occludersInFrustrum.Length; i++)
        {
            bool isInIgnoreList = false;
            for (int k = 0; k < m_ignoredOccluders.Length; k++)
            {
               if(OccluderManager.Instance.occludersInFrustrum[i] == m_ignoredOccluders[k])
                {
                    isInIgnoreList = true;
                    break;
                }
            }
            if (!isInIgnoreList)
            {
                for (int j = 0; j < OccluderManager.Instance.occludersInFrustrum[i].myColliders.Length; j++)
                {
                    #region First Point and Last Point
                    Segment _segment = null;
                    EndPoint _p1 = new EndPoint(0.0f, 0.0f);
                    _p1.segment = _segment;
                    EndPoint _p2 = new EndPoint(0.0f, 0.0f);
                    _p2.segment = _segment;
                    _segment = new Segment();
                    //Save the endpoint coordinates in world space
                    _p1.x = OccluderManager.Instance.occludersInFrustrum[i].myColliders[j].transform.TransformPoint(OccluderManager.Instance.occludersInFrustrum[i].myColliders[j].points[OccluderManager.Instance.occludersInFrustrum[i].myColliders[j].points.Length - 1]).x;
                    _p1.y = OccluderManager.Instance.occludersInFrustrum[i].myColliders[j].transform.TransformPoint(OccluderManager.Instance.occludersInFrustrum[i].myColliders[j].points[OccluderManager.Instance.occludersInFrustrum[i].myColliders[j].points.Length - 1]).y;
                    _p2.x = OccluderManager.Instance.occludersInFrustrum[i].myColliders[j].transform.TransformPoint(OccluderManager.Instance.occludersInFrustrum[i].myColliders[j].points[0]).x;
                    _p2.y = OccluderManager.Instance.occludersInFrustrum[i].myColliders[j].transform.TransformPoint(OccluderManager.Instance.occludersInFrustrum[i].myColliders[j].points[0]).y;
                    _p1.segment = _segment;
                    _p2.segment = _segment;
                    _segment.p1 = _p1;
                    _segment.p2 = _p2;
                    _segment.parent = OccluderManager.Instance.occludersInFrustrum[i];
                    m_allSegments.Add(_segment);
                    m_allEndPoints.Add(_p1);
                    m_allEndPoints.Add(_p2);
                    #endregion
                    #region All Other Points on Collider
                    for (int x = 0; x < OccluderManager.Instance.occludersInFrustrum[i].myColliders[j].points.Length - 1; x++)
                    {
                        Segment segment = null;
                        EndPoint p1 = new EndPoint(0.0f, 0.0f);
                        p1.segment = segment;
                        EndPoint p2 = new EndPoint(0.0f, 0.0f);
                        p2.segment = segment;
                        segment = new Segment();
                        //Save the endpoint coordinates in world space
                        p1.x = OccluderManager.Instance.occludersInFrustrum[i].myColliders[j].transform.TransformPoint(OccluderManager.Instance.occludersInFrustrum[i].myColliders[j].points[x]).x;
                        p1.y = OccluderManager.Instance.occludersInFrustrum[i].myColliders[j].transform.TransformPoint(OccluderManager.Instance.occludersInFrustrum[i].myColliders[j].points[x]).y;
                        p2.x = OccluderManager.Instance.occludersInFrustrum[i].myColliders[j].transform.TransformPoint(OccluderManager.Instance.occludersInFrustrum[i].myColliders[j].points[x + 1]).x;
                        p2.y = OccluderManager.Instance.occludersInFrustrum[i].myColliders[j].transform.TransformPoint(OccluderManager.Instance.occludersInFrustrum[i].myColliders[j].points[x + 1]).y;
                        p1.segment = segment;
                        p2.segment = segment;
                        segment.p1 = p1;
                        segment.p2 = p2;
                        segment.parent = OccluderManager.Instance.occludersInFrustrum[i];
                        m_allSegments.Add(segment);
                        m_allEndPoints.Add(p1);
                        m_allEndPoints.Add(p2);
                    }
                    #endregion
                }
            }
        }
    }

    /// <summary>
    /// Helper function to construct segments along the outside perimeter
    /// </summary>
    /// <param name="size">Size of the square</param>
    private void LoadSquare(float size)
    {
        AddBoundarySegment(transform.position.x + size, transform.position.y + size, transform.position.x + size, transform.position.y - size);
        AddBoundarySegment(transform.position.x + size, transform.position.y - size, transform.position.x - size, transform.position.y - size);
        AddBoundarySegment(transform.position.x - size, transform.position.y - size, transform.position.x - size, transform.position.y + size);
        AddBoundarySegment(transform.position.x - size, transform.position.y + size, transform.position.x + size, transform.position.y + size);
    }

    /// <summary>
    /// Add the boundary segments
    /// </summary>
    /// <param name="x1">X minimum</param>
    /// <param name="y1">Y minimum</param>
    /// <param name="x2">X maximum</param>
    /// <param name="y2">Y maximum</param>
    private void AddBoundarySegment(float x1, float y1, float x2, float y2)
    {
        Segment segment = null;
        EndPoint p1 = new EndPoint(0.0f, 0.0f);
        p1.segment = segment;
        EndPoint p2 = new EndPoint(0.0f, 0.0f);
        p2.segment = segment;
        segment = new Segment();
        p1.x = x1; p1.y = y1;
        p2.x = x2; p2.y = y2;
        p1.segment = segment;
        p2.segment = segment;
        segment.p1 = p1;
        segment.p2 = p2;

        m_allSegments.Add(segment);
        m_allEndPoints.Add(p1);
        m_allEndPoints.Add(p2);
    }

    /// <summary>
    /// Sets the light location
    /// Updates all segments based on the new position
    /// </summary>
    /// <param name="x">Light X position</param>
    /// <param name="y">Light Y position</param>
    public void SetLightPosition(float x, float y)
    {
        m_lightPosition.x = x;
        m_lightPosition.y = y;

        foreach(Segment segment in m_allSegments)
        {
            float dx = 0.5f * (segment.p1.x + segment.p2.x) - x;
            float dy = 0.5f * (segment.p1.y + segment.p2.y) - y;

            segment.p1.angle = Mathf.Atan2(segment.p1.y - y, segment.p1.x - x);
            segment.p2.angle = Mathf.Atan2(segment.p2.y - y, segment.p2.x - x);

            float dAngle = segment.p2.angle - segment.p1.angle;
            if (dAngle <= -Mathf.PI) {
                dAngle += 2 * Mathf.PI;
            }
            if (dAngle > Mathf.PI) {
                dAngle -= 2 * Mathf.PI;
            }
            segment.p1.begin = (dAngle > 0f);
            segment.p2.begin = !segment.p1.begin;
        }
    }

    /// <summary>
    /// Comparison function for sorting points by angle
    /// </summary>
    /// <param name="a">First Endpoint</param>
    /// <param name="b">Second Endpoint</param>
    /// <returns>Comparator value</returns>
    static private int CompareEndpoints(EndPoint a, EndPoint b) {
        if (a.angle > b.angle)
        {
            return 1;
        }
        if (a.angle < b.angle)
        {
            return -1;
        }

        if (!a.begin && b.begin)
        {
            return 1;
        }
        if (a.begin && !b.begin)
        {
            return -1;
        }

        return 0;
    }

    /// <summary>
    /// Helper: leftOf(segment, point) returns true if point is "left" of segment treated as a vector.
    /// Note: Operates using X and Y coordinates
    /// </summary>
    /// <param name="s">Segment</param>
    /// <param name="p">Point to check</param>
    /// <returns>Point left of segment</returns>
    static private bool LeftOf(Segment s, Point p) {
        float cross = (s.p2.x - s.p1.x) * (p.y - s.p1.y) - (s.p2.y - s.p1.y) * (p.x - s.p1.x);
        return cross < 0f;
    }
    
    /// <summary>
    /// Interpolates between two points based on t
    /// </summary>
    /// <param name="p">Point 1</param>
    /// <param name="q">Point 2</param>
    /// <param name="t">Interpolation amount</param>
    /// <returns>Interpolated point</returns>
    static private Point Interpolate(Point p, Point q, float t) {
        return new Point(p.x*(1-t) + q.x* t, p.y*(1-t) + q.y* t);
    }

    bool A1, A2, A3, B1, B2, B3;

    /// <summary>
    /// Returns true if segment A is in front of Segment b from the perspective of Point relativeTo
    /// Further detail can be found here: http://www.redblobgames.com/articles/visibility/segment-sorting.html
    /// </summary>
    /// <param name="a">First segment</param>
    /// <param name="b">Second segment</param>
    /// <param name="relativeTo">Relative to this point</param>
    /// <returns>Status of segment check</returns>
    private bool SegmentInFrontOfSegment(Segment a, Segment b, Point relativeTo) {
        A1 = LeftOf(a, Interpolate(b.p1, b.p2, 0.01f));
        A2 = LeftOf(a, Interpolate(b.p2, b.p1, 0.01f));
        A3 = LeftOf(a, relativeTo);
        B1 = LeftOf(b, Interpolate(a.p1, a.p2, 0.01f));
        B2 = LeftOf(b, Interpolate(a.p2, a.p1, 0.01f));
        B3 = LeftOf(b, relativeTo);

        if (B1 == B2 && B2 != B3) return true;
        if (A1 == A2 && A2 == A3) return true;
        if (A1 == A2 && A2 != A3) return false;
        if (B1 == B2 && B2 == B3) return false;

        return false;
    }

    /// <summary>
    /// Run the algorithm, sweeping over all or part of the circle to find the visible area, represented as a set of triangles
    /// </summary>
    /// <param name="maxAngle">Maximum allowed angle for the sweep</param>
    public void Sweep(float maxAngle = 999.0f)
    {
        m_outputPoints.Clear();
        m_allEndPoints.Sort(CompareEndpoints);

        m_sweepList.Clear();
        float beginAngle = 0.0f;
        
        for (int pass = 0; pass < 2; pass++)
        {
            foreach (EndPoint p in m_allEndPoints)
            {

                if (pass == 1 && p.angle > maxAngle)
                {
                    break;
                }
                
                Segment current_old = m_sweepList.Count==0 ? null : m_sweepList.First.Value;

                if (p.begin)
                {
                    LinkedListNode<Segment> node = m_sweepList.First;
                    while (node != null && SegmentInFrontOfSegment(p.segment, node.Value, m_lightPosition))
                    {
                        node = node.Next;
                    }
                    if (node == null)
                    {
                        m_sweepList.AddLast(p.segment);
                    }
                    else
                    {
                        m_sweepList.AddBefore(node, p.segment);
                    }
                }
                else
                {
                    m_sweepList.Remove(p.segment);
                }

                Segment current_new = m_sweepList.Count ==0 ? null : m_sweepList.First.Value;
                if (current_old != current_new)
                {
                    if (pass == 1)
                    {
                        AddTriangle(beginAngle, p.angle, current_old);
                    }
                    beginAngle = p.angle;
                }
            }
        }
    }

    /// <summary>
    /// Returns the point of intersection between two lines
    /// Line intersection code from http://paulbourke.net/geometry/lineline2d/
    /// </summary>
    /// <param name="p1">First line point A</param>
    /// <param name="p2">First line point B</param>
    /// <param name="p3">Second line point A</param>
    /// <param name="p4">Second line point B</param>
    /// <returns></returns>
    public Point LineIntersection(Point p1, Point p2, Point p3, Point p4) {
        float s = ((p4.x - p3.x) * (p1.y - p3.y) - (p4.y - p3.y) * (p1.x - p3.x)) / ((p4.y - p3.y) * (p2.x - p1.x) - (p4.x - p3.x) * (p2.y - p1.y));
        return new Point(p1.x + s* (p2.x - p1.x), p1.y + s* (p2.y - p1.y));
    }
    
    /// <summary>
    /// Adds points to outputPoints list from a segment and two angles using pythagoras
    /// </summary>
    /// <param name="angle1">First angle</param>
    /// <param name="angle2">Second angle</param>
    /// <param name="segment">Given segment</param>
    private void AddTriangle(float angle1, float angle2, Segment segment)
    {
        Point p1 = m_lightPosition;
        Point p2 = new Point(m_lightPosition.x + Mathf.Cos(angle1), m_lightPosition.y + Mathf.Sin(angle1));
        Point p3 = new Point(0.0f, 0.0f);
        Point p4 = new Point(0.0f, 0.0f);

        if (segment != null)
        {
            p3.x = segment.p1.x;
            p3.y = segment.p1.y;
            p4.x = segment.p2.x;
            p4.y = segment.p2.y;
        }
        else
        {
            p3.x = m_lightPosition.x + Mathf.Cos(angle1) * 500;
            p3.y = m_lightPosition.y + Mathf.Sin(angle1) * 500;
            p4.x = m_lightPosition.x + Mathf.Cos(angle2) * 500;
            p4.y = m_lightPosition.y + Mathf.Sin(angle2) * 500;
        }

        Point pBegin = LineIntersection(p3, p4, p1, p2);

        p2.x = m_lightPosition.x + Mathf.Cos(angle2);
        p2.y = m_lightPosition.y + Mathf.Sin(angle2);
        Point pEnd = LineIntersection(p3, p4, p1, p2);

        m_outputPoints.Add(pBegin);
        m_outputPoints.Add(pEnd);
    }
}
