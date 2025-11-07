/**

    This class demonstrates the code discussed in these two articles:

    http://devmag.org.za/2011/04/05/bzier-curves-a-tutorial/
    http://devmag.org.za/2011/06/23/bzier-path-algorithms/

    Use this code as you wish, at your own risk. If it blows up 
    your computer, makes a plane crash, or otherwise cause damage,
    injury, or death, it is not my fault.

    @author Herman Tulleken, dev.mag.org.za

*/


using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Main : MonoBehaviour 
{
	private List<Vector3> points;
    private List<Vector3> gizmos;	
	private LineRenderer lineRenderer;
	public BezierPath bezierPath;

	public FingerControls finger;

	// Use this for initialization
	void Start () 
	{
		lineRenderer = GetComponent<LineRenderer>();
		points = new List<Vector3>();
		bezierPath = new BezierPath();

		finger = GameObject.FindObjectOfType(typeof(FingerControls)) as FingerControls;
	}
	
	// Update is called once per frame
	void Update () 
	{
		ProcessInput();
		BezierInterpolate();
	}
	
	private void ProcessInput()
	{
        if (Input.GetMouseButton(0))
        {
            Vector2 screenPosition = Input.mousePosition;
            Vector3 worldPosition = Camera.main.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, 10));

			// Ako je broj selektovanih slova veci od 0 setujemo tacke
			if (finger.listOfSelectedLetters.Count > 0)
			{
				points.Clear();

				for (int i = 0; i < finger.listOfSelectedLetters.Count; i++)
				{
					points.Add(new Vector3(finger.listOfSelectedLetters[i].transform.position.x, finger.listOfSelectedLetters[i].transform.position.y, 0));
				}
			}

			if (points.Count > 0 && finger.listOfSelectedLetters.Count > 0)
            	points.Add(worldPosition);
        }

		if (Input.GetMouseButtonUp(0))
		{
			points.Clear();
		}
	}

	public void AddPoint(Vector3 worldPosition)
	{
		points.Add(worldPosition);
	}

	public void ClearPoints()
	{
		points.Clear();
	}
	
	private void RenderLineSegments()
	{
        gizmos = points;
        SetLinePoints(points);
	}

	private void RenderBezier()
	{
		BezierPath bezierPath = new BezierPath();
		
		bezierPath.SetControlPoints(points);
		List<Vector3> drawingPoints = bezierPath.GetDrawingPoints2();

        gizmos = drawingPoints;

        SetLinePoints(drawingPoints);
	}

    private void BezierInterpolate()
    {
        BezierPath bezierPath = new BezierPath();
        bezierPath.Interpolate(points, .25f);

        List<Vector3> drawingPoints = bezierPath.GetDrawingPoints2();
        
        gizmos = bezierPath.GetControlPoints();  

        SetLinePoints(drawingPoints);
    }

    private void BezierReduce()
    {
        bezierPath.SamplePoints(points, 10, 100, 0.33f);
        
        List<Vector3> drawingPoints = bezierPath.GetDrawingPoints2();

        gizmos = bezierPath.GetControlPoints();
        SetLinePoints(drawingPoints);
    }

    private void SetLinePoints(List<Vector3> drawingPoints)
    {
        lineRenderer.SetVertexCount(drawingPoints.Count);

        for (int i = 0; i < drawingPoints.Count; i++)
        {
            lineRenderer.SetPosition(i, drawingPoints[i]);
        }
    }

    public void OnDrawGizmos()
    {
        if (gizmos == null)
        {
            return;
        }        

        for (int i = 0; i < gizmos.Count; i++)
        {
            Gizmos.DrawWireSphere(gizmos[i], 1f);            
        }
    }
}
