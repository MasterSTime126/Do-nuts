using Unity.Mathematics;
using UnityEngine;

public class FieldOfView1 : MonoBehaviour
{

    [SerializeField] private LayerMask obstacleMask;  // Set this in Inspector

    private Vector3 GetVector3FromAngles(float angle)
    {
        float angleRad = angle * (Mathf.PI / 180f);
        return new Vector3(math.cos(angleRad),  math.sin(angleRad), 0);
    }

    private void Start()
    {
        Mesh mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;

        Vector3 origin = Vector3.zero;  // Local origin for mesh vertices (must be local space!)
        float fov = 90f;
        int rayCount = 50;
        float angle = 0f;
        float angleIncrease = fov / rayCount;
        float viewDistance = 5f;

        Vector3[] vertices = new Vector3[rayCount + 1 + 1];
        Vector2[] uv = new Vector2[vertices.Length];
        int[] triangles = new int[rayCount * 3];


        vertices[0] = origin;

        int vertexIndex = 1;
        int triangleIndex = 0;
        for(int i = 0; i <= rayCount; i++)
        {
            Vector3 vertex;
            Vector3 direction = GetVector3FromAngles(angle);
            
            // Use world position for raycast origin, force Z to 0 for 2D
            Vector2 worldOrigin2D = new Vector2(transform.position.x, transform.position.y);
            RaycastHit2D raycastHit2D = Physics2D.Raycast(worldOrigin2D, direction, viewDistance);
            
            // Debug: visualize the ray
            Debug.DrawRay(transform.position, direction * viewDistance, Color.red, 1f);
            
            if(raycastHit2D.collider != null)
            {
                // Convert hit point to local space for the mesh
                vertex = (Vector3)raycastHit2D.point - transform.position;
                Debug.Log("Hit: " + raycastHit2D.collider.name);
            }
            else{
                vertex = origin + direction * viewDistance;
            }
            vertices[vertexIndex] = vertex;

            if(i > 0)
            {
                triangles[triangleIndex+0] = 0;
                triangles[triangleIndex+1] = vertexIndex - 1;
                triangles[triangleIndex+2] = vertexIndex;   
                triangleIndex += 3;
            }

            vertexIndex++;
            angle -= angleIncrease;

        }

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uv;
    }
}
