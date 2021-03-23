using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PositionReporter : Detector
{
    // Detects and reports Turtle movement for line drawing

    protected int _lastUpdateFrame = -1;
    protected float _startTime = 0.0f;
    protected float _stopTime = 0.0f;

    protected bool _shouldStart = false;
    protected bool _shouldStop = false;
    protected bool _shouldSegmentStart = false;

    protected bool _didChange = false;

    protected Vector3 _position;
    protected Quaternion _rotation;
    protected Vector3 _direction = Vector3.forward;
    protected Vector3 _normal = Vector3.up;


    /// <summary>
    /// Returns whether or not the value of IsActive is different than the value reported during
    /// the previous frame.
    /// </summary>
    public virtual bool DidChangeFromLastFrame
    {
        get
        {
            return _didChange;
        }
    }

    /// <summary>
    /// Returns whether or not the movement is currently active.
    /// </summary>
    public virtual bool IsMoving
    {
        get
        {
            ensureUpToDate();
            return IsActive;
        }
    }

    /// <summary>
    /// Returns whether or not the value of IsMoving changed to true between this frame and the previous.
    /// </summary>
    public virtual bool DidStart
    {
        get
        {
            ensureUpToDate();
            return DidChangeFromLastFrame && IsMoving;
        }
    }

    /// <summary>
    /// Returns whether or not the value of IsMoving changed to false between this frame and the previous.
    /// </summary>
    public virtual bool DidStop
    {
        get
        {
            ensureUpToDate();
            return DidChangeFromLastFrame && !IsMoving;
        }
    }

    /// <summary>
    /// Returns whether or not the value of IsMoving changed to false between this frame and the previous.
    /// </summary>
    public virtual bool DidSegmentStart
    {
        get
        {
            ensureUpToDate();
            return DidChangeFromLastFrame && !IsMoving;
        }
    }

    /// <summary>
    /// Returns the position value of the detected pinch or grab.  If a pinch or grab is not currently being
    /// detected, returns the most recent position value.
    /// </summary>
    public Vector3 Position
    {
        get
        {
            ensureUpToDate();
            return _position;
        }
    }

    public void ScheduleStart()
    {
        _shouldStart = true;
    }

    public void ScheduleStop()
    {
        _shouldStop = true;
    }

    void ensureUpToDate()
    {
        if (Time.frameCount == _lastUpdateFrame)
        {
            return;
        }

        _lastUpdateFrame = Time.frameCount;

        _didChange = false;

        _rotation = transform.rotation;
        _position = transform.position;
        _direction = transform.forward;
        _normal = transform.up;

        if (IsActive)
        {
            if (_shouldStop)
            {
                changeState(false);
                _shouldStop = false;
            }
        }
        else
        {
            if (_shouldStart)
            {
                changeState(true);
                _shouldStart = false;
            }
        }
    }

    protected virtual void changeState(bool shouldBeActive)
    {
        bool currentState = IsActive;
        if (shouldBeActive)
        {
            _startTime = Time.time;
            Activate();
        }
        else
        {
            _stopTime = Time.time;
            Deactivate();
        }
        if (currentState != IsActive)
        {
            _didChange = true;
        }
    }


    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //ensureUpToDate();
    }
}
