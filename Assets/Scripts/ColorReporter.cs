using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorReporter : Detector
{

    Color _color = Color.white;

    protected bool _didChange = false;
    protected bool _shouldChange = false;
    protected int _lastUpdateFrame = -1;

    public Color Color
    {
        get
        {
            ensureUpToDate();
            return _color;
        }
    }

    public virtual bool ColorChanged
    {
        get
        {
            ensureUpToDate();
            return _didChange;
        }
    }

    public void SetColor(Color col)
    {
        //Debug.Log("Set color");
        _color = col;
        _shouldChange = true;
    }

    void ensureUpToDate()
    {
        if (Time.frameCount == _lastUpdateFrame)
        {
            return;
        }

        _lastUpdateFrame = Time.frameCount;

        _didChange = false;

        if (IsActive)
        {

            changeState(false);
            _shouldChange = false;
        }
        else
        {
            if (_shouldChange)
            {
                changeState(true);
                _shouldChange = false;
            }
        }

        //if (_shouldChange != _didChange)
        //{
        //    Debug.Log("Ensure up to date, shouldChange: " + _shouldChange + ", _didChange: " + _didChange);
        //}
    }

    protected virtual void changeState(bool shouldBeActive)
    {
        bool currentState = IsActive;
        if (shouldBeActive)
        {
            Activate();
        }
        else
        {
            Deactivate();
        }
        if (currentState != IsActive && currentState != true) // Short circuit logic - we only care when on-event fires, not off-event
        {
            _didChange = true;
        }
    }

}
