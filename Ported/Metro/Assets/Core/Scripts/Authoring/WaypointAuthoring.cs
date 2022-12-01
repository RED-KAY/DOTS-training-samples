using Unity.Entities;
using UnityEngine;

class WaypointAuthoring : MonoBehaviour
{
    public Transform NextWaypoint;

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        
        var myIndex = transform.GetSiblingIndex();

        if (myIndex < transform.parent.childCount - 1)
        {
            var nextChild = transform.parent.GetChild(myIndex + 1);
            Gizmos.DrawLine(transform.position, nextChild.transform.position);
        }

       // Gizmos.DrawCube(transform.position, Vector3.one * 1f);
    }
}
 
class WaypointBaker : Baker<WaypointAuthoring>
{
    public override void Bake(WaypointAuthoring authoring)
    {
        var pathAuth = GetComponentInParent<PathAuthoring>();
     
        
        
        AddComponent(new Waypoint
        {
            PathEntity = pathAuth != null ? GetEntity(pathAuth.transform) : default,
            NextWaypointEntity = GetEntity(authoring.NextWaypoint)
        });
    }
}