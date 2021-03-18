using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/**
 * Base class for detectors.
 * 
 * A Detector is an object that observes some aspect of a scene and reports true
 * when the specified conditions are met.
 * 
 * Detector implementations must call Activate() when their conditions are met and
 * Deactivate() when those conditions are no longer met. Implementations should
 * also call Deactivate() when they, or the object they are a component of become disabled.
 * Implementations can call Activate() and Deactivate() more often than is strictly necessary.
 * This Detector base class keeps track of the IsActive status and only dispatches events
 * when the status changes.

 */
public class Detector : MonoBehaviour
{
    /** The current detector state. 
     */
    public bool IsActive { get { return _isActive; } }
    private bool _isActive = false;

    /** Dispatched when the detector activates (becomes true). 
     */
    [Tooltip("Dispatched when condition is detected.")]
    public UnityEvent OnActivate;

    /** Dispatched when the detector deactivates (becomes false). 
     */
    [Tooltip("Dispatched when condition is no longer detected.")]
    public UnityEvent OnDeactivate;

    /**
    * Invoked when this detector activates.
    * Subclasses must call this function when the detector's conditions become true.
    */
    public virtual void Activate()
    {
        if (!IsActive)
        {
            _isActive = true;
            OnActivate.Invoke();
        }
    }

    /**
    * Invoked when this detector deactivates.
    * Subclasses must call this function when the detector's conditions change from true to false.
    */
    public virtual void Deactivate()
    {
        if (IsActive)
        {
            _isActive = false;
            OnDeactivate.Invoke();
        }
    }

}