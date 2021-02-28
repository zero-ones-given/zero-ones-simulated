using UnityEngine;
using System.Collections;

/*
    This controller makes sure the objects bounce back instead of going through 
    the arena walls when moving too fast for the collisions Unity calculates.
*/
public class DynamicObjectController : MonoBehaviour, Draggable
{
    float _bounceMultiplier = 0.8f;
    Rigidbody _dynamicObject;
    float _arenaMaxX = 1.5f / 2;
    float _arenaMaxY = 2;
    float _arenaMaxZ = 1.5f / 2;
    MeshRenderer _renderer;
    Collider _collider;
    Color _originalColor;
    bool _isHovering = false;
    public bool isFlickering = false;
    public bool isGhost = false;

    void Start()
    {
        _dynamicObject = GetComponent<Rigidbody>();
        _renderer = _dynamicObject.GetComponent<MeshRenderer>();
        if (_renderer)
        {
            _originalColor = _renderer.material.color;
        }
    }

    void Update()
    {
        _dynamicObject.velocity = FlipVelocityIfOver(_dynamicObject, _arenaMaxX, _arenaMaxY, _arenaMaxZ);
        _dynamicObject.transform.position = EnsurePositionIsWithin(_dynamicObject.transform.position, _arenaMaxX, _arenaMaxY, _arenaMaxZ, 0.1f);

        if (!_renderer) {
            return;
        }

        if (_isHovering) {
            _renderer.material.color = Color.cyan;
            _isHovering = false;
        }
        else if (isFlickering || isGhost)
        {
            var probability = isGhost ? 0.025f : 0.8f;
            if (Random.Range(0f, 1f) > probability)
            {
                var color = new Color(_originalColor.r, _originalColor.g, _originalColor.b, isGhost ? 0f : 0.1f);
                _renderer.material.color = color;
            }
        } else {
            _renderer.material.color = _originalColor;
        }
    }

    public void Hover()
    {
        _isHovering = true;
    }

    public void Drag(Vector3 point)
    {
        _dynamicObject.transform.position = new Vector3(
            point.x,
            _dynamicObject.transform.position.y,
            point.z
        );
        _dynamicObject.velocity = new Vector3(0, 0, 0);
    }

    public void PointAt(Vector3 point)
    {
    }

    float FlipIfOver(float target, float number, float lowerLimit, float upperLimit)
    {
        if (number > upperLimit || number < lowerLimit)
        {
            return -_bounceMultiplier * target;
        }
        return target;
    }

    float EnsureNumberIsWithin(float number, float lowerLimit, float upperLimit, float margin)
    {
        if (number > upperLimit || number < lowerLimit)
        {
            return Mathf.Clamp(number, lowerLimit+margin, upperLimit-margin);
        }
        return number;
    }

    Vector3 FlipVelocityIfOver(Rigidbody body, float xLimit, float yLimit, float zLimit)
    {
        var position = body.transform.position;
        return new Vector3(
            FlipIfOver(body.velocity.x, position.x, -xLimit, xLimit),
            FlipIfOver(body.velocity.y, position.y, 0, yLimit),
            FlipIfOver(body.velocity.z, position.z, -zLimit, zLimit)
        );
    }

    Vector3 EnsurePositionIsWithin(Vector3 position, float xLimit, float yLimit, float zLimit, float margin)
    {
        return new Vector3(
            EnsureNumberIsWithin(position.x, -xLimit, xLimit, margin),
            EnsureNumberIsWithin(position.y, 0, yLimit, margin),
            EnsureNumberIsWithin(position.z, -zLimit, zLimit, margin)
        );
    }
}
