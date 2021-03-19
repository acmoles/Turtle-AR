using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/VisualizePosition", order = 1)]
public class VisualizePosition : ScriptableObject
{
    public static List<GameObject> spheres = new List<GameObject>();

    public static void Create(GameObject target, Vector3 targetsLocalPosition, float size, Color color)
    {
        GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphere.GetComponent<Collider>().enabled = false;

        Material SphereMaterial = Resources.Load<Material>("Lime");
        sphere.GetComponent<MeshRenderer>().material = SphereMaterial;
        sphere.GetComponent<MeshRenderer>().material.color = color;

        sphere.transform.localScale = new Vector3(size, size, size);
        sphere.transform.position = targetsLocalPosition;
        if (target)
        {
            sphere.transform.parent = target.transform;
        }
        spheres.Add(sphere);
    }

}