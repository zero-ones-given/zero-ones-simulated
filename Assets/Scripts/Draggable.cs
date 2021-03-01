using UnityEngine;

public class Draggable : MonoBehaviour
{
    bool _isHovering = false;
    bool _resetColor = false;
    Color _originalColor2;
    MeshRenderer _renderer;
    Rigidbody _body;

    public virtual void Start()
    {
        _originalColor2 = GetComponent<MeshRenderer>().material.color;
        _renderer = GetComponent<MeshRenderer>();
        _body = GetComponent<Rigidbody>();
    }

    public virtual void Update() {
        if (_isHovering) {
            _renderer.material.color = Color.cyan;
            _isHovering = false;
            _resetColor = true;
        } else if (_resetColor) {
            _renderer.material.color = _originalColor2;
            _resetColor = false;
        }
    }

    public void Hover()
    {
        _isHovering = true;
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
