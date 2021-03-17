using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Miniscript;


public class ScriptManager : MonoBehaviour
{
    static bool intrinsicsAdded = false;

    public Interpreter interpreter;
    public String lastError;

    private String globalVarName = "GLOBAL";
    private IEnumerator coroutine;
    private Intrinsic iFunction;
    private String script = @"

for i in range(10,1)
    // print ""Lift off! "" + """" + i
    wait
end for

print ""Global from miniscript: ""
print GLOBAL

wait

GLOBAL.x = 20
GLOBAL.y = 10
GLOBAL.rot = 180

print ""Global changed by miniscript: ""
print GLOBAL

// TODO intrinsic

wait
print ""Test intrinsic""
wait

walk 20


";

    private String context = @"

reset = function(); self.x=0; self.y = 0; self.rot = 0; end function

print ""Loaded context code""

";

    void Awake()
    {
        interpreter = new Interpreter();

        interpreter.standardOutput = (string s) => Debug.Log(s);
        interpreter.implicitOutput = (string s) => Debug.Log(
            "<color=#66bb66>" + s + "</color>");
        interpreter.errorOutput = (string s) => {
            Debug.Log("<color=red>" + s + "</color>");
            interpreter.Stop();
            lastError = s;
        };

        AddIntrinsics();

        // Test

        PrepareScript(script);

        coroutine = delayChange(2f);
        StartCoroutine(coroutine);
    }

    public void PrepareScript(string sourceCode)
    {
        interpreter.Reset(context + sourceCode);
        interpreter.Compile();

        setMyGlobals(transform.localPosition.x, transform.localPosition.y, transform.localEulerAngles.z);
    }

    public void AddIntrinsics()
    {
        if (intrinsicsAdded) return; // already done!
        intrinsicsAdded = true;

        iFunction = Intrinsic.Create("walk");
        iFunction.AddParam("distance", 1);
        iFunction.code = (context, partialResult) => {
            //var rs = context.interpreter.hostData;

            float distance = (float)context.GetVar("distance").DoubleValue();
            ValMap val = context.GetVar(globalVarName) as ValMap;
            // ValMap val = interpreter.GetGlobalValue(globalVarName) as ValMap;
            if (IntrinsicChange(val, distance)) return Intrinsic.Result.True;
            return Intrinsic.Result.False;
        };
    }

    void Update()
    {
        try
        {
            interpreter.RunUntilDone(0.01);
        }
        catch (MiniscriptException err)
        {
            Debug.Log("Script error: " + err.Description());
        }

    }

    IEnumerator delayChange(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);

        Debug.Log("Getting global: ");
        ValMap val = interpreter.GetGlobalValue(globalVarName) as ValMap;
        if (val != null) Debug.Log(val);

        yield return new WaitForSeconds(waitTime);

        Debug.Log("Setting global...");

        transform.position = transform.position += new Vector3(50f, 10f, 0f);
        transform.localEulerAngles += new Vector3(0f, 0f, 270f);

        setMyGlobals(transform.localPosition.x, transform.localPosition.y, transform.localEulerAngles.z);

        yield return new WaitForSeconds(waitTime);

        Debug.Log("Getting global 2nd time: ");
        // TODO get globals method (and globals struct in class?)
        val = interpreter.GetGlobalValue(globalVarName) as ValMap;
        if (val != null) Debug.Log(val["x"]);
    }

    void setMyGlobals(float x, float y, float rot)
    {
        // Set globals in Miniscript
        ValMap data = new ValMap();

        data["x"] = new ValNumber(x);
        data["y"] = new ValNumber(y);
        data["rot"] = new ValNumber(rot);

        interpreter.SetGlobalValue(globalVarName, data);
    }

    bool IntrinsicChange(ValMap global, float distance)
    {
        Debug.Log("Getting global from inside intrinsic context: ");
        Debug.Log(global);

        Debug.Log("Getting intrinsic param: ");
        Debug.Log(distance);
        return true;
    }
}
