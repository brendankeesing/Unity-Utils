using System.Collections;
using UnityEngine;

public static class PathUtils
{
    static int _doorLayerMask = LayerMask.GetMask("Default");
    static LinePathPoint _cachedPathPoint0 = new LinePathPoint();
    static LinePathPoint _cachedPathPoint1 = new LinePathPoint();

    // fastmoveamount is 0 (not move), 1 (walk), 2 (run)
    public static bool MoveAlongPath(CharacterController character, LinePath path, float targetdistance, float fastmoveamount, float standamount = 1, float snapdistance = 0.5f, float aheaddistance = 1, float arrivedistance = 0.2f, bool opendoors = true)
    {
        aheaddistance = Mathf.Max(aheaddistance, arrivedistance + 0.1f);

        Vector3 currentposition = character.cachedRigidbody.position;

        float nearestdistance = path.GetNearestPointAtPosition(currentposition, _cachedPathPoint0).distance;
        if (!float.IsInfinity(targetdistance) && Mathf.Abs(nearestdistance - targetdistance) < arrivedistance)
            return true;
        Vector3 nearestpoint = path.GetPointAtDistance(nearestdistance, _cachedPathPoint0).position;

#if UNITY_EDITOR
        Debug.DrawLine(currentposition, nearestpoint, Color.green);
#endif

        // make sure character isn't too far from path
        if ((nearestpoint - currentposition).sqrMagnitude > snapdistance * snapdistance)
        {
            currentposition = Vector3.MoveTowards(nearestpoint, currentposition, snapdistance);
            character.cachedTransform.position = currentposition;
        }

        // find position ahead
        aheaddistance *= 1 + fastmoveamount;
        if (targetdistance > nearestdistance)
            targetdistance = nearestdistance + aheaddistance;
        else
            targetdistance = nearestdistance - aheaddistance;
        if (path.loop)
        {
            targetdistance %= path.totalDistance;
            if (targetdistance < 0)
                targetdistance += path.totalDistance;
        }
        else
            targetdistance = Mathf.Clamp(targetdistance, 0, path.totalDistance);

        character.MoveTowardPoint(path.GetPointAtDistance(targetdistance, _cachedPathPoint0).position, fastmoveamount, standamount, arrivedistance);

        // open any doors in the way
        if (opendoors)
        {
            Vector3 origin = character.cachedTransform.position + Vector3.up;
            Vector3 direction = character.cachedTransform.forward;
            float reachdistance = 1;
            Debug.DrawLine(origin, origin + direction * reachdistance, Color.red);
            if (character.isOnGround && Physics.Raycast(origin, direction, out RaycastHit hit, reachdistance, _doorLayerMask))
            {
                Door door = hit.collider.GetComponent<Door>();
                if (door != null)
                    door.OpenIfClose(character);
            }
        }

        return false;
    }

    // fastmoveamount is 0 (not move), 1 (walk), 2 (run)
    public static IEnumerator MoveAlongPathUntilArrive(CharacterController character, LinePath path, float targetdistance, float fastmoveamount, float standamount = 1, float snapdistance = 0.1f, float aheaddistance = 1, float arrivedistance = 0.2f, bool opendoors = true)
    {
        while (!MoveAlongPath(character, path, targetdistance, fastmoveamount, standamount, snapdistance, aheaddistance, arrivedistance, opendoors))
            yield return null;
    }

    public static bool MoveAlongPath(Transform transform, LinePath path, float targetdistance, float acceleration, float maxspeed, ref Vector3 velocity, float snapdistance = 0.5f, float arrivedistance = 0.2f)
    {
        Vector3 pos = transform.position;
        Quaternion rot = transform.rotation;
        bool reachedtarget = MoveAlongPath(ref pos, ref rot, path, targetdistance, acceleration, maxspeed, ref velocity, snapdistance, arrivedistance);
        transform.position = pos;
        transform.rotation = rot;
        return reachedtarget;
    }

    public static bool MoveAlongPath(Rigidbody rigidbody, LinePath path, float targetdistance, float acceleration, float maxspeed, float snapdistance = 0.5f, float arrivedistance = 0.2f)
    {
        Vector3 pos = rigidbody.position;
        Quaternion rot = rigidbody.rotation;
        Vector3 vel = rigidbody.velocity;
        bool reachedtarget = MoveAlongPath(ref pos, ref rot, path, targetdistance, acceleration, maxspeed, ref vel, snapdistance, arrivedistance);
        //rigidbody.position = pos; // position should update itself
        rigidbody.rotation = rot;
        rigidbody.velocity = vel;
        return reachedtarget;
    }

    public static bool MoveAlongPath(ref Vector3 currentposition, ref Quaternion currentrotation, LinePath path, float targetdistance, float acceleration, float maxspeed, ref Vector3 velocity, float snapdistance = 0.5f, float arrivedistance = 0.2f)
    {
        LinePathPoint point = path.GetNearestPointAtPosition(currentposition, _cachedPathPoint1);

        float nearestdistance = point.distance;
        if (Mathf.Abs(nearestdistance - targetdistance) < arrivedistance)
            return true;
        Vector3 nearestpoint = path.GetPointAtDistance(nearestdistance, _cachedPathPoint0).position;

#if UNITY_EDITOR
        Debug.DrawLine(currentposition, nearestpoint, Color.green);
#endif

        // make sure character isn't too far from path
        if ((nearestpoint - currentposition).sqrMagnitude > snapdistance * snapdistance)
            currentposition = Vector3.MoveTowards(nearestpoint, currentposition, snapdistance);

        // find position ahead
        if (targetdistance > nearestdistance || float.IsPositiveInfinity(targetdistance))
            targetdistance = nearestdistance + maxspeed;
        else
            targetdistance = nearestdistance - maxspeed;
        if (path.loop)
        {
            targetdistance %= path.totalDistance;
            if (targetdistance < 0)
                targetdistance += path.totalDistance;
        }
        else
            targetdistance = Mathf.Clamp(targetdistance, 0, path.totalDistance);
        Vector3 targetpoint = path.GetPointAtDistance(targetdistance, _cachedPathPoint0).position;

        currentposition = Vector3.SmoothDamp(currentposition, targetpoint, ref velocity, 1 / acceleration, maxspeed, Time.deltaTime);
        if (velocity.sqrMagnitude > 0.1f)
            currentrotation = Quaternion.Slerp(currentrotation, Quaternion.LookRotation(targetdistance < nearestdistance ? -point.direction : point.direction), Time.deltaTime * 5);
        return false;
    }

    public static bool MoveAlongPath(ref float currentdistance, out bool isgoingforwards, LinePath path, float targetdistance, float acceleration, float maxspeed, ref float velocity, float arrivedistance = 0.2f)
    {
        if (Mathf.Abs(currentdistance - targetdistance) < arrivedistance)
        {
            isgoingforwards = true;
            return true;
        }

        // find position ahead
        if (targetdistance > currentdistance)
            targetdistance = currentdistance + maxspeed;
        else
            targetdistance = currentdistance - maxspeed;
        if (path.loop)
        {
            targetdistance %= path.totalDistance;
            if (targetdistance < 0)
                targetdistance += path.totalDistance;
        }
        else
            targetdistance = Mathf.Clamp(targetdistance, 0, path.totalDistance);

        isgoingforwards = targetdistance > currentdistance;
        currentdistance = Mathf.SmoothDamp(currentdistance, targetdistance, ref velocity, 1 / acceleration, maxspeed, Time.deltaTime);
        return false;
    }

    public static IEnumerator MoveAlongPathUntilArrive(Transform transform, LinePath path, float targetdistance, float acceleration, float maxspeed, float snapdistance = 0.5f, float arrivedistance = 0.2f)
    {
        Vector3 velocity = Vector3.zero;
        while (!MoveAlongPath(transform, path, targetdistance, acceleration, maxspeed, ref velocity, snapdistance, arrivedistance))
            yield return null;
    }
}
