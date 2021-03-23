using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Turtle : MonoBehaviour
{
    //public float moveTime = 5f;
    public float moveAcceleration = 2f;
    public float moveSpeed = 5f;
    public float rotateAcceleration = 20f;
    public float rotateSpeed = 200f;

    private PositionReporter reporter;

    // Start is called before the first frame update
    void Start()
    {
        reporter = GetComponent<PositionReporter>();
        //StartCoroutine(MoveOverSeconds(gameObject, endPosition, moveTime));
        StartCoroutine(DoSequence());
    }

    private IEnumerator DoSequence()
    {
        Debug.Log("Sequence Started!");
        reporter.ScheduleStart();
        yield return null;
        yield return new WaitForSeconds(2);
        yield return Move(gameObject, 5f, moveSpeed);
        yield return Turn(gameObject, 80f, rotateSpeed);
        yield return Move(gameObject, 10f, moveSpeed);
        yield return Turn(gameObject, -120f, rotateSpeed);
        yield return Move(gameObject, 5f, moveSpeed);
        yield return Turn(gameObject, 80f, rotateSpeed);
        yield return Move(gameObject, 10f, moveSpeed);
        yield return Turn(gameObject, -120f, rotateSpeed);
        yield return Move(gameObject, 5f, moveSpeed);
        reporter.ScheduleStop();
        Debug.Log("Sequence Done!");
        // TODO repeat sequence button in Unity UI
    }

    private IEnumerator Turn(GameObject objectToMove, float angle, float speed)
    {
        Debug.Log("start turn, Y: " + objectToMove.transform.rotation.eulerAngles.y);
        Quaternion end = objectToMove.transform.rotation * Quaternion.Euler(0f, angle, 0f);

        // TODO use acceleration/deceleration 
        while (objectToMove.transform.rotation != end)
        {
            objectToMove.transform.rotation = Quaternion.RotateTowards(objectToMove.transform.rotation, end, speed * Time.deltaTime);
            yield return new WaitForEndOfFrame();
        }
        Debug.Log("end turn, Y: " + objectToMove.transform.rotation.eulerAngles.y);
        yield return null;
    }

    // TODO roll and dive (combine turn and roll)

    public IEnumerator Move(GameObject objectToMove, float distance, float speed)
    {
        Debug.Log("start move");
        
        Vector3 end = objectToMove.transform.position + objectToMove.transform.forward * distance;

        // TODO use acceleration/deceleration 
        while (objectToMove.transform.position != end)
        {
            objectToMove.transform.position = Vector3.MoveTowards(objectToMove.transform.position, end, speed * Time.deltaTime);
            yield return new WaitForEndOfFrame();
        }
        Debug.Log("end move");
        
        yield return null;
    }

    //tranform.Translate(Vector3.forward* curSpeed);
 
    //curSpeed += acceleration;
 
    //if (curSpeed > maxSpeed)
    //    curSpeed = maxSpeed;

    // TODO and slowing down before stopping?


    public IEnumerator MoveOverSeconds(GameObject objectToMove, Vector3 end, float seconds)
    {
        Debug.Log("start move");
        float elapsedTime = 0;
        Vector3 startingPos = objectToMove.transform.position;
        while (elapsedTime < seconds)
        {
            transform.position = Vector3.Lerp(startingPos, end, (elapsedTime / seconds));
            elapsedTime += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        transform.position = end;
        Debug.Log("end move");
    }

    // Update is called once per frame
    void Update()
    {
        Debug.DrawLine(gameObject.transform.position, gameObject.transform.position + (gameObject.transform.forward * 1f), Color.red);
    }
}
