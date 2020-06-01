using UnityEngine;

/*
    This controller makes sure the objects bounce back instead of going through 
    the arena walls when moving too fast for the collisions Unity calculates.
*/
public class DynamicObjectController : MonoBehaviour
{
    float _bounceMultiplier = 0.8f;
    Rigidbody _dynamicObject;

    void Start()
    {
        _dynamicObject = GetComponent<Rigidbody>();
    }

    void Update()
    {
        _dynamicObject.velocity = FlipVelocityIfOver(_dynamicObject, 2f, 4f, 2f);
        _dynamicObject.transform.position = EnsurePositionIsWithin(_dynamicObject.transform.position, 2f, 6f, 2f, 0.1f);
    }

    float FlipIfOver(float target, float number, float lowerLimit, float upperLimit)
    {
        if (number > upperLimit || number < lowerLimit) {
            return -_bounceMultiplier * target;
        }
        return target;
    }

    float EnsureNumberIsWithin(float number, float lowerLimit, float upperLimit, float margin)
    {
        if (number > upperLimit || number < lowerLimit) {
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
