using UnityEditor;
using UnityEngine;

public class Resize : MonoBehaviour
{
    [Tooltip("Activer bounding box"), SerializeField] bool activerGuizmo = false;
    [Tooltip("Couleur bounding box"), SerializeField] UnityEngine.Color guizmoColor = UnityEngine.Color.red;
    [Space(5), Header("----------------------------------")]
    [Tooltip("GameObject a resize"), SerializeField] GameObject go;
    [Tooltip("Unité de mesure Unity Unit (Uu)")] public float uu;
    [Tooltip("L'axe sur lequel la rotation aura lieu."), SerializeField] Dropdown axis;

    Bounds _totalBounds;
    enum Dropdown { X, Y, Z }

    private void OnDrawGizmos()
    {
        if (activerGuizmo)
        {
            Gizmos.color = guizmoColor;
            Gizmos.DrawWireCube(_totalBounds.center, new(_totalBounds.size.x, _totalBounds.size.y, _totalBounds.size.z));
        }
    }

    public void Initialisation()
    {
        uu = Mathf.Abs(uu);
        _totalBounds = CalculateTotalBounds(go.transform);


        Vector3 size = _totalBounds.size;
        // Debug.Log("La taille de la bounding box est : " + size.x + " " + size.y + " " + size.z);

        float multiplicateur = CalculateRatio(axis, size);
        go.transform.localScale = go.transform.localScale * multiplicateur;
    }


    private Bounds CalculateTotalBounds(Transform currentTransform)
    {
        MeshFilter meshFilter = currentTransform.GetComponentInChildren<MeshFilter>();
        SkinnedMeshRenderer skinnedMeshRenderer = currentTransform.GetComponentInChildren<SkinnedMeshRenderer>();
        Bounds combinedBounds;

        if (meshFilter != null)
        {
            combinedBounds = CalculateBoundsWithRotation(meshFilter.sharedMesh, currentTransform);
        }
        else if (skinnedMeshRenderer != null)
        {
            combinedBounds = CalculateBoundsWithRotation(skinnedMeshRenderer.sharedMesh, currentTransform);
        }
        else
        {
            combinedBounds = new Bounds(currentTransform.position, Vector3.zero);
        }

        // Parcourir les enfants du GameObject actuel
        for (int i = 0; i < currentTransform.childCount; i++)
        {
            Transform childTransform = currentTransform.GetChild(i);

            // Récursivement calculer la bounding box des enfants
            Bounds childBounds = CalculateTotalBounds(childTransform);

            // Étendre la bounding box totale avec la bounding box  de l'enfant
            combinedBounds.Encapsulate(childBounds);
        }

        return combinedBounds;
    }

    Bounds CalculateBoundsWithRotation(Mesh mesh, Transform transform)
    {
        // Transformez les points du maillage en tenant compte de la rotation de l'objet
        Vector3[] vertices = mesh.vertices;
        for (int i = 0; i < vertices.Length; i++)
        {
            vertices[i] = transform.TransformPoint(vertices[i]);
        }

        // Calculez la boîte à partir des points transformés
        Bounds bounds = new Bounds(vertices[0], Vector3.zero);
        for (int i = 1; i < vertices.Length; i++)
        {
            bounds.Encapsulate(vertices[i]);
        }
        return bounds;
    }

    private float CalculateRatio(Dropdown dropdownValue, Vector3 size)
    {
        float number;

        switch (dropdownValue)
        {
            case Dropdown.X:
                number = uu / size.x;
                return number;
            case Dropdown.Y:
                number = uu / size.y;

                return number;
            case Dropdown.Z:

                number = uu / size.z;
                return number;
            default:
                return 0;
        }
    }
}

[CustomEditor(typeof(Resize))]
public class ResizeFBX_Editor : Editor
{
    public override void OnInspectorGUI()
    {
        Resize resizeFBX = (Resize)target;
        base.OnInspectorGUI();

        if (GUILayout.Button("Resize") && resizeFBX.uu != 0)
        {
            resizeFBX.Initialisation();
            resizeFBX.Initialisation();
            //SceneView.RepaintAll();
            // EditorApplication.QueuePlayerLoopUpdate();
        }
    }
}