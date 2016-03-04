using UnityEngine;
using System.Collections;
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
    private float m_size = 0f;
    public float m_margin = 3f;

    public bool m_secondaryMesh = true;

    public bool m_findAllColliders = true;

    public LightOccluder[] toIgnore;

    public LightOccluder[] occluders;
    
    public List<Segment> allSegments = new List<Segment>();

    public List<EndPoint> allEndPoints = new List<EndPoint>();

    public Point center = new Point(0f, 0f);

    public LinkedList<Segment> open = new LinkedList<Segment>();
    public List<Point> output = new List<Point>();

    private List<Vector3> poly = new List<Vector3>();

    List<Segment> lines = new List<Segment>();

    public MeshRenderer meshRenderer;
    public MeshFilter meshFilter;
    public Material lightMaterial;
    [HideInInspector]
    public Material lightMaterialInstance;

    public MeshRenderer secondaryRenderer;
    public MeshFilter secondaryMeshFilter;

    public Color lightColor = Color.white;
    
    void Start ()
    {
        lightMaterialInstance = new Material(lightMaterial);
        lightMaterialInstance.color = lightColor;
        GetAllColliders();

        if (meshRenderer == null && GetComponent<MeshRenderer>() != null)
        {
            meshRenderer = GetComponent<MeshRenderer>();
        }
        if (meshFilter == null && GetComponent<MeshFilter>() != null)
        {
            meshFilter = GetComponent<MeshFilter>();
        }

        meshFilter.mesh = new Mesh();
        meshFilter.mesh.MarkDynamic();
        meshRenderer.material = lightMaterialInstance;
        meshFilter.mesh.vertices = new Vector3[512];
        meshFilter.mesh.vertices[0] = Vector3.zero;
        meshFilter.mesh.triangles = new int[512 * 3];

        secondaryMeshFilter.mesh = new Mesh();
        secondaryRenderer.material = lightMaterialInstance;
        secondaryMeshFilter.mesh.MarkDynamic();
        secondaryMeshFilter.mesh.vertices = new Vector3[512];
        secondaryMeshFilter.mesh.vertices[0] = Vector3.zero;
        secondaryMeshFilter.mesh.triangles = new int[512 * 3];
    }

    public void Update()
    {
        lightMaterialInstance.color = lightColor;
    }
    
	void LateUpdate () {
        transform.rotation = Quaternion.identity;
        allEndPoints.Clear();
        allSegments.Clear();
        CreateSegmentsFromColliders();

        if (center.x != transform.position.x || center.y != transform.position.y)
        {
            UpdateLightPosition();
        }

        Sweep();
        CreatePolyList();
        BuildMesh();
    }

    void UpdateLightPosition()
    {
        LoadSquare(m_size, m_margin);
        SetLightPosition(transform.position.x, transform.position.y);
    }

    void GetAllColliders()
    {
        List<LightOccluder> allOccluders = new List<LightOccluder>(FindObjectsOfType<LightOccluder>());
        for (int i = 0; i < toIgnore.Length; i++)
        {
            allOccluders.Remove(toIgnore[i]);
        }
        occluders = allOccluders.ToArray();
    }

    void CreatePolyList()
    {
        poly.Clear();
        for (int i = 0; i < output.Count; i++)
        {
            Vector3 v = new Vector3();
            v.x = output[i].x;
            v.y = output[i].y;
            v.z = 0f;
            poly.Add(v);
        }
    }

    Vector2[] BuildUVs(Vector3[] vertices)
    {

        float xMin = m_margin + m_size;
        float yMin = m_margin + m_size;
        float xMax = -m_margin + m_size;
        float yMax = -m_margin + m_size;

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

    public Vector3[] verts = new Vector3[512];
    public int[] tris = new int[512 * 3];
    public Vector3[] verts2 = new Vector3[512];
    public int[] tris2 = new int[512 * 3];
    void BuildMesh()
    {
        if (poly == null || poly.Count < 3)
        {
            return;
        }

        Vector3 center = transform.position;
        
        verts = meshFilter.mesh.vertices;
        for (int i = 0; i < verts.Length; i++)
        {
            if (i < poly.Count)
            {
               verts[i + 1] = poly[i] - center;
            }
            else
            {
                verts[i] = poly[poly.Count-1] - center;
            }
        }

        tris = new int[verts.Length * 3];

        for (int i = 0; i < verts.Length; i++)
        {
            if (i < poly.Count)
            {
                tris[i * 3] = i + 1;
                tris[i * 3 + 1] = 0;
                tris[i * 3 + 2] = i;
            }
            else
            {
                tris[i * 3] = 0;
                tris[i * 3 + 1] = 0;
                tris[i * 3 + 2] = 0;
            }
        }

        tris[(poly.Count) * 3] = 1;
        tris[(poly.Count) * 3 + 1] = 0;
        tris[(poly.Count) * 3 + 2] = poly.Count;

        //Assign verts to mesh vertices
        meshFilter.mesh.vertices = verts;
        meshFilter.mesh.triangles = tris;

        //Rebuild UVs
        meshFilter.mesh.uv = BuildUVs(meshFilter.mesh.vertices);

        if (m_secondaryMesh)
        {

            verts2 = secondaryMeshFilter.mesh.vertices;
            for (int i = 0; i < verts2.Length; i++)
            {
                if (i < poly.Count)
                {
                    verts2[i + 1] = (poly[i] - center) + ((poly[i] - center).normalized * 0.15f); ;
                }
                else
                {
                    verts2[i] = poly[poly.Count - 1] - center;
                }
            }

            //Assign verts to mesh vertices
            secondaryMeshFilter.mesh.vertices = verts2;
            secondaryMeshFilter.mesh.triangles = tris;

            //Rebuild UVs
            secondaryMeshFilter.mesh.uv = BuildUVs(secondaryMeshFilter.mesh.vertices);
        }
    }

    void OnDrawGizmos()
    {
        for (int i = 0; i < output.Count; i++)
        {
            Gizmos.DrawWireSphere(new Vector3(output[i].x, output[i].y, 0f), 0.05f);
        }

        //for (int x = 0; x < secondaryMeshFilter.mesh.vertices.Length; x++)
        //{
        //    Gizmos.DrawWireCube(transform.TransformPoint(secondaryMeshFilter.mesh.vertices[x]), Vector3.one * 0.05f);
        //}
    }
    
    private void CreateSegmentsFromColliders()
    {
        for (int i = 0; i < occluders.Length; i++)
        {
            bool anyRenderersVisible = false;
            for (int k = 0; k < occluders[i].myRenderer.Length; k++)
            {
                if (occluders[i].myRenderer[k].isVisible)
                {
                    anyRenderersVisible = true;
                    break;
                }
            }
            if (anyRenderersVisible)
            {
                for (int j = 0; j < occluders[i].myColliders.Length; j++)
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
                    _p1.x = occluders[i].myColliders[j].transform.TransformPoint(occluders[i].myColliders[j].points[occluders[i].myColliders[j].points.Length - 1]).x;
                    _p1.y = occluders[i].myColliders[j].transform.TransformPoint(occluders[i].myColliders[j].points[occluders[i].myColliders[j].points.Length - 1]).y;
                    _p2.x = occluders[i].myColliders[j].transform.TransformPoint(occluders[i].myColliders[j].points[0]).x;
                    _p2.y = occluders[i].myColliders[j].transform.TransformPoint(occluders[i].myColliders[j].points[0]).y;
                    _p1.segment = _segment;
                    _p2.segment = _segment;
                    _segment.p1 = _p1;
                    _segment.p2 = _p2;
                    _segment.d = 0.0f;
                    _segment.parent = occluders[i];
                    allSegments.Add(_segment);
                    allEndPoints.Add(_p1);
                    allEndPoints.Add(_p2);
                    #endregion
                    #region All Other Points on Collider
                    for (int x = 0; x < occluders[i].myColliders[j].points.Length - 1; x++)
                    {
                        Segment segment = null;
                        EndPoint p1 = new EndPoint(0.0f, 0.0f);
                        p1.segment = segment;
                        p1.visualize = true;
                        EndPoint p2 = new EndPoint(0.0f, 0.0f);
                        p2.segment = segment;
                        p2.visualize = false;
                        segment = new Segment();
                        p1.x = occluders[i].myColliders[j].transform.TransformPoint(occluders[i].myColliders[j].points[x]).x;
                        p1.y = occluders[i].myColliders[j].transform.TransformPoint(occluders[i].myColliders[j].points[x]).y;
                        p2.x = occluders[i].myColliders[j].transform.TransformPoint(occluders[i].myColliders[j].points[x + 1]).x;
                        p2.y = occluders[i].myColliders[j].transform.TransformPoint(occluders[i].myColliders[j].points[x + 1]).y;
                        p1.segment = segment;
                        p2.segment = segment;
                        segment.p1 = p1;
                        segment.p2 = p2;
                        segment.d = 0.0f;
                        segment.parent = occluders[i];
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
    private void LoadSquare(float size, float margin)
    {
        AddBoundarySegment(transform.position.x + margin, transform.position.y + margin, transform.position.x + margin, transform.position.y + size - margin);
        AddBoundarySegment(transform.position.x + margin, transform.position.y + size - margin, transform.position.x + size - margin, transform.position.y + size - margin);
        AddBoundarySegment(transform.position.x + size - margin, transform.position.y + size - margin, transform.position.x + size - margin, transform.position.y + margin);
        AddBoundarySegment(transform.position.x + size - margin, transform.position.y + margin, transform.position.x + margin, transform.position.y + margin);
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
        center.x = x;
        center.y = y;

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
        // NOTE: we slightly shorten the segments so that
        // intersections of the endpoints (common) don't count as
        // intersections in this algorithm
         A1 = LeftOf(a, Interpolate(b.p1, b.p2, 0.01f));
         A2 = LeftOf(a, Interpolate(b.p2, b.p1, 0.01f));
         A3 = LeftOf(a, relativeTo);
         B1 = LeftOf(b, Interpolate(a.p1, a.p2, 0.01f));
         B2 = LeftOf(b, Interpolate(a.p2, a.p1, 0.01f));
         B3 = LeftOf(b, relativeTo);

        // NOTE: this algorithm is probably worthy of a short article
        // but for now, draw it on paper to see how it works. Consider
        // the line A1-A2. If both B1 and B2 are on one side and
        // relativeTo is on the other side, then A is in between the
        // viewer and B. We can do the same with B1-B2: if A1 and A2
        // are on one side, and relativeTo is on the other side, then
        // B is in between the viewer and A.
        if (B1 == B2 && B2 != B3) return true;
        if (A1 == A2 && A2 == A3) return true;
        if (A1 == A2 && A2 != A3) return false;
        if (B1 == B2 && B2 == B3) return false;

        // If A1 != A2 and B1 != B2 then we have an intersection.
        // Expose it for the GUI to show a message. A more robust
        // implementation would split segments at intersections so
        // that part of the segment is in front and part is behind.
        return false;

        // NOTE: previous implementation was a.d < b.d. That's simpler
        // but trouble when the segments are of dissimilar sizes. If
        // you're on a grid and the segments are similarly sized, then
        // using distance will be a simpler and faster implementation.
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
                    while (node != null && SegmentInFrontOfSegment(p.segment, node.Value, center))
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
        Point p1 = center;
        Point p2 = new Point(center.x + Mathf.Cos(angle1), center.y + Mathf.Sin(angle1));
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
            p3.x = center.x + Mathf.Cos(angle1) * 500;
            p3.y = center.y + Mathf.Sin(angle1) * 500;
            p4.x = center.x + Mathf.Cos(angle2) * 500;
            p4.y = center.y + Mathf.Sin(angle2) * 500;
        }

        var pBegin = LineIntersection(p3, p4, p1, p2);

        p2.x = center.x + Mathf.Cos(angle2);
        p2.y = center.y + Mathf.Sin(angle2);
        var pEnd = LineIntersection(p3, p4, p1, p2);

        output.Add(pBegin);
        output.Add(pEnd);
    }


}
