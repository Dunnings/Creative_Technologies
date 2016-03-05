using UnityEngine;
using System.Collections.Generic;

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
public class EndPoint: Point
{
    public bool begin = false;
    public Segment segment = null;
    public float angle = 0f;
    public bool visualize = false;
    public EndPoint(float _x, float _y) : base(_x,_y)
    {
        x = _x;
        y = _y;
    }
}

public class Segment
{
    public LightOccluder parent;
    public EndPoint p1;
    public EndPoint p2;
    public float d;
}

public class Mesh_Light : MonoBehaviour
{
    public float lightSize = 3f;

    public LightOccluder[] ignoreOccluders;
    
    private List<Segment> allSegments = new List<Segment>();

    private List<EndPoint> allEndPoints = new List<EndPoint>();

    private Point lightPosition = new Point(0f, 0f);

    private LinkedList<Segment> open = new LinkedList<Segment>();

    public List<Point> output = new List<Point>();

    private List<Vector3> pointList = new List<Vector3>();

    public Material lightMaterial;

    public MeshRenderer meshRenderer;

    public MeshFilter meshFilter;

    private Material lightMaterialInstance;

    public MeshRenderer expandedRenderer;

    public MeshFilter expandedMeshFilter;

    public Color lightColor = Color.white;
    
    private const int meshVertexCount = 512;
    private Vector3[] tempMeshVertices = new Vector3[meshVertexCount];
    private int[] tempMeshTriangles = new int[meshVertexCount * 3];
    private Vector3[] tempExpandedMeshVertices = new Vector3[meshVertexCount];
    private int[] expandedMeshTriangles = new int[meshVertexCount * 3];

    /// <summary>
    /// Run on light creation
    /// </summary>
    void Start ()
    {
        //Instantiate a new light material so we can change the light colour independantly of other lights
        lightMaterialInstance = new Material(lightMaterial);
        lightMaterialInstance.color = lightColor;

        //Create a new mesh, mark it as dynamic and give it default vertex and triangle arrays
        meshFilter.mesh = new Mesh();
        meshFilter.mesh.MarkDynamic();
        meshRenderer.material = lightMaterialInstance;
        meshFilter.mesh.vertices = new Vector3[meshVertexCount];
        meshFilter.mesh.vertices[0] = Vector3.zero;
        meshFilter.mesh.triangles = new int[meshVertexCount * 3];

        //Repeat for expanded mesh
        expandedMeshFilter.mesh = new Mesh();
        expandedRenderer.material = lightMaterialInstance;
        expandedMeshFilter.mesh.MarkDynamic();
        expandedMeshFilter.mesh.vertices = new Vector3[meshVertexCount];
        expandedMeshFilter.mesh.vertices[0] = Vector3.zero;
        expandedMeshFilter.mesh.triangles = new int[meshVertexCount * 3];

        //First pass of colliders
        UpdateLightPosition();
    }

    /// <summary>
    /// Run every frame
    /// </summary>
    public void Update()
    {
        lightMaterialInstance.color = lightColor;
    }

    /// <summary>
    /// Run immediately after update
    /// </summary>
    void LateUpdate () {
        //Make sure the light does not rotate
        transform.rotation = Quaternion.identity;

        //Clear the endpoints and segments every frame
        allEndPoints.Clear();
        allSegments.Clear();

        //Create the segments in the view frustrum this frame
        CreateSegmentsFromColliders();


        UpdateLightPosition();
        Sweep();
        CreatePolyList();
        BuildMesh();
    }

    void UpdateLightPosition()
    {
        LoadSquare(lightSize);
        SetLightPosition(transform.position.x, transform.position.y);
    }

    /// <summary>
    /// Turns a list of type Points into a list of type Vector3
    /// </summary>
    void CreatePolyList()
    {
        pointList.Clear();
        for (int i = 0; i < output.Count; i++)
        {
            Vector3 v = new Vector3();
            v.x = output[i].x;
            v.y = output[i].y;
            v.z = 0f;
            pointList.Add(v);
        }
    }

    /// <summary>
    /// Convert list of vertices into UV coordinates
    /// </summary>
    /// <param name="vertices">List of vertices to be converted</param>
    /// <returns></returns>
    Vector2[] BuildUVs(Vector3[] vertices)
    {
        float xMin = lightSize;
        float yMin = lightSize;
        float xMax = -lightSize;
        float yMax = -lightSize;

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
        if (pointList == null || pointList.Count < 3)
        {
            return;
        }

        //If there are more points than vertices in the mesh, throw an error and return
        if(pointList.Count >= meshVertexCount)
        {
            Debug.Log("Error, mesh does not have enough vertices");
            return;
        }

        //First point is located at the lights origin
        Vector3 origin = transform.position;
        
        tempMeshVertices = meshFilter.mesh.vertices;
        for (int i = 0; i < meshFilter.mesh.vertices.Length; i++)
        {
            if (i < pointList.Count)
            {
                tempMeshVertices[i + 1] = pointList[i] - origin;
            }
            else
            {
                tempMeshVertices[i] = pointList[pointList.Count-1] - origin;
            }
        }

        tempMeshTriangles = new int[tempMeshVertices.Length * 3];

        for (int i = 0; i < tempMeshVertices.Length; i++)
        {
            if (i < pointList.Count)
            {
                tempMeshTriangles[i * 3] = i + 1;
                tempMeshTriangles[i * 3 + 1] = 0;
                tempMeshTriangles[i * 3 + 2] = i;
            }
            else
            {
                tempMeshTriangles[i * 3] = 0;
                tempMeshTriangles[i * 3 + 1] = 0;
                tempMeshTriangles[i * 3 + 2] = 0;
            }
        }

        tempMeshTriangles[(pointList.Count) * 3] = 1;
        tempMeshTriangles[(pointList.Count) * 3 + 1] = 0;
        tempMeshTriangles[(pointList.Count) * 3 + 2] = pointList.Count;

        //Assign verts to mesh vertices
        meshFilter.mesh.vertices = tempMeshVertices;
        meshFilter.mesh.triangles = tempMeshTriangles;

        meshFilter.mesh.RecalculateBounds();

        //Rebuild UVs
        meshFilter.mesh.uv = BuildUVs(meshFilter.mesh.vertices);
        
        //Secondary mesh
        tempExpandedMeshVertices = expandedMeshFilter.mesh.vertices;
        for (int i = 0; i < tempExpandedMeshVertices.Length; i++)
        {
            if (i < pointList.Count)
            {
                tempExpandedMeshVertices[i + 1] = (pointList[i] - origin) + ((pointList[i] - origin).normalized * 0.15f); ;
            }
            else
            {
                tempExpandedMeshVertices[i] = pointList[pointList.Count - 1] - origin;
            }
        }

        //Assign verts to mesh vertices
        expandedMeshFilter.mesh.vertices = tempExpandedMeshVertices;
        expandedMeshFilter.mesh.triangles = tempMeshTriangles;

        expandedMeshFilter.mesh.RecalculateBounds();

        //Rebuild UVs
        expandedMeshFilter.mesh.uv = BuildUVs(expandedMeshFilter.mesh.vertices);
    }

    void OnDrawGizmos()
    {
        //for (int i = 0; i < output.Count; i++)
        //{
        //    Gizmos.DrawWireSphere(new Vector3(output[i].x, output[i].y, 0f), 0.05f);
        //}

        //for (int x = 0; x < meshFilter.mesh.vertices.Length; x++)
        //{
        //    Gizmos.DrawWireCube(transform.TransformPoint(meshFilter.mesh.vertices[x]), Vector3.one * 0.05f);
        //}
    }
    
    private void CreateSegmentsFromColliders()
    {
        for (int i = 0; i < OccluderManager.Instance.occludersInFrustrum.Length; i++)
        {
            bool isInIgnoreList = false;
            for (int k = 0; k < ignoreOccluders.Length; k++)
            {
               if(OccluderManager.Instance.occludersInFrustrum[i] == ignoreOccluders[k])
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
                    _p1.visualize = true;
                    EndPoint _p2 = new EndPoint(0.0f, 0.0f);
                    _p2.segment = _segment;
                    _p2.visualize = false;
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
                    _segment.d = 0.0f;
                    _segment.parent = OccluderManager.Instance.occludersInFrustrum[i];
                    allSegments.Add(_segment);
                    allEndPoints.Add(_p1);
                    allEndPoints.Add(_p2);
                    #endregion
                    #region All Other Points on Collider
                    for (int x = 0; x < OccluderManager.Instance.occludersInFrustrum[i].myColliders[j].points.Length - 1; x++)
                    {
                        Segment segment = null;
                        EndPoint p1 = new EndPoint(0.0f, 0.0f);
                        p1.segment = segment;
                        p1.visualize = true;
                        EndPoint p2 = new EndPoint(0.0f, 0.0f);
                        p2.segment = segment;
                        p2.visualize = false;
                        segment = new Segment();
                        p1.x = OccluderManager.Instance.occludersInFrustrum[i].myColliders[j].transform.TransformPoint(OccluderManager.Instance.occludersInFrustrum[i].myColliders[j].points[x]).x;
                        p1.y = OccluderManager.Instance.occludersInFrustrum[i].myColliders[j].transform.TransformPoint(OccluderManager.Instance.occludersInFrustrum[i].myColliders[j].points[x]).y;
                        p2.x = OccluderManager.Instance.occludersInFrustrum[i].myColliders[j].transform.TransformPoint(OccluderManager.Instance.occludersInFrustrum[i].myColliders[j].points[x + 1]).x;
                        p2.y = OccluderManager.Instance.occludersInFrustrum[i].myColliders[j].transform.TransformPoint(OccluderManager.Instance.occludersInFrustrum[i].myColliders[j].points[x + 1]).y;
                        p1.segment = segment;
                        p2.segment = segment;
                        segment.p1 = p1;
                        segment.p2 = p2;
                        segment.d = 0.0f;
                        segment.parent = OccluderManager.Instance.occludersInFrustrum[i];
                        allSegments.Add(segment);
                        allEndPoints.Add(p1);
                        allEndPoints.Add(p2);
                    }
                    #endregion
                }
            }
        }
    }

    // Helper function to construct segments along the outside perimeter
    private void LoadSquare(float margin)
    {
        AddBoundarySegment(transform.position.x + margin, transform.position.y + margin, transform.position.x + margin, transform.position.y - margin);
        AddBoundarySegment(transform.position.x + margin, transform.position.y - margin, transform.position.x - margin, transform.position.y - margin);
        AddBoundarySegment(transform.position.x - margin, transform.position.y - margin, transform.position.x - margin, transform.position.y + margin);
        AddBoundarySegment(transform.position.x - margin, transform.position.y + margin, transform.position.x + margin, transform.position.y + margin);
    }

    // Add a segment, where the first point shows up in the
    // visualization but the second one does not. (Every endpoint is
    // part of two segments, but we want to only show them once.)
    //private void AddSegment(float x1, float y1, float x2, float y2)
    //{
    //    Segment segment = null;
    //    EndPoint p1 = new EndPoint(0.0f, 0.0f);
    //    p1.segment = segment;
    //    p1.visualize = true;
    //    EndPoint p2 = new EndPoint(0.0f, 0.0f);
    //    p2.segment = segment;
    //    p2.visualize = false;
    //    segment = new Segment();
    //    p1.x = x1; p1.y = y1;
    //    p2.x = x2; p2.y = y2;
    //    p1.segment = segment;
    //    p2.segment = segment;
    //    segment.p1 = p1;
    //    segment.p2 = p2;
    //    segment.d = 0.0f;

    //    if (segment.parent.tag == "DynamicLightOccluder")
    //    {
    //        staticSegments.Add(segment);
    //    }
    //    staticEndpoints.Add(p1);
    //    staticEndpoints.Add(p2);
    //}

    // Add a segment, where the first point shows up in the
    // visualization but the second one does not. (Every endpoint is
    // part of two segments, but we want to only show them once.)
    private void AddBoundarySegment(float x1, float y1, float x2, float y2)
    {
        Segment segment = null;
        EndPoint p1 = new EndPoint(0.0f, 0.0f);
        p1.segment = segment;
        p1.visualize = true;
        EndPoint p2 = new EndPoint(0.0f, 0.0f);
        p2.segment = segment;
        p2.visualize = false;
        segment = new Segment();
        p1.x = x1; p1.y = y1;
        p2.x = x2; p2.y = y2;
        p1.segment = segment;
        p2.segment = segment;
        segment.p1 = p1;
        segment.p2 = p2;
        segment.d = 0.0f;

        allSegments.Add(segment);
        allEndPoints.Add(p1);
        allEndPoints.Add(p2);
    }

    // Set the light location. Segment and EndPoint data can't be
    // processed until the light location is known.
    public void SetLightPosition(float x, float y)
    {
        lightPosition.x = x;
        lightPosition.y = y;

        foreach(Segment segment in allSegments)
        {
            float dx = 0.5f * (segment.p1.x + segment.p2.x) - x;
            float dy = 0.5f * (segment.p1.y + segment.p2.y) - y;

            segment.d = dx * dx + dy * dy;

            segment.p1.angle = Mathf.Atan2(segment.p1.y - y, segment.p1.x - x);
            segment.p2.angle = Mathf.Atan2(segment.p2.y - y, segment.p2.x - x);

            var dAngle = segment.p2.angle - segment.p1.angle;
            if (dAngle <= -Mathf.PI) { dAngle += 2 * Mathf.PI; }
            if (dAngle > Mathf.PI) { dAngle -= 2 * Mathf.PI; }
            segment.p1.begin = (dAngle > 0.0);
            segment.p2.begin = !segment.p1.begin;
        }
    }

    //Comparison function for sorting points by angle
    static private int CompareEndpoints(EndPoint a, EndPoint b) {
        if (a.angle > b.angle) return 1;
        if (a.angle<b.angle) return -1;

        if (!a.begin && b.begin) return 1;
        if (a.begin && !b.begin) return -1;
        return 0;
    }

    // Helper: leftOf(segment, point) returns true if point is "left"
    // of segment treated as a vector. Note that this assumes a 2D
    // coordinate system in which the Y axis grows downwards, which
    // matches common 2D graphics libraries, but is the opposite of
    // the usual convention from mathematics and in 3D graphics
    // libraries.
    static private bool LeftOf(Segment s, Point p) {
        var cross = (s.p2.x - s.p1.x) * (p.y - s.p1.y)
                  - (s.p2.y - s.p1.y) * (p.x - s.p1.x);
        return cross< 0;
        // Also note that this is the naive version of the test and
        // isn't numerically robust. See
        // <https://github.com/mikolalysenko/robust-arithmetic> for a
        // demo of how this fails when a point is very close to the
        // line.
    }
    
    static private Point Interpolate(Point p, Point q, float f) {
        return new Point(p.x*(1-f) + q.x* f, p.y*(1-f) + q.y* f);
    }

    bool A1, A2, A3, B1, B2, B3;

    // Helper: do we know that segment a is in front of b?
    // Implementation not anti-symmetric (that is to say,
    // _segment_in_front_of(a, b) != (!_segment_in_front_of(b, a)).
    // Also note that it only has to work in a restricted set of cases
    // in the visibility algorithm; I don't think it handles all
    // cases. See http://www.redblobgames.com/articles/visibility/segment-sorting.html
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

    // Run the algorithm, sweeping over all or part of the circle to find
    // the visible area, represented as a set of triangles
    public void Sweep(float maxAngle = 999.0f)
    {
        output.Clear();
        allEndPoints.Sort(CompareEndpoints);

        open.Clear();
        float beginAngle = 0.0f;
        
        for (int pass = 0; pass < 2; pass++)
        {
            foreach (EndPoint p in allEndPoints)
            {

                if (pass == 1 && p.angle > maxAngle)
                {
                    break;
                }
                
                Segment current_old = open.Count==0 ? null : open.First.Value;

                if (p.begin)
                {
                    LinkedListNode<Segment> node = open.First;
                    while (node != null && SegmentInFrontOfSegment(p.segment, node.Value, lightPosition))
                    {
                        node = node.Next;
                    }
                    if (node == null)
                    {
                        open.AddLast(p.segment);
                    }
                    else
                    {
                        open.AddBefore(node, p.segment);
                    }
                }
                else
                {
                    open.Remove(p.segment);
                }

                Segment current_new = open.Count ==0 ? null : open.First.Value;
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

    public Point LineIntersection(Point p1, Point p2, Point p3, Point p4) {
        // From http://paulbourke.net/geometry/lineline2d/
        float s = ((p4.x - p3.x) * (p1.y - p3.y) - (p4.y - p3.y) * (p1.x - p3.x))
            / ((p4.y - p3.y) * (p2.x - p1.x) - (p4.x - p3.x) * (p2.y - p1.y));
        return new Point(p1.x + s* (p2.x - p1.x), p1.y + s* (p2.y - p1.y));
    }
    
    private void AddTriangle(float angle1, float angle2, Segment segment)
    {
        Point p1 = lightPosition;
        Point p2 = new Point(lightPosition.x + Mathf.Cos(angle1), lightPosition.y + Mathf.Sin(angle1));
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
            p3.x = lightPosition.x + Mathf.Cos(angle1) * 500;
            p3.y = lightPosition.y + Mathf.Sin(angle1) * 500;
            p4.x = lightPosition.x + Mathf.Cos(angle2) * 500;
            p4.y = lightPosition.y + Mathf.Sin(angle2) * 500;
        }

        var pBegin = LineIntersection(p3, p4, p1, p2);

        p2.x = lightPosition.x + Mathf.Cos(angle2);
        p2.y = lightPosition.y + Mathf.Sin(angle2);
        var pEnd = LineIntersection(p3, p4, p1, p2);

        output.Add(pBegin);
        output.Add(pEnd);
    }


}
