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
    public GameObject parent;
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
    public PolygonCollider2D[] colliders;

    public List<Segment> boundarySegments = new List<Segment>();
    public List<Segment> staticSegments = new List<Segment>();
    public List<Segment> dynamicSegments = new List<Segment>();

    public List<Segment> combinedSegments = new List<Segment>();

    public List<EndPoint> staticEndpoints = new List<EndPoint>();
    public List<EndPoint> dynamicEndPoints = new List<EndPoint>();
    public List<EndPoint> boundaryEndPoints = new List<EndPoint>();

    public List<EndPoint> combinedEndPoints = new List<EndPoint>();
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
        
        staticSegments.Clear();
        staticEndpoints.Clear();
        dynamicSegments.Clear();
        dynamicEndPoints.Clear();
        CreateSegmentsFromColliders();
        UpdateLightPosition();
    }

    public void UpdateSegmentsForGameObject(PolygonCollider2D go)
    {
        Segment _segment = null;
        EndPoint _p1 = new EndPoint(0.0f, 0.0f);
        _p1.segment = _segment;
        _p1.visualize = true;
        EndPoint _p2 = new EndPoint(0.0f, 0.0f);
        _p2.segment = _segment;
        _p2.visualize = false;
        _segment = new Segment();
        _p1.x = go.transform.TransformPoint(go.points[go.points.Length - 1]).x;
        _p1.y = go.transform.TransformPoint(go.points[go.points.Length - 1]).y;
        _p2.x = go.transform.TransformPoint(go.points[0]).x;
        _p2.y = go.transform.TransformPoint(go.points[0]).y;
        _p1.segment = _segment;
        _p2.segment = _segment;
        _segment.p1 = _p1;
        _segment.p2 = _p2;
        _segment.d = 0.0f;
        _segment.parent = go.gameObject;
        dynamicSegments.Add(_segment);
        dynamicEndPoints.Add(_p1);
        dynamicEndPoints.Add(_p2);
        for (int x = 0; x < go.points.Length - 1; x++)
        {
            Segment segment = null;
            EndPoint p1 = new EndPoint(0.0f, 0.0f);
            p1.segment = segment;
            p1.visualize = true;
            EndPoint p2 = new EndPoint(0.0f, 0.0f);
            p2.segment = segment;
            p2.visualize = false;
            segment = new Segment();
            p1.x = go.transform.TransformPoint(go.points[x]).x;
            p1.y = go.transform.TransformPoint(go.points[x]).y;
            p2.x = go.transform.TransformPoint(go.points[x + 1]).x;
            p2.y = go.transform.TransformPoint(go.points[x + 1]).y;
            p1.segment = segment;
            p2.segment = segment;
            segment.p1 = p1;
            segment.p2 = p2;
            segment.d = 0.0f;
            segment.parent = go.gameObject;
            dynamicSegments.Add(segment);
            dynamicEndPoints.Add(p1);
            dynamicEndPoints.Add(p2);
        }
    }
	
	void LateUpdate () {
        transform.rotation = Quaternion.identity;
        dynamicEndPoints.Clear();
        dynamicSegments.Clear();
        for (int i = 0; i < colliders.Length; i++)
        {
            if(colliders[i].gameObject.tag == "DynamicLightOccluder")
            {
                UpdateSegmentsForGameObject(colliders[i]);
            }
        }
        UpdateLightPosition();

        if (Input.GetKeyDown(KeyCode.L))
        {
            GetAllColliders();
            //Static colliders
            staticSegments.Clear();
            staticEndpoints.Clear();
            dynamicSegments.Clear();
            dynamicEndPoints.Clear();
            CreateSegmentsFromColliders();
            UpdateLightPosition();
        }
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
        boundarySegments.Clear();
        boundaryEndPoints.Clear();
        LoadSquare(m_size, m_margin);
        combinedSegments.Clear();
        combinedSegments.AddRange(staticSegments);
        combinedSegments.AddRange(dynamicSegments);
        combinedSegments.AddRange(boundarySegments);
        combinedEndPoints.Clear();
        combinedEndPoints.AddRange(staticEndpoints);
        combinedEndPoints.AddRange(dynamicEndPoints);
        combinedEndPoints.AddRange(boundaryEndPoints);
        SetLightPosition(transform.position.x, transform.position.y);
    }

    void GetAllColliders()
    {
        if (m_findAllColliders)
        {
            colliders = FindObjectsOfType<PolygonCollider2D>();
        }
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

        //foreach (Vector3 v3 in vertices)
        //{
        //    if (v3.x < xMin)
        //        xMin = v3.x;
        //    if (v3.y < yMin)
        //        yMin = v3.y;
        //    if (v3.x > xMax)
        //        xMax = v3.x;
        //    if (v3.y > yMax)
        //        yMax = v3.y;
        //}

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

    void BuildMesh()
    {
        if (poly == null || poly.Count < 3)
        {
            return;
        }

        Mesh mesh = new Mesh();
        meshFilter.mesh = mesh;

        meshRenderer.material = lightMaterialInstance;

        Vector3 center = transform.position;

        Vector3[] vertices = new Vector3[poly.Count + 1];
        vertices[0] = Vector3.zero;

        for (int i = 0; i < poly.Count; i++)
        {
            vertices[i + 1] = poly[i] - center;
        }

        mesh.vertices = vertices;

        int[] triangles = new int[poly.Count * 3];

        for (int i = 0; i < poly.Count - 1; i++)
        {
            triangles[i * 3] = i + 2;
            triangles[i * 3 + 1] = 0;
            triangles[i * 3 + 2] = i + 1;
        }

        triangles[(poly.Count - 1) * 3] = 1;
        triangles[(poly.Count - 1) * 3 + 1] = 0;
        triangles[(poly.Count - 1) * 3 + 2] = poly.Count;

        mesh.triangles = triangles;
        mesh.uv = BuildUVs(vertices);
        
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();

        if (m_secondaryMesh)
        {

            Mesh mesh2 = new Mesh();
            secondaryMeshFilter.mesh = mesh2;

            secondaryRenderer.material = lightMaterialInstance;

            Vector3 center2 = transform.position;

            Vector3[] vertices2 = new Vector3[poly.Count + 1];
            vertices2[0] = Vector3.zero;

            for (int i = 0; i < poly.Count; i++)
            {
                vertices2[i + 1] = (poly[i] - center) + ((poly[i] - center).normalized * 0.15f);
            }

            mesh2.vertices = vertices2;
            
            mesh2.triangles = triangles;
            mesh2.uv = BuildUVs(vertices2);

            mesh2.RecalculateBounds();
            mesh2.RecalculateNormals();
        }
        else
        {
            secondaryMeshFilter.mesh = null;
            secondaryRenderer.material = null;
        }
    }

    void OnDrawGizmos()
    {
        for (int i = 0; i < output.Count; i++)
        {
            Gizmos.DrawWireSphere(new Vector3(output[i].x, output[i].y, 0f), 0.2f);
        }
    }
    
    private void CreateSegmentsFromColliders()
    {
        for (int i = 0; i < colliders.Length; i++)
        {
            Segment _segment = null;
            EndPoint _p1 = new EndPoint(0.0f, 0.0f);
            _p1.segment = _segment;
            _p1.visualize = true;
            EndPoint _p2 = new EndPoint(0.0f, 0.0f);
            _p2.segment = _segment;
            _p2.visualize = false;
            _segment = new Segment();
            _p1.x = colliders[i].transform.TransformPoint(colliders[i].points[colliders[i].points.Length - 1]).x;
            _p1.y = colliders[i].transform.TransformPoint(colliders[i].points[colliders[i].points.Length - 1]).y;
            _p2.x = colliders[i].transform.TransformPoint(colliders[i].points[0]).x;
            _p2.y = colliders[i].transform.TransformPoint(colliders[i].points[0]).y;
            _p1.segment = _segment;
            _p2.segment = _segment;
            _segment.p1 = _p1;
            _segment.p2 = _p2;
            _segment.d = 0.0f;
            _segment.parent = colliders[i].gameObject;
            if (_segment.parent.tag == "StaticLightOccluder")
            {
                staticSegments.Add(_segment);
                staticEndpoints.Add(_p1);
                staticEndpoints.Add(_p2);
            }
            else if (_segment.parent.tag == "DynamicLightOccluder")
            {
                dynamicSegments.Add(_segment);
                dynamicEndPoints.Add(_p1);
                dynamicEndPoints.Add(_p2);
            }
            for (int x = 0; x < colliders[i].points.Length-1; x++)
            {
                Segment segment = null;
                EndPoint p1 = new EndPoint(0.0f, 0.0f);
                p1.segment = segment;
                p1.visualize = true;
                EndPoint p2 = new EndPoint(0.0f, 0.0f);
                p2.segment = segment;
                p2.visualize = false;
                segment = new Segment();
                p1.x = colliders[i].transform.TransformPoint(colliders[i].points[x]).x;
                p1.y = colliders[i].transform.TransformPoint(colliders[i].points[x]).y;
                p2.x = colliders[i].transform.TransformPoint(colliders[i].points[x+1]).x;
                p2.y = colliders[i].transform.TransformPoint(colliders[i].points[x+1]).y;
                p1.segment = segment;
                p2.segment = segment;
                segment.p1 = p1;
                segment.p2 = p2;
                segment.d = 0.0f;
                segment.parent = colliders[i].gameObject;
                if (segment.parent.tag == "StaticLightOccluder")
                {
                    staticSegments.Add(segment);
                    staticEndpoints.Add(p1);
                    staticEndpoints.Add(p2);
                }
                else if (segment.parent.tag == "DynamicLightOccluder")
                {
                    dynamicSegments.Add(segment);
                    dynamicEndPoints.Add(p1);
                    dynamicEndPoints.Add(p2);
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

    // Load a set of square blocks, plus any other line segments
    public void UpdateStaticSegments(List<Segment> walls)
    {
        staticSegments.Clear();

        staticEndpoints.Clear();
        foreach (Segment wall in walls)
        {
            if (wall.parent.tag != "DynamicLightOccluder")
            {
                staticSegments.Add(wall);
                staticEndpoints.Add(wall.p1);
                staticEndpoints.Add(wall.p2);
            }
        }
    }
    // Load a set of square blocks, plus any other line segments
    public void UpdateDynamicSegments(List<Segment> walls)
    {
        dynamicSegments.Clear();
        
        dynamicEndPoints.Clear();
        foreach (Segment wall in walls)
        {
            if (wall.parent.tag == "DynamicLightOccluder")
            {
                dynamicSegments.Add(wall);
                dynamicEndPoints.Add(wall.p1);
                dynamicEndPoints.Add(wall.p2);
            }
        }
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

        boundarySegments.Add(segment);
        boundaryEndPoints.Add(p1);
        boundaryEndPoints.Add(p2);
    }

    // Set the light location. Segment and EndPoint data can't be
    // processed until the light location is known.
    public void SetLightPosition(float x, float y)
    {
        center.x = x;
        center.y = y;

        foreach(Segment segment in combinedSegments)
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

    // Return p*(1-f) + q*f
    static private Point Interpolate(Point p, Point q, float f) {
        return new Point(p.x*(1-f) + q.x* f, p.y*(1-f) + q.y* f);
    }

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
        var A1 = LeftOf(a, Interpolate(b.p1, b.p2, 0.01f));
        var A2 = LeftOf(a, Interpolate(b.p2, b.p1, 0.01f));
        var A3 = LeftOf(a, relativeTo);
        var B1 = LeftOf(b, Interpolate(a.p1, a.p2, 0.01f));
        var B2 = LeftOf(b, Interpolate(a.p2, a.p1, 0.01f));
        var B3 = LeftOf(b, relativeTo);

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
        combinedEndPoints.Sort(CompareEndpoints);

        open.Clear();
        float beginAngle = 0.0f;
        
        for (int pass = 0; pass < 2; pass++)
        {
            foreach (EndPoint p in combinedEndPoints)
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
