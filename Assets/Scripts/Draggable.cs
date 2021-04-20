using UnityEngine;

public class Draggable : MonoBehaviour
{
    public bool IsHighlighted = false;
    protected MeshRenderer _renderer;
    protected Color _originalColor;
    Rigidbody _body;

    public virtual void Start()
    {
        _originalColor = GetComponent<MeshRenderer>().material.color;
        _renderer = GetComponent<MeshRenderer>();
        _body = GetComponent<Rigidbody>();
    }

    public void Highlight()
    {
        IsHighlighted = true;
        _renderer.material.color = Color.cyan;
    }

    public void ResetHighlight()
    {
        IsHighlighted = false;
        _renderer.material.color = _originalColor;
    }

    public void Drag(Vector3 point)
    {
        _body.transform.position = new Vector3(
            point.x,
            _body.transform.position.y,
            point.z
        );
        _body.velocity = new Vector3(0, 0, 0);
    }

    public void PointAt(Vector3 point)
    {
        var directionVector = point - GetComponent<Rigidbody>().transform.position;
        transform.rotation = Quaternion.LookRotation(new Vector3(directionVector.x, 0, directionVector.z));
    }
}
